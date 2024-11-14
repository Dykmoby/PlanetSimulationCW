using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    class Planet
    {
        public Vector3D Position { get; private set; }
        public Vector3D Velocity { get; private set; }
        public double Mass { get; private set; }
        public double Radius { get; private set; }
        public Color Color { get; private set; }

        public bool Destroyed { get; private set; }

        public Planet(Vector3D position, double mass, double radius, Color color)
        {
            Position = position;
            Mass = mass;
            Radius = radius;
            Velocity = new Vector3D(0, 0, 0);
            Color = color;
        }

        public void Collide(Planet otherPlanet)
        {
            Mass += otherPlanet.Mass;

            // Объем планеты после столкновения
            double V = (4d / 3d * Math.PI * Math.Pow(Radius, 3) + 4d / 3d * Math.PI * Math.Pow(otherPlanet.Radius, 3));
            Radius = Math.Pow(3d / 4d * V / Math.PI, 1d / 3d);
            Velocity = Velocity + otherPlanet.Velocity * otherPlanet.Mass / Mass;
            otherPlanet.Destroyed = true;
        }

        public void AddVelocity(Vector3D addVelocity)
        {
            Velocity += addVelocity;
        }

        public void SetVelocity(Vector3D setVelocity)
        {
            Velocity = setVelocity;
        }

        public void Move(double deltaTime)
        {
            Position += Velocity * deltaTime;
        }
    }
}
