using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW
{
    class MeshUtils
    {
        public static MeshGeometry3D CreatePlanetGeometry()
        {
            MeshGeometry3D planetMesh = new MeshGeometry3D();

            planetMesh.Positions.Add(new Point3D(0, 0, 0));
            planetMesh.Positions.Add(new Point3D(1, 0, 0));
            planetMesh.Positions.Add(new Point3D(0.5, 1, 0));

            planetMesh.TriangleIndices.Add(0);
            planetMesh.TriangleIndices.Add(1);
            planetMesh.TriangleIndices.Add(2);

            return planetMesh;
        }

        public static GeometryModel3D CreatePlanetGeometryModel()
        {
            MeshGeometry3D planetMesh = CreatePlanetGeometry();

            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            GeometryModel3D planetModel = new GeometryModel3D(planetMesh, material);

            return planetModel;
        }
    }
}
