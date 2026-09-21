using System;
using System.Collections.Generic;
using JTLStudio.SDK.Services.Json;
using UnityEditor;
using UnityEngine.Networking;

namespace JTLStudio.SDK.Editor.Updates
{
    public class GitHubReleases
    {
        private const string ApiRoot = "https://api.github.com/repos/";
        private const string UserAgent = "JTLSDK-Toolkit";
        private const long NotFound = 404;

        private readonly JsonParser _parser = new JsonParser();

        public void Fetch(string repository, Action<ReleaseCheckResult> onDone)
        {
            if (onDone == null)
            {
                throw new ArgumentNullException(nameof(onDone));
            }

            UnityWebRequest request = UnityWebRequest.Get(ApiRoot + repository + "/releases");
            request.SetRequestHeader("User-Agent", UserAgent);
            request.SetRequestHeader("Accept", "application/vnd.github+json");
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            void Poll()
            {
                if (operation.isDone == false)
                {
                    return;
                }

                EditorApplication.update -= Poll;
                ReleaseCheckResult result = ToResult(request);
                request.Dispose();
                onDone(result);
            }

            EditorApplication.update += Poll;
        }

        public int CompareVersions(string left, string right)
        {
            string[] leftParts = (left ?? "0").Split('-')[0].Split('.');
            string[] rightParts = (right ?? "0").Split('-')[0].Split('.');

            for (int index = 0; index < Math.Max(leftParts.Length, rightParts.Length); index++)
            {
                int leftNumber = index < leftParts.Length && int.TryParse(leftParts[index], out int parsedLeft) ? parsedLeft : 0;
                int rightNumber = index < rightParts.Length && int.TryParse(rightParts[index], out int parsedRight) ? parsedRight : 0;

                if (leftNumber != rightNumber)
                {
                    return leftNumber.CompareTo(rightNumber);
                }
            }

            return 0;
        }

        private ReleaseCheckResult ToResult(UnityWebRequest request)
        {
            if (request.responseCode == NotFound)
            {
                return new ReleaseCheckResult(false, true, request.error, new List<ReleaseInfo>());
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                return new ReleaseCheckResult(false, false, request.error, new List<ReleaseInfo>());
            }

            try
            {
                return new ReleaseCheckResult(true, false, "", Parse(request.downloadHandler.text));
            }
            catch (FormatException exception)
            {
                return new ReleaseCheckResult(false, false, exception.Message, new List<ReleaseInfo>());
            }
        }

        private List<ReleaseInfo> Parse(string json)
        {
            List<ReleaseInfo> releases = new List<ReleaseInfo>();

            if (_parser.Parse(json) is List<object> items == false)
            {
                return releases;
            }

            foreach (object item in items)
            {
                if (item is Dictionary<string, object> release == false)
                {
                    continue;
                }

                if (release.TryGetValue("draft", out object draft) && draft is bool isDraft && isDraft)
                {
                    continue;
                }

                string tag = Text(release, "tag_name");
                string name = Text(release, "name");
                string published = Text(release, "published_at");
                bool preRelease = release.TryGetValue("prerelease", out object flag) && flag is bool isPreRelease && isPreRelease;
                releases.Add(new ReleaseInfo(tag, string.IsNullOrEmpty(name) ? tag : name, published.Length >= 10 ? published.Substring(0, 10) : published, preRelease, Notes(Text(release, "body"))));
            }

            return releases;
        }

        private List<string> Notes(string body)
        {
            List<string> notes = new List<string>();

            foreach (string line in body.Split('\n'))
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
                {
                    notes.Add(trimmed.Substring(2).Trim());
                }
            }

            return notes;
        }

        private string Text(Dictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out object value) && value is string text ? text : "";
        }
    }
}
