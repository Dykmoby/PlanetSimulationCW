﻿using System.Windows.Media;
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

        public double simulationSpeedMultiplier = 100000000000;

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
                if (rand.Next(0, 1000) > 990)
                    planetRadius *= 20;
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
                Vector3D acceleration = octree.Root.GetForce(planet, THETA) / planet.Mass;
                planet.AddVelocity(acceleration * deltaTime);
                planet.Move(deltaTime * simulationSpeedMultiplier);
            }

            for (int i = 0; i < planets.Count; i++)
            {
                if (planets[i].Destroyed == true)
                {
                    planets.RemoveAt(i);
                }
            }
        }
    }
}
