using BetForMe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BetForMe.Helpers {

    public class TextBlockForegroundConverter : IValueConverter {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) {
                return Brushes.Red;
            }

            Simulation sim = (Simulation)value;

            if (sim.FinalBankroll > sim.InitialBankroll) {
                return Brushes.Green;
            } else {
                return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
