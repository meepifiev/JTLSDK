using System;
using System.Runtime.InteropServices;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.YandexMetrica
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexMetricaAnalyticsProvider : IAnalyticsProvider
    {
        [SerializeField] private int _counterId;
        [SerializeField] private bool _webvisor;
        [SerializeField] private bool _clickmap = true;
        [SerializeField] private bool _trackLinks = true;
        [SerializeField] private bool _accurateTrackBounce = true;

#if UNITY_WEBGL && !UNITY_EDITOR && JTLSDK_YANDEX_GAMES
        [DllImport("__Internal")] private static extern int JTLSDK_Metrica_Initialize(int counterId, int options);
        [DllImport("__Internal")] private static extern void JTLSDK_Metrica_ReachGoal(int counterId, string goal, string parametersJson);
#endif

        private int Options => (_webvisor ? 1 : 0) | (_clickmap ? 2 : 0) | (_trackLinks ? 4 : 0) | (_accurateTrackBounce ? 8 : 0);

        public void Initialize(Action<ProviderState> onInitialized)
        {
            if (onInitialized == null)
            {
                throw new ArgumentNullException(nameof(onInitialized));
            }

            if (_counterId <= 0)
            {
                onInitialized(ProviderState.Failed);
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR && JTLSDK_YANDEX_GAMES
            bool started = JTLSDK_Metrica_Initialize(_counterId, Options) == 1;
            onInitialized(started ? ProviderState.Ready : ProviderState.Unsupported);
#else
            onInitialized(ProviderState.Unsupported);
#endif
        }

        public void Report(string eventName, string parametersJson)
        {
#if UNITY_WEBGL && !UNITY_EDITOR && JTLSDK_YANDEX_GAMES
            JTLSDK_Metrica_ReachGoal(_counterId, eventName, parametersJson ?? "");
#endif
        }
    }
}
