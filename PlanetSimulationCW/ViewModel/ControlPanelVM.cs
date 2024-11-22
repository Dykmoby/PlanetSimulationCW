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

        private string planetInfoText;
        public string PlanetInfoText
        {
            get { return planetInfoText; }
            set
            {
                planetInfoText = value;
                OnPropertyChanged(nameof(PlanetInfoText));
            }
        }

        public RelayCommand ToggleSimulationCommand { get; private set; }
        public RelayCommand ControlPanelClosed { get; private set; }

        public ControlPanelVM()
        {
            ToggleSimulationCommand = new RelayCommand(ToggleSimulation);
            ControlPanelClosed = new RelayCommand(Close);


            if (Global.simulationStopped)
            {
                PauseButtonText = "Resume";
            }
            else
            {
                PauseButtonText = "Pause";
            }
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

        private void Close(object e)
        {
            Global.controlPanelWindow = null;
        }

        public void DisplayPlanetInfo(Planet planet)
        {
            PlanetInfoText = planet.Color.ToString();
        }

        public void ClearPlanetInfo()
        {
            PlanetInfoText = "";
        }
    }
}
