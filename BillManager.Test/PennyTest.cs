using BillManagerServerless.Logic;
using NUnit.Framework;

namespace BillManagerService.Test
{
    public class Tests
    {
        BillLogic _billLogic;

        [SetUp]
        public void Setup()
        {
            _billLogic = new BillLogic(null); ;
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
            Assert.AreEqual(_billLogic.ValueHasMoreThanTwoDecimalPlaces(share), pennyNeedsAdjustment, "PennyNeedsAdjustmentTest failed on total: " + total + " numOfPersons: " + numOfPersons);
        }
    }
}