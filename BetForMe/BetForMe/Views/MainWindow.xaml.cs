using BetForMe.Model;
using BetForMe.ViewModels;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
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
        private readonly BetForMeViewModel _viewModel;

        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();

            _log.Info("Starting application...");

            _viewModel = new BetForMeViewModel();
            DataContext = _viewModel;
        }

        private void ButtonSimulation_Click(object sender, RoutedEventArgs e) {
            tiSimulationResult.IsSelected = true;
        }

        private void ButtonMatrixSimulation_Click(object sender, RoutedEventArgs e) {
            tiMatrixSimulationResult.IsSelected = true;
        }
    }
}
