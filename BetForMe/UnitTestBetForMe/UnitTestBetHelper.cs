using System;
using BetForMe.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestBetForMe {
    [TestClass]
    public class UnitTestBetHelper {

        private BetHelper _betHelper = new BetHelper();

        [TestMethod]
        public void TestCalculateBet() {

            Assert.AreEqual(150, _betHelper.CalculateBet(100, 1.5, true));
            Assert.AreEqual(150, _betHelper.CalculateBet(100, 1.5));
            Assert.AreEqual(0, _betHelper.CalculateBet(100, 1.5, false));

        }
    }
}
