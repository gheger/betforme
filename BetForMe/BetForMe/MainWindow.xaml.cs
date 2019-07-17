using BetForMe.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetForMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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
