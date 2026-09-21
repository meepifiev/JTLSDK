using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesPlayerProvider : IPlayerProvider
    {
        public bool IsAuthorized => false;
        public string Id => "";
        public string Name => "";
        public string AvatarUrl => "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }

        public void Authorize(Action<bool> onResult)
        {
            onResult(false);
        }
    }
}
