using System;
using System.Collections.Generic;
using System.Linq;
using BetForMe.Helpers;
using BetForMe.Model;
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

        [TestMethod]
        public void TestGetLeagueTableTop() {

            string sqlQuery = "select * from England where season like '2014-2015';";

            List<England> allGames;

            using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                allGames = c.Database.SqlQuery<England>(sqlQuery).ToList<England>();
            }

            List<string> topTen = _betHelper.GetLeagueTableTop(allGames, 5, new DateTime(2014, 09, 15));


        }
    }
}
