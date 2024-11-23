namespace PlanetSimulationCW.Model
{
    static class Global
    {
        public static bool simulationStopped = false;
        public static bool fpsLocked = false;
        public static long targetFrameTime;
        public static ControlPanelWindow? controlPanelWindow;
        public static ApplicationContext? db;
    }
}
