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
    public interface IBillService
    {
        Task<List<BillDetailDto>> GetBillsAsync();

        Task<Bill> GetBillAsync(long id);

        Task<BillDetailDto> GetBillDetailAsync(long billId);

        Task<BillDetailDto> CreateBillAsync(BillRequest billRequest);

        Task<Bill> DeleteBillAsync(Bill bill);

        Task CreateBillSharesAsync(BillRequest billRequest, long billId);

        Bill GetBillObject(BillRequest billRequest);
    }
}
