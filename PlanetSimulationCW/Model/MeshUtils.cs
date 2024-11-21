using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW
{
    static class MeshUtils
    {
        public enum LOD
        {
            ZERO,
            ONE,
            TWO,
            THREE,
            FOUR,
            DONT_RENDER
        }

        static MeshGeometry3D planetLOD0Mesh = CreatePlanetGeometry(12);
        static MeshGeometry3D planetLOD1Mesh = CreatePlanetGeometry(8);
        static MeshGeometry3D planetLOD2Mesh = CreatePlanetGeometry(5);
        static MeshGeometry3D planetLOD3Mesh = CreatePlanetGeometry(3);
        static MeshGeometry3D planetLOD4Mesh = CreatePlanetGeometry(2);

        public static MeshGeometry3D CreatePlanetGeometry(int lod)
        {
            return CreateSphereMesh(1, lod, (int)(lod * 1.5f + 0.5f));
        }

        public static GeometryModel3D CreatePlanetGeometryModel(Color planetColor, LOD lod)
        {
            //MeshGeometry3D planetMesh = CreatePlanetGeometry(lod);
            MeshGeometry3D planetMesh;
            switch (lod)
            {
                case LOD.ZERO:
                    planetMesh = planetLOD0Mesh.Clone();
                    break;
                case LOD.ONE:
                    planetMesh = planetLOD1Mesh.Clone();
                    break;
                case LOD.TWO:
                    planetMesh = planetLOD2Mesh.Clone();
                    break;
                case LOD.THREE:
                    planetMesh = planetLOD3Mesh.Clone();
                    break;
                case LOD.FOUR:
                    planetMesh = planetLOD4Mesh.Clone();
                    break;
                default:
                    planetMesh = planetLOD4Mesh.Clone();
                    break;
            }

            Material material = new DiffuseMaterial(new SolidColorBrush(planetColor));
            GeometryModel3D planetModel = new GeometryModel3D(planetMesh, material);

            return planetModel;
        }

        public static GeometryModel3D CreateOctantGeometryModel()
        {
            MeshGeometry3D octantMesh = new MeshGeometry3D();
            AddCube(octantMesh);
            //Random rand = new Random();
            Color color = Color.FromArgb(10, 180, 180, 255);
            Material material = new DiffuseMaterial(new SolidColorBrush(color));
            GeometryModel3D octantModel = new GeometryModel3D(octantMesh, material);

            return octantModel;
        }

        public static void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
        {
            int i = mesh.Positions.Count;

            mesh.Positions.Add(point1);
            mesh.Positions.Add(point2);
            mesh.Positions.Add(point3);

            mesh.TriangleIndices.Add(i++);
            mesh.TriangleIndices.Add(i++);
            mesh.TriangleIndices.Add(i);
        }

        public static void AddCube(MeshGeometry3D mesh, double size = 1.0)
        {
            double half = size / 2.0;

            Point3D[] points =
            [
                new Point3D(-half, -half, -half),
                new Point3D(half, -half, -half),
                new Point3D(half, half, -half),
                new Point3D(-half, half, -half),
                new Point3D(-half, -half, half),
                new Point3D(half, -half, half),
                new Point3D(half, half, half),
                new Point3D(-half, half, half)
            ];

            // Передняя грань
            AddTriangle(mesh, points[0], points[1], points[2]);
            AddTriangle(mesh, points[0], points[2], points[3]);

            // Задняя грань
            AddTriangle(mesh, points[4], points[6], points[5]);
            AddTriangle(mesh, points[4], points[7], points[6]);

            // Левая грань
            AddTriangle(mesh, points[0], points[3], points[7]);
            AddTriangle(mesh, points[0], points[7], points[4]);

            // Правая грань
            AddTriangle(mesh, points[1], points[5], points[6]);
            AddTriangle(mesh, points[1], points[6], points[2]);

            // Нижняя грань
            AddTriangle(mesh, points[0], points[4], points[5]);
            AddTriangle(mesh, points[0], points[5], points[1]);

            // Верхняя грань
            AddTriangle(mesh, points[3], points[2], points[6]);
            AddTriangle(mesh, points[3], points[6], points[7]);
        }

        public static MeshGeometry3D CreateSphereMesh(double radius, int rowCount, int columnCount)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            double phi0, theta0;
            double dphi = Math.PI / rowCount;
            double dtheta = 2 * Math.PI / columnCount;

            phi0 = 0;
            double y0 = radius * Math.Cos(phi0);
            double r0 = radius * Math.Sin(phi0);

            for (int i = 0; i < rowCount; i++)
            {
                double phi1 = phi0 + dphi;
                double y1 = radius * Math.Cos(phi1);
                double r1 = radius * Math.Sin(phi1);

                theta0 = 0;
                Point3D pt00 = new Point3D(r0 * Math.Cos(theta0), y0, r0 * Math.Sin(theta0));
                Point3D pt10 = new Point3D(r1 * Math.Cos(theta0), y1, r1 * Math.Sin(theta0));

                for (int j = 0; j < columnCount; j++)
                {
                    double theta1 = theta0 + dtheta;
                    Point3D pt01 = new Point3D(r0 * Math.Cos(theta1), y0, r0 * Math.Sin(theta1));
                    Point3D pt11 = new Point3D(r1 * Math.Cos(theta1), y1, r1 * Math.Sin(theta1));

                    AddTriangle(mesh, pt00, pt11, pt10);
                    AddTriangle(mesh, pt00, pt01, pt11);

                    theta0 = theta1;
                    pt00 = pt01;
                    pt10 = pt11;
                }

                phi0 = phi1;
                y0 = y1;
                r0 = r1;
            }

            return mesh;
        }
    }
}
