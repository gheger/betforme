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

            List<string> topFive14_15real = new List<string> { "Chelsea", "Man City", "Arsenal", "Man United", "Tottenham" };
            List<string> topFive15_16real = new List<string> { "Leicester", "Arsenal", "Tottenham", "Man United", "Man City" };
            List<string> topFive16_17real = new List<string> { "Chelsea", "Tottenham", "Man City", "Liverpool", "Arsenal" };
            List<string> topFive17_18real = new List<string> { "Man City", "Man United", "Tottenham", "Liverpool", "Chelsea" };
            List<string> topFive18_19real = new List<string> { "Man City", "Liverpool", "Chelsea", "Tottenham", "Arsenal" };

            List<Game> allGames14_15, allGames15_16, allGames16_17, allGames17_18, allGames18_19;
            using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                allGames14_15 = c.Database.SqlQuery<Game>("select * from England where season like '2014-2015';").ToList<Game>();
                allGames15_16 = c.Database.SqlQuery<Game>("select * from England where season like '2015-2016';").ToList<Game>();
                allGames16_17 = c.Database.SqlQuery<Game>("select * from England where season like '2016-2017';").ToList<Game>();
                allGames17_18 = c.Database.SqlQuery<Game>("select * from England where season like '2017-2018';").ToList<Game>();
                allGames18_19 = c.Database.SqlQuery<Game>("select * from England where season like '2018-2019';").ToList<Game>();
            }

            List<string> topFive14_15computed = _betHelper.GetLeagueTableTop("England", allGames14_15, 5, new DateTime(2015, 06, 01));
            List<string> topFive15_16computed = _betHelper.GetLeagueTableTop("England", allGames15_16, 5, new DateTime(2016, 06, 01));
            List<string> topFive16_17computed = _betHelper.GetLeagueTableTop("England", allGames16_17, 5, new DateTime(2017, 06, 01));
            List<string> topFive17_18computed = _betHelper.GetLeagueTableTop("England", allGames17_18, 5, new DateTime(2018, 06, 01));
            List<string> topFive18_19computed = _betHelper.GetLeagueTableTop("England", allGames18_19, 5, new DateTime(2019, 06, 01));

            CollectionAssert.AreEqual(topFive14_15computed, topFive14_15real);
            CollectionAssert.AreEqual(topFive15_16computed, topFive15_16real);
            CollectionAssert.AreEqual(topFive16_17computed, topFive16_17real);
            CollectionAssert.AreEqual(topFive17_18computed, topFive17_18real);
            CollectionAssert.AreEqual(topFive18_19computed, topFive18_19real);

        }

        [TestMethod]
        public void TestGetOddsCoordinates() {
            IList<double> list = _betHelper.GetOddsCoordinates(1.3, 2.2, 10);

            Assert.AreEqual(list.First(), 1.3);
            Assert.AreEqual(list.Last(), 2.2);
            Assert.AreEqual(list[5], 1.8);
        }
    }
}
