using BetForMe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.Helpers {
    public class BetHelper {

        public enum OddType {
            Home = 'H',
            Away = 'A',
            //Draw = 'D', //not used yet
            //Both = 'B'
        }

        public double CalculateBet(double stake, double odd, bool isBetWon = true) {
            if(isBetWon) {
                return stake * odd;
            } else {
                return 0;
            }
        }

        public double GetOddForBookmaker(object obj, string oddField) {
            return (double)obj.GetType().GetProperty(oddField).GetValue(obj, null);
        }

        public bool IsGameWon(England game, BetHelper.OddType oddType) {
            if (oddType == OddType.Home) {
                return game.FTR.Equals("H");
            } else if (oddType == OddType.Away) {
                return game.FTR.Equals("A");
            }
            return false;
        }

        public List<string> GetLeagueTableTop(List<England> allChampGames, int top, DateTime untilDate) {

           

                return null;
        }

    }
}
