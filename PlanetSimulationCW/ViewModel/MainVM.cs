using PlanetSimulationCW.Model;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PlanetSimulationCW.ViewModel
{
    public static class QuaternionExtensions
    {
        // Вспомогательный метод расширения для поворота вектора кватернионом
        public static Vector3D Rotate(this Quaternion q, Vector3D v, double rotationSpeed = 1)
        {
            Quaternion vQuat = new Quaternion(v, rotationSpeed);
            Quaternion resultQuat = q * vQuat * Conjugate(q);
            return new Vector3D(resultQuat.X, resultQuat.Y, resultQuat.Z);
        }

        // Метод расширения для сопряжения кватерниона
        public static Quaternion Conjugate(Quaternion q)
        {
            return new Quaternion(-q.X, -q.Y, -q.Z, q.W);
        }
    }

    class MainVM : Property
    {
        int frame = 0;

        private Point prevMousePos;
        private Quaternion rotation = Quaternion.Identity;
        private double rotationSpeed = 5;

        private Model3DGroup modelGroup;
        private PerspectiveCamera camera;
        private string log;

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

        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                OnPropertyChanged(nameof(Log));
            }
        }

        Simulation simulation;

        public MainVM()
        {
            simulation = new Simulation(100);

            Camera = new PerspectiveCamera();
            Camera.Position = new Point3D(100, 100, 300);
            Camera.LookDirection = new Vector3D(0, 0, -1);
            Camera.UpDirection = new Vector3D(0, 1, 0);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Render;
            timer.Start();

            MouseRightButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnMouseRightButtonDown);
            MouseMoveCommand = new RelayCommand<MouseEventArgs>(OnMouseMove);
            MouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnMouseRightButtonUp);
        }

        private void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Mouse.Capture((UIElement)e.Source);
            prevMousePos = e.GetPosition((UIElement)e.Source);
        }

        private void OnMouseMove(MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point currentMousePos = e.GetPosition((UIElement)e.Source);

                // Вычисляем изменения по осям
                double deltaX = currentMousePos.X - prevMousePos.X;
                double deltaY = currentMousePos.Y - prevMousePos.Y;

                // Создаем кватернионы для поворотов по осям X и Y
                Quaternion horizontalRotation = new Quaternion(new Vector3D(0, -1, 0), deltaX / rotationSpeed);
                Quaternion verticalRotation = new Quaternion(new Vector3D(-1, 0, 0), deltaY / rotationSpeed);

                // Накапливаем общий поворот
                rotation *= horizontalRotation * verticalRotation;
                rotation.Normalize();

                // Обновляем направление камеры
                UpdateCameraDirection();

                // Обновляем предыдущее положение мыши
                prevMousePos = currentMousePos;
            }
        }

        private void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void UpdateCameraDirection()
        {
            // Исходное направление камеры (можно изменить на начальное состояние камеры)
            Vector3D initialLookDirection = new Vector3D(0, 0, -1);
            Vector3D initialUpDirection = new Vector3D(0, 1, 0);

            // Применяем текущий кватернион поворота к вектору направления и вертикали
            Vector3D lookDirection = rotation.Rotate(initialLookDirection, rotationSpeed);
            Vector3D upDirection = rotation.Rotate(initialUpDirection, rotationSpeed);

            Camera.LookDirection = lookDirection;
            Camera.UpDirection = upDirection;

            //Log = lookDirection.ToString() + " / " + upDirection.ToString();
        }


        private RelayCommand<KeyEventArgs>? keyDownCommand;
        public RelayCommand<MouseButtonEventArgs> MouseRightButtonDownCommand { get; private set; }
        public RelayCommand<MouseEventArgs> MouseMoveCommand { get; private set; }
        public RelayCommand<MouseButtonEventArgs> MouseRightButtonUpCommand { get; private set; }
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
                              moveDirection = -Vector3D.CrossProduct(Camera.LookDirection, Camera.UpDirection);
                              break;
                          case Key.D:
                              moveDirection = Vector3D.CrossProduct(Camera.LookDirection, Camera.UpDirection);
                              break;
                      }

                      if (moveDirection.Length > 0)
                      {
                          moveDirection.Normalize();
                          Camera.Position += moveRate * moveDirection;
                      }
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
                transformGroup.Children.Add(new TranslateTransform3D(planet.Position.X, planet.Position.Y, planet.Position.Z));
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
