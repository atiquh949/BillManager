using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BillManagerServerless.Validations
{
    public class MustHaveOneElementAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var array = (int[])value;
            if (array != null)
            {
                return array.Count() > 0;
            }
            return false;
        }
    }
}