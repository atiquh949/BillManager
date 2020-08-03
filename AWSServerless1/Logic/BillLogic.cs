using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using BillManagerServerless.Common;
using BillManagerServerless.Data;
using Microsoft.EntityFrameworkCore;

namespace BillManagerServerless.Logic
{
    public class BillLogic
    {
        private readonly BillManagerDBContext _context;

        public BillLogic(BillManagerDBContext context)
        {
            _context = context;
        }

        public async Task<List<BillDetail>> GetBills()
        {
            List<BillDetail> result = new List<BillDetail>();

            foreach (var bill in await _context.Bill.ToListAsync())
            {
                result.Add(await GetBillDetail(bill));
            }

            return result;
        }

        public async Task<Bill> GetBill(long id)
        {
            return await _context.Bill.FindAsync(id);
        }
        public async Task<BillDetail> GetBillDetail(long id)
        {
            return await GetBillDetail(await _context.Bill.FindAsync(id));
        }

        public async Task<BillDetail> GetBillDetail(Bill bill)
        {
            if (bill == null)
                return null;

            Dictionary<long, decimal> peopleAmounts = await _context.PersonBill
                                                              .Where(pb => pb.BillId == bill.Id)
                                                              .ToDictionaryAsync(pb => pb.PersonId, pb => pb.Share);

            List<Person> people = _context.Person
                                          .Where(p => peopleAmounts.Keys.Contains(p.Id))
                                          .ToList();

            List<PersonDetail> personBill = new List<PersonDetail>();

            foreach(Person person in people)
            {
                PersonDetail ppb = new PersonDetail
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PhoneNumber = person.PhoneNumber,
                    Share = peopleAmounts[person.Id]
                };
                personBill.Add(ppb);
            }

            BillDetail billDetail = new BillDetail
            {
                Id = bill.Id,
                TotalAmount = bill.TotalAmount,
                Title = bill.Title,
                CreateDateTime = bill.CreateDateTime,
                Persons = personBill
            };

            return billDetail;
        }

        public async Task<BillDetail> CreateBill(BillRequest billRequest)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Bill bill = GetBillObject(billRequest);

                    _context.Bill.Add(bill);

                    await _context.SaveChangesAsync();

                    await CreateBillShares(billRequest, bill.Id);

                    await dbContextTransaction.CommitAsync();

                    return await GetBillDetail(bill);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();

                    //TODO: Overload ToString Method
                    LambdaLogger.Log("Error in CreateBill. Bill Request: " + billRequest.ToString() + " Exception: " + e.ToString()); 

                    if (e.InnerException != null && e.InnerException.Message.Contains("FK_PersonBillShare_Person_PersonId"))
                        throw new BillCreatePersonMissingException();
                    else
                        throw new BillCreateException();
                }
            }
        }

        private async Task CreateBillShares(BillRequest billRequest, long billID)
        {
            decimal share = billRequest.TotalAmount / billRequest.PersonIds.Length;
            bool pennyAdjustNeeded = ValueHasMoreThanTwoDecimalPlaces(share);

            share = TruncateDecimal(share, 2);

            List<PersonBillShare> peopleBill = new List<PersonBillShare>();

            foreach (long personId in billRequest.PersonIds)
            {
                PersonBillShare personBill = new PersonBillShare();
                personBill.PersonId = personId;
                personBill.BillId = billID;
                if (pennyAdjustNeeded)
                {
                    personBill.Share = share + (decimal)0.01;
                    pennyAdjustNeeded = false;
                }
                else
                {
                    personBill.Share = share;
                }
                peopleBill.Add(personBill);
            }
            _context.PersonBill.AddRange(peopleBill);
            await _context.SaveChangesAsync();
        }

        private static Bill GetBillObject(BillRequest billRequest)
        {
            Bill bill = new Bill
            {
                TotalAmount = billRequest.TotalAmount,
                Title = billRequest.Title,
                CreateDateTime = DateTimeOffset.Now
            };

            return bill;
        }

        public async Task<Bill> DeleteBill(Bill bill)
        {
            var peopleBills = _context.PersonBill.Where(pb => pb.BillId == bill.Id);
            _context.PersonBill.RemoveRange(peopleBills);
            await _context.SaveChangesAsync();

            _context.Bill.Remove(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

        public string ValidateBillRequest(BillRequest billRequest)
        {
            string error_result = "";

            if (billRequest.TotalAmount <= 0)
                error_result += "- Total Amount must be greater than zero." + Environment.NewLine;

            if (ValueHasMoreThanTwoDecimalPlaces(billRequest.TotalAmount))
                error_result += "- Total Amount is in invalid format." + Environment.NewLine;

            if (string.IsNullOrEmpty(billRequest.Title))
                error_result += "- Title is required." + Environment.NewLine;

            if (billRequest.PersonIds == null || billRequest.PersonIds.Length == 0)
                error_result += "- Bill must include at least one person." + Environment.NewLine;

            if (billRequest.PersonIds.Distinct().Count() != billRequest.PersonIds.Count())
                error_result += "- List of ids of persons associated will bill should have no repeat value." + Environment.NewLine;

            return error_result;
        }

        public bool ValueHasMoreThanTwoDecimalPlaces(decimal number)
        {
            return (number - Math.Truncate(number)).ToString().Length > 4;
        }

        public decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }
    }
}