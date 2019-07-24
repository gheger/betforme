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

        private string _statusBarText;
        private string _simulationResult;
        private ComboBoxItem _selectedChampionship;

        private readonly string defaultChampionship = "Premier League";
        private readonly double defaultMinOdd = 0.0;
        private readonly double defaultMaxOdd = 1.5;

        public ICommand SimulateCommand { get; private set; }

        public BetForMeViewModel() {
            SimulateCommand = new ActionCommand(ExecuteSimulateCommand);

            MinOdd = defaultMinOdd;
            MaxOdd = defaultMaxOdd;

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
        }

        #region Commands

        private void ExecuteSimulateCommand() {
            _log.Debug("Received SimulateCommand");


            using (BetForMeDBContainer c = new BetForMeDBContainer()) {

                var allGames = c.England_18_19.Where(w => (double)w.IWH >= MinOdd && (double)w.IWH <= MaxOdd).Select(s => new { s.HomeTeam, s.AwayTeam, s.FTHG, s.FTAG, s.FTR, s.IWA, s.IWD, s.IWH }).ToList();

                switch (SelectedChampionship.Content) {
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

                double bankroll = 100.0;
                double bankrollUsedInPercent = 5.0;

                foreach (var g in allGames) {

                    var stake = (bankroll * bankrollUsedInPercent / 100);
                    var betResult = _bh.CalculateBet(stake, (double)g.IWH, g.FTR.Equals("H"));

                    bankroll -= stake;
                    bankroll += betResult;
                }
                SimulationResult = string.Format("Bankroll at the end of the season: {0}", bankroll);

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
        public string SimulationResult {
            get { return _simulationResult; }
            set {
                if (_simulationResult != value) {
                    _simulationResult = value;
                    OnNotifyPropertyChanged();
                }
            }
        }

        public ComboBoxItem SelectedChampionship {
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

        #endregion Properties
    }
}