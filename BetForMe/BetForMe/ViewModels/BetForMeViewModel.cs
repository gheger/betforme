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
        private Simulation _currentSimulation;

        private readonly double defaultMinOdd = 1.05;
        private readonly double defaultMaxOdd = 1.5;
        private readonly double defaultBankrollToPlay = 5.0;

        public ICommand SimulateCommand { get; private set; }

        public BetForMeViewModel() {
            SimulateCommand = new ActionCommand(ExecuteSimulateCommand);

            MinOdd = defaultMinOdd;
            MaxOdd = defaultMaxOdd;
            BankrollToPlay = defaultBankrollToPlay;

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
            LoadGames();
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
                    name NOT LIKE 'sqlite_%';
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
        private void LoadGames() {
            foreach (BetHelper.OddType ot in Enum.GetValues(typeof(BetHelper.OddType))) {
                Games.Add(ot);
            }
            SelectedGames = Games.Where(g => g.Equals(BetHelper.OddType.Home)).FirstOrDefault();
        }

        private void LoadBookmakers() {
            using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                Bookmakers = c.Bookmakers.ToList<Bookmakers>();
                SelectedBookmaker = Bookmakers.Where(b => b.Name.Equals("Interwetten")).FirstOrDefault();
            }
        }

        #endregion

        #region Commands

        private void ExecuteSimulateCommand() {
            //_log.Debug("Received SimulateCommand");

            //Allow to simulate only if everything is loaded
            if (!_isLoaded) {
                return;
            }

            //Create a simulation (not executed yet)
            Simulation sim = new Simulation() {
                InitialBankroll = 100.0,
                Championship = SelectedChampionship,
                Season = SelectedSeason,
                Bookmaker = SelectedBookmaker,
                Games = SelectedGames,
                MinOdd = MinOdd,
                MaxOdd = MaxOdd,
                BankrollToPlay = BankrollToPlay,
            };

            sim.Simulate();
            CurrentSimulation = sim;            

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

        public string SelectedSeason {
            get { return _selectedSeason; }
            set {
                if (_selectedSeason != value) {
                    _selectedSeason = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulateCommand();
                }
            }
        }

        public Bookmakers SelectedBookmaker {
            get { return _selectedBookmaker; }
            set {
                if (_selectedBookmaker != value) {
                    _selectedBookmaker = value;
                    OnNotifyPropertyChanged();
                    ExecuteSimulateCommand();
                }
            }
        }        

        public double MinOdd { get; set; }

        public double MaxOdd { get; set; }

        public double BankrollToPlay { get; set; }

        public IList<string> Championships { get; set; } = new List<string>();

        public IList<string> Seasons { get; set; } = new List<string>();

        public IList<BetHelper.OddType> Games { get; set; } = new List<BetHelper.OddType>();

        public IList<Bookmakers> Bookmakers { get; set; } = new List<Bookmakers>();

        public BetHelper.OddType SelectedGames { get; set; }

        #endregion Properties
    }
}