using System;
using System.IO;
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
        private DateTime _settingsWriteTime;
        private bool _overlayAttached;

        public SimulationSession()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            LoadFallbackSettings();
            _host = new GameViewHost(styleSheet);
            _overlay = new GameViewOverlay(this);
            _presenter = new PrototypeRequestPresenter(_host, this);
            UpdateOverlayAttachment();
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

            if (File.Exists(PrototypeSimulationSettings.FilePath) && File.GetLastWriteTimeUtc(PrototypeSimulationSettings.FilePath) != _settingsWriteTime)
            {
                LoadFallbackSettings();
            }

            UpdateOverlayAttachment();
            _host.Refresh();
            _overlay.Refresh();
        }

        private void LoadFallbackSettings()
        {
            _fallbackSettings.Load();
            _settingsWriteTime = File.Exists(PrototypeSimulationSettings.FilePath) ? File.GetLastWriteTimeUtc(PrototypeSimulationSettings.FilePath) : default;
        }

        private void UpdateOverlayAttachment()
        {
            bool visible = Settings.OverlayInGameView;

            if (visible == _overlayAttached)
            {
                return;
            }

            _overlayAttached = visible;

            if (visible)
            {
                _host.Attach(_overlay);
            }
            else
            {
                _host.Detach(_overlay);
            }
        }
    }
}
