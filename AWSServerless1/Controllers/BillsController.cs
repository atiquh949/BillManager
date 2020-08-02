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
    public class BillsController : ControllerBase
    {
        private readonly BillLogic _logic;

        public BillsController(BillManagerDBContext context)
        {
            _logic = new BillLogic(context);
        }

        // GET: api/Bills
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillDetail>>> GetBills()
        {
            try
            {
                return StatusCode(StatusCodes.Status200OK, await _logic.GetBills());
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetBills: " + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // GET: api/Bills/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BillDetail>> GetBill(long id)
        {
            try
            {
                var billDetail = await _logic.GetBillDetail(id);

                if (billDetail == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                return StatusCode(StatusCodes.Status200OK, billDetail);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in GetBill" + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        // POST: api/Bills
        [HttpPost]
        public async Task<ActionResult<BillDetail>> PostBill(BillRequest billRequest)
        {
            try
            {
                string errors = _logic.ValidateBillRequest(billRequest);

                if (!String.IsNullOrEmpty(errors))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, errors);
                }

                BillDetail bill = await _logic.CreateBill(billRequest);
                return StatusCode(StatusCodes.Status200OK, bill);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in PostBill" + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

        //DELETE: api/Bills/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBill(long id)
        {
            try
            {
                var bill = await _logic.GetBill(id);

                if (bill == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                await _logic.DeleteBill(bill);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Error in DeleteBill" + e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error.");
            }
        }

    }
}