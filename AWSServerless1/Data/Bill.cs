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
        public long Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Title { get; set; }

        public DateTimeOffset CreateDateTime { get; set; }

        public virtual ICollection<PersonBillShare> PersonBillShares { get; set; }
    }

    public class BillRequest
    {
        public long Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Title { get; set; }
        public int[] People { get; set; }
    }

    public class BillDetail
    {
        public long Id { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Title { get; set; }
        public DateTimeOffset CreateDateTime { get; set; }

        public List<PersonDetail> People { get; set; }
    }
}