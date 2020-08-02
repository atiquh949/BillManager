using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillManagerServerless.Data
{
    [Table("Bill")]
    public class Bill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Title { get; set; }
        public DateTime Datetime { get; set; }

        public virtual ICollection<PersonBillShare> PersonBillShares { get; set; }
    }

    public class BillRequest
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Title { get; set; }
        public int[] People { get; set; }
    }

    public class BillDetail
    {
        public int Id { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Title { get; set; }
        public DateTime CreateDateTime { get; set; }

        public List<PersonDetail> People { get; set; }
    }
}