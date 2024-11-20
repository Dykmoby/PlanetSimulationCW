using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace PlanetSimulationCW.Model
{
    class Ray3DHelper
    {
        public static GeometryModel3D CreateRay(Point3D origin, Vector3D direction, double length = 10,
            double thickness = 0.1, Color? color = null)
        {
            var rayGroup = new Model3DGroup();

            // Normalize direction vector
            direction.Normalize();

            // Calculate end point
            var endPoint = origin + direction * length;

            // Create cylinder geometry for the ray
            var rayMesh = CreateCylinder(origin, endPoint, thickness);

            // Create material
            var material = new DiffuseMaterial(
                new SolidColorBrush(color ?? Color.FromArgb(80, 100, 100, 180)));

            // Create geometric model
            var rayModel = new GeometryModel3D(rayMesh, material);

            return rayModel;
        }

        private static MeshGeometry3D CreateCylinder(Point3D start, Point3D end, double radius)
        {
            var mesh = new MeshGeometry3D();
            var direction = end - start;
            var length = direction.Length;
            direction.Normalize();

            // Calculate perpendicular vectors
            var perpendicular = GetPerpendicularVector(direction);
            perpendicular.Normalize();
            var binormal = Vector3D.CrossProduct(direction, perpendicular);
            binormal.Normalize();

            const int segments = 12; // Number of segments around cylinder

            // Create vertices
            for (int i = 0; i <= segments; i++)
            {
                double angle = i * (2 * Math.PI / segments);
                var cos = Math.Cos(angle);
                var sin = Math.Sin(angle);

                // Calculate points around cylinder
                var circleVector = perpendicular * cos + binormal * sin;

                // Add vertices for start and end caps
                mesh.Positions.Add(start + circleVector * radius);
                mesh.Positions.Add(end + circleVector * radius);
            }

            // Create triangles
            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 2;

                // First triangle
                mesh.TriangleIndices.Add(baseIndex);
                mesh.TriangleIndices.Add(baseIndex + 1);
                mesh.TriangleIndices.Add(baseIndex + 2);

                // Second triangle
                mesh.TriangleIndices.Add(baseIndex + 1);
                mesh.TriangleIndices.Add(baseIndex + 3);
                mesh.TriangleIndices.Add(baseIndex + 2);
            }

            return mesh;
        }

        private static Vector3D GetPerpendicularVector(Vector3D v)
        {
            if (Math.Abs(v.Z) > Math.Abs(v.X))
                return new Vector3D(1, 0, -v.X / v.Z);
            return new Vector3D(-v.Y / v.X, 1, 0);
        }
    }
}
