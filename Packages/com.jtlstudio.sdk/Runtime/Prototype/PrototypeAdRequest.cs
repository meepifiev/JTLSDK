#if UNITY_EDITOR
using System;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeAdRequest
    {
        private readonly Action<AdResult> _onResult;
        private bool _completed;

        public PrototypeAdRequest(bool rewarded, string rewardId, PlatformId platform, Action<AdResult> onResult)
        {
            IsRewarded = rewarded;
            RewardId = rewardId ?? "";
            Platform = platform;
            _onResult = onResult ?? throw new ArgumentNullException(nameof(onResult));
        }

        public bool IsRewarded { get; }
        public string RewardId { get; }
        public PlatformId Platform { get; }
        public bool IsCompleted => _completed;

        public void Complete(AdResult result)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _onResult(result);
        }
    }
}
#endif
