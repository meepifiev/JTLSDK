using System.Collections.Generic;
using JTLStudio.SDK.Services.Json;

namespace JTLStudio.SDK.Bridge
{
    public readonly struct BridgeResponse
    {
        private static readonly Dictionary<string, object> EmptyValues = new Dictionary<string, object>();

        public BridgeResponse(BridgeResultCode code, string payload)
        {
            Code = code;
            Payload = payload ?? "";
            Values = ParseValues(Payload);
        }

        public BridgeResultCode Code { get; }
        public string Payload { get; }
        public Dictionary<string, object> Values { get; }
        public bool IsSuccess => Code == BridgeResultCode.Ok;

        public string GetString(string key, string defaultValue = "")
        {
            return Values.TryGetValue(key, out object value) && value is string text ? text : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return Values.TryGetValue(key, out object value) && value is bool flag ? flag : defaultValue;
        }

        public long GetLong(string key, long defaultValue = 0)
        {
            if (Values.TryGetValue(key, out object value) == false)
            {
                return defaultValue;
            }

            switch (value)
            {
                case long number:
                    return number;

                case double number:
                    return (long)number;

                default:
                    return defaultValue;
            }
        }

        public double GetDouble(string key, double defaultValue = 0)
        {
            if (Values.TryGetValue(key, out object value) == false)
            {
                return defaultValue;
            }

            switch (value)
            {
                case double number:
                    return number;

                case long number:
                    return number;

                default:
                    return defaultValue;
            }
        }

        public List<object> GetList(string key)
        {
            return Values.TryGetValue(key, out object value) && value is List<object> list ? list : new List<object>();
        }

        private static Dictionary<string, object> ParseValues(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return EmptyValues;
            }

            try
            {
                return new JsonParser().Parse(payload) as Dictionary<string, object> ?? EmptyValues;
            }
            catch (System.FormatException)
            {
                return EmptyValues;
            }
        }
    }
}
