using PlanetSimulationCW.ViewModel;
using System.Windows;

namespace PlanetSimulationCW
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM(viewport);
        }
    }
}