using System;
using JTLStudio.SDK.Prototype;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class SimulationSession : IDisposable
    {
        private const string StyleSheetPath = "Packages/com.jtlstudio.sdk/Editor/Simulation/Simulation.uss";
        private const double RefreshIntervalSeconds = 0.25;

        private readonly GameViewHost _host;
        private readonly PrototypeSimulationSettings _fallbackSettings = new PrototypeSimulationSettings();
        private readonly GameViewOverlay _overlay;
        private readonly PrototypeRequestPresenter _presenter;
        private double _nextRefreshTime;

        public SimulationSession()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            _fallbackSettings.Load();
            _host = new GameViewHost(styleSheet);
            _overlay = new GameViewOverlay(this);
            _presenter = new PrototypeRequestPresenter(_host, this);

            if (_fallbackSettings.OverlayInGameView)
            {
                _host.Attach(_overlay);
            }

            EditorApplication.update += OnEditorUpdate;
        }

        public PrototypeSimulationSettings Settings => PrototypeBridge.ActiveSettings ?? _fallbackSettings;

        public void Dispose()
        {
            EditorApplication.update -= OnEditorUpdate;
            _presenter.Dispose();
            _host.Detach(_overlay);
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup < _nextRefreshTime)
            {
                return;
            }

            _nextRefreshTime = EditorApplication.timeSinceStartup + RefreshIntervalSeconds;
            _host.Refresh();
            _overlay.Refresh();
        }
    }
}
