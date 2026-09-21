using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public abstract class ModuleBase : IModule
    {
        private readonly List<Action> _readyCallbacks = new List<Action>();
        private readonly SdkLogger _logger;

        protected ModuleBase(SdkLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal event Action<ModuleBase> StateChanged;

        public ModuleState State { get; private set; } = ModuleState.Pending;
        public bool IsSupported => State != ModuleState.Unsupported;
        public bool IsReady => State != ModuleState.Pending;

        internal abstract string ModuleName { get; }

        protected SdkLogger Logger => _logger;

        public void WhenReady(Action onReady)
        {
            if (onReady == null)
            {
                throw new ArgumentNullException(nameof(onReady));
            }

            if (IsReady)
            {
                onReady();
                return;
            }

            _readyCallbacks.Add(onReady);
        }

        internal abstract void Initialize();

        internal virtual void Dispose()
        {
        }

        internal void MarkFailed()
        {
            if (State == ModuleState.Pending)
            {
                SetState(ModuleState.Failed);
            }
        }

        protected void CompleteInitialization(ProviderState providerState)
        {
            switch (providerState)
            {
                case ProviderState.Ready:
                    SetState(ModuleState.Ready);
                    break;

                case ProviderState.Failed:
                    SetState(ModuleState.Failed);
                    break;

                case ProviderState.Unsupported:
                    SetState(ModuleState.Unsupported);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(providerState));
            }
        }

        protected void SetState(ModuleState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(this);

            if (state != ModuleState.Pending)
            {
                DrainReadyCallbacks();
            }
        }

        protected bool RejectIfNotReady(string operation)
        {
            if (IsReady)
            {
                return false;
            }

            _logger.Warning(ModuleName + "." + operation + " was called before the module became ready.");
            return true;
        }

        private void DrainReadyCallbacks()
        {
            List<Action> callbacks = new List<Action>(_readyCallbacks);
            _readyCallbacks.Clear();

            foreach (Action callback in callbacks)
            {
                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }
        }
    }
}
