using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    public static class QuaternionExtensions
    {
        // Вспомогательный метод расширения для поворота вектора кватернионом
        public static Vector3D Rotate(this Quaternion q, Vector3D v, double rotationSpeed = 1)
        {
            Quaternion vQuat = new Quaternion(v, rotationSpeed);
            Quaternion resultQuat = q * vQuat * Conjugate(q);
            return new Vector3D(resultQuat.X, resultQuat.Y, resultQuat.Z);
        }

        // Метод расширения для сопряжения кватерниона
        public static Quaternion Conjugate(Quaternion q)
        {
            return new Quaternion(-q.X, -q.Y, -q.Z, q.W);
        }
    }
}
