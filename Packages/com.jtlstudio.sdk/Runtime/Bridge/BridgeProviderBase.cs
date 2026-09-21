using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Bridge
{
    public abstract class BridgeProviderBase : IBridgeConsumer
    {
        private WebBridge _bridge;

        protected WebBridge Bridge => _bridge;
        protected bool IsBridgeReady => _bridge != null && _bridge.IsAvailable;

        public virtual void Attach(WebBridge bridge)
        {
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        }

        protected void Call(string module, string action, BridgePayload payload, Action<BridgeResponse> onResponse)
        {
            if (IsBridgeReady == false)
            {
                onResponse(new BridgeResponse(BridgeResultCode.Unavailable, ""));
                return;
            }

            _bridge.Call(module, action, payload, onResponse);
        }

        protected void InitializeWith(string module, string action, Action<ProviderState> onInitialized, Action<BridgeResponse> onSuccess)
        {
            if (IsBridgeReady == false)
            {
                onInitialized(ProviderState.Failed);
                return;
            }

            _bridge.Call(module, action, null, response =>
            {
                if (response.IsSuccess == false)
                {
                    onInitialized(ProviderState.Failed);
                    return;
                }

                onSuccess?.Invoke(response);
                onInitialized(ProviderState.Ready);
            });
        }

        protected Dictionary<string, object> AsObject(object value)
        {
            return value as Dictionary<string, object> ?? new Dictionary<string, object>();
        }

        protected string GetString(Dictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out object value) && value is string text ? text : "";
        }

        protected long GetLong(Dictionary<string, object> values, string key)
        {
            if (values.TryGetValue(key, out object value) == false)
            {
                return 0;
            }

            switch (value)
            {
                case long number:
                    return number;

                case double number:
                    return (long)number;

                default:
                    return 0;
            }
        }

        protected double GetDouble(Dictionary<string, object> values, string key)
        {
            if (values.TryGetValue(key, out object value) == false)
            {
                return 0;
            }

            switch (value)
            {
                case double number:
                    return number;

                case long number:
                    return number;

                default:
                    return 0;
            }
        }

        protected bool GetBool(Dictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out object value) && value is bool flag && flag;
        }
    }
}
