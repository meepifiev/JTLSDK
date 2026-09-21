using System;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Services
{
    public class AudioService : ModuleBase, IAudio
    {
        private const float DefaultVolume = 1f;

        private readonly IPlatformProvider _platformProvider;
        private readonly PauseService _pause;
        private float _volume = DefaultVolume;

        public AudioService(IPlatformProvider platformProvider, PauseService pause, SdkLogger logger) : base(logger)
        {
            _platformProvider = platformProvider ?? throw new ArgumentNullException(nameof(platformProvider));
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _pause.Changed += OnPauseChanged;
            _platformProvider.PlatformMuteChanged += OnPlatformMuteChanged;
        }

        public event Action<bool> PlatformMuteChanged;

        public float Volume
        {
            get => _volume;
            set
            {
                if (value < 0f || value > 1f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _volume = value;
                Apply();
            }
        }

        public bool IsPlatformMuted => _platformProvider.SupportsPlatformMute && _platformProvider.IsPlatformMuted;

        internal override string ModuleName => "Audio";

        internal override void Initialize()
        {
            Apply();
            SetState(ModuleState.Ready);
        }

        internal override void Dispose()
        {
            _pause.Changed -= OnPauseChanged;
            _platformProvider.PlatformMuteChanged -= OnPlatformMuteChanged;
            AudioListener.pause = false;
            AudioListener.volume = DefaultVolume;
        }

        private void Apply()
        {
            bool silenced = _pause.IsPaused || IsPlatformMuted;
            AudioListener.pause = _pause.IsPaused;
            AudioListener.volume = silenced ? 0f : _volume;
        }

        private void OnPauseChanged(bool paused)
        {
            Apply();
        }

        private void OnPlatformMuteChanged(bool muted)
        {
            Apply();
            PlatformMuteChanged?.Invoke(muted);
        }
    }
}
