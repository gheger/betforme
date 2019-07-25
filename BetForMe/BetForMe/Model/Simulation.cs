using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.Model {
    public class Simulation: INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private double _initialBankroll = 0.0;
        private double _finalBankroll = 0.0;
        public int _totalBets = 0;
        public int _betsWon = 0;
        public int _betsLost = 0;
        public double _wonPercentage = 0;
        public double _lostPercentage = 0;
        

        public double InitialBankroll {
            get { return _initialBankroll; }
            set {
                if (_initialBankroll != value) {
                    _initialBankroll = value;
                    OnNotifyPropertyChanged();
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

        protected void OnNotifyPropertyChanged([CallerMemberName] string memberName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(memberName));
            }
        }
    }
}
