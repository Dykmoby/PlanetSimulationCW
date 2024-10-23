using PlanetSimulationCW.Model;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PlanetSimulationCW.ModelView
{
    class MainVM : Property
    {
        int frame = 0;

        private Model3DGroup modelGroup;
        private PerspectiveCamera? camera;

        public Model3DGroup ModelGroup
        {
            get { return modelGroup; }
            set
            {
                modelGroup = value;
                OnPropertyChanged(nameof(ModelGroup));
            }
        }

        public PerspectiveCamera Camera
        {
            get { return camera; }
            set
            {
                camera = value;
                OnPropertyChanged(nameof(Camera));
            }
        }

        Simulation simulation;

        public MainVM()
        {
            simulation = new Simulation(100);

            Camera = new PerspectiveCamera();
            Camera.Position = new Point3D(200, 200, 300);
            Camera.LookDirection = new Vector3D(0, 0, -1);
            Camera.UpDirection = new Vector3D(0, 1, 0);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Render;
            timer.Start();
        }

        private RelayCommand<KeyEventArgs>? keyDownCommand;
        public RelayCommand<KeyEventArgs> KeyDownCommand
        {
            get
            {
                return keyDownCommand ??
                  (keyDownCommand = new RelayCommand<KeyEventArgs>(e =>
                  {
                      const float moveRate = 10;
                      const float rotationSensitivity = 0.05f;
                      Vector3D rotation = new Vector3D();
                      Vector3D moveDirection = new Vector3D();

                      switch (e.Key)
                      {
                          case Key.W:
                              moveDirection = Camera.LookDirection;
                              break;
                          case Key.S:
                              moveDirection = -Camera.LookDirection;
                              break;
                          case Key.A:
                              moveDirection = -Vector3D.CrossProduct(Camera.LookDirection, new Vector3D(0, 1, 0));
                              break;
                          case Key.D:
                              moveDirection = Vector3D.CrossProduct(Camera.LookDirection, new Vector3D(0, 1, 0));
                              break;
                          case Key.Up:
                              rotation = new Vector3D(0, -1, 0);
                              break;
                          case Key.Down:
                              rotation = new Vector3D(0, 1, 0);
                              break;
                          case Key.Left:
                              rotation = new Vector3D(1, 0, 0);
                              break;
                          case Key.Right:
                              rotation = new Vector3D(-1, 0, 0);
                              break;
                      }

                      Camera.Position += moveRate * moveDirection;
                      Camera.LookDirection = new Vector3D(
                              Camera.LookDirection.X - rotation.X * rotationSensitivity,
                              Camera.LookDirection.Y - rotation.Y * rotationSensitivity,
                              Camera.LookDirection.Z
                          );
                  }));
            }
        }

        private void Render(object? sender, EventArgs e) // Отрисовка кадра симуляции
        {
            frame++;

            ModelGroup = CreateModelGroup(simulation.planets); // Обновление модели в представлении

            simulation.SimulateStep();
        }

        private Model3DGroup CreateModelGroup(List<Planet> planets)
        {
            Model3DGroup modelGroup = new Model3DGroup();

            foreach (Planet planet in planets)
            {
                GeometryModel3D geometryModel = MeshUtils.CreatePlanetGeometryModel();
                Transform3DGroup transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(new TranslateTransform3D(planet.Position.X, planet.Position.Y, -2)); // Добавить сюда потом planet.Position.Z
                geometryModel.Transform = transformGroup;

                modelGroup.Children.Add(geometryModel);
            }

            return modelGroup;
        }

        private bool CheckIfPointInBounds(float x, float y, int xMax, int yMax)
        {
            if (x < 0 || y < 0 || x >= xMax || y >= yMax)
            {
                return false;
            }
            return true;
        }
    }
}
