using UnityEditor;

namespace JTLStudio.SDK.Editor.Simulation
{
    [InitializeOnLoad]
    public static class SimulationBootstrap
    {
        private static SimulationSession _session;

        static SimulationBootstrap()
        {
            EditorApplication.delayCall += StartSession;
            AssemblyReloadEvents.beforeAssemblyReload += StopSession;
        }

        private static void StartSession()
        {
            StopSession();
            _session = new SimulationSession();
        }

        private static void StopSession()
        {
            _session?.Dispose();
            _session = null;
        }
    }
}
