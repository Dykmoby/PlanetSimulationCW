using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    class Octree
    {
        public class Node
        {
            public Point3D center;
            public double size;
            public List<Planet> planets;
            public Node[] children;

            public Node(Point3D center, double size)
            {
                this.center = center;
                this.size = size;
                planets = new List<Planet>();
                children = new Node[8];
            }

            public bool IsLeaf => children.All(c => c == null);
        }

        private Node root;
        public Node Root { get { return root; } private set { root = value; } }

        private int minSize = 5; // Меньше этого размера пространство не будет подразделяться

        public Octree(Point3D center, double size)
        {
            root = new Node(center, size);
        }

        public void Insert(Planet planet)
        {
            Insert(root, planet);
        }

        private void Insert(Node node, Planet planet)
        {
            if (node.size <= minSize)
            {
                node.planets.Add(planet);
                return;
            }

            short octant = GetOctant(node.center, new Point3D(planet.Position.X, planet.Position.Y, planet.Position.Z));
            if (node.children[octant] == null)
            {
                double childSize = node.size / 2;
                Point3D childCenter = GetChildCenter(node.center, childSize, octant);
                node.children[octant] = new Node(childCenter, childSize);
            }

            Insert(node.children[octant], planet);
        }

        private short GetOctant(Point3D center, Point3D point)
        {
            short octant = 0;
            if (point.X >= center.X) octant |= 1;
            if (point.Y >= center.Y) octant |= 2;
            if (point.Z >= center.Z) octant |= 4;
            return octant;
        }

        private Point3D GetChildCenter(Point3D parentCenter, double offset, short octant)
        {
            double x = parentCenter.X + ((octant & 1) == 1 ? offset : -offset);
            double y = parentCenter.Y + ((octant & 2) == 2 ? offset : -offset);
            double z = parentCenter.Z + ((octant & 4) == 4 ? offset : -offset);
            return new Point3D(x, y, z);
        }

        public List<Planet> GetPlanetsInOctant(short octant)
        {
            return GetPlanetsInOctant(root, octant);
        }

        private List<Planet> GetPlanetsInOctant(Node node, short octant)
        {
            if (node == null) return new List<Planet>();
            if (node.IsLeaf) return node.planets;

            List<Planet> result = new List<Planet>();
            if (node.children[octant] != null)
            {
                result.AddRange(GetPlanetsInOctant(node.children[octant], octant));
            }
            return result;
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

        public void UpdateOctants() // Обновить октанты, если положения планет изменились
        {
            // Сохраняем список всех планет до очистки
            List<Planet> planets = GetAllPlanets();

            // Затем очищаем все октанты
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

            node.planets.Clear();

            for (int i = 0; i < 8; i++)
            {
                ClearAllPlanets(node.children[i]);
                node.children[i] = null;
            }
        }

        public List<Planet> GetAllPlanets()
        {
            List<Planet> planets = new List<Planet>();
            CollectPlanets(root, planets);
            return planets;
        }

        private void CollectPlanets(Node node, List<Planet> planets)
        {
            if (node == null) return;

            // Добавляем планеты из текущего узла
            planets.AddRange(node.planets);

            // Рекурсивно проходимся по каждому потомку
            foreach (var child in node.children)
            {
                CollectPlanets(child, planets);
            }
        }

        public bool RemovePlanet(Planet planet)
        {
            return RemovePlanet(root, planet);
        }

        private bool RemovePlanet(Node node, Planet planet)
        {
            if (node == null) return false;

            if (node.planets.Contains(planet))
            {
                node.planets.Remove(planet);
                return true;
            }
            short octant = GetOctant(node.center, new Point3D(planet.Position.X, planet.Position.Y, planet.Position.Z));
            if (RemovePlanet(node.children[octant], planet))
            {
                return true;
            }
            return false;
        }
    }
}
