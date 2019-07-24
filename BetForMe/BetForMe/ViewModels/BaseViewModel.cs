using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        protected void OnNotifyPropertyChanged([CallerMemberName] string memberName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(memberName));
            }
        }
    }
}
