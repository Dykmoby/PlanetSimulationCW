using System.Numerics;

namespace PlanetSimulationCW.Model
{
    class Simulation
    {
        const double G = 6.6743015E-11F;
        const int SPEED_OF_LIGHT = 299792458;
        public List<Planet> planets;

        // Время между кадрами (влияет на скорость симуляции). Пусть это будет постоянным значением пока что.
        private float deltaTime = 0.16f;

        public Simulation(int planetCount)
        {
            planets = new List<Planet>();
            Random rand = new Random();

            for (int i = 0; i < planetCount; i++)
            {
                Vector3 planetPosition = new Vector3(rand.Next(0, 200), rand.Next(0, 200), rand.Next(0, 200));
                float planetRadius = rand.Next(10, 40) / 10.0f;
                float planetMass = (float) (Math.PI * planetRadius * planetRadius);

                planets.Add(new Planet(planetPosition, planetMass, planetRadius));
            }
        }

        public void SimulateStep()
        {
            float deltaTimeSquared = deltaTime * deltaTime;
            for (int i = 0; i < planets.Count; i++)
            {
                if (planets[i].Destroyed)
                {
                    continue;
                }
                for (int k = 0; k < planets.Count; k++)
                {
                    if (i == k || planets[k].Destroyed)
                    {
                        continue;
                    }

                    Vector3 delta = planets[k].Position - planets[i].Position;

                    float distanceSqr = Vector3.Dot(delta, delta);

                    // Collision
                    if (distanceSqr < (planets[i].Radius + planets[k].Radius) * (planets[i].Radius + planets[k].Radius))
                    {
                        if (planets[i].Mass > planets[k].Mass)
                        {
                            planets[i].Collide(planets[k]);
                            continue;
                        }
                        else
                        {
                            planets[k].Collide(planets[i]);
                            break;
                        }
                    }

                    float invDistanceSqr = 1.0f / distanceSqr;
                    Vector3 direction = Vector3.Normalize(delta);

                    float velocity = (float)(10000000000 * G * planets[k].Mass * invDistanceSqr);

                    planets[i].AddVelocity(direction * velocity);

                    if (planets[i].Velocity.Length() >= SPEED_OF_LIGHT)
                    {
                        planets[i].SetVelocity(direction * (SPEED_OF_LIGHT - 100));
                    }
                }
            }

            for (int i = 0; i < planets.Count; i++)
            {
                planets[i].Move(deltaTimeSquared);
            }
        }
    }
}
