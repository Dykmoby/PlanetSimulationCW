using PlanetSimulationCW.Model;
using PlanetSimulationCW.ModelView;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PlanetSimulationCW
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainGrid.DataContext = new MainVM();
        }
    }
}