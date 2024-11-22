using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using PlanetSimulationCW.Model;
using System.Windows;
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
        public RelayCommand LoadDBCommand { get; private set; }
        public RelayCommand SaveDBCommand { get; private set; }
        public RelayCommand ControlPanelClosedCommand { get; private set; }

        public ControlPanelVM()
        {
            ToggleSimulationCommand = new RelayCommand(ToggleSimulation);
            LoadDBCommand = new RelayCommand(LoadDB);
            SaveDBCommand = new RelayCommand(SaveDB);
            ControlPanelClosedCommand = new RelayCommand(Close);


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
            PlanetInfoText = planet.Color.ToString();
        }

        public void ClearPlanetInfo()
        {
            PlanetInfoText = "";
        }
    }
}
