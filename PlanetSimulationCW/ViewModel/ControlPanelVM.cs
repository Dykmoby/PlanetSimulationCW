﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using PlanetSimulationCW.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

        private string fpsLockValue;
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

        public RelayCommand ToggleSimulationCommand { get; private set; }
        public RelayCommand ToggleFPSLockCommand { get; private set; }
        public RelayCommand<KeyEventArgs> FpsLockTextBoxKeyUpCommand { get; private set; }
        public RelayCommand LoadDBCommand { get; private set; }
        public RelayCommand SaveDBCommand { get; private set; }
        public RelayCommand ControlPanelClosedCommand { get; private set; }

        public ControlPanelVM()
        {
            ToggleSimulationCommand = new RelayCommand(ToggleSimulation);
            ToggleFPSLockCommand = new RelayCommand(LockFPS);
            FpsLockTextBoxKeyUpCommand = new RelayCommand<KeyEventArgs>(FpsLockTextBoxKeyUp);
            LoadDBCommand = new RelayCommand(LoadDB);
            SaveDBCommand = new RelayCommand(SaveDB);
            ControlPanelClosedCommand = new RelayCommand(Close);


            if (Global.simulationStopped)
                PauseButtonText = "Resume";
            else
                PauseButtonText = "Pause";

            if (Global.fpsLocked)
                ToggleFPSLockButtonText = "Unlock FPS";
            else
                ToggleFPSLockButtonText = "Lock FPS";
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
            PlanetInfoText = planet.Color.ToString();
        }

        public void ClearPlanetInfo()
        {
            PlanetInfoText = "";
        }
    }
}
