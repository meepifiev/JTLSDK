using System;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Build
{
    [Serializable]
    public class BuildRecord
    {
        [SerializeField] private string _name;
        [SerializeField] private string _path;
        [SerializeField] private PlatformId _platform;
        [SerializeField] private long _bytes;
        [SerializeField] private bool _success;
        [SerializeField] private long _ticks;

        public BuildRecord(string name, string path, PlatformId platform, long bytes, bool success, DateTime time)
        {
            _name = name ?? "";
            _path = path ?? "";
            _platform = platform;
            _bytes = bytes;
            _success = success;
            _ticks = time.ToUniversalTime().Ticks;
        }

        public string Name => _name;
        public string Path => _path;
        public PlatformId Platform => _platform;
        public long Bytes => _bytes;
        public bool IsSuccess => _success;
        public DateTime Time => new DateTime(_ticks, DateTimeKind.Utc).ToLocalTime();
    }
}
