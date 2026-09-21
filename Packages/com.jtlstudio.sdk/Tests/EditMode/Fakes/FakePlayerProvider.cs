using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakePlayerProvider : IPlayerProvider
    {
        public bool IsAuthorized { get; private set; }
        public string Id => IsAuthorized ? "player-1" : "";
        public string Name => IsAuthorized ? "Tester" : "";
        public string AvatarUrl => "";
        public bool AuthorizationSucceeds { get; set; } = true;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void Authorize(Action<bool> onResult)
        {
            IsAuthorized = AuthorizationSucceeds;
            onResult(AuthorizationSucceeds);
        }
    }
}
