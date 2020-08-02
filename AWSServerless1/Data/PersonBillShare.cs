using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillManagerServerless.Data
{
    [Table("PersonBillShare")]
    public class PersonBillShare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Person")]
        public long PersonId { get; set; }

        [ForeignKey("Bill")]
        public long BillId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Share { get; set; }


        public virtual Bill Bill { get; set; }
        public virtual Person Person { get; set; }

    }
}
