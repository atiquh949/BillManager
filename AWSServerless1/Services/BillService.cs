using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using BillManagerServerless.Common;
using BillManagerServerless.Data;
using BillManagerServerless.Dto;
using BillManagerServerless.Helpers;
using BillManagerServerless.Models;
using BillManagerServerless.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace BillManagerServerless.Services
{
    public class BillService : IBillService
    {
        private readonly BillManagerDBContext _context;

        public BillService(BillManagerDBContext context)
        {
            _context = context;
        }

        //public async Task<List<BillDetailDto>> Old_GetBillsAsync()
        //{
        //    var result = new List<BillDetailDto>();

        //    foreach (var bill in await _context.Bill.ToListAsync())
        //    {
        //        result.Add(await GetBillDetailAsync(bill.Id));
        //    }

        //    return result;
        //}

        public async Task<Bill> GetBillAsync(long id)
        {
            return await _context.Bill.FindAsync(id);
        }

        public async Task<List<BillDetailDto>> GetBillsAsync()
        {
            return await _context.Bill
                .Include(x => x.PersonBillShares)
                .ThenInclude(x => x.Person)
                .Select(x => new BillDetailDto
                {
                    Id = x.Id,
                    TotalAmount = x.TotalAmount,
                    Title = x.Title,
                    CreateDateTime = x.CreateDateTime,
                    Persons = x.PersonBillShares.Select(y => new PersonDetailDto
                    {
                        Id = y.Person.Id,
                        FirstName = y.Person.FirstName,
                        LastName = y.Person.LastName,
                        PhoneNumber = y.Person.PhoneNumber,
                        Share = y.Share
                    }).ToList()
                }).ToListAsync();
        }


        public async Task<BillDetailDto> GetBillDetailAsync(long billId)
        {
            return await _context.Bill
                .Include(x => x.PersonBillShares)
                .ThenInclude(x => x.Person)
                .Select(x => new BillDetailDto
                {
                    Id = x.Id,
                    TotalAmount = x.TotalAmount,
                    Title = x.Title,
                    CreateDateTime = x.CreateDateTime,
                    Persons = x.PersonBillShares.Select(y => new PersonDetailDto
                    {
                        Id = y.Person.Id,
                        FirstName = y.Person.FirstName,
                        LastName = y.Person.LastName,
                        PhoneNumber = y.Person.PhoneNumber,
                        Share = y.Share
                    }).ToList()
                })
                .SingleOrDefaultAsync(x => x.Id == billId);
        }

        public async Task<BillDetailDto> CreateBillAsync(BillRequest billRequest)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var bill = GetBillObject(billRequest);

                    _context.Bill.Add(bill);

                    await _context.SaveChangesAsync();

                    await CreateBillSharesAsync(billRequest, bill.Id);

                    await dbContextTransaction.CommitAsync();

                    return await GetBillDetailAsync(bill.Id);
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

        public async Task CreateBillSharesAsync(BillRequest billRequest, long billId)
        {
            var share = billRequest.TotalAmount / billRequest.PersonIds.Length;
            var pennyAdjustNeeded = NumberHelper.ValueHasMoreThanTwoDecimalPlaces(share);

            share = NumberHelper.TruncateDecimal(share, 2);

            var peopleBill = new List<PersonBillShare>();

            foreach (long personId in billRequest.PersonIds)
            {
                PersonBillShare personBill = new PersonBillShare();
                personBill.PersonId = personId;
                personBill.BillId = billId;
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

        public Bill GetBillObject(BillRequest billRequest)
        {
            return new Bill
            {
                TotalAmount = billRequest.TotalAmount,
                Title = billRequest.Title,
                CreateDateTime = DateTimeOffset.Now
            };
        }

        public async Task<Bill> DeleteBillAsync(Bill bill)
        {
            var peopleBills = _context.PersonBill.Where(pb => pb.BillId == bill.Id);
            _context.PersonBill.RemoveRange(peopleBills);
            await _context.SaveChangesAsync();

            _context.Bill.Remove(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

    }
}