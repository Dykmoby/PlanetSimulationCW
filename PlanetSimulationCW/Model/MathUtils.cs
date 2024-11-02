﻿namespace PlanetSimulationCW.Model
{
    static class MathUtils
    {
        public static float Linear(float argMin, float argMax, float arg, float valMin, float valMax)
        {
            arg = Math.Clamp(arg, argMin, argMax);
            return valMin + (arg - argMin) / (argMax - argMin) * (valMax - valMin);
        }
    }
}