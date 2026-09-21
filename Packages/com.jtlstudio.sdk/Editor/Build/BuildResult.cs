using System.Collections.Generic;

namespace JTLStudio.SDK.Editor.Build
{
    public class BuildResult
    {
        public BuildResult(bool success, string outputPath, long totalBytes, double seconds, IReadOnlyList<BuildCheck> postChecks, string error)
        {
            IsSuccess = success;
            OutputPath = outputPath;
            TotalBytes = totalBytes;
            Seconds = seconds;
            PostChecks = postChecks;
            Error = error;
        }

        public bool IsSuccess { get; }
        public string OutputPath { get; }
        public long TotalBytes { get; }
        public double Seconds { get; }
        public IReadOnlyList<BuildCheck> PostChecks { get; }
        public string Error { get; }
    }
}
