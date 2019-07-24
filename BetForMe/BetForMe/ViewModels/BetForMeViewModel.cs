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
using System.Windows.Input;

namespace BetForMe.ViewModels
{
    public class BetForMeViewModel : BaseViewModel {

        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ResourceManager _rm = new ResourceManager("BetForMe.Resources.Resources", Assembly.GetExecutingAssembly());

        private string _statusBarText;

        public ICommand ExampleCommand { get; private set; }

        public BetForMeViewModel() {
            ExampleCommand = new ActionCommand(ExecuteExampleCommand);

            //TODO GHE
            using (BetForMeDBContainer c = new BetForMeDBContainer()) {
                var result = from r in c.Germany_18_19 select r;
                var resultAsList = result.ToList<Germany_18_19>();

                var result2 = from r2 in c.England_18_19 select r2;
                var resultAsList2 = result2.ToList<England_18_19>();

                var result3 = from r3 in c.Spain_18_19 select r3;
                var resultAsList3 = result3.ToList<Spain_18_19>();
            }


        }

        #region Commands

        private void ExecuteExampleCommand() {
            _log.Debug("Received ExampleCommand");

            //TODO stuff

            StatusBarText = "ExampleCommand";
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

        #endregion Properties
    }
}
