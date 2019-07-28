using BetForMe.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.Model {
    public class Simulation : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private BetHelper _bh = new BetHelper();

        //Result
        private double _finalBankroll = 0.0;

        public int _totalBets = 0;
        public int _betsWon = 0;
        public int _betsLost = 0;
        public double _wonPercentage = 0;
        public double _lostPercentage = 0;

        public void Simulate() {

            using (BetForMeDBContainer c = new BetForMeDBContainer()) {

                /* 1st step: find games that match criteria
                 *  - Championship
                 *  - Season
                 *  - Games (home, away, both)
                 *  - other to come...
                 */

                string select = "SELECT * ";
                string from = "FROM {0} "; //Championship
                string where = "WHERE 1=1 ";
                string whereSeason = "AND season LIKE '{1}' "; //Season
                string whereMinOdd = "AND IWH >= {2} "; //MinOdd
                string whereMaxOdd = "AND IWH <= {3} "; //MaxOdd

                //Build query
                string query = string.Format(select + from + where + whereSeason + whereMinOdd + whereMaxOdd + ";", Championship, Season, MinOdd, MaxOdd);

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
                    foreach (var game in gameDay) {

                        //Update stats
                        TotalBets++;
                        if (game.FTR.Equals("H")) {
                            BetsWon++;
                        } else {
                            BetsLost++;
                        }

                        //Compute bet
                        var stake = (FinalBankroll * BankrollToPlay / 100);
                        var betResult = _bh.CalculateBet(stake, (double)game.IWH, game.FTR.Equals("H"));

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
        public string Championship { get; set; }
        public string Season { get; set; }
        public string Games { get; set; }
        public double MinOdd { get; set; }
        public double MaxOdd { get; set; }
        public double InitialBankroll { get; set; }
        public double BankrollToPlay { get; set; }

        protected void OnNotifyPropertyChanged([CallerMemberName] string memberName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(memberName));
            }
        }
    }
}
