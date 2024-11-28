using PlanetSimulationCW.Model;
using System.Diagnostics;
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
        private long frameTime = 0; // Время в миллисекундах, за которое отрисовался текущий кадр (без дополнительного ожидания после отрисовки)
        private long deltaTime = 0; // Время между прошлым и текущим кадром в миллисекундах

        private Stopwatch movementStopwatch = new Stopwatch();
        private Stopwatch frameStopwatch = new Stopwatch();
        private Stopwatch deltaTimeStopwatch = new Stopwatch();

        private Model3DGroup modelGroup;
        private PerspectiveCamera camera;
        private Point3D cameraDeltaPos;
        private Viewport3D viewport;
        private string log;

        private Planet selectedPlanet;

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
        public RelayCommand MainWindowClosedCommand { get; private set; }
        public RelayCommand MainWindowDeactivatedCommand { get; private set; }

        public MainVM(Viewport3D viewport)
        {
            this.viewport = viewport;
            Global.setCameraDeltaPos += () => { cameraDeltaPos = (Point3D)(Camera.Position - (Point3D)selectedPlanet!.Position); };
            Global.controlPanelWindow = new ControlPanelWindow();
            Global.controlPanelWindow.Show();

            Simulation.Instance.AddPlanets(800);

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
            MainWindowClosedCommand = new RelayCommand(obj => { Application.Current.Shutdown(0); });
            MainWindowDeactivatedCommand = new RelayCommand(obj => { pressedKeys.Clear(); });
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

        // Перемещение камерой при помощи ПКМ
        private void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            rightMouseDown = false;
        }

        // Выбор планеты при помощи ЛКМ
        private void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Global.followPlanet = false;
            selectedPlanet = SelectPlanet(e.GetPosition((UIElement)e.Source));
            if (selectedPlanet == null)
            {
                (Global.controlPanelWindow?.DataContext as ControlPanelVM)?.ClearPlanetInfo();
                return;
            }

            (Global.controlPanelWindow?.DataContext as ControlPanelVM)?.DisplayPlanetInfo(selectedPlanet);
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

            if (e.Key == Key.P) // Если нажата P, открыть панель управления (если она закрыта)
            {
                if (Global.controlPanelWindow == null)
                {
                    Global.controlPanelWindow = new ControlPanelWindow();
                    Global.controlPanelWindow.Show();
                }
            }

            if (e.Key == Key.Space) // Если нажата P, открыть панель управления (если она закрыта)
            {
                (Global.controlPanelWindow?.DataContext as ControlPanelVM)?.ToggleSimulation(null);
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

            // Отрисовка планет во View
            ModelGroup = CreateModelGroup(Simulation.Instance.planets, Simulation.Instance.octree);

            Simulation.Instance.SimulateStep(deltaTime / 1000d);

            frameTime = frameStopwatch.ElapsedMilliseconds;
            if (Global.fpsLocked && frameTime <= Global.targetFrameTime)
            {
                Thread.Sleep((int)(Global.targetFrameTime - frameTime));
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
                Console.WriteLine(Simulation.Instance.planets.Capacity);
                moveDirection = Camera.UpDirection;
                moveDirection.Normalize();
            }

            if (moveDirection.Length > 0)
            {
                moveDirection.Normalize();
                if (Global.followPlanet)
                {
                    cameraDeltaPos += MathUtils.Linear(0, moveSpeedMaxTime, movementStopwatch.ElapsedMilliseconds, moveSpeedMin, moveSpeedMax) * moveDirection * deltaTime / 1000d;
                }
                else
                {
                    Camera.Position += MathUtils.Linear(0, moveSpeedMaxTime, movementStopwatch.ElapsedMilliseconds, moveSpeedMin, moveSpeedMax) * moveDirection * deltaTime / 1000d;
                }
            }

            if (Global.followPlanet)
                Camera.Position = selectedPlanet.Position + cameraDeltaPos;
        }

        public Ray3D GetRayFromScreen(Point mousePosition)
        {
            double aspect = viewport.ActualWidth / viewport.ActualHeight;

            Point point01 = new Point(mousePosition.X / viewport.ActualWidth, mousePosition.Y / viewport.ActualHeight);

            // Точка в NDC пространстве
            var pointNormalized = new Point3D(
                (2.0 * point01.X - 1.0),
                -(2.0 * point01.Y - 1.0),
                0
            );

            // Получаем матрицы камеры
            Matrix3D viewMatrix = GetViewMatrix(camera);
            Matrix3D projectionMatrix = GetProjectionMatrix(camera, aspect);

            Matrix3D viewProjectionMatrix = viewMatrix * projectionMatrix;
            viewProjectionMatrix.Invert();

            // Точки для вычисления на near и far frustum planes
            pointNormalized.Z = 0.0;
            Point3D nearPoint = viewProjectionMatrix.Transform(pointNormalized);
            pointNormalized.Z = 1.0;
            Point3D farPoint = viewProjectionMatrix.Transform(pointNormalized);

            // Создаем луч
            Point3D rayOrigin = camera.Position;
            Vector3D rayDirection = farPoint - nearPoint;
            rayDirection.Normalize();

            return new Ray3D((Vector3D)rayOrigin, rayDirection);
        }

        public static Matrix3D GetViewMatrix(PerspectiveCamera camera)
        {
            Vector3D lookDirection = -camera.LookDirection;
            Vector3D upDirection = camera.UpDirection;
            Point3D position = camera.Position;

            lookDirection.Normalize();

            Vector3D rightVector = Vector3D.CrossProduct(upDirection, lookDirection);
            rightVector.Normalize();

            upDirection = Vector3D.CrossProduct(lookDirection, rightVector);
            upDirection.Normalize();

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

            foreach (Planet planet in Simulation.Instance.octree.FindNearestPlanets((Vector3D)Camera.Position, maxRaycastDistance))
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

            return closestPlanet;
        }


        private Model3DGroup CreateModelGroup(List<Planet> planets, Octree octree)
        {
            Model3DGroup modelGroup = new Model3DGroup();

            foreach (Planet planet in planets) // Отрисовка планет
            {
                MeshUtils.LOD lod = GetLODByDistance(planet);
                if (lod == MeshUtils.LOD.DONT_RENDER) continue;
                GeometryModel3D geometryModel = MeshUtils.CreatePlanetGeometryModel(planet.Color, lod);
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
        private MeshUtils.LOD GetLODByDistance(Planet planet)
        {
            Vector3D dir = planet.Position - (Vector3D)Camera.Position;
            double distanceSqr = Vector3D.DotProduct(dir, dir);
            distanceSqr /= planet.Radius;
            if (distanceSqr < 10000) // LOD 0
            {
                if (Global.showColorLODS)
                    planet.Color = Color.FromArgb(255, 255, 125, 0);
                return MeshUtils.LOD.ZERO;
            }
            else if (distanceSqr >= 10000 && distanceSqr < 90000) // LOD 1
            {
                if (Global.showColorLODS)
                    planet.Color = Color.FromArgb(255, 180, 255, 0);
                return MeshUtils.LOD.ONE;
            }
            else if (distanceSqr >= 90000 && distanceSqr < 1000000) // LOD 2
            {
                if (Global.showColorLODS)
                    planet.Color = Color.FromArgb(255, 50, 180, 180);
                return MeshUtils.LOD.TWO;
            }
            else if (distanceSqr >= 1000000 && distanceSqr < 9000000) // LOD 3
            {
                if (Global.showColorLODS)
                    planet.Color = Color.FromArgb(255, 15, 30, 255);
                return MeshUtils.LOD.THREE;
            }
            else if (distanceSqr >= 9000000 && distanceSqr < 2500000000) // LOD 4
            {
                if (Global.showColorLODS)
                    planet.Color = Color.FromArgb(255, 180, 255, 255);
                return MeshUtils.LOD.FOUR;
            }
            else if (distanceSqr >= 2500000000) // LOD 5 (NO RENDER)
            {
                return MeshUtils.LOD.DONT_RENDER; // Не рендерить планету
            }

            return MeshUtils.LOD.DONT_RENDER; // Не рендерить планету
        }
    }
}
