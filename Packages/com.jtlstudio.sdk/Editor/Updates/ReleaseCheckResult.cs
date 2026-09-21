using System.Collections.Generic;

namespace JTLStudio.SDK.Editor.Updates
{
    public class ReleaseCheckResult
    {
        public ReleaseCheckResult(bool success, bool repositoryMissing, string error, IReadOnlyList<ReleaseInfo> releases)
        {
            IsSuccess = success;
            IsRepositoryMissing = repositoryMissing;
            Error = error;
            Releases = releases;
        }

        public bool IsSuccess { get; }
        public bool IsRepositoryMissing { get; }
        public string Error { get; }
        public IReadOnlyList<ReleaseInfo> Releases { get; }
    }
}
