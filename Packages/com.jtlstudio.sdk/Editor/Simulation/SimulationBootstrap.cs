using UnityEditor;

namespace JTLStudio.SDK.Editor.Simulation
{
    [InitializeOnLoad]
    public static class SimulationBootstrap
    {
        private static SimulationSession _session;

        static SimulationBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
            {
                StartSession();
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    StartSession();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    StopSession();
                    break;
            }
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
