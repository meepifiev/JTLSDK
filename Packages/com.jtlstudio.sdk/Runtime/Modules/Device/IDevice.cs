using UnityEngine;

namespace JTLStudio.SDK
{
    public interface IDevice : IModule
    {
        bool IsMobile { get; }
        DeviceType Type { get; }
        bool CursorVisible { get; set; }
        CursorLockMode CursorLock { get; set; }
    }
}
