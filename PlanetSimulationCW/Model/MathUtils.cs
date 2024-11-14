namespace PlanetSimulationCW.Model
{
    static class MathUtils
    {
        public static double Linear(double argMin, double argMax, double arg, double valMin, double valMax)
        {
            arg = Math.Clamp(arg, argMin, argMax);
            return valMin + (arg - argMin) / (argMax - argMin) * (valMax - valMin);
        }
    }
}
