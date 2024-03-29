﻿using BetForMe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.Helpers {
    public class BetHelper {

        public enum OddType {
            Home = 'H',
            Away = 'A',
            //Draw = 'D', //not used yet
            Both = 'B'
        }

        public enum LimitTeamsType {
            Top,
            Diff
        }

        public enum XYSelection {
            Championship,
            Season,
            Bookmaker,
            Odds,
            GameType,
            LimitTeamsType
        }

        public double CalculateBet(double stake, double odd, bool isBetWon = true) {
            if(isBetWon) {
                return stake * odd;
            } else {
                return 0;
            }
        }

        public double GetOddForBookmaker(object obj, string bookmakerPrefix, OddType gameType, out OddType whichOdd) {
            switch (gameType) {
                case OddType.Home:
                case OddType.Away:
                    whichOdd = gameType;
                    return (double)obj.GetType().GetProperty(bookmakerPrefix + (char)gameType).GetValue(obj, null);
                case OddType.Both:
                    double homeOdd = (double)obj.GetType().GetProperty(bookmakerPrefix + (char)OddType.Home).GetValue(obj, null);
                    double awayOdd = (double)obj.GetType().GetProperty(bookmakerPrefix + (char)OddType.Away).GetValue(obj, null);
                    if (homeOdd < awayOdd) {
                        whichOdd = OddType.Home;
                    } else {
                        whichOdd = OddType.Away;
                    }
                    return Math.Min(homeOdd, awayOdd);
                default: //not possible
                    whichOdd = OddType.Both;
                    return 0.0;
            }
        }

        public bool IsGameWon(Game game, OddType oddType) {
            if (oddType == OddType.Home) {
                return game.FTR.Equals("H");
            } else if (oddType == OddType.Away) {
                return game.FTR.Equals("A");
            }
            return false;
        }

        /**
         * Calculates the league table at a certain point of time.
         * Return the top N teams
         *
         */
        public List<string> GetLeagueTableTop(string championship, List<Game> allChampGames, int top, DateTime untilDate) {

            int victoryPoints = 3;
            int drawPoints = 1;

            var gamesUntil = allChampGames.Where(w => w.Date < untilDate);

            Dictionary<string, int> leagueTable = new Dictionary<string, int>(); //Team name, points
            List<string> topN = new List<string>();

            foreach (var g in gamesUntil) {

                //Add 2 teams if not exists                
                if (!leagueTable.ContainsKey(g.HomeTeam)) {
                    leagueTable.Add(g.HomeTeam, 0);
                }
                if (!leagueTable.ContainsKey(g.AwayTeam)) {
                    leagueTable.Add(g.AwayTeam, 0);
                }

                //Count points
                if (IsGameWon(g, OddType.Home)) { //Home won
                    leagueTable[g.HomeTeam] += victoryPoints;
                } else if (IsGameWon(g, OddType.Away)) { //Away won
                    leagueTable[g.AwayTeam] += victoryPoints;
                } else { //Draw
                    leagueTable[g.HomeTeam] += drawPoints;
                    leagueTable[g.AwayTeam] += drawPoints;
                }
            }

            topN = leagueTable.OrderByDescending(x => x.Value).Take(top).Select(s => s.Key).ToList<string>();

            //When there is not enough game played, return the pre-season odds
            if (topN.Count < top) {
                string season = allChampGames.First().Season;

                try {
                    using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                        return c.PreseasonOdds.Where(w => w.Season.Equals(season) && w.Championship.Equals(championship)).Select(s => s.TeamsList).First().Split(';').Take(top).ToList<string>();
                    }
                } catch (Exception ex) { }
            }

            return topN;
        }

        public IList<double> GetOddsCoordinates(double minOdd, double maxOdd, int nbCoordinates) {
            IList<double> ret = new List<double>();

            double step = (maxOdd - minOdd) / (nbCoordinates - 1);

            for (double i = minOdd; i < maxOdd; i+=step) {
                ret.Add(Math.Round(i, 2));
            }
            ret.Add(Math.Round(maxOdd, 2));

            return ret;
        }
    }
}
