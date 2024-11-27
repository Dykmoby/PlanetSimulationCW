using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    class Simulation
    {
        private static Simulation instance;
        public static Simulation Instance
        {
            get
            {
                instance ??= new Simulation();
                return instance;
            }
        }

        public const double G = 6.6743015E-11;
        public const double THETA = 0.35;
        public const int SPEED_OF_LIGHT = 299792458;
        public List<Planet> planets;
        public Octree octree;
        private double octreeMaxSize = 1000000;

        private double simulationSpeedMultiplier = 100000000000;

        public Simulation()
        {
            octree = new Octree(new Vector3D(0, 0, 0), octreeMaxSize);
            planets = new List<Planet>();
        }

        public void AddPlanets(int planetCount)
        {
            Random rand = new Random();
            for (int i = 0; i < planetCount; i++)
            {
                Vector3D planetPosition = new Vector3D(rand.Next(-2000, 2000), rand.Next(-2000, 2000), rand.Next(-2000, 2000));
                double planetRadius = rand.Next(10, 50) / 10.0f;
                double planetMass = 4 / 3 * Math.PI * Math.Pow(planetRadius, 3);
                Color color = Color.FromArgb(255, (byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255));
                //Color color = Color.FromArgb(255, 255, 255, 255);

                planets.Add(new Planet(planetPosition, planetMass, planetRadius, color));
            }
        }

        public void LoadPlanetsFromDB(bool clearExistingPlanets = true)
        {
            if (clearExistingPlanets)
            {
                planets.Clear();
            }

            List<PlanetDBEntry> planetDBEntries = Global.db!.Planets.Local.ToList();
            foreach (PlanetDBEntry planetDBEntry in planetDBEntries)
            {
                planets.Add(new Planet(planetDBEntry));
            }
        }

        public void SimulateStep(double deltaTime)
        {
            if (Global.simulationStopped) return;

            octree.UpdateOctants(planets);

            foreach (Planet planet in planets)
            {
                Vector3D acceleration = octree.Root.GetForce(planet, THETA) / planet.Mass * simulationSpeedMultiplier;
                planet.AddVelocity(acceleration * deltaTime);
                planet.Move(deltaTime);
            }

            for (int i = 0; i < planets.Count; i++)
            {
                if (planets[i].Destroyed == true)
                {
                    planets.RemoveAt(i);
                }
            }


            //float deltaTimeSquared = deltaTime * deltaTime;
            //for (int i = 0; i < planets.Count; i++)
            //{
            //    if (planets[i].Destroyed)
            //    {
            //        continue;
            //    }
            //    for (int k = 0; k < planets.Count; k++)
            //    {
            //        if (i == k || planets[k].Destroyed)
            //        {
            //            continue;
            //        }

            //        Vector3 delta = planets[k].Position - planets[i].Position;

            //        float distanceSqr = Vector3.Dot(delta, delta);

            //        // Collision
            //        if (distanceSqr < (planets[i].Radius + planets[k].Radius) * (planets[i].Radius + planets[k].Radius))
            //        {
            //            if (planets[i].Mass > planets[k].Mass)
            //            {
            //                planets[i].Collide(planets[k]);
            //                continue;
            //            }
            //            else
            //            {
            //                planets[k].Collide(planets[i]);
            //                break;
            //            }
            //        }

            //        float invDistanceSqr = 1.0f / distanceSqr;
            //        Vector3 direction = Vector3.Normalize(delta);

            //        float acceleration = (float)(5000000000 * G * planets[k].Mass * invDistanceSqr);

            //        planets[i].AddAcceleration(direction * acceleration);

            //        if (planets[i].Acceleration.Length() >= SPEED_OF_LIGHT)
            //        {
            //            planets[i].SetAcceleration(direction * (SPEED_OF_LIGHT - 100));
            //        }
            //    }
            //}

            //for (int i = 0; i < planets.Count; i++)
            //{
            //    if (planets[i].Destroyed == true)
            //    {
            //        planets.RemoveAt(i);
            //    }
            //}

            //for (int i = 0; i < planets.Count; i++)
            //{
            //    planets[i].Move(deltaTimeSquared);
            //}
        }
    }
}
