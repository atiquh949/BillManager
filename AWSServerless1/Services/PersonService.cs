using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using BillManagerServerless.Common;
using BillManagerServerless.Data;
using BillManagerServerless.Dto;
using BillManagerServerless.Models;
using BillManagerServerless.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace BillManagerServerless.Services
{
    public class PersonService : IPersonService
    {
        private readonly BillManagerDBContext _context;

        public PersonService(BillManagerDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PersonDetailDto>> GetPersonsAsync()
        {
            var result = new List<PersonDetailDto>();

            foreach (var person in await _context.Person.ToListAsync())
            {
                result.Add(await GetPersonDetailAsync(person.Id));
            }

            return result;
        }

        public async Task<PersonDetailDto> GetPersonDetailAsync(long id)
        {

            return await _context.Person
                .Include(x => x.PersonBillShares)
                .ThenInclude(x => x.Bill)
                .Select(x => new PersonDetailDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    PhoneNumber = x.PhoneNumber,
                    Bills = x.PersonBillShares.Select(y => new BillDetailDto
                    {
                        Id = y.BillId,
                        TotalAmount = y.Bill.TotalAmount,
                        Title = y.Bill.Title,
                        CreateDateTime = y.Bill.CreateDateTime
                    }).ToList()
                })
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Person> GetPersonAsync(long id)
        {
            return await _context.Person.FindAsync(id);
        }

        public async Task<PersonDetailDto> UpdatePersonAsync(Person existingPerson, PersonRequest person)
        {
            existingPerson.FirstName = person.FirstName;
            existingPerson.LastName = person.LastName;

            _context.Entry(existingPerson).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var personDetail = await GetPersonDetailAsync(existingPerson.Id);
            return personDetail;
        }

        public async Task<PersonDetailDto> CreatePersonAsync(PersonRequest personRequest)
        {
            var person = GetPersonObj(personRequest);

            _context.Person.Add(person);
            await _context.SaveChangesAsync();

            return await GetPersonDetailAsync(person.Id);
        }

        public Person GetPersonObj(PersonRequest person)
        {
            return new Person
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                PhoneNumber = person.PhoneNumber
            };
        }

        public async Task<Person> DeletePersonAsync(Person person)
        {
            try
            {
                _context.Person.Remove(person);
                await _context.SaveChangesAsync();
                return person;
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in DeletePerson. Person id: " + person.Id + " Exception: " + e.ToString());

                if (e.InnerException != null && e.InnerException.Message.Contains("FK_PersonBillShare_Person_PersonId"))
                    throw new PersonDeleteBillAssociatedException();
                else
                    throw new PersonDeleteException();
            }
        }

        public async Task<bool> IsPhoneAlreadyExistsAsync(long personId, string personPhoneNumber)
        {
            return await _context.Person.AnyAsync(p => p.PhoneNumber == personPhoneNumber
                                             && p.Id != personId);
        }
    }
}