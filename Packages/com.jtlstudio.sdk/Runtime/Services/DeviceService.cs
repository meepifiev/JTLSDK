using System;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Services
{
    public class DeviceService : ModuleBase, IDevice
    {
        private readonly IPlatformProvider _platformProvider;
        private readonly PauseService _pause;
        private bool _cursorVisible = true;
        private CursorLockMode _cursorLock = CursorLockMode.None;

        public DeviceService(IPlatformProvider platformProvider, PauseService pause, SdkLogger logger) : base(logger)
        {
            _platformProvider = platformProvider ?? throw new ArgumentNullException(nameof(platformProvider));
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _pause.Changed += OnPauseChanged;
        }

        public bool IsMobile => Type == DeviceType.Mobile || Type == DeviceType.Tablet;
        public DeviceType Type => _platformProvider.DeviceType;

        public bool CursorVisible
        {
            get => _cursorVisible;
            set
            {
                _cursorVisible = value;
                Apply();
            }
        }

        public CursorLockMode CursorLock
        {
            get => _cursorLock;
            set
            {
                _cursorLock = value;
                Apply();
            }
        }

        internal override string ModuleName => "Device";

        internal override void Initialize()
        {
            SetState(ModuleState.Ready);
        }

        internal override void Dispose()
        {
            _pause.Changed -= OnPauseChanged;
        }

        private void Apply()
        {
            if (_pause.IsPaused)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return;
            }

            Cursor.visible = _cursorVisible;
            Cursor.lockState = _cursorLock;
        }

        private void OnPauseChanged(bool paused)
        {
            Apply();
        }
    }
}
