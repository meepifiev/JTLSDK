using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace JTLStudio.SDK.Bridge
{
    public class WebBridge
    {
        private delegate void MessageCallback(int requestId, int code, string payload);

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int JTLSDK_Select(string platform);
        [DllImport("__Internal")] private static extern int JTLSDK_IsAvailable();
        [DllImport("__Internal")] private static extern void JTLSDK_Register(MessageCallback callback);
        [DllImport("__Internal")] private static extern void JTLSDK_Call(string module, string action, string payload, int requestId);
        [DllImport("__Internal")] private static extern string JTLSDK_Query(string module, string action, string payload);
#endif

        private static WebBridge _current;

        private readonly Dictionary<int, Action<BridgeResponse>> _pending = new Dictionary<int, Action<BridgeResponse>>();
        private readonly SdkLogger _logger;
        private readonly string _platform;
        private int _nextRequestId = 1;
        private bool _connected;
        private bool _selected;

        public WebBridge(SdkLogger logger, PlatformId platform)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _platform = PlatformKey(platform);
        }

        public event Action<BridgeEventCode, BridgeResponse> EventReceived;

        public bool IsAvailable
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                Select();
                return JTLSDK_IsAvailable() == 1;
#else
                return false;
#endif
            }
        }

        private void Select()
        {
            if (_selected || string.IsNullOrEmpty(_platform))
            {
                return;
            }

            _selected = true;
#if UNITY_WEBGL && !UNITY_EDITOR
            JTLSDK_Select(_platform);
#endif
        }

        private string PlatformKey(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.YandexGames:
                    return "yandex";

                case PlatformId.YouTubePlayables:
                    return "youtube";

                default:
                    return "";
            }
        }

        public void Connect()
        {
            if (_connected || IsAvailable == false)
            {
                return;
            }

            _connected = true;
            _current = this;
#if UNITY_WEBGL && !UNITY_EDITOR
            JTLSDK_Register(OnMessage);
#endif
        }

        public void Disconnect()
        {
            if (_current == this)
            {
                _current = null;
            }

            _pending.Clear();
            _connected = false;
        }

        public void Call(string module, string action, BridgePayload payload, Action<BridgeResponse> onResponse)
        {
            if (string.IsNullOrEmpty(module))
            {
                throw new ArgumentException(nameof(module));
            }

            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentException(nameof(action));
            }

            if (_connected == false)
            {
                onResponse?.Invoke(new BridgeResponse(BridgeResultCode.Unavailable, ""));
                return;
            }

            int requestId = _nextRequestId++;

            if (onResponse != null)
            {
                _pending[requestId] = onResponse;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            JTLSDK_Call(module, action, payload == null ? "" : payload.ToString(), requestId);
#else
            _pending.Remove(requestId);
            onResponse?.Invoke(new BridgeResponse(BridgeResultCode.Unavailable, ""));
#endif
        }

        public BridgeResponse Query(string module, string action, BridgePayload payload)
        {
            if (_connected == false)
            {
                return new BridgeResponse(BridgeResultCode.Unavailable, "");
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            string raw = JTLSDK_Query(module, action, payload == null ? "" : payload.ToString());
            BridgeResponse envelope = new BridgeResponse(BridgeResultCode.Ok, raw);
            BridgeResultCode code = (BridgeResultCode)envelope.GetLong("code", (long)BridgeResultCode.Unknown);
            string value = envelope.Values.TryGetValue("value", out object inner) ? new Services.Json.JsonWriter().Write(inner) : "";
            return new BridgeResponse(code, code == BridgeResultCode.Ok ? value : raw);
#else
            return new BridgeResponse(BridgeResultCode.Unavailable, "");
#endif
        }

        [MonoPInvokeCallback(typeof(MessageCallback))]
        private static void OnMessage(int requestId, int code, string payload)
        {
            _current?.Dispatch(requestId, code, payload);
        }

        private void Dispatch(int requestId, int code, string payload)
        {
            BridgeResponse response = new BridgeResponse((BridgeResultCode)code, payload);

            if (requestId == 0)
            {
                try
                {
                    EventReceived?.Invoke((BridgeEventCode)code, response);
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }

                return;
            }

            if (_pending.TryGetValue(requestId, out Action<BridgeResponse> onResponse) == false)
            {
                return;
            }

            _pending.Remove(requestId);

            try
            {
                onResponse(response);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }
        }
    }
}
