using PlanetSimulationCW.ViewModel;
using System.Windows;

namespace PlanetSimulationCW
{
    public partial class ControlPanelWindow : Window
    {
        public ControlPanelWindow()
        {
            InitializeComponent();
            DataContext = new ControlPanelVM();
        }
    }
}