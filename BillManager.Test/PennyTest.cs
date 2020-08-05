using BillManagerServerless.Helpers;
using BillManagerServerless.Services;
using NUnit.Framework;

namespace BillManagerService.Test
{
    public class Tests
    {
        BillService _billService;

        [SetUp]
        public void Setup()
        {
            _billService = new BillService(null); ;
        }

        [TestCase(10.00, 1, false)]
        [TestCase(10.11, 1, false)]
        [TestCase(10.111, 1, true)]
        [TestCase(100.00, 2, false)]
        [TestCase(100.00, 3, true)]
        [TestCase(0.25, 3, true)]
        public void PennyNeedsAdjustmentTest(decimal total, int numOfPersons, bool pennyNeedsAdjustment)
        {
            decimal share = total / numOfPersons;
            Assert.AreEqual(NumberHelper.ValueHasMoreThanTwoDecimalPlaces(share), pennyNeedsAdjustment, "PennyNeedsAdjustmentTest failed on total: " + total + " numOfPersons: " + numOfPersons);
        }
    }
}