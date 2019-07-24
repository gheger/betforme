using BetForMe.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace BetForMe.ViewModels
{
    public class BetForMeViewModel : BaseViewModel {

        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ResourceManager _rm = new ResourceManager("BetForMe.Resources.Resources", Assembly.GetExecutingAssembly());

        public BetForMeViewModel() {

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
    }
}
