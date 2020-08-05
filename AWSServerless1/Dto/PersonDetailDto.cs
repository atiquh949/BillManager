using System.Collections.Generic;
using BillManagerServerless.Models;

namespace BillManagerServerless.Dto
{
    public class PersonDetailDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public decimal? Share { get; set; }

        public List<BillDetailDto> Bills { get; set; }
    }
}
