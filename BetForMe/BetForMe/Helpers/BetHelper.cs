using BetForMe.Model;
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
            //Both = 'B'
        }

        public enum XYSelection {
            Championship,
            Season,
            Bookmaker,
            Odds,
            GameType,
            TopTeams
        }

        public double CalculateBet(double stake, double odd, bool isBetWon = true) {
            if(isBetWon) {
                return stake * odd;
            } else {
                return 0;
            }
        }

        public double GetOddForBookmaker(object obj, string oddField) {
            return (double)obj.GetType().GetProperty(oddField).GetValue(obj, null);
        }

        public bool IsGameWon(England game, BetHelper.OddType oddType) {
            if (oddType == OddType.Home) {
                return game.FTR.Equals("H");
            } else if (oddType == OddType.Away) {
                return game.FTR.Equals("A");
            }
            return false;
        }

        /**
         * Calculates the league table at a certain point of time.
         * Return the top N times
         * 
         * TODO GHE
         * Possible improvement: at the start of the season, return the teams with the lowest odds about winning the championship
         */
        public List<string> GetLeagueTableTop(List<England> allChampGames, int top, DateTime untilDate) {

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

                using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                    return c.PreseasonOdds.Where(w => w.Season.Equals(season)).Select(s => s.TeamsList).First().Split(';').Take(top).ToList<string>();
                }
            }

            return topN;
        }

    }
}
