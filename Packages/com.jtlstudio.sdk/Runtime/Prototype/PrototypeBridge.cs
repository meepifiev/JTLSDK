#if UNITY_EDITOR
using System;

namespace JTLStudio.SDK.Prototype
{
    public static class PrototypeBridge
    {
        public static event Action<PrototypeAdRequest> AdRequested;
        public static event Action<PrototypePurchaseRequest> PurchaseRequested;
        public static event Action PlatformChanged;

        public static PrototypePlatformProvider ActivePlatform { get; private set; }
        public static PrototypeSimulationSettings ActiveSettings => ActivePlatform?.Settings;

        public static bool HasAdPresenter => AdRequested != null;
        public static bool HasPurchasePresenter => PurchaseRequested != null;

        internal static void RequestAd(PrototypeAdRequest request)
        {
            AdRequested?.Invoke(request);
        }

        internal static void RequestPurchase(PrototypePurchaseRequest request)
        {
            PurchaseRequested?.Invoke(request);
        }

        internal static void RegisterPlatform(PrototypePlatformProvider platform)
        {
            ActivePlatform = platform;
            PlatformChanged?.Invoke();
        }

        internal static void UnregisterPlatform(PrototypePlatformProvider platform)
        {
            if (ActivePlatform != platform)
            {
                return;
            }

            ActivePlatform = null;
            PlatformChanged?.Invoke();
        }
    }
}
#endif
