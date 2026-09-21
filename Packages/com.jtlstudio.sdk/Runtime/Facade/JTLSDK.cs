using System;
using UnityEngine;

namespace JTLStudio.SDK
{
    public static class JTLSDK
    {
        public const string Version = "0.1.0";

        private static SdkInstance _instance;

        public static bool IsCreated => _instance != null;
        public static bool IsReady => Instance.IsReady;

        public static IAds Ads => Instance.Ads;
        public static IData Data => Instance.Data;
        public static IPayments Payments => Instance.Payments;
        public static ILanguage Language => Instance.Language;
        public static IPause Pause => Instance.Pause;
        public static ITime Time => Instance.Time;
        public static IAudio Audio => Instance.Audio;
        public static IGameplay Gameplay => Instance.Gameplay;
        public static ILeaderboards Leaderboards => Instance.Leaderboards;
        public static IPlayer Player => Instance.Player;
        public static IFlags Flags => Instance.Flags;
        public static IPlatform Platform => Instance.Platform;
        public static IDevice Device => Instance.Device;
        public static IReview Review => Instance.Review;
        public static IShortcut Shortcut => Instance.Shortcut;

        internal static SdkInstance Current => _instance;

        private static SdkInstance Instance => _instance ?? throw new InvalidOperationException(nameof(Create));

        public static void Create()
        {
            JTLSDKSettings settings = Resources.Load<JTLSDKSettings>(JTLSDKSettings.ResourcePath);

            if (settings == null)
            {
                throw new InvalidOperationException(JTLSDKSettings.ResourcePath);
            }

            Create(settings);
        }

        public static void WhenReady(Action onReady)
        {
            Instance.WhenReady(onReady);
        }

        internal static void Create(JTLSDKSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (_instance != null)
            {
                throw new InvalidOperationException(nameof(Create));
            }

            SdkInstance instance = new SdkInstance(settings);
            _instance = instance;

            try
            {
                instance.Initialize();
            }
            catch
            {
                _instance = null;
                instance.Dispose();
                throw;
            }
        }

        internal static void Destroy()
        {
            SdkInstance instance = _instance;
            _instance = null;
            instance?.Dispose();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }
    }
}
