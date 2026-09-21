using UnityEngine;

namespace JTLStudio.SDK
{
    [AddComponentMenu("")]
    public class SdkRuntimeBehaviour : MonoBehaviour
    {
        private SdkInstance _instance;

        internal void Bind(SdkInstance instance)
        {
            _instance = instance;
        }

        private void Update()
        {
            _instance?.Tick(Time.unscaledDeltaTime);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _instance?.HandleApplicationFocus(hasFocus);
        }

        private void OnApplicationPause(bool isPaused)
        {
            _instance?.HandleApplicationPause(isPaused);
        }

        private void OnApplicationQuit()
        {
            _instance?.HandleApplicationQuit();
        }

        private void OnDestroy()
        {
            SdkInstance instance = _instance;
            _instance = null;
            instance?.HandleBehaviourDestroyed();
        }
    }
}
