using System;

namespace JTLStudio.SDK.Services
{
    public class AdShow
    {
        private readonly AdsService _ads;
        private readonly IDisposable _pauseHold;
        private readonly Action<AdResult> _onResult;
        private bool _completed;

        public AdShow(AdsService ads, IDisposable pauseHold, Action<AdResult> onResult)
        {
            _ads = ads ?? throw new ArgumentNullException(nameof(ads));
            _pauseHold = pauseHold ?? throw new ArgumentNullException(nameof(pauseHold));
            _onResult = onResult;
        }

        public void Complete(AdResult result)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _ads.EndShow(_pauseHold, _onResult, result);
        }
    }
}
