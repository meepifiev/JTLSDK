using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class PlayerService : ModuleBase, IPlayer
    {
        private readonly IPlayerProvider _provider;
        private readonly DataService _data;

        public PlayerService(IPlayerProvider provider, DataService data, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public event Action Authorized;

        public bool IsAuthorized => State == ModuleState.Ready && _provider.IsAuthorized;
        public string Id => State == ModuleState.Ready ? _provider.Id ?? "" : "";
        public string Name => State == ModuleState.Ready ? _provider.Name ?? "" : "";
        public string AvatarUrl => State == ModuleState.Ready ? _provider.AvatarUrl ?? "" : "";

        internal override string ModuleName => "Player";


        public void Authorize(Action<bool> onResult)
        {
            if (onResult == null)
            {
                throw new ArgumentNullException(nameof(onResult));
            }

            if (State != ModuleState.Ready)
            {
                onResult(false);
                return;
            }

            if (_provider.IsAuthorized)
            {
                onResult(true);
                return;
            }

            _provider.Authorize(success => OnAuthorized(success, onResult));
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }

        private void OnAuthorized(bool success, Action<bool> onResult)
        {
            if (success == false)
            {
                onResult(false);
                return;
            }

            _data.ReloadAfterAuthorization(() => OnDataReloaded(onResult));
        }

        private void OnDataReloaded(Action<bool> onResult)
        {
            try
            {
                Authorized?.Invoke();
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }

            onResult(true);
        }
    }
}
