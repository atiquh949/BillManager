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
    public class BillsController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillsController(IBillService billService)
        {
            _billService = billService;
        }

        // GET: api/Bills
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillDetailDto>>> GetBills()
        {
            try
            {
                return await _billService.GetBillsAsync();
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetBills: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // GET: api/Bills/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BillDetailDto>> GetBill(long id)
        {
            try
            {
                var billDetail = await _billService.GetBillDetailAsync(id);

                if (billDetail == null)
                {
                    return NotFound();
                }

                return billDetail;
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetBill" + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // POST: api/Bills
        [HttpPost]
        public async Task<ActionResult<BillDetailDto>> PostBill(BillRequest billRequest)
        {
            try
            {
                BillDetailDto bill = await _billService.CreateBillAsync(billRequest);
                return bill;
            }
            catch (BillCreatePersonMissingException)
            {
                return BadRequest("Unable to create bill. Associated person does not exist");
            }
            catch (BillCreateException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "");
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in PostBill" + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error."); //TODO: return reference
            }
        }

        //DELETE: api/Bills/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBill(long id)
        {
            try
            {
                var bill = await _billService.GetBillAsync(id);

                if (bill == null)
                {
                    return NotFound();
                }

                await _billService.DeleteBillAsync(bill);
                return Ok();
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in DeleteBill" + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

    }
}