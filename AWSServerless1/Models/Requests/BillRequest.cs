using System.ComponentModel.DataAnnotations;
using BillManagerServerless.Validations;

namespace BillManagerServerless.Models.Requests
{
    public class BillRequest
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0.01, int.MaxValue, ErrorMessage = "Total Amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "PersonIds are required")]
        [UniqueIds(ErrorMessage = "List of ids should have no repeat value.")]
        [MustHaveOneElement(ErrorMessage = "Bill must include at least one person.")]
        public int[] PersonIds { get; set; }
    }
}
