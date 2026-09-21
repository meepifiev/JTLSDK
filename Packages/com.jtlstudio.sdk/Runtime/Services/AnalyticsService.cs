using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using JTLStudio.SDK.Services.Json;

namespace JTLStudio.SDK.Services
{
    public class AnalyticsService : ModuleBase, IAnalytics
    {
        private readonly IAnalyticsProvider _provider;
        private readonly JsonWriter _writer = new JsonWriter();

        public AnalyticsService(IAnalyticsProvider provider, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        internal override string ModuleName => "Analytics";

        public void Report(string eventName)
        {
            Report(eventName, null);
        }

        public void Report(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException(nameof(eventName));
            }

            if (State != ModuleState.Ready)
            {
                return;
            }

            _provider.Report(eventName, parameters == null || parameters.Count == 0 ? "" : _writer.Write(ToDictionary(parameters)));
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }

        private Dictionary<string, object> ToDictionary(IReadOnlyDictionary<string, object> parameters)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> pair in parameters)
            {
                result[pair.Key] = pair.Value;
            }

            return result;
        }
    }
}
