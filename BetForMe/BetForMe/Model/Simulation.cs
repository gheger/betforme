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

        //Streak computing
        private int _currentWinningStreak = 0;
        private int _currentLosingStreak = 0;
        private bool _wasLastGameWon = false;

        public void Simulate() {

            using (BetForMeDBContainer c = new BetForMeDBContainer()) {

                /* 1st step: find games that match criteria
                 *  - Championship
                 *  - Season
                 *  - Game type (home, away, both)
                 *  - Only top N teams
                 */

                string select = "SELECT * ";
                string from = "FROM {0} "; //Championship
                string where = "WHERE 1=1 ";
                string whereSeason = "AND season LIKE '{1}' "; //Season

                string whereMinOdd = string.Empty; //Bookmaker odds field & MinOdd
                string whereMaxOdd = string.Empty; //Bookmaker odds field & MaxOdd
                string bookmakerOddsField = string.Format("{0}{1}", Bookmaker.Prefix, (char)GameTypes);
                string bookmakerOddsHomeField = string.Format("{0}{1}", Bookmaker.Prefix, (char)BetHelper.OddType.Home);
                string bookmakerOddsAwayField = string.Format("{0}{1}", Bookmaker.Prefix, (char)BetHelper.OddType.Away);
                switch (GameTypes) {
                    case BetHelper.OddType.Home:
                    case BetHelper.OddType.Away:
                        whereMinOdd += "AND " + bookmakerOddsField + " >= {3} ";
                        whereMaxOdd += "AND " + bookmakerOddsField + " <= {4} ";
                        break;
                    case BetHelper.OddType.Both:
                        whereMinOdd += "AND ((" + bookmakerOddsHomeField + " >= {3} AND " + bookmakerOddsHomeField + " <= {4}) ";
                        whereMaxOdd += "OR (" + bookmakerOddsAwayField + " >= {3} AND " + bookmakerOddsAwayField + " <= {4})) ";
                        bookmakerOddsField = bookmakerOddsHomeField; //because 'B' odds does not exist
                        break;
                }

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

                var allGames = c.Database.SqlQuery<Game>(query).ToList<Game>();
                var allGamesGrouped = allGames.GroupBy(g => g.Date);

                /* 2nd step: simulate over the full season
                 *  - With BK management (same BK for a game day)
                 *  - Update stats
                 *  - Update BK
                 */

                FinalBankroll = InitialBankroll;

                foreach (var gameDay in allGamesGrouped) {
                    var dateKey = gameDay.Key;

                    List<string> todaysTop = _bh.GetLeagueTableTop(Championship, allGames, OnlyTopNteams, (DateTime)dateKey);

                    foreach (var game in gameDay) {

                        //Get the odd and on which game the bet must be placed
                        BetHelper.OddType whichOdd = BetHelper.OddType.Both;
                        double odd = _bh.GetOddForBookmaker(game, Bookmaker.Prefix, GameTypes, out whichOdd);

                        //If OnlyTopNteams param is set, limit games to these teams
                        if (OnlyTopNteams > 0 && todaysTop.Count > 0 &&
                            (whichOdd == BetHelper.OddType.Home && !todaysTop.Contains(game.HomeTeam) ||
                            whichOdd == BetHelper.OddType.Away && !todaysTop.Contains(game.AwayTeam))) {
                                continue;
                        }

                        //Update stats
                        TotalBets++;

                        bool isWon = _bh.IsGameWon(game, whichOdd);
                        if (isWon) {
                            BetsWon++;
                        } else {
                            BetsLost++;
                        }

                        //_log.DebugFormat("Game {0}", isWon ? "WON" : "lost");
                        ComputeStreak(isWon);

                        //Compute bet
                        var stake = (FinalBankroll * BankrollToPlay / 100);
                        var betResult = _bh.CalculateBet(stake, odd, isWon);

                        //Update bankroll
                        FinalBankroll -= stake;
                        FinalBankroll += betResult;
                    }
                }
            }
        }
        public void SetDynamicParameter(BetHelper.XYSelection selection, object coordinates, int i) {
            switch (selection) {
                case BetHelper.XYSelection.Championship:
                    Championship = ((List<string>)coordinates)[i];
                    break;
                case BetHelper.XYSelection.Season:
                    Season = ((List<string>)coordinates)[i];
                    break;
                case BetHelper.XYSelection.Bookmaker:
                    Bookmaker = ((List<Bookmakers>)coordinates)[i];
                    break;
                case BetHelper.XYSelection.Odds:
                    MinOdd = ((List<double>)coordinates).First(); //Min odd is always the first
                    MaxOdd = ((List<double>)coordinates)[i];      //Max off is dynamic
                    break;
                case BetHelper.XYSelection.GameType:
                    GameTypes = ((List<BetHelper.OddType>)coordinates)[i];
                    break;
                case BetHelper.XYSelection.TopTeams:
                    OnlyTopNteams = ((List<int>)coordinates)[i];
                    break;
            }
        }

        private void ComputeStreak(bool isWon) {
            if (isWon && _wasLastGameWon) { //game is won and last was too!
                _currentWinningStreak++;
                _currentLosingStreak = 0;
            } else if (!isWon && !_wasLastGameWon) { //game is lost and last was too!
                _currentLosingStreak++;
                _currentWinningStreak = 0;
            } else if (isWon && !_wasLastGameWon) { //game is won and last was lost!
                _currentWinningStreak = _currentLosingStreak = 0;
                _currentWinningStreak++;
            } else if (!isWon && _wasLastGameWon) { //game is lost and last was won!
                _currentWinningStreak = _currentLosingStreak = 0;
                _currentLosingStreak++;
            }

            _wasLastGameWon = isWon;

            WinningStreak = (_currentWinningStreak > WinningStreak) ? _currentWinningStreak : WinningStreak;
            LosingStreak = (_currentLosingStreak > LosingStreak) ? _currentLosingStreak : LosingStreak;
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

        public int WinningStreak {
            get;
            set;
        }

        public int LosingStreak {
            get;
            set;
        }

        public string Message { get; set; }
        public string Championship { get; set; }
        public string Season { get; set; }
        public BetHelper.OddType GameTypes { get; set; }
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
