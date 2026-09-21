#if UNITY_EDITOR
using System;
using UnityEditor;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeTimer
    {
        private readonly double _dueTime;
        private readonly Action _onElapsed;
        private bool _completed;

        public PrototypeTimer(float delaySeconds, Action onElapsed)
        {
            _onElapsed = onElapsed ?? throw new ArgumentNullException(nameof(onElapsed));
            _dueTime = EditorApplication.timeSinceStartup + delaySeconds;

            if (delaySeconds <= 0f)
            {
                Complete();
                return;
            }

            EditorApplication.update += OnUpdate;
        }

        public void Cancel()
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            EditorApplication.update -= OnUpdate;
        }

        private void OnUpdate()
        {
            if (EditorApplication.timeSinceStartup < _dueTime)
            {
                return;
            }

            EditorApplication.update -= OnUpdate;
            Complete();
        }

        private void Complete()
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _onElapsed();
        }
    }
}
#endif
