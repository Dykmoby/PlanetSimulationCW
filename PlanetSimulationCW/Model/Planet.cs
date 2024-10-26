using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    class Planet
    {
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float Mass { get; private set; }
        public float Radius { get; private set; }

        public bool Destroyed { get; private set; }

        public Planet(Vector3 position, float mass, float radius)
        {
            Position = position;
            Mass = mass;
            Radius = radius;
            Velocity = new Vector3(0, 0, 0);
        }

        public void Collide(Planet otherPlanet)
        {
            Mass += otherPlanet.Mass;
            Velocity = Velocity + otherPlanet.Velocity * otherPlanet.Mass / Mass;
            otherPlanet.Destroyed = true;
        }

        public void AddVelocity(Vector3 addVelocity)
        {
            Velocity += addVelocity;
        }

        public void SetVelocity(Vector3 setVelocity)
        {
            Velocity = setVelocity;
        }

        public void Move(float deltaTimeSquared)
        {
            Position += new Vector3(Velocity.X * deltaTimeSquared, Velocity.Y * deltaTimeSquared, Velocity.Z * deltaTimeSquared);
        }
    }
}
