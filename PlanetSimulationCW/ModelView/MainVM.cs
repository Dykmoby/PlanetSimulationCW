using PlanetSimulationCW.Model;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PlanetSimulationCW.ModelView
{
    class MainVM : Property
    {
        int frame = 0;

        private Model3DGroup modelGroup;
        public Model3DGroup ModelGroup
        {
            get { return modelGroup; }
            set
            {
                modelGroup = value;
                OnPropertyChanged(nameof(ModelGroup));
            }
        }

        Simulation simulation;

        public MainVM()
        {
            simulation = new Simulation(100);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Render;
            timer.Start();
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
