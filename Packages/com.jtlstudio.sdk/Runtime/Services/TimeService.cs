using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class TimeService : ModuleBase, ITime
    {
        private const float DefaultScale = 1f;

        private readonly ITimeProvider _provider;
        private readonly PauseService _pause;
        private float _scale = DefaultScale;

        public TimeService(ITimeProvider provider, PauseService pause, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _pause.Changed += OnPauseChanged;
        }

        public float Scale
        {
            get => _scale;
            set
            {
                if (value < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _scale = value;
                Apply();
            }
        }

        public DateTimeOffset Now => _provider.Now;
        public bool IsServerTime => State == ModuleState.Ready && _provider.IsServerTime;

        internal override string ModuleName => "Time";

        internal override void Initialize()
        {
            Apply();
            _provider.Initialize(CompleteInitialization);
        }

        internal override void Dispose()
        {
            _pause.Changed -= OnPauseChanged;
            UnityEngine.Time.timeScale = DefaultScale;
        }

        private void Apply()
        {
            UnityEngine.Time.timeScale = _pause.IsPaused ? 0f : _scale;
        }

        private void OnPauseChanged(bool paused)
        {
            Apply();
        }
    }
}
