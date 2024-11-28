using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using PlanetSimulationCW.Model;
using PlanetSimulationCW.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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

        private string followPlanetButtonText;
        public string FollowPlanetButtonText
        {
            get { return followPlanetButtonText; }
            set
            {
                followPlanetButtonText = value;
                OnPropertyChanged(nameof(FollowPlanetButtonText));
            }
        }

        private string toggleFPSLockButtonText;
        public string ToggleFPSLockButtonText
        {
            get { return toggleFPSLockButtonText; }
            set
            {
                toggleFPSLockButtonText = value;
                OnPropertyChanged(nameof(ToggleFPSLockButtonText));
            }
        }

        private string fpsLockValue = "60";
        public string FpsLockValue
        {
            get { return fpsLockValue; }
            set
            {
                fpsLockValue = value;
                SetTargetFPS(new object());
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

        private bool planetButtonsEnabled;
        public bool PlanetButtonsEnabled
        {
            get { return planetButtonsEnabled; }
            set
            {
                planetButtonsEnabled = value;
                OnPropertyChanged(nameof(PlanetButtonsEnabled));
            }
        }

        private string simulationSpeedMinBase;
        public string SimulationSpeedMinBase
        {
            get { return simulationSpeedMinBase; }
            set
            {
                if (int.TryParse(value, out int simSpeedMinBase))
                {
                    simulationSpeedMinBase = value;
                    simulationSpeedSliderValueMin = simSpeedMinBase * Math.Pow(10, int.Parse(SimulationSpeedMinPow));
                    UpdateSimulationSpeed();
                    OnPropertyChanged(nameof(SimulationSpeedMinBase));
                }
                else
                {
                    MessageBox.Show("Incorrect value");
                }
            }
        }

        private string simulationSpeedMaxBase;
        public string SimulationSpeedMaxBase
        {
            get { return simulationSpeedMaxBase; }
            set
            {
                if (int.TryParse(value, out int simSpeedMaxBase))
                {
                    simulationSpeedMaxBase = value;
                    simulationSpeedSliderValueMax = simSpeedMaxBase * Math.Pow(10, int.Parse(SimulationSpeedMaxPow));
                    UpdateSimulationSpeed();
                    OnPropertyChanged(nameof(SimulationSpeedMaxBase));
                }
                else
                {
                    MessageBox.Show("Incorrect value");
                }
            }
        }

        private string simulationSpeedMinPow;
        public string SimulationSpeedMinPow
        {
            get { return simulationSpeedMinPow; }
            set
            {
                if (int.TryParse(value, out int simSpeedMinPow))
                {
                    simulationSpeedMinPow = value;
                    simulationSpeedSliderValueMin = int.Parse(SimulationSpeedMinBase) * Math.Pow(10, simSpeedMinPow);
                    UpdateSimulationSpeed();
                    OnPropertyChanged(nameof(SimulationSpeedMinPow));
                }
                else
                {
                    MessageBox.Show("Incorrect value");
                }
            }
        }

        private string simulationSpeedMaxPow;
        public string SimulationSpeedMaxPow
        {
            get { return simulationSpeedMaxPow; }
            set
            {
                if (int.TryParse(value, out int simSpeedMaxPow))
                {
                    simulationSpeedMaxPow = value;
                    simulationSpeedSliderValueMax = int.Parse(SimulationSpeedMaxBase) * Math.Pow(10, simSpeedMaxPow);
                    UpdateSimulationSpeed();
                    OnPropertyChanged(nameof(SimulationSpeedMaxPow));
                }
                else
                {
                    MessageBox.Show("Incorrect value");
                }
            }
        }

        private double simulationSpeedSliderValueMin;

        private double simulationSpeedSliderValueMax;

        private double simulationSpeed;
        public double SimulationSpeed
        {
            get { return simulationSpeed; }
            set
            {
                simulationSpeed = value;
                UpdateSimulationSpeed();
                OnPropertyChanged(nameof(SimulationSpeed));
            }
        }

        private string simulationSpeedText;
        public string SimulationSpeedText
        {
            get { return simulationSpeedText; }
            set
            {
                simulationSpeedText = value;
                OnPropertyChanged(nameof(SimulationSpeedText));
            }
        }


        public RelayCommand ToggleSimulationCommand { get; private set; }
        public RelayCommand ToggleFPSLockCommand { get; private set; }
        public RelayCommand<KeyEventArgs> FpsLockTextBoxKeyUpCommand { get; private set; }
        public RelayCommand<KeyEventArgs> SimulationSpeedTextBoxKeyUpCommand { get; private set; }
        public RelayCommand LoadDBCommand { get; private set; }
        public RelayCommand SaveDBCommand { get; private set; }
        public RelayCommand ControlPanelClosedCommand { get; private set; }
        public RelayCommand EditPlanetCommand { get; private set; }
        public RelayCommand FollowPlanetCommand { get; private set; }


        public Planet? selectedPlanet;

        public ControlPanelVM()
        {
            ToggleSimulationCommand = new RelayCommand(ToggleSimulation);
            ToggleFPSLockCommand = new RelayCommand(LockFPS);
            FpsLockTextBoxKeyUpCommand = new RelayCommand<KeyEventArgs>(FpsLockTextBoxKeyUp);
            SimulationSpeedTextBoxKeyUpCommand = new RelayCommand<KeyEventArgs>(SimulationSpeedTextBoxKeyUp);
            LoadDBCommand = new RelayCommand(LoadDB);
            SaveDBCommand = new RelayCommand(SaveDB);
            ControlPanelClosedCommand = new RelayCommand(Close);
            EditPlanetCommand = new RelayCommand(EditPlanet);
            FollowPlanetCommand = new RelayCommand(FollowPlanet);


            if (Global.simulationStopped)
                PauseButtonText = "Resume";
            else
                PauseButtonText = "Pause";

            if (Global.followPlanet)
                FollowPlanetButtonText = "Unfollow";
            else
                FollowPlanetButtonText = "Follow";

            if (Global.fpsLocked)
                ToggleFPSLockButtonText = "Unlock FPS";
            else
                ToggleFPSLockButtonText = "Lock FPS";

            simulationSpeedMinBase = "1";
            simulationSpeedMinPow = "0";
            simulationSpeedMaxBase = "1";
            simulationSpeedMaxPow = "12";
            SimulationSpeedMinBase = "1";
            SimulationSpeedMinPow = "0";
            SimulationSpeedMaxBase = "1";
            SimulationSpeedMaxPow = "12";
        }

        private void EditPlanet(object e)
        {
            if (selectedPlanet == null)
                return;

            // Надо ли возобнавить симуляцию после закрытия диалогового окна редактирования планеты
            // (если симуляция была остановлена до диалогового окна, то она не возобновится после его закрытия)
            bool startSimulationAfterPlanetEdit = true;
            if (Global.simulationStopped)
                startSimulationAfterPlanetEdit = false;

            Global.simulationStopped = true;

            EditPlanetVM editPlanetVM = new EditPlanetVM(selectedPlanet);
            EditPlanetWindow editPlanetWindow = new EditPlanetWindow(editPlanetVM);
            if (editPlanetWindow.ShowDialog() == true)
            {
                selectedPlanet.Position = new Vector3D(double.Parse(editPlanetVM.PositionX), double.Parse(editPlanetVM.PositionY), double.Parse(editPlanetVM.PositionZ));
                selectedPlanet.Velocity = new Vector3D(double.Parse(editPlanetVM.VelocityX), double.Parse(editPlanetVM.VelocityY), double.Parse(editPlanetVM.VelocityZ));
                selectedPlanet.Mass = double.Parse(editPlanetVM.Mass);
                selectedPlanet.Radius = double.Parse(editPlanetVM.Radius);
                selectedPlanet.Color = Color.FromRgb(byte.Parse(editPlanetVM.ColorR), byte.Parse(editPlanetVM.ColorG), byte.Parse(editPlanetVM.ColorB));
            }

            if (startSimulationAfterPlanetEdit)
                Global.simulationStopped = false;
        }

        private void UpdateSimulationSpeed()
        {
            double simSpeed = MathUtils.Linear(0, 10, SimulationSpeed, simulationSpeedSliderValueMin, simulationSpeedSliderValueMax);
            SimulationSpeedText = simSpeed.ToString();
            Simulation.Instance.simulationSpeedMultiplier = simSpeed;
        }

        private void FollowPlanet(object e)
        {
            if (selectedPlanet == null)
                return;

            if (Global.followPlanet)
            {
                Global.followPlanet = false;
                FollowPlanetButtonText = "Follow";
                return;
            }

            Global.setCameraDeltaPos!.Invoke();
            Global.followPlanet = true;
            FollowPlanetButtonText = "Unfollow";
        }

        private void ToggleSimulation(object e)
        {
            Global.simulationStopped = !Global.simulationStopped;
            if (Global.simulationStopped)
                PauseButtonText = "Resume";
            else
                PauseButtonText = "Pause";
        }

        private void LockFPS(object e)
        {
            Global.fpsLocked = !Global.fpsLocked;
            if (Global.fpsLocked)
                ToggleFPSLockButtonText = "Unlock FPS";
            else
                ToggleFPSLockButtonText = "Lock FPS";
        }

        private void FpsLockTextBoxKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = ((TextBox)e.Source).GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
                SetTargetFPS(e);
            }
        }

        private void SimulationSpeedTextBoxKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = ((TextBox)e.Source).GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
            }
        }

        private void SetTargetFPS(object e)
        {
            if (int.TryParse(FpsLockValue, out int fps) == true)
            {
                Global.targetFrameTime = (long)(1 / (double)fps * 1000);
            }
            else
            {
                MessageBox.Show("Incorrect FPS value");
            }
        }

        private void LoadDB(object e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Load DB",
                Filter = "SQLite DB (*.db)|*.db"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                DbContextOptions<ApplicationContext> options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseSqlite("Data Source=" + filePath)
                .Options;

                Global.db = new ApplicationContext(options);
                Global.db.Database.EnsureCreated();
                Global.db.Planets.Load();

                Simulation.Instance.LoadPlanetsFromDB();

                MessageBox.Show($"Выбран файл: {openFileDialog.FileName}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveDB(object e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save DB",
                Filter = "SQLite DB (*.db)|*.db",
                DefaultExt = ".db",
                AddExtension = true
            };

            // Открываем диалог и проверяем, был ли выбран путь для сохранения
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                DbContextOptions<ApplicationContext> options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseSqlite("Data Source=" + filePath)
                .Options;

                Global.db = new ApplicationContext(options);
                Global.db.Database.EnsureCreated();

                foreach (Planet planet in Simulation.Instance.planets)
                {
                    Global.db.Add(planet.PlanetDBEntry);
                }
                Global.db.SaveChanges();

                MessageBox.Show($"Файл сохранен: {filePath}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close(object e)
        {
            Global.controlPanelWindow = null;
        }

        public void DisplayPlanetInfo(Planet planet)
        {
            selectedPlanet = planet;
            FollowPlanetButtonText = "Follow";
            PlanetButtonsEnabled = true;
            PlanetInfoText = "Color: " + selectedPlanet.Color.ToString() + '\n' +
                             "Position:\n" +
                             "  X: " + selectedPlanet.Position.X.ToString("##.##") + '\n' +
                             "  Y: " + selectedPlanet.Position.Y.ToString("##.##") + '\n' +
                             "  Z: " + selectedPlanet.Position.Z.ToString("##.##") + '\n' +
                             "Velocity: \n" +
                             "  X: " + selectedPlanet.Velocity.X.ToString() + '\n' +
                             "  Y: " + selectedPlanet.Velocity.Y.ToString() + '\n' +
                             "  Z: " + selectedPlanet.Velocity.Z.ToString() + '\n' +
                             "Mass: " + selectedPlanet.Mass.ToString("##.##") + '\n' +
                             "Radius: " + selectedPlanet.Radius.ToString("##.##");
        }

        public void ClearPlanetInfo()
        {
            selectedPlanet = null;
            FollowPlanetButtonText = "Follow";
            PlanetButtonsEnabled = false;
            PlanetInfoText = "";
        }
    }
}
