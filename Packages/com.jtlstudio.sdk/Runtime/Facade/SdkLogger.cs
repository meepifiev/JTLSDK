using System;
using UnityEngine;

namespace JTLStudio.SDK
{
    public class SdkLogger
    {
        private const string Prefix = "[JTL SDK] ";

        private readonly LogLevel _level;

        public SdkLogger(LogLevel level)
        {
            _level = level;
        }

        public void Info(string message)
        {
            if (_level >= LogLevel.All)
            {
                Debug.Log(Prefix + message);
            }
        }

        public void Warning(string message)
        {
            if (_level >= LogLevel.ErrorsAndWarnings)
            {
                Debug.LogWarning(Prefix + message);
            }
        }

        public void Error(string message)
        {
            if (_level >= LogLevel.Errors)
            {
                Debug.LogError(Prefix + message);
            }
        }

        public void Exception(Exception exception)
        {
            if (_level >= LogLevel.Errors)
            {
                Debug.LogException(exception);
            }
        }
    }
}
