using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesTimeProvider : BridgeProviderBase, ITimeProvider
    {
        private DateTimeOffset _serverAtSync;
        private DateTimeOffset _localAtSync;

        public bool IsServerTime { get; private set; }
        public DateTimeOffset Now => IsServerTime ? _serverAtSync + (DateTimeOffset.UtcNow - _localAtSync) : DateTimeOffset.Now;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("time", "server", onInitialized, response =>
            {
                long milliseconds = response.GetLong("milliseconds");

                if (milliseconds <= 0)
                {
                    return;
                }

                _serverAtSync = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                _localAtSync = DateTimeOffset.UtcNow;
                IsServerTime = true;
            });
        }
    }
}
