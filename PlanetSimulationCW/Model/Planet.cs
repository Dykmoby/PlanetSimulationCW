using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSimulationCW.Model
{
    class Planet
    {
        public Vector2 Position { get; private set; }
        private Vector2 a;
        public Vector2 Velocity { get; private set; }
        public float Mass { get; private set; }
        public float Radius { get; private set; }

        public bool Destroyed { get; private set; }

        public Planet(Vector2 position, float mass, float radius)
        {
            Position = position;
            Mass = mass;
            Radius = radius;
            Velocity = new Vector2(0, 0);
        }

        public void Collide(Planet otherPlanet)
        {
            Mass += otherPlanet.Mass;
            Velocity = Velocity + otherPlanet.Velocity * otherPlanet.Mass / Mass;
            otherPlanet.Destroyed = true;
        }

        public void AddVelocity(Vector2 addVelocity)
        {
            Velocity += addVelocity;
        }

        public void SetVelocity(Vector2 setVelocity)
        {
            Velocity = setVelocity;
        }

        public void Move(float deltaTimeSquared)
        {
            Position += new Vector2(Velocity.X * deltaTimeSquared, Velocity.Y * deltaTimeSquared);
        }
    }
}
