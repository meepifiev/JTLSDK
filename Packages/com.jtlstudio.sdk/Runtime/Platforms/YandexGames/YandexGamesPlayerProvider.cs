using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesPlayerProvider : BridgeProviderBase, IPlayerProvider
    {
        public bool IsAuthorized { get; private set; }
        public string Id { get; private set; } = "";
        public string Name { get; private set; } = "";
        public string AvatarUrl { get; private set; } = "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("player", "info", onInitialized, ApplyInfo);
        }

        public void Authorize(Action<bool> onResult)
        {
            Call("player", "authorize", null, response =>
            {
                if (response.IsSuccess)
                {
                    ApplyInfo(response);
                }

                onResult(IsAuthorized);
            });
        }

        private void ApplyInfo(BridgeResponse response)
        {
            IsAuthorized = response.GetBool("authorized");
            Id = response.GetString("id");
            Name = response.GetString("name");
            AvatarUrl = response.GetString("avatar");
        }
    }
}
