using System;

namespace JTLStudio.SDK.Services
{
    public class PauseHold : IDisposable
    {
        private readonly PauseService _pause;
        private readonly string _source;
        private bool _released;

        public PauseHold(PauseService pause, string source)
        {
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void Dispose()
        {
            if (_released)
            {
                return;
            }

            _released = true;
            _pause.Set(_source, false);
        }
    }
}
