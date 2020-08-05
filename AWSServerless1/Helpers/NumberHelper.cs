using System;

namespace BillManagerServerless.Helpers
{
    public static class NumberHelper
    {
        public static bool ValueHasMoreThanTwoDecimalPlaces(decimal number)
        {
            return (number - Math.Truncate(number)).ToString().Length > 4;
        }

        public static decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }
    }
}