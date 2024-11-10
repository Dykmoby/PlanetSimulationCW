using System.Numerics;
using System.Windows.Media;

namespace PlanetSimulationCW.Model
{
    class Planet
    {
        public Vector3 Position { get; private set; }
        public Vector3 Acceleration { get; private set; }
        public float Mass { get; private set; }
        public float Radius { get; private set; }
        public Color Color { get; private set; }

        public bool Destroyed { get; private set; }

        public Planet(Vector3 position, float mass, float radius, Color color)
        {
            Position = position;
            Mass = mass;
            Radius = radius;
            Acceleration = new Vector3(0, 0, 0);
            Color = color;
        }

        public void Collide(Planet otherPlanet)
        {
            Mass += otherPlanet.Mass;

            // Объем планеты после столкновения
            float V = (float)(4d / 3d * Math.PI * Math.Pow(Radius, 3) + 4d / 3d * Math.PI * Math.Pow(otherPlanet.Radius, 3));
            Radius = (float)Math.Pow(3d / 4d * V / Math.PI, 1d / 3d);
            Acceleration = Acceleration + otherPlanet.Acceleration * otherPlanet.Mass / Mass;
            otherPlanet.Destroyed = true;
        }

        public void AddAcceleration(Vector3 addAcceleration)
        {
            Acceleration += addAcceleration;
        }

        public void SetAcceleration(Vector3 setAcceleration)
        {
            Acceleration = setAcceleration;
        }

        public void Move(float deltaTimeSquared)
        {
            Position += Acceleration * deltaTimeSquared;
        }
    }
}
