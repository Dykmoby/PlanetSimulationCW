using PlanetSimulationCW.Model;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.ViewModel
{
    class ControlPanelVM : Property
    {

        private string pauseButtonText;
        public string PauseButtonText
        {
            get { return pauseButtonText; }
            set
            {
                pauseButtonText = value;
                OnPropertyChanged(nameof(PauseButtonText));
            }
        }

        public RelayCommand ToggleSimulationCommand { get; private set; }

        public ControlPanelVM()
        {
            ToggleSimulationCommand = new RelayCommand(ToggleSimulation);
            PauseButtonText = "Pause";
        }

        private void ToggleSimulation(object e)
        {
            Global.simulationStopped = !Global.simulationStopped;
            if (Global.simulationStopped)
            {
                PauseButtonText = "Resume";
            }
            else
            {
                PauseButtonText = "Pause";
            }
        }
    }
}
