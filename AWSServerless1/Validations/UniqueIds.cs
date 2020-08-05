using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BillManagerServerless.Validations
{
    public class UniqueIds : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var ids = (int[])value;

            if (ids.Count() != ids.Distinct().Count())
            {
                return false;
            }

            return true;
        }
    }
}