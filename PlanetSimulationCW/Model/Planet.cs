﻿using System.Numerics;
using System.Windows.Media;

namespace PlanetSimulationCW.Model
{
    class Planet
    {
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float Mass { get; private set; }
        public float Radius { get; private set; }
        public Color Color { get; private set; }

        public bool Destroyed { get; private set; }

        public Planet(Vector3 position, float mass, float radius, Color color)
        {
            Position = position;
            Mass = mass;
            Radius = radius;
            Velocity = new Vector3(0, 0, 0);
            Color = color;
        }

        public void Collide(Planet otherPlanet)
        {
            Mass += otherPlanet.Mass;

            // Объем планеты после столкновения
            float V = (float)(4d / 3d * Math.PI * Math.Pow(Radius, 3) + 4d / 3d * Math.PI * Math.Pow(otherPlanet.Radius, 3));
            Radius = (float)Math.Pow(3d / 4d * V / Math.PI, 1d / 3d);
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
