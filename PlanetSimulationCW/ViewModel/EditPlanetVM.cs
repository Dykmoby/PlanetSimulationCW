using PlanetSimulationCW.Model;
using System.Windows;

namespace PlanetSimulationCW.ViewModel
{
    public class EditPlanetVM : Property
    {
        public Planet planet;

        private string positionX;
        public string PositionX
        {
            get { return positionX; }
            set
            {
                positionX = value;
                OnPropertyChanged(nameof(PositionX));
            }
        }

        private string positionY;
        public string PositionY
        {
            get { return positionY; }
            set
            {
                positionY = value;
                OnPropertyChanged(nameof(PositionY));
            }
        }

        private string positionZ;
        public string PositionZ
        {
            get { return positionZ; }
            set
            {
                positionZ = value;
                OnPropertyChanged(nameof(PositionZ));
            }
        }

        private string velocityX;
        public string VelocityX
        {
            get { return velocityX; }
            set
            {
                velocityX = value;
                OnPropertyChanged(nameof(VelocityX));
            }
        }

        private string velocityY;
        public string VelocityY
        {
            get { return velocityY; }
            set
            {
                velocityY = value;
                OnPropertyChanged(nameof(VelocityY));
            }
        }

        private string velocityZ;
        public string VelocityZ
        {
            get { return velocityZ; }
            set
            {
                velocityZ = value;
                OnPropertyChanged(nameof(VelocityZ));
            }
        }

        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand ApplyCommand { get; private set; }

        public EditPlanetVM(Planet planet)
        {
            CancelCommand = new RelayCommand(Cancel);
            ApplyCommand = new RelayCommand(Apply);

            this.planet = planet;

            PositionX = planet.Position.X.ToString("0.##");
            PositionY = planet.Position.Y.ToString("0.##");
            PositionZ = planet.Position.Z.ToString("0.##");

            VelocityX = planet.Velocity.X.ToString("0.##");
            VelocityY = planet.Velocity.Y.ToString("0.##");
            VelocityZ = planet.Velocity.Z.ToString("0.##");
        }

        private void Cancel(object e)
        {
            ((Window)e).DialogResult = false;
        }

        private void Apply(object e)
        {
            string incorrectValuesMessage = "Incorrect values:\n";
            int incorrectValuesCount = 0;
            if (!double.TryParse(PositionX, out double _))
            {
                incorrectValuesMessage += nameof(PositionX) + '\n';
                incorrectValuesCount++;
            }
            if (!double.TryParse(PositionY, out double _))
            {
                incorrectValuesMessage += nameof(PositionY) + '\n';
                incorrectValuesCount++;
            }
            if (!double.TryParse(PositionZ, out double _))
            {
                incorrectValuesMessage += nameof(PositionZ) + '\n';
                incorrectValuesCount++;
            }

            if (!double.TryParse(VelocityX, out double _))
            {
                incorrectValuesMessage += nameof(VelocityX) + '\n';
                incorrectValuesCount++;
            }
            if (!double.TryParse(VelocityY, out double _))
            {
                incorrectValuesMessage += nameof(VelocityY) + '\n';
                incorrectValuesCount++;
            }
            if (!double.TryParse(VelocityZ, out double _))
            {
                incorrectValuesMessage += nameof(VelocityZ) + '\n';
                incorrectValuesCount++;
            }

            if (incorrectValuesCount > 0)
            {
                MessageBox.Show(incorrectValuesMessage);
                return;
            }

            ((Window)e).DialogResult = true;
        }
    }
}
