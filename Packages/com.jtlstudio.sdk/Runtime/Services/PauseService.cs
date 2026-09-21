using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Services
{
    public class PauseService : ModuleBase, IPause
    {
        private readonly HashSet<string> _sources = new HashSet<string>();
        private readonly IPlatformProvider _platformProvider;
        private readonly bool _pauseOnFocusLoss;

        public PauseService(IPlatformProvider platformProvider, bool pauseOnFocusLoss, SdkLogger logger) : base(logger)
        {
            _platformProvider = platformProvider ?? throw new ArgumentNullException(nameof(platformProvider));
            _pauseOnFocusLoss = pauseOnFocusLoss;
            _platformProvider.PauseRequested += OnPlatformPauseRequested;
        }

        public event Action<bool> Changed;

        public bool IsPaused => _sources.Count > 0;
        public IReadOnlyCollection<string> Sources => _sources;

        internal override string ModuleName => "Pause";

        public void Set(string source, bool paused)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException(nameof(source));
            }

            bool wasPaused = IsPaused;
            bool changed = paused ? _sources.Add(source) : _sources.Remove(source);

            if (changed == false || wasPaused == IsPaused)
            {
                return;
            }

            Logger.Info(IsPaused ? "Paused by " + source + "." : "Resumed after " + source + ".");
            Changed?.Invoke(IsPaused);
        }

        public IDisposable Hold(string source)
        {
            Set(source, true);
            return new PauseHold(this, source);
        }

        public void ShowContinuePrompt(Action onContinue = null)
        {
            _platformProvider.ShowContinuePrompt(() => OnContinuePromptClosed(onContinue));
        }

        internal override void Initialize()
        {
            SetState(ModuleState.Ready);
        }

        internal override void Dispose()
        {
            _platformProvider.PauseRequested -= OnPlatformPauseRequested;
        }

        internal void HandleApplicationFocus(bool hasFocus)
        {
            if (_pauseOnFocusLoss == false || Application.isEditor)
            {
                return;
            }

            Set(PauseSources.Focus, hasFocus == false);
        }

        private void OnContinuePromptClosed(Action onContinue)
        {
            Set(PauseSources.Focus, false);
            onContinue?.Invoke();
        }

        private void OnPlatformPauseRequested(bool paused)
        {
            Set(PauseSources.Platform, paused);
        }
    }
}
