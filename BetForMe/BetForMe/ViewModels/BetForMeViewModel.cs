using BetForMe.Helpers;
using BetForMe.Model;
using log4net;
using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private bool _isLoaded = false;

        private string _statusBarText;
        private string _selectedChampionship;
        private string _selectedSeason;
        private Bookmakers _selectedBookmaker;
        private BetHelper.OddType _selectedGameTypes;
        private Simulation _currentSimulation;
        private ObservableCollection<ObservableCollection<Simulation>> _currentMatrixSimulation;
        private List<string> _currentMatrixSimulationHeadersX;
        private List<string> _currentMatrixSimulationHeadersY;

        private readonly double _defaultInitialBankroll = 100.0;
        private readonly double _defaultMinOdd = 1.25;
        private readonly double _defaultMaxOdd = 1.6;
        private readonly int _defaultOnlyTopNteams = 0;
        private readonly double _defaultBankrollToPlay = 5.0; //in percent
        private readonly BetHelper.XYSelection _defaultXSelection = BetHelper.XYSelection.Season;
        private readonly BetHelper.XYSelection _defaultYSelection = BetHelper.XYSelection.Bookmaker;

        public ICommand SimulatationCommand { get; private set; }
        public ICommand MatrixSimulationCommand { get; private set; }        

        public BetForMeViewModel() {
            SimulatationCommand = new ActionCommand(ExecuteSimulationCommand);
            MatrixSimulationCommand = new ActionCommand(ExecuteMatrixSimulationCommand);

            MinOdd = _defaultMinOdd;
            MaxOdd = _defaultMaxOdd;
            BankrollToPlay = _defaultBankrollToPlay;
            OnlyTopNteams = _defaultOnlyTopNteams;
            XSelection = _defaultXSelection;
            YSelection = _defaultYSelection;

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

            LoadChampionships(); // = tables in the DB
            LoadSeasons();
            LoadGameTypes();
            LoadBookmakers();

            _isLoaded = true;
        }

        #region Private methods

        private void LoadChampionships() {
            string sqlListTables = @"
                SELECT 
                    name
                FROM 
                    sqlite_master 
                WHERE 
                    type ='table' AND 
                    name NOT LIKE 'sqlite_%' AND
                    name NOT LIKE 'Bookmakers' AND
                    name NOT LIKE 'PreseasonOdds';
            ";

            using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                var result = c.Database.SqlQuery<string>(sqlListTables);
                Championships = result.ToList<string>();
                SelectedChampionship = Championships.First();
            }                       
        }

        private void LoadSeasons() {
            string sqlListSeasons = "SELECT distinct Season FROM {0}";

            using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                var result = c.Database.SqlQuery<string>(string.Format(sqlListSeasons, SelectedChampionship));
                Seasons = result.ToList<string>();
                SelectedSeason = Seasons.Last();
            }
        }
        private void LoadGameTypes() {
            foreach (BetHelper.OddType ot in Enum.GetValues(typeof(BetHelper.OddType))) {
                GameTypes.Add(ot);
            }
            SelectedGameType = GameTypes.Where(g => g.Equals(BetHelper.OddType.Home)).FirstOrDefault();
        }

        private void LoadBookmakers() {
            using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                Bookmakers = c.Bookmakers.ToList<Bookmakers>();
                SelectedBookmaker = Bookmakers.Where(b => b.Name.Equals("Interwetten")).FirstOrDefault();
            }
        }

        #endregion

        #region Commands

        private void ExecuteSimulationCommand() {

            //Allow to simulate only if everything is loaded
            if (!_isLoaded) {
                return;
            }

            //Create a simulation (not executed yet)
            Simulation sim = new Simulation() {
                InitialBankroll = _defaultInitialBankroll,
                Championship = SelectedChampionship,
                Season = SelectedSeason,
                Bookmaker = SelectedBookmaker,
                GameTypes = SelectedGameType,
                MinOdd = MinOdd,
                MaxOdd = MaxOdd,
                OnlyTopNteams = OnlyTopNteams,
                BankrollToPlay = BankrollToPlay,
            };

            sim.Simulate();
            CurrentSimulation = sim;            

            StatusBarText = "Simulation done";
        }

        private List<string> GetXYobjects(BetHelper.XYSelection selection, out object coordinates, out int size) {
            coordinates = null;
            size = 0;
            switch (selection) {
                case BetHelper.XYSelection.Championship:
                    coordinates = Championships;
                    size = Championships.Count;
                    return (List<string>)Championships;
                case BetHelper.XYSelection.Season:
                    coordinates = Seasons;
                    size = Seasons.Count;
                    return (List<string>)Seasons;
                case BetHelper.XYSelection.Bookmaker:
                    coordinates = Bookmakers;
                    size = Bookmakers.Count;
                    return Bookmakers.Select(b => b.Name).ToList<string>();
                case BetHelper.XYSelection.Odds:
                    var listOfOdds =  _bh.GetOddsCoordinates(MinOdd, MaxOdd, 10);
                    coordinates = listOfOdds;
                    size = 10;
                    return listOfOdds.Select(l => l.ToString()).ToList<string>();
                case BetHelper.XYSelection.GameType:
                    coordinates = GameTypes;
                    size = GameTypes.Count;
                    return GameTypes.Select(g => g.ToString()).ToList<string>();
                case BetHelper.XYSelection.TopTeams:
                    var listTopTeams = Enumerable.Range(1, OnlyTopNteams).ToList<int>();
                    coordinates = listTopTeams;
                    size = OnlyTopNteams;
                    return listTopTeams.Select(tt => tt.ToString()).ToList<string>();
            }
            return new List<string>();
        }

        private void ExecuteMatrixSimulationCommand() {

            object xCoordinates = null;
            object yCoordinates = null;
            int xSize = 0;
            int ySize = 0;

            CurrentMatrixSimulationHeadersX = GetXYobjects(XSelection, out xCoordinates, out xSize);
            CurrentMatrixSimulationHeadersY = GetXYobjects(YSelection, out yCoordinates, out ySize);

            CurrentMatrixSimulation = new ObservableCollection<ObservableCollection<Simulation>>();

            // The array will be filled like this:
            // y
            // ^
            // |.. .. .. .. .. .. ..
            // |.. .. .. .. .. .. ..
            // |.. .. .. .. .. .. ..
            // |14 .. .. .. .. .. ..
            // |07 08 09 10 11 12 13
            // |00 01 02 03 04 05 06
            // ------------------------> x

            for(int y = 0; y < ySize; y++) { //Take y values ...
                CurrentMatrixSimulation.Add(new ObservableCollection<Simulation>());

                for (int x = 0; x < xSize; x++) { //... over x axis
                    //First, create any simulation with all parameters
                    Simulation sim = new Simulation() {
                        InitialBankroll = _defaultInitialBankroll,
                        Championship = SelectedChampionship,
                        Season = SelectedSeason,
                        Bookmaker = SelectedBookmaker,
                        GameTypes = SelectedGameType,
                        MinOdd = MinOdd,
                        MaxOdd = MaxOdd,
                        OnlyTopNteams = OnlyTopNteams,
                        BankrollToPlay = BankrollToPlay,
                    };

                    //Then set changing 'Y' parameter
                    sim.SetDynamicParameter(YSelection, yCoordinates, y);

                    //And finally set changing 'X' parameter
                    sim.SetDynamicParameter(XSelection, xCoordinates, x);

                    //Excuse simulation
                    sim.Simulate();

                    //Save simulation
                    CurrentMatrixSimulation[y].Add(sim);
                }
            }
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

        public List<string> CurrentMatrixSimulationHeadersX {
            get { return _currentMatrixSimulationHeadersX; }
            set {
                if (_currentMatrixSimulationHeadersX != value) {
                    _currentMatrixSimulationHeadersX = value;
                    OnNotifyPropertyChanged();
                }
            }
        }

        public List<string> CurrentMatrixSimulationHeadersY {
            get { return _currentMatrixSimulationHeadersY; }
            set {
                if (_currentMatrixSimulationHeadersY != value) {
                    _currentMatrixSimulationHeadersY = value;
                    OnNotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<ObservableCollection<Simulation>> CurrentMatrixSimulation {
            get { return _currentMatrixSimulation; }
            set {
                if (_currentMatrixSimulation != value) {
                    _currentMatrixSimulation = value;
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
                    ExecuteSimulationCommand();
                }
            }
        }
        public string SelectedSeason {
            get { return _selectedSeason; }
            set {
                if (_selectedSeason != value) {
                    _selectedSeason = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulationCommand();
                }
            }
        }
        public Bookmakers SelectedBookmaker {
            get { return _selectedBookmaker; }
            set {
                if (_selectedBookmaker != value) {
                    _selectedBookmaker = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulationCommand();
                }
            }
        }
        public BetHelper.OddType SelectedGameType {
            get { return _selectedGameTypes; }
            set {
                if (_selectedGameTypes != value) {
                    _selectedGameTypes = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulationCommand();
                }
            }
        }
        public double MinOdd { get; set; }
        public double MaxOdd { get; set; }
        public int OnlyTopNteams { get; set; }
        public double BankrollToPlay { get; set; }
        public IList<string> Championships { get; set; } = new List<string>();
        public IList<string> Seasons { get; set; } = new List<string>();
        public IList<BetHelper.OddType> GameTypes { get; set; } = new List<BetHelper.OddType>();
        public IList<Bookmakers> Bookmakers { get; set; } = new List<Bookmakers>();
        public BetHelper.XYSelection XSelection { get; set; }
        public BetHelper.XYSelection YSelection { get; set; }

        #endregion Properties
    }
}