using BetForMe.Helpers;
using BetForMe.Model;
using log4net;
using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetForMe.ViewModels {
    public class BetForMeViewModel : BaseViewModel {

        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ResourceManager _rm = new ResourceManager("BetForMe.Resources.Resources", Assembly.GetExecutingAssembly());

        private BetHelper _bh = new BetHelper();

        private IList<string> _championships = new List<string>();
        private string _statusBarText;
        private string _selectedChampionship;
        private Simulation _currentSimulation;

        public readonly string defaultChampionship = "Premier League";
        private readonly double defaultMinOdd = 1.05;
        private readonly double defaultMaxOdd = 1.5;

        public ICommand SimulateCommand { get; private set; }

        public BetForMeViewModel() {
            SimulateCommand = new ActionCommand(ExecuteSimulateCommand);

            MinOdd = defaultMinOdd;
            MaxOdd = defaultMaxOdd;
            SelectedChampionship = defaultChampionship;

            //Check dababases availability
            try {
                using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                    c.Database.ExecuteSqlCommand("SELECT 'something'");

                    _log.Debug("SQLite database OK");


                }
                StatusBarText = "DB loaded !";
            } catch (Exception ex) {
                StatusBarText = "DB could not be loaded : " + ex.Message;
                _log.Error(ex);
            }

            LoadChampionships();
        }

        #region Private methods

        private void LoadChampionships() {
            Championships.Add("Premier League");
            Championships.Add("Bundesliga");
            Championships.Add("La Liga");                                
        }

        #endregion

        #region Commands

        private void ExecuteSimulateCommand() {
            _log.Debug("Received SimulateCommand");

            Simulation sim = new Simulation() {
                InitialBankroll = 100.0
            };

            using (BetForMeDBContainer c = new BetForMeDBContainer()) {

                var allGames = c.England_18_19.Where(w => (double)w.IWH >= MinOdd && (double)w.IWH <= MaxOdd).Select(s => new { s.HomeTeam, s.AwayTeam, s.FTHG, s.FTAG, s.FTR, s.IWA, s.IWD, s.IWH }).ToList();

                switch (SelectedChampionship) {
                    case "Premier League":
                        //allGames = c.England_18_19.Where(w => (double)w.IWH >= MinOdd && (double)w.IWH <= MaxOdd).Select(s => new { s.HomeTeam, s.AwayTeam, s.FTHG, s.FTAG, s.FTR, s.IWA, s.IWD, s.IWH }).ToList();
                        break;
                    case "Bundesliga":
                        allGames = c.Germany_18_19.Where(w => (double)w.IWH >= MinOdd && (double)w.IWH <= MaxOdd).Select(s => new { s.HomeTeam, s.AwayTeam, s.FTHG, s.FTAG, s.FTR, s.IWA, s.IWD, s.IWH }).ToList();
                        break;
                    case "La Liga":
                        allGames = c.Spain_18_19.Where(w => (double)w.IWH >= MinOdd && (double)w.IWH <= MaxOdd).Select(s => new { s.HomeTeam, s.AwayTeam, s.FTHG, s.FTAG, s.FTR, s.IWA, s.IWD, s.IWH }).ToList();
                        break;
                }

                double bankroll = sim.InitialBankroll;
                double bankrollUsedInPercent = 5.0;

                foreach (var g in allGames) {

                    var stake = (bankroll * bankrollUsedInPercent / 100);

                    //Update stats
                    sim.TotalBets++;
                    if (g.FTR.Equals("H")) {
                        sim.BetsWon++;
                    } else {
                        sim.BetsLost++;
                    }

                    //Compute bet
                    var betResult = _bh.CalculateBet(stake, (double)g.IWH, g.FTR.Equals("H"));

                    //Update bankroll
                    bankroll -= stake;
                    bankroll += betResult;
                }

                sim.FinalBankroll = bankroll;
                CurrentSimulation = sim;

                /*var result = from r in c.Germany_18_19 select r;
                        var resultAsList = result.ToList<Germany_18_19>();

                        var result2 = from r2 in c.England_18_19 select r2;
                        var resultAsList2 = result2.ToList<England_18_19>();

                        var result3 = from r3 in c.Spain_18_19 select r3;
                        var resultAsList3 = result3.ToList<Spain_18_19>();*/
            }


            StatusBarText = "Simulation done";
        }

        #endregion Commands

        #region Properties
        public string StatusBarText {
            get { return _statusBarText; }
            set {
                if (_statusBarText != value) {
                    _statusBarText = value;
                    OnNotifyPropertyChanged();
                }
            }
        }
        public Simulation CurrentSimulation {
            get { return _currentSimulation; }
            set {
                if (_currentSimulation != value) {
                    _currentSimulation = value;
                    OnNotifyPropertyChanged();
                }
            }
        }

        public string SelectedChampionship {
            get { return _selectedChampionship; }
            set {
                if (_selectedChampionship != value) {
                    _selectedChampionship = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulateCommand();
                }
            }
        }

        public double MinOdd { get; set; }

        public double MaxOdd { get; set; }

        public IList<string> Championships { get => _championships; set => _championships = value; }

        #endregion Properties
    }
}