using BetForMe.Helpers;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.Model {
    public class Simulation : INotifyPropertyChanged {

        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event PropertyChangedEventHandler PropertyChanged;

        private BetHelper _bh = new BetHelper();

        //Result
        private double _finalBankroll = 0.0;
        private int _totalBets = 0;
        private int _betsWon = 0;
        private int _betsLost = 0;

        public void Simulate() {

            using (BetForMeDBContainer c = new BetForMeDBContainer()) {

                /* 1st step: find games that match criteria
                 *  - Championship
                 *  - Season
                 *  - Games (home, away, both)
                 *  - Only top N teams
                 */

                string select = "SELECT * ";
                string from = "FROM {0} "; //Championship
                string where = "WHERE 1=1 ";
                string whereSeason = "AND season LIKE '{1}' "; //Season
                string whereMinOdd = "AND {2} >= {3} "; //Bookmaker odds field & MinOdd
                string whereMaxOdd = "AND {2} <= {4} "; //Bookmaker odds field & MaxOdd

                string bookmakerOddsField = string.Format("{0}{1}", Bookmaker.Prefix, (char)Games);

                //Check if generated bookmaker field exists
                try {
                    int nbOdds = c.Database.SqlQuery<int>(string.Format("SELECT count({0}) FROM {1} WHERE season LIKE '{2}';", bookmakerOddsField, Championship, Season)).First();
                    _log.DebugFormat("{0} odds found for bookmaker {1} for season {2}", nbOdds, Bookmaker.Name, Season);
                    //Okay, go ahead an do simulation
                } catch (Exception ex) {
                    _log.ErrorFormat("Bookmaker field not found: {0}", bookmakerOddsField);
                    //Bookmaker does not exist! Abort simulation
                    Message = string.Format("No odds found for this bookmaker ({0})", Bookmaker.Name);
                    return;
                }

                //Build query
                string query = string.Format(select + from + where + whereSeason + whereMinOdd + whereMaxOdd + ";", Championship, Season, bookmakerOddsField, MinOdd, MaxOdd);

                var allGames = c.Database.SqlQuery<England>(query).ToList<England>();
                var allGamesGrouped = allGames.GroupBy(g => g.Date);

                /* 2nd step: simulate over the full season
                 *  - With BK management (same BK for a game day)
                 *  - Update stats
                 *  - Update BK
                 */

                FinalBankroll = InitialBankroll;

                foreach (var gameDay in allGamesGrouped) {
                    var dateKey = gameDay.Key;

                    List<string> todaysTop = _bh.GetLeagueTableTop(allGames, OnlyTopNteams, (DateTime)dateKey);

                    foreach (var game in gameDay) {

                        //If OnlyTopNteams param is set, limit games to these teams
                        if (OnlyTopNteams > 0 && todaysTop.Count > 0 &&
                            (Games == BetHelper.OddType.Home && !todaysTop.Contains(game.HomeTeam) ||
                            Games == BetHelper.OddType.Away && !todaysTop.Contains(game.AwayTeam))) {
                                continue;
                        }


                        double odd = _bh.GetOddForBookmaker(game, bookmakerOddsField);

                        //Update stats
                        TotalBets++;
                        if (_bh.IsGameWon(game, Games)) {
                            BetsWon++;
                        } else {
                            BetsLost++;
                        }

                        //Compute bet
                        var stake = (FinalBankroll * BankrollToPlay / 100);
                        var betResult = _bh.CalculateBet(stake, odd, _bh.IsGameWon(game, Games));

                        //Update bankroll
                        FinalBankroll -= stake;
                        FinalBankroll += betResult;

                    }
                }
            }
        }

        public double FinalBankroll {
            get { return _finalBankroll; }
            set {
                if (_finalBankroll != value) {
                    _finalBankroll = value;
                    OnNotifyPropertyChanged();
                }
            }
        }
        public int TotalBets {
            get { return _totalBets; }
            set {
                if (_totalBets != value) {
                    _totalBets = value;
                    OnNotifyPropertyChanged();
                }
            }
        }
        public int BetsWon {
            get { return _betsWon; }
            set {
                if (_betsWon != value) {
                    _betsWon = value;
                    OnNotifyPropertyChanged();
                }
            }
        }
        public int BetsLost {
            get { return _betsLost; }
            set {
                if (_betsLost != value) {
                    _betsLost = value;
                    OnNotifyPropertyChanged();
                }
            }
        }

        public double WonPercentage {
            get { return (double)(BetsWon * 100) / TotalBets; }
        }

        public double LostPercentage {
            get { return (double)(BetsLost * 100) / TotalBets; }
           
        }

        public string Message { get; set; }
        public string Championship { get; set; }
        public string Season { get; set; }
        public BetHelper.OddType Games { get; set; }
        public double MinOdd { get; set; }
        public double MaxOdd { get; set; }
        public double InitialBankroll { get; set; }
        public double BankrollToPlay { get; set; }
        public Bookmakers Bookmaker { get; set; }
        public int OnlyTopNteams { get; set; } //consider if > 0

        protected void OnNotifyPropertyChanged([CallerMemberName] string memberName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(memberName));
            }
        }
    }
}
