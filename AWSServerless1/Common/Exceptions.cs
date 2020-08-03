using System;

namespace BillManagerServerless.Common
{
    public class PersonDeleteException : Exception { }
    public class PersonDeleteBillAssociatedException : Exception { }

    public class BillCreateException : Exception { }
    public class BillCreatePersonMissingException : Exception { }
}