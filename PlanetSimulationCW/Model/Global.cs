using PlanetSimulationCW.ViewModel;

namespace PlanetSimulationCW.Model
{
    static class Global
    {
        public static bool simulationStopped = false;
        public static bool fpsLocked = false;
        public static bool showColorLODS = false;
        public static bool followPlanet = false;
        public static long targetFrameTime;
        public static ControlPanelVM controlPanelVM;
        public static ApplicationContext? db;
        public static Action? setCameraDeltaPos;
        public static Action? hideControlPanel;
    }
}
