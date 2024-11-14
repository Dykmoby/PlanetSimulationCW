using System.Numerics;
using System.Windows.Media;

namespace PlanetSimulationCW.Model
{
    class Simulation
    {
        public const float G = 6.6743015E-11F;
        public const float THETA = 0.4f;
        public const int SPEED_OF_LIGHT = 299792458;
        public List<Planet> planets;
        public Octree octree;
        private float octreeMaxSize = 1000000;

        int frame = 0;

        // Время между кадрами (влияет на скорость симуляции). Пусть это будет постоянным значением пока что.
        private float deltaTime = 0.16f;

        public Simulation(int planetCount)
        {
            octree = new Octree(new Vector3(0, 0, 0), octreeMaxSize);
            planets = new List<Planet>();
            Random rand = new Random();

            for (int i = 0; i < planetCount; i++)
            {
                Vector3 planetPosition = new Vector3(rand.Next(-2000, 2000), rand.Next(-2000, 2000), rand.Next(-2000, 2000));
                float planetRadius = rand.Next(10, 50) / 10.0f;
                float planetMass = (float)(4 / 3 * Math.PI * Math.Pow(planetRadius, 3));
                Color color = Color.FromArgb(255, (byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255));
                //Color color = Color.FromArgb(255, 255, 255, 255);

                planets.Add(new Planet(planetPosition, planetMass, planetRadius, color));
            }
        }

        public void SimulateStep()
        {
            octree.UpdateOctants(planets);

            foreach (Planet planet in planets)
            {
                Vector3 acceleration = octree.Root.GetForce(planet, THETA) / planet.Mass;
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

            frame++;

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
