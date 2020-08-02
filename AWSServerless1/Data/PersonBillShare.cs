using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillManagerServerless.Data
{
    [Table("PersonBill")]
    public class PersonBillShare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Person")]
        public int PersonId { get; set; }

        [ForeignKey("Bill")]
        public int BillId { get; set; }

        public decimal Share { get; set; }


        public virtual Bill Bill { get; set; }
        public virtual Person Person { get; set; }

    }
}
