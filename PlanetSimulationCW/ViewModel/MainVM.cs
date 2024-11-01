using PlanetSimulationCW.Model;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PlanetSimulationCW.ViewModel
{
    class MainVM : Property
    {
        private Point prevMousePos;
        private Quaternion cameraRotation = Quaternion.Identity;
        private double rotationSpeed = 5;
        const float moveRate = 10;
        const float rotationSensitivity = 0.05f;
        private List<Key> pressedKeys = new List<Key>();
        private bool rightMouseDown = false;

        Simulation simulation;

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

        public RelayCommand<MouseButtonEventArgs> MouseRightButtonDownCommand { get; private set; }
        public RelayCommand<MouseEventArgs> MouseMoveCommand { get; private set; }
        public RelayCommand<MouseButtonEventArgs> MouseRightButtonUpCommand { get; private set; }
        public RelayCommand<KeyEventArgs> KeyDownCommand { get; private set; }
        public RelayCommand<KeyEventArgs> KeyUpCommand { get; private set; }

        public MainVM()
        {
            simulation = new Simulation(100);

            Camera = new PerspectiveCamera();
            Camera.Position = new Point3D(100, 100, 300);
            Camera.LookDirection = new Vector3D(0, 0, -1);
            Camera.UpDirection = new Vector3D(0, 1, 0);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Update;
            timer.Start();

            MouseRightButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnMouseRightButtonDown);
            MouseMoveCommand = new RelayCommand<MouseEventArgs>(OnMouseMove);
            MouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnMouseRightButtonUp);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(OnKeyDown);
            KeyUpCommand = new RelayCommand<KeyEventArgs>(OnKeyUp);
        }

        private void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Mouse.Capture((UIElement)e.Source);
            prevMousePos = e.GetPosition((UIElement)e.Source);
            rightMouseDown = true;
        }

        private void OnMouseMove(MouseEventArgs e)
        {
            if (rightMouseDown == true)
            {
                Point currentMousePos = e.GetPosition((UIElement)e.Source);

                double deltaX = currentMousePos.X - prevMousePos.X;
                double deltaY = currentMousePos.Y - prevMousePos.Y;

                Quaternion horizontalRotation = new Quaternion(new Vector3D(0, -1, 0), deltaX / rotationSpeed);
                Quaternion verticalRotation = new Quaternion(new Vector3D(-1, 0, 0), deltaY / rotationSpeed);

                cameraRotation *= horizontalRotation * verticalRotation;
                cameraRotation.Normalize();

                UpdateCameraDirection();

                prevMousePos = currentMousePos;
            }
        }

        private void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            rightMouseDown = false;
        }

        private void OnKeyDown(KeyEventArgs e)
        {
            if (!pressedKeys.Contains(e.Key))
            {
                pressedKeys.Add(e.Key);
            }
        }

        private void OnKeyUp(KeyEventArgs e)
        {
            if (pressedKeys.Contains(e.Key))
            {
                pressedKeys.Remove(e.Key);
            }
        }

        private void UpdateCameraDirection()
        {
            // Исходное направление камеры (можно изменить на начальное состояние камеры)
            Vector3D initialLookDirection = new Vector3D(0, 0, -1);
            Vector3D initialUpDirection = new Vector3D(0, 1, 0);

            Vector3D lookDirection = cameraRotation.Rotate(initialLookDirection, rotationSpeed);
            Vector3D upDirection = cameraRotation.Rotate(initialUpDirection, rotationSpeed);

            Camera.LookDirection = lookDirection;
            Camera.UpDirection = upDirection;

            //Log = lookDirection.ToString() + " / " + upDirection.ToString();
        }

        private void Update(object? sender, EventArgs e)
        {
            MoveCamera();
            ModelGroup = CreateModelGroup(simulation.planets); // Отрисовка планет во View
            simulation.SimulateStep();
        }

        private void MoveCamera()
        {
            Vector3D moveDirection = new Vector3D();

            if (pressedKeys.Contains(Key.W))
            {
                moveDirection = Camera.LookDirection;
                moveDirection.Normalize();
            }
            else if (pressedKeys.Contains(Key.S))
            {
                moveDirection = -Camera.LookDirection;
                moveDirection.Normalize();
            }

            if (pressedKeys.Contains(Key.A))
            {
                Vector3D leftDirection = -Vector3D.CrossProduct(Camera.LookDirection, Camera.UpDirection);
                leftDirection.Normalize();
                moveDirection += leftDirection;
            }
            else if (pressedKeys.Contains(Key.D))
            {
                Vector3D rightDirection = Vector3D.CrossProduct(Camera.LookDirection, Camera.UpDirection);
                rightDirection.Normalize();
                moveDirection += rightDirection;
            }

            if (moveDirection.Length > 0)
            {
                moveDirection.Normalize();
                Camera.Position += moveRate * moveDirection;
            }
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
