using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedPlayerProvider : IPlayerProvider
    {
        public bool IsAuthorized => false;
        public string Id => "";
        public string Name => "";
        public string AvatarUrl => "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public void Authorize(Action<bool> onResult)
        {
            onResult(false);
        }
    }
}
