using System.Numerics;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    class Octree
    {
        public class Node
        {
            public Vector3 center;
            public float size;
            public Node[] children;
            private PlanetAggregate planetsAggregate;
            private List<Planet> planets;

            public Node(Vector3 center, float size)
            {
                this.center = center;
                this.size = size;
                children = new Node[8];
                planetsAggregate = new PlanetAggregate();
                planets = new List<Planet>();
            }

            public bool IsLeaf => children.All(c => c == null);

            public void AddPlanet(Planet planet)
            {
                planetsAggregate.AddParticle(planet);
                planets.Add(planet);
            }

            public void RemovePlanets()
            {
                planets.Clear();
            }

            public Vector3 GetForce(Planet planet, float theta)
            {
                if (IsLeaf)
                {
                    Vector3 force = new Vector3();
                    for (int i = 0; i < planets.Count; i++)
                    {
                        if (planets[i] == planet) continue;
                        if (planets[i].Destroyed) continue;
                        if (planet.Destroyed) break;

                        Vector3 delta = planets[i].Position - planet.Position;
                        Vector3 direction = Vector3.Normalize(delta);
                        float distanceSqr = Vector3.Dot(delta, delta);

                        if (delta.Length() < planets[i].Radius + planet.Radius)
                        {
                            if (planets[i].Mass > planet.Mass)
                            {
                                planet.Collide(planets[i]);
                                continue;
                            }
                            else
                            {
                                planets[i].Collide(planet);
                                break;
                            }
                        }

                        force += 100000000 * direction * Simulation.G * planets[i].Mass * planet.Mass / distanceSqr;
                    }
                    return force;
                }
                if (size / (center - planet.Position).Length() < theta)
                {
                    return planetsAggregate.CalculateForce(planet);
                }

                return GetForceFromSubNodes(planet, theta);
            }

            private Vector3 GetForceFromSubNodes(Planet planet, float theta)
            {
                Vector3 force = new Vector3();

                foreach (Node child in children)
                {
                    if (child == null) continue;
                    force = force + child.GetForce(planet, theta);
                }

                return force;
            }

            private class PlanetAggregate
            {

                private double mass;
                Vector3D centerOfMass = new Vector3D();

                public void AddParticle(Planet planet)
                {
                    Vector3D centerOfMassTemp = new Vector3D(centerOfMass.X * mass + planet.Position.X * planet.Mass,
                                                         centerOfMass.Y * mass + planet.Position.Y * planet.Mass,
                                                         centerOfMass.Z * mass + planet.Position.Z * planet.Mass);
                    mass += planet.Mass;
                    centerOfMass = centerOfMassTemp / mass;
                }

                public Vector3 CalculateForce(Planet planet)
                {
                    Vector3D planetPosition = new Vector3D(planet.Position.X, planet.Position.Y, planet.Position.Z);
                    double distance = (centerOfMass - planetPosition).Length;
                    double product = 100000000 * Simulation.G * mass * planet.Mass / (distance * distance);

                    Vector3D answ = (centerOfMass - planetPosition) * product;

                    //return new Vector3();
                    return new Vector3((float) answ.X, (float) answ.Y, (float) answ.Z);
                }
            }
        }

        private Node root;
        public Node Root { get { return root; } private set { root = value; } }

        private int minSize = 50; // Меньше этого размера пространство не будет подразделяться

        public Octree(Vector3 center, float size)
        {
            root = new Node(center, size);
        }

        public void Insert(Planet planet)
        {
            Insert(root, planet);
        }

        public void Insert(List<Planet> planets)
        {
            foreach (Planet planet in planets)
            {
                Insert(root, planet);
            }
        }

        private void Insert(Node node, Planet planet)
        {
            if (node.size <= minSize)
            {
                node.AddPlanet(planet);
                return;
            }

            node.AddPlanet(planet);

            short octant = GetOctant(node.center, new Vector3(planet.Position.X, planet.Position.Y, planet.Position.Z));
            if (node.children[octant] == null)
            {
                float childSize = node.size / 2;
                Vector3 childCenter = GetChildCenter(node.center, childSize, octant);
                node.children[octant] = new Node(childCenter, childSize);
            }
            Insert(node.children[octant], planet);
        }

        private short GetOctant(Vector3 center, Vector3 point)
        {
            short octant = 0;
            if (point.X >= center.X) octant |= 1;
            if (point.Y >= center.Y) octant |= 2;
            if (point.Z >= center.Z) octant |= 4;
            return octant;
        }

        private Vector3 GetChildCenter(Vector3 parentCenter, float offset, short octant)
        {
            float x = (float)(parentCenter.X + ((octant & 1) == 1 ? offset : -offset));
            float y = (float)(parentCenter.Y + ((octant & 2) == 2 ? offset : -offset));
            float z = (float)(parentCenter.Z + ((octant & 4) == 4 ? offset : -offset));
            return new Vector3(x, y, z);
        }

        public List<Node> GetAllNodes()
        {
            List<Node> nodes = new List<Node>();
            GetAllNodes(root, nodes);
            return nodes;
        }

        private void GetAllNodes(Node node, List<Node> nodes)
        {
            if (node == null) return;

            nodes.Add(node);

            foreach (var child in node.children)
            {
                GetAllNodes(child, nodes);
            }
        }

        public void UpdateOctants(List<Planet> planets) // Обновить октанты, если положения планет изменились
        {
            // Очищаем все октанты
            ClearAllPlanets(root);

            // Повторно добавляем все планеты
            foreach (var planet in planets)
            {
                Insert(planet);
            }
        }

        private void ClearAllPlanets(Node node)
        {
            if (node == null) return;

            node.RemovePlanets();

            for (int i = 0; i < 8; i++)
            {
                ClearAllPlanets(node.children[i]);
                node.children[i] = null;
            }
        }
    }
}
