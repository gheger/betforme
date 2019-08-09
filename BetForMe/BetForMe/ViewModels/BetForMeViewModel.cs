﻿using BetForMe.Helpers;
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

        private bool _isLoaded = false;

        private string _statusBarText;
        private string _selectedChampionship;
        private string _selectedSeason;
        private Bookmakers _selectedBookmaker;
        private BetHelper.OddType _selectedGameTypes;
        private Simulation _currentSimulation;

        private readonly double _defaultInitialBankroll = 100.0;
        private readonly double _defaultMinOdd = 1.25;
        private readonly double _defaultMaxOdd = 1.6;
        private readonly int _defaultOnlyTopNteams = 0;
        private readonly double _defaultBankrollToPlay = 5.0; //in percent
        private readonly BetHelper.XYSelection _defaultXSelection = BetHelper.XYSelection.Championship;
        private readonly BetHelper.XYSelection _defaultYSelection = BetHelper.XYSelection.Season;

        public ICommand SimulatationCommand { get; private set; }
        public ICommand MatrixSimulationCommand { get; private set; }        

        public BetForMeViewModel() {
            SimulatationCommand = new ActionCommand(ExecuteSimulatationCommand);
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
                    name NOT LIKE 'Bookmakers';
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

        private void ExecuteSimulatationCommand() {

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

        private void ExecuteMatrixSimulationCommand() {
            //TODO GHE do implementation

            //Use XSelection and YSelection

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
                    ExecuteSimulatationCommand();
                }
            }
        }
        public string SelectedSeason {
            get { return _selectedSeason; }
            set {
                if (_selectedSeason != value) {
                    _selectedSeason = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulatationCommand();
                }
            }
        }
        public Bookmakers SelectedBookmaker {
            get { return _selectedBookmaker; }
            set {
                if (_selectedBookmaker != value) {
                    _selectedBookmaker = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulatationCommand();
                }
            }
        }
        public BetHelper.OddType SelectedGameType {
            get { return _selectedGameTypes; }
            set {
                if (_selectedGameTypes != value) {
                    _selectedGameTypes = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulatationCommand();
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