using PlanetSimulationCW.Model;
using PlanetSimulationCW.ModelView;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            ((MainVM)mainGrid.DataContext).KeyDownCommand.Execute(e);
        }
    }
}