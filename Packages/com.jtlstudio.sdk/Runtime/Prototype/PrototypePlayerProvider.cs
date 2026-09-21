#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypePlayerProvider : IPlayerProvider
    {
        private readonly PrototypeSimulationSettings _settings;
        private bool _authorized;

        public PrototypePlayerProvider(PrototypeSimulationSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool IsAuthorized => _authorized;
        public string Id => _settings.PlayerId;
        public string Name => _settings.PlayerName;
        public string AvatarUrl => "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            _authorized = _settings.Authorized;
            onInitialized(ProviderState.Ready);
        }

        public void Authorize(Action<bool> onResult)
        {
            _authorized = true;
            onResult(true);
        }
    }
}
#endif
