using System.Numerics;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    class Octree
    {
        public class Node
        {
            public Vector3D center;
            public double size;
            public Node[] children;
            private PlanetAggregate planetsAggregate;
            public List<Planet> planets;

            public Node(Vector3D center, double size)
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

            public Vector3D GetForce(Planet planet, double theta)
            {
                if (IsLeaf)
                {
                    Vector3D force = new Vector3D();
                    for (int i = 0; i < planets.Count; i++)
                    {
                        if (planets[i] == planet) continue;
                        if (planets[i].Destroyed) continue;
                        if (planet.Destroyed) break;

                        Vector3D direction = planets[i].Position - planet.Position;
                        double distanceSqr = Vector3D.DotProduct(direction, direction);
                        direction.Normalize();

                        if (distanceSqr < Math.Pow(planets[i].Radius + planet.Radius, 2))
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

                        force += direction * Simulation.G * planets[i].Mass * planet.Mass / distanceSqr;
                    }
                    return force;
                }
                if (size / (center - planet.Position).Length < theta)
                {
                    return planetsAggregate.CalculateForce(planet);
                }

                return GetForceFromSubNodes(planet, theta);
            }

            private Vector3D GetForceFromSubNodes(Planet planet, double theta)
            {
                Vector3D force = new Vector3D();

                foreach (Node child in children)
                {
                    if (child == null) continue;
                    force += child.GetForce(planet, theta);
                }

                return force;
            }

            private class PlanetAggregate
            {

                private double mass;
                Vector3D centerOfMass = new Vector3D();

                public void AddParticle(Planet planet)
                {
                    Vector3D centerOfMassTemp = centerOfMass * mass + planet.Position * planet.Mass;
                    mass += planet.Mass;
                    centerOfMass = centerOfMassTemp / mass;
                }

                public Vector3D CalculateForce(Planet planet)
                {
                    double distance = (centerOfMass - planet.Position).Length;
                    double product = Simulation.G * mass * planet.Mass / (distance * distance);

                    return (centerOfMass - planet.Position) * product;
                }
            }
        }

        private Node root;
        public Node Root { get { return root; } private set { root = value; } }

        private int minSize = 50; // Меньше этого размера пространство не будет подразделяться

        public Octree(Vector3D center, double size)
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

            short octant = GetOctant(node.center, new Vector3D(planet.Position.X, planet.Position.Y, planet.Position.Z));
            if (node.children[octant] == null)
            {
                double childSize = node.size / 2;
                Vector3D childCenter = GetChildCenter(node.center, childSize, octant);
                node.children[octant] = new Node(childCenter, childSize);
            }
            Insert(node.children[octant], planet);
        }

        private short GetOctant(Vector3D center, Vector3D point)
        {
            short octant = 0;
            if (point.X >= center.X) octant |= 1;
            if (point.Y >= center.Y) octant |= 2;
            if (point.Z >= center.Z) octant |= 4;
            return octant;
        }

        private Vector3D GetChildCenter(Vector3D parentCenter, double offset, short octant)
        {
            double x = parentCenter.X + ((octant & 1) == 1 ? offset : -offset);
            double y = parentCenter.Y + ((octant & 2) == 2 ? offset : -offset);
            double z = parentCenter.Z + ((octant & 4) == 4 ? offset : -offset);
            return new Vector3D(x, y, z);
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

        public List<Planet> FindNearestPlanets(Vector3D point, double radius)  // Возвращает ближайшие к зададанной точке и в заданном радиусе от этой точки планеты
        {
            List<Planet> result = new List<Planet>();
            FindNearestPlanets(root, point, radius, result);
            return result;
        }

        private void FindNearestPlanets(Node node, Vector3D point, double radius, List<Planet> result)
        {
            if (!IntersectsSphere(node.center, node.size, point, radius))
                return;

            if (node.IsLeaf)
            {
                foreach (Planet planet in node.planets)
                {
                    if ((point - planet.Position).Length <= radius)
                        result.Add(planet);
                }
            }
            else
            {
                foreach (Node child in node.children)
                {
                    if (child != null)
                        FindNearestPlanets(child, point, radius, result);
                }
            }
        }

        private bool IntersectsSphere(Vector3D octantCenter, double octantSize, Vector3D sphereCenter, double sphereRadius)
        {
            // Находим самую ближайшую точку к центру сферы на октанте
            double closestX = Math.Clamp(sphereCenter.X, octantCenter.X - octantSize, octantCenter.X + octantSize);
            double closestY = Math.Clamp(sphereCenter.Y, octantCenter.Y - octantSize, octantCenter.Y + octantSize);
            double closestZ = Math.Clamp(sphereCenter.Z, octantCenter.Z - octantSize, octantCenter.Z + octantSize);

            // Расстояние от этой точки до центра сферы
            double distanceSquared = (closestX - sphereCenter.X) * (closestX - sphereCenter.X)
                                   + (closestY - sphereCenter.Y) * (closestY - sphereCenter.Y)
                                   + (closestZ - sphereCenter.Z) * (closestZ - sphereCenter.Z);

            return distanceSquared <= sphereRadius * sphereRadius;
        }
    }
}
