using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.Helpers {
    public class BetHelper {

        public double CalculateBet(double stake, double odd, bool isBetWon = true) {
            if(isBetWon) {
                return stake * odd;
            } else {
                return 0;
            }
        }

    }
}
