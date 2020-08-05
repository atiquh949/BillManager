using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillManagerServerless.Data;
using BillManagerServerless.Dto;
using BillManagerServerless.Models;
using BillManagerServerless.Models.Requests;

namespace BillManagerServerless.Services
{
    public interface IPersonService
    {
        Task<IEnumerable<PersonDetailDto>> GetPersonsAsync();

        Task<PersonDetailDto> GetPersonDetailAsync(long id);

        Task<Person> GetPersonAsync(long id);

        Task<PersonDetailDto> UpdatePersonAsync(Person existingPerson, PersonRequest person);

        Task<PersonDetailDto> CreatePersonAsync(PersonRequest personRequest);

        Task<Person> DeletePersonAsync(Person person);

        Task<bool> IsPhoneAlreadyExistsAsync(long personId, string personPhoneNumber);

        Person GetPersonObj(PersonRequest person);
    }
}
