using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using BillManagerServerless.Data;
using BillManagerServerless.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BillManagerServerless.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        private readonly PersonLogic _logic;

        public PersonsController(BillManagerDBContext context)
        {
            _logic = new PersonLogic(context);
        }

        // GET: api/Persons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonDetail>>> GetPersons() //TODO: do not return Person table object
        {
            try
            {
                return StatusCode(StatusCodes.Status200OK, await _logic.GetPersons());
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetPersons: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // GET: api/Persons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDetail>> GetPerson(int id)
        {
            try
            {
                var person = await _logic.GetPersonDetail(id);

                if (person == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                return StatusCode(StatusCodes.Status200OK, person);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetPerson: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // PUT: api/Persons/5
        [HttpPut("{id}")]
        public async Task<ActionResult<PersonDetail>> PutPerson(int id, PersonRequest person)
        {
            try
            {
                if (id != person.Id)
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                Person existingPerson;

                if ((existingPerson = await _logic.GetPerson(id)) == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                string errors = await _logic.ValidatePerson(person);

                if (!String.IsNullOrEmpty(errors))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, errors);
                }

                PersonDetail personDetail = await _logic.UpdatePerson(existingPerson, person);
                return StatusCode(StatusCodes.Status200OK, personDetail);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in PutPerson: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // POST: api/Persons
        [HttpPost]
        public async Task<ActionResult<PersonDetail>> PostPerson(PersonRequest person)
        {
            try
            {
                string errors = await _logic.ValidatePerson(person);
                if (!String.IsNullOrEmpty(errors))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, errors);
                }

                PersonDetail personDetail = await _logic.PostPerson(person);
                return StatusCode(StatusCodes.Status200OK, personDetail);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in PostPerson: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // DELETE: api/Persons/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePerson(int id)
        {
            try
            {
                var person = await _logic.GetPerson(id);

                if (person == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                await _logic.DeletePerson(person);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in DeletePerson: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

    }
}