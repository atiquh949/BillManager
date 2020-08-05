using System;
using System.Collections.Generic;

namespace BillManagerServerless.Dto
{
    public class BillDetailDto
    {
        public long Id { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Title { get; set; }
        public DateTimeOffset CreateDateTime { get; set; }

        public List<PersonDetailDto> Persons { get; set; }
    }
}
