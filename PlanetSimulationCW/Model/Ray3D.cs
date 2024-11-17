using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    class Ray3D
    {
        public Vector3D Origin { get; private set; }
        public Vector3D Direction { get; private set; }

        public Ray3D(Vector3D origin, Vector3D direction)
        {
            Origin = origin;
            Direction = direction;
            Direction.Normalize();
        }
    }
}
