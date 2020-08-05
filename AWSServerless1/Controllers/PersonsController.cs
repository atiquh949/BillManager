using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using BillManagerServerless.Common;
using BillManagerServerless.Data;
using BillManagerServerless.Dto;
using BillManagerServerless.Models;
using BillManagerServerless.Models.Requests;
using BillManagerServerless.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BillManagerServerless.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonsController(IPersonService personService)
        {
            _personService = personService;
        }

        // GET: api/Persons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonDetailDto>>> GetPersons()
        {
            try
            {
                return Ok(await _personService.GetPersonsAsync());
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetPersons: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // GET: api/Persons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDetailDto>> GetPerson(long id)
        {
            try
            {
                var person = await _personService.GetPersonDetailAsync(id);

                if (person == null)
                {
                    return NotFound();
                }

                return person;
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetPerson: " + e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // PUT: api/Persons/5
        [HttpPut("{id}")]
        public async Task<ActionResult<PersonDetailDto>> PutPerson(long id, PersonRequest person)
        {
            try
            {
                if (id != person.Id)
                {
                    return BadRequest();
                }

                var existingPerson = await _personService.GetPersonAsync(id);

                if (existingPerson == null)
                {
                    return NotFound();
                }

                // Validate Phone 
                var phoneExists = await _personService.IsPhoneAlreadyExistsAsync(person.Id, person.PhoneNumber);
                if (phoneExists)
                {
                    ModelState.AddModelError(nameof(person.PhoneNumber), "Phone Number already exists.");
                    return BadRequest(ModelState);
                }

                return await _personService.UpdatePersonAsync(existingPerson, person);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in PutPerson: " + e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // POST: api/Persons
        [HttpPost]
        public async Task<ActionResult<PersonDetailDto>> PostPerson(PersonRequest person)
        {
            try
            {
                // Validate Phone 
                var phoneExists = await _personService.IsPhoneAlreadyExistsAsync(person.Id, person.PhoneNumber);
                if (phoneExists)
                {
                    ModelState.AddModelError(nameof(person.PhoneNumber), "Phone Number already exists.");
                    return BadRequest(ModelState);
                }

                return await _personService.CreatePersonAsync(person);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in PostPerson: " + e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // DELETE: api/Persons/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePerson(long id)
        {
            try
            {
                var person = await _personService.GetPersonAsync(id);

                if (person == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                await _personService.DeletePersonAsync(person);
                return Ok();
            }
            catch (PersonDeleteBillAssociatedException)
            {
                ModelState.AddModelError(nameof(Person.Id), "Unable to delete person as its associated with a bill");
                return BadRequest(ModelState);
            }
            catch (PersonDeleteException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to delete person");
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in DeletePerson: " + e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }
    }
}