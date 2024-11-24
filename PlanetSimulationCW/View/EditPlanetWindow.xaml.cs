using PlanetSimulationCW.Model;
using PlanetSimulationCW.ViewModel;
using System.Windows;

namespace PlanetSimulationCW.View
{
    public partial class EditPlanetWindow : Window
    {
        public EditPlanetWindow(EditPlanetVM vm)
        {
            InitializeComponent();

            DataContext = vm;
        }
    }
}
