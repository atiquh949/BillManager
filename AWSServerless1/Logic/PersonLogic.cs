using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BillManagerServerless.Data;

namespace BillManagerServerless.Logic
{
    public class PersonLogic
    {
        private readonly BillManagerDBContext _context;

        public PersonLogic(BillManagerDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PersonDetail>> GetPersons()
        {
            List<PersonDetail> result = new List<PersonDetail>();

            foreach (var person in await _context.Person.ToListAsync())
            {
                result.Add(GetPersonDetail(person));
            }

            return result;
        }

        private PersonDetail GetPersonDetail(Person person)
        {
            if (person == null)
                return null;

            long[] billIds = _context.PersonBill
                                        .Where(pb => pb.PersonId == person.Id).Select(pb => pb.BillId).ToArray();

            List<Bill> bills = _context.Bill
                                        .Where(b => billIds.Contains(b.Id))
                                        .ToList();

            List<BillDetail> billDetails = new List<BillDetail>();

            foreach (Bill bill in bills)
            {
                BillDetail bd = new BillDetail
                {
                    Id = bill.Id,
                    TotalAmount = bill.TotalAmount,
                    Title = bill.Title,
                    CreateDateTime = bill.CreateDateTime
                };
                billDetails.Add(bd);
            }

            PersonDetail personDetail = new PersonDetail
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                PhoneNumber = person.PhoneNumber,
                Bills = billDetails?.Count == 0 ? null : billDetails
            };

            return personDetail;
        }

        public async Task<PersonDetail> GetPersonDetail(long id)
        {
            Person person = await _context.Person.FindAsync(id);
            
            if (person == null)
                return null;

            PersonDetail personDetail = GetPersonDetail(person);
            return personDetail;
        }

        public async Task<Person> GetPerson(long id)
        {
            return await _context.Person.FindAsync(id);
        }

        public async Task<PersonDetail> UpdatePerson(Person existingPerson, PersonRequest person)
        {
            existingPerson.FirstName = person.FirstName;
            existingPerson.LastName = person.LastName;

            _context.Entry(existingPerson).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            PersonDetail personDetail = GetPersonDetail(existingPerson);
            return personDetail;
        }

        public async Task<PersonDetail> PostPerson(PersonRequest person)
        {
            Person personObj = GetPersonObj(person);

            _context.Person.Add(personObj);
            await _context.SaveChangesAsync();

            PersonDetail personDetail = GetPersonDetail(personObj);
            return personDetail;
        }

        private static Person GetPersonObj(PersonRequest person)
        {
            Person personObj = new Person
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                PhoneNumber = person.PhoneNumber
            };

            return personObj;
        }

        public async Task<Person> DeletePerson(Person person)
        {
            _context.Person.Remove(person);
            await _context.SaveChangesAsync();
            return person;
        }

        public async Task<string> ValidatePerson(PersonRequest person)
        {
            string error_result = "";

            if (string.IsNullOrEmpty(person.FirstName))
                error_result += ("- First Name is required." + Environment.NewLine);
            if (string.IsNullOrEmpty(person.LastName))
                error_result += ("- Last Name is required." + Environment.NewLine);
            if (string.IsNullOrEmpty(person.PhoneNumber))
                error_result += ("- Phone Number is required." + Environment.NewLine);
            else
            {
                if (!Regex.Match(person.PhoneNumber, @"^\d{3}-\d{3}-\d{4}$").Success)
                    error_result += ("- Phone Number has incorrect format." + Environment.NewLine);

                Person person_result = await _context.Person.Where(p => p.PhoneNumber == person.PhoneNumber
                                                                    && p.Id != person.Id).FirstOrDefaultAsync();
                if (person_result != null)
                    error_result += ("- Phone Number already exists." + Environment.NewLine);
            }
            return error_result;
        }
    }
}