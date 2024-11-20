using PlanetSimulationCW.Model;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PlanetSimulationCW.ViewModel
{
    class MainVM : Property
    {
        private Point prevMousePos;
        private Quaternion cameraRotation = Quaternion.Identity;
        private double rotationSpeed = 10;
        private double moveSpeedMax = 4000;
        private double moveSpeedMin = 1;
        private double moveSpeedMaxTime = 3000; // Через сколько миллисекунд достигается максимальная скорость перемещения камеры (с зажатой клавишой перемещения)
        private double rotationSensitivity = 0.05f;
        private int maxRaycastDistance = 10000; // Максимальное расстояние луча, испускаемого из камеры (для выбора планеты нажатием)
        private List<Key> pressedKeys = new List<Key>();
        private bool rightMouseDown = false;
        private readonly Key[] movementKeys = { Key.W, Key.A, Key.S, Key.D, Key.Q, Key.E };
        private bool renderOctants = false;
        private long targetFrameTime = 16; // 60 fps
        private long frameTime = 0; // Время в миллисекундах, за которое отрисовался текущий кадр (без дополнительного ожидания после отрисовки)
        private long deltaTime = 0; // Время между прошлым и текущим кадром в миллисекундах

        private ControlPanelWindow controlPanelWindow;

        private Stopwatch movementStopwatch = new Stopwatch();
        private Stopwatch frameStopwatch = new Stopwatch();
        private Stopwatch deltaTimeStopwatch = new Stopwatch();

        Simulation simulation;

        private Model3DGroup modelGroup;
        private PerspectiveCamera camera;
        private Viewport3D viewport;
        private string log;

        private Point selMousePos;

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
        public RelayCommand<MouseButtonEventArgs> MouseLeftButtonUpCommand { get; private set; }
        public RelayCommand<KeyEventArgs> KeyDownCommand { get; private set; }
        public RelayCommand<KeyEventArgs> KeyUpCommand { get; private set; }
        public RelayCommand MainWindowClosed { get; private set; }

        public MainVM(Viewport3D viewport)
        {
            this.viewport = viewport;
            controlPanelWindow = new ControlPanelWindow();
            controlPanelWindow.Show();

            simulation = new Simulation(300);

            Camera = new PerspectiveCamera();
            Camera.Position = new Point3D(0, 0, 0);
            Camera.LookDirection = new Vector3D(0, 0, -1);
            Camera.UpDirection = new Vector3D(0, 1, 0);
            Camera.FarPlaneDistance = 1000000;
            Camera.FieldOfView = 45;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Update;
            timer.Start();

            MouseRightButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(OnMouseRightButtonDown);
            MouseMoveCommand = new RelayCommand<MouseEventArgs>(OnMouseMove);
            MouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnMouseRightButtonUp);
            MouseLeftButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(OnMouseLeftButtonUp);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(OnKeyDown);
            KeyUpCommand = new RelayCommand<KeyEventArgs>(OnKeyUp);
            MainWindowClosed = new RelayCommand(obj => { Application.Current.Shutdown(0); });
        }

        private void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Mouse.Capture((UIElement)e.Source);
            prevMousePos = e.GetPosition((UIElement)e.Source);
            rightMouseDown = true;
        }

        private void OnMouseMove(MouseEventArgs e)
        {
            selMousePos = e.GetPosition((UIElement)e.Source);
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

        private void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //Planet planet = SelectPlanet(e.GetPosition((UIElement)e.Source));
            //MessageBox.Show(planet?.Color.ToString() + "\n" + e.GetPosition((UIElement)e.Source));
        }

        private void OnKeyDown(KeyEventArgs e)
        {
            if (!pressedKeys.Contains(e.Key))
            {
                TrySetMovementStopwatch(e.Key);
                pressedKeys.Add(e.Key);
            }
        }

        private void OnKeyUp(KeyEventArgs e)
        {
            if (pressedKeys.Contains(e.Key))
            {
                TryResetMovementStopwatch(e.Key);
                pressedKeys.Remove(e.Key);
            }
        }

        // Запускает таймер (movementStopwatch), который влияет на плавное увеличение скорости перемещения камеры, если клавиши перемещения зажаты
        private void TrySetMovementStopwatch(Key key)
        {
            // Если любая из двигающих камеру клавиш нажата впервые
            if (pressedKeys.Intersect(movementKeys).Any() == false && movementKeys.Contains(key))
            {
                movementStopwatch.Restart();
            }
        }

        // Останавливает movementStopwatch, если была отжата последняя клавиша перемещения камеры
        private void TryResetMovementStopwatch(Key key)
        {
            // Если любая из двигающих камеру клавиш была отжата, и больше никакие клавиши перемещения не нажаты
            if (pressedKeys.Intersect(movementKeys).ToArray().Length == 1 && movementKeys.Contains(key))
            {
                movementStopwatch.Stop();
            }
        }

        private void UpdateCameraDirection()
        {
            // Исходное направление камеры
            Vector3D initialLookDirection = new Vector3D(0, 0, -1);
            Vector3D initialUpDirection = new Vector3D(0, 1, 0);

            Vector3D lookDirection = cameraRotation.Rotate(initialLookDirection, rotationSpeed);
            Vector3D upDirection = cameraRotation.Rotate(initialUpDirection, rotationSpeed);

            Camera.LookDirection = lookDirection;
            Camera.UpDirection = upDirection;
        }

        private void Update(object? sender, EventArgs e)
        {
            frameStopwatch.Restart();
            MoveCamera();

            SelectPlanet(selMousePos);

            // Отрисовка планет во View
            ModelGroup = CreateModelGroup(simulation.planets, simulation.octree);

            simulation.SimulateStep(deltaTime / 1000d);

            frameTime = frameStopwatch.ElapsedMilliseconds;
            if (frameTime <= targetFrameTime)
            {
                Thread.Sleep((int)(targetFrameTime - frameTime));
            }

            //Log = (1000 / frameStopwatch.ElapsedMilliseconds).ToString();

            deltaTime = deltaTimeStopwatch.ElapsedMilliseconds;
            deltaTimeStopwatch.Restart();
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

            if (pressedKeys.Contains(Key.Q))
            {
                moveDirection = -Camera.UpDirection;
                moveDirection.Normalize();
            }
            else if (pressedKeys.Contains(Key.E))
            {
                Console.WriteLine(simulation.planets.Capacity);
                moveDirection = Camera.UpDirection;
                moveDirection.Normalize();
            }

            if (moveDirection.Length > 0)
            {
                moveDirection.Normalize();
                Camera.Position += MathUtils.Linear(0, moveSpeedMaxTime, movementStopwatch.ElapsedMilliseconds, moveSpeedMin, moveSpeedMax) * moveDirection * deltaTime / 1000d;
            }
        }

        public Ray3D GetRayFromScreen(Point mousePosition)
        {
            //return Point2DtoRay3D(viewport, mousePosition);
            double aspect = viewport.ActualWidth / viewport.ActualHeight;

            Point point01 = new Point(mousePosition.X / viewport.ActualWidth, mousePosition.Y / viewport.ActualHeight);
            
            // NDC space
            var pointNormalized = new Point3D(
                (2.0 * point01.X - 1.0),
                -(2.0 * point01.Y - 1.0),
                0
            );

            // Get Camera Matrices
            Matrix3D viewMatrix = GetViewMatrix(camera);
            Matrix3D projectionMatrix = GetProjectionMatrix(camera, aspect);

            Matrix3D viewProjectionMatrix = viewMatrix * projectionMatrix;
            viewProjectionMatrix.Invert();

            // Get Points on near and far frustum planes on frustum
            pointNormalized.Z = 0.0;
            Point3D nearPoint = viewProjectionMatrix.Transform(pointNormalized);
            pointNormalized.Z = 1.0;
            Point3D farPoint = viewProjectionMatrix.Transform(pointNormalized);

            // Create ray
            Point3D rayOrigin = camera.Position;
            Vector3D rayDirection = farPoint - nearPoint;
            rayDirection.Normalize();

            return new Ray3D((Vector3D)rayOrigin, rayDirection);
        }

        public static Matrix3D GetViewMatrix(PerspectiveCamera camera)
        {
            // Создаем матрицу вида на основе параметров камеры
            Vector3D lookDirection = -camera.LookDirection;
            Vector3D upDirection = camera.UpDirection;
            Point3D position = camera.Position;

            // Нормализуем направление взгляда
            lookDirection.Normalize();

            // Вычисляем правый вектор как векторное произведение
            Vector3D rightVector = Vector3D.CrossProduct(upDirection, lookDirection);
            rightVector.Normalize();

            // Пересчитываем up вектор для обеспечения ортогональности
            upDirection = Vector3D.CrossProduct(lookDirection, rightVector);
            upDirection.Normalize();

            // Создаем матрицу вида
            Matrix3D viewMatrix = new Matrix3D(
                rightVector.X, upDirection.X, lookDirection.X, 0,
                rightVector.Y, upDirection.Y, lookDirection.Y, 0,
                rightVector.Z, upDirection.Z, lookDirection.Z, 0,
                0, 0, 0, 1
            );

            return viewMatrix;
        }
        public static Matrix3D GetProjectionMatrix(PerspectiveCamera camera, double aspect)
        {
            double fovRadians = camera.FieldOfView * (Math.PI / 360.0);
            double n = camera.NearPlaneDistance;
            double f = camera.FarPlaneDistance;

            double h = 1.0 / Math.Tan(fovRadians);

            double fn = f - n;

            return new Matrix3D(
                h, 0, 0, 0,
                0, h * aspect, 0, 0,
                0, 0, -(f + n) / fn, -1,
                0, 0, -2.0 * f * n / fn, 0);
        }

        // Проверка пересечения луча с планетой
        private bool RayPlanetIntersection(Ray3D ray, Planet planet, out double distance)
        {
            distance = double.MaxValue;
            Vector3D l = planet.Position - ray.Origin;
            double tca = Vector3D.DotProduct(l, ray.Direction);

            // Если tca < 0, луч уже никак не пересечет планету, потому что он направлен в совершенно другую сторону
            if (tca < 0) return false;

            double d2 = Vector3D.DotProduct(l, l) - tca * tca;
            double r2 = planet.Radius * planet.Radius;

            if (d2 > r2) return false;

            double thc = Math.Sqrt(r2 - d2);
            double t0 = tca - thc;
            double t1 = tca + thc;

            if (t0 < 0 && t1 < 0) return false;

            distance = t0 < 0 ? t1 : t0;
            return true;
        }

        // Выбрать планету при помощи мышки (курсором)
        public Planet? SelectPlanet(Point mousePosition)
        {
            Ray3D ray = GetRayFromScreen(mousePosition);
            if (ray == null) return null;

            Planet? closestPlanet = null;
            double closestDistance = double.MaxValue;

            foreach (Planet planet in simulation.octree.FindNearestPlanets((Vector3D)Camera.Position, maxRaycastDistance))
            {
                double distance;
                if (RayPlanetIntersection(ray, planet, out distance))
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlanet = planet;
                    }
                }
            }

            Log = ray.Origin.ToString() + "\n" + ray.Direction.ToString() + "\n" + closestPlanet?.Color.ToString();
            return closestPlanet;
        }


        private Model3DGroup CreateModelGroup(List<Planet> planets, Octree octree)
        {
            Model3DGroup modelGroup = new Model3DGroup();

            Ray3D ray = GetRayFromScreen(selMousePos);

            GeometryModel3D geometryModelDebug = MeshUtils.CreatePlanetGeometryModel(Color.FromArgb(255, 255, 255, 255), 8);
            Transform3DGroup transformGroupDebug = new Transform3DGroup();
            transformGroupDebug.Children.Add(new TranslateTransform3D(ray.Origin + ray.Direction * 50));
            geometryModelDebug.Transform = transformGroupDebug;

            modelGroup.Children.Add(geometryModelDebug);

            //GeometryModel3D rayGroup = Ray3DHelper.CreateRay((Point3D)ray.Origin, ray.Direction);
            //modelGroup.Children.Add(rayGroup);

            foreach (Planet planet in planets) // Отрисовка планет
            {
                int lod = GetLODByDistance(planet);
                if (lod == 0) continue;
                GeometryModel3D geometryModel = MeshUtils.CreatePlanetGeometryModel(planet.Color, lod);
                //GeometryModel3D geometryModel = MeshUtils.CreatePlanetGeometryModel(planet.Color);
                Transform3DGroup transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(new ScaleTransform3D(planet.Radius, planet.Radius, planet.Radius));
                transformGroup.Children.Add(new TranslateTransform3D(planet.Position.X, planet.Position.Y, planet.Position.Z));
                geometryModel.Transform = transformGroup;

                modelGroup.Children.Add(geometryModel);
            }

            if (renderOctants)
            {
                foreach (Octree.Node node in octree.GetAllNodes()) // Отрисовка октантов
                {
                    if (node == octree.Root)
                        continue;

                    GeometryModel3D geometryModel = MeshUtils.CreateOctantGeometryModel();
                    Transform3DGroup transformGroup = new Transform3DGroup();
                    transformGroup.Children.Add(new ScaleTransform3D(node.size * 2, node.size * 2, node.size * 2));
                    transformGroup.Children.Add(new TranslateTransform3D(node.center.X, node.center.Y, node.center.Z));
                    geometryModel.Transform = transformGroup;

                    modelGroup.Children.Add(geometryModel);
                }
            }

            return modelGroup;
        }

        // Получить LOD в зависимости от расстояния между камерой и планетой
        private int GetLODByDistance(Planet planet)
        {
            double distance = (planet.Position - (Vector3D)Camera.Position).Length;
            if (distance >= 0 && distance < 100) // LOD 0
            {
                return 12;
            }
            else if (distance >= 100 && distance < 300) // LOD 1
            {
                return 8;
            }
            else if (distance >= 300 && distance < 1000) // LOD 2
            {
                return 5;
            }
            else if (distance >= 1000 && distance < 3000) // LOD 3
            {
                return 3;
            }
            else if (distance >= 3000 && distance < 50000) // LOD 4
            {
                return 2;
            }
            else if (distance >= 50000) // LOD 5 (NO RENDER)
            {
                return 0; // Не рендерить планету
            }

            return 0; // Не рендерить планету
        }
    }
}
