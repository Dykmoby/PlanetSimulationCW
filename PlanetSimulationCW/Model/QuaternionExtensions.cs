using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    public static class QuaternionExtensions
    {
        // Поворот вектора кватернионом
        public static Vector3D Rotate(this Quaternion q, Vector3D v, double rotationSpeed = 1)
        {
            Quaternion vQuat = new Quaternion(v, rotationSpeed);
            Quaternion resultQuat = q * vQuat * Conjugate(q);
            return new Vector3D(resultQuat.X, resultQuat.Y, resultQuat.Z);
        }

        // Сопряжение кватерниона
        public static Quaternion Conjugate(Quaternion q)
        {
            return new Quaternion(-q.X, -q.Y, -q.Z, q.W);
        }
    }
}
