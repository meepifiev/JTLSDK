using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using JTLStudio.SDK.Editor.Configuration;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Build
{
    public class SdkBuildService
    {
        public const float BytesPerMegabyte = 1000000f;

        private const long MaximumFileBytes = 30L * 1024 * 1024;
        private const int MaximumFiles = 8000;
        private const string YouTubeScriptHost = "https://www.youtube.com/game_api/";

        private readonly TemplateService _template = new TemplateService();
        private readonly PlatformBridgeFilter _bridges = new PlatformBridgeFilter();
        private readonly ConfigurationValidator _validator = new ConfigurationValidator();
        private readonly DefineSymbolService _defines = new DefineSymbolService();

        public List<BuildCheck> PreChecks(SdkConfiguration configuration)
        {
            List<BuildCheck> checks = new List<BuildCheck>
            {
                new BuildCheck("build.check.configuration", configuration != null),
                new BuildCheck("build.check.webgl", BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL)),
                new BuildCheck("build.check.scenes", EnabledScenes().Length > 0),
                new BuildCheck("build.check.settings", AssetDatabase.LoadAssetAtPath<JTLSDKSettings>(SettingsAssetService.SettingsAssetPath) != null),
                new BuildCheck("build.check.template", _template.IsInstalled)
            };

            if (configuration == null)
            {
                return checks;
            }

            string define = _defines.Current();
            checks.Add(new BuildCheck("build.check.define", string.IsNullOrEmpty(configuration.DefineSymbol) || define == configuration.DefineSymbol, configuration.DefineSymbol));

            foreach (string issue in _validator.Validate(configuration))
            {
                checks.Add(new BuildCheck("build.check.issue", false, issue));
            }

            if (configuration.Platform == PlatformId.YouTubePlayables)
            {
                checks.Add(new BuildCheck("build.check.compression", PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Disabled));
            }

            return checks;
        }

        public string ResolveName(JTLSDKEditorSettings settings, SdkConfiguration configuration, int buildNumber)
        {
            string configurationName = configuration == null ? "Editor" : configuration.DisplayName.Replace(" ", "");
            string name = settings.BuildNamePattern
                .Replace("{product}", Sanitize(PlayerSettings.productName))
                .Replace("{configuration}", Sanitize(configurationName))
                .Replace("{build}", buildNumber.ToString(CultureInfo.InvariantCulture))
                .Replace("{version}", Sanitize(PlayerSettings.bundleVersion));
            return Sanitize(name);
        }

        public BuildResult Build(JTLSDKEditorSettings settings, SdkConfiguration configuration)
        {
            foreach (BuildCheck check in PreChecks(configuration))
            {
                if (check.Passed == false)
                {
                    return new BuildResult(false, "", 0, 0, new List<BuildCheck> { check }, "");
                }
            }

            int buildNumber = settings.BuildNumber + 1;
            PlatformId platform = configuration == null ? PlatformId.Editor : configuration.Platform;
            string configurationName = configuration == null ? "Editor" : configuration.DisplayName;
            string folder = Path.Combine(settings.BuildPath, ResolveName(settings, configuration, buildNumber));

            if (_template.IsOutdated)
            {
                _template.Update();
            }

            PlayerSettings.WebGL.template = TemplateService.TemplateSetting;
            _bridges.Register();
            SessionState.SetInt(TemplateService.BuildNumberKey, buildNumber);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EnabledScenes(),
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                locationPathName = folder,
                options = BuildOptions.None
            };

            Stopwatch stopwatch = Stopwatch.StartNew();
            BuildReport report = BuildPipeline.BuildPlayer(options);
            stopwatch.Stop();
            SessionState.EraseInt(TemplateService.BuildNumberKey);

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Record(folder, platform, 0, false);
                return new BuildResult(false, folder, 0, stopwatch.Elapsed.TotalSeconds, new List<BuildCheck>(), FirstError(report));
            }

            settings.BuildNumber = buildNumber;
            settings.Persist();
            List<BuildCheck> postChecks = PostChecks(platform, folder);
            long bytes = FolderBytes(folder);
            string output = folder;

            if (settings.BuildOutput == BuildOutput.Zip)
            {
                output = folder + ".zip";
                Zip(folder, output);
                Directory.Delete(folder, true);
                bytes = new FileInfo(output).Length;
            }

            UnityEngine.Debug.Log("[JTL SDK] Build " + buildNumber + " · " + configurationName + " · " + (bytes / BytesPerMegabyte).ToString("0.0", CultureInfo.InvariantCulture) + " MB · " + Path.GetFullPath(output));

            Record(output, platform, bytes, true);

            if (settings.OpenFolderAfterBuild)
            {
                EditorUtility.RevealInFinder(output);
            }

            return new BuildResult(true, output, bytes, stopwatch.Elapsed.TotalSeconds, postChecks, "");
        }

        private void Record(string output, PlatformId platform, long bytes, bool success)
        {
            BuildHistory.instance.Add(new BuildRecord(Path.GetFileName(output), output, platform, bytes, success, DateTime.Now));
        }

        private string FirstError(BuildReport report)
        {
            foreach (BuildStep step in report.steps)
            {
                foreach (BuildStepMessage message in step.messages)
                {
                    if (message.type == LogType.Error || message.type == LogType.Exception)
                    {
                        return message.content;
                    }
                }
            }

            return report.summary.result.ToString();
        }

        private List<BuildCheck> PostChecks(PlatformId platform, string folder)
        {
            string index = Path.Combine(folder, "index.html");
            List<BuildCheck> checks = new List<BuildCheck>
            {
                new BuildCheck("build.check.variables", File.Exists(index) && File.ReadAllText(index).Contains("{{{") == false && File.ReadAllText(index).Contains("%JTLSDK_") == false)
            };

            if (platform != PlatformId.YouTubePlayables)
            {
                return checks;
            }

            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            long largest = 0;

            foreach (string file in files)
            {
                largest = Math.Max(largest, new FileInfo(file).Length);
            }

            checks.Add(new BuildCheck("build.check.fileSize", largest < MaximumFileBytes, (largest / 1048576f).ToString("0.0", CultureInfo.InvariantCulture)));
            checks.Add(new BuildCheck("build.check.fileCount", files.Length <= MaximumFiles, files.Length));
            checks.Add(new BuildCheck("build.check.compression", PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Disabled));
            checks.Add(new BuildCheck("build.check.scripts", HasOnlyAllowedScripts(index)));
            return checks;
        }

        private bool HasOnlyAllowedScripts(string indexPath)
        {
            if (File.Exists(indexPath) == false)
            {
                return false;
            }

            foreach (Match match in Regex.Matches(File.ReadAllText(indexPath), "<script[^>]+src=\"(https?:[^\"]+)\""))
            {
                if (match.Groups[1].Value.StartsWith(YouTubeScriptHost) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private string[] EnabledScenes()
        {
            List<string> scenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }

        private long FolderBytes(string folder)
        {
            long total = 0;

            foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                total += new FileInfo(file).Length;
            }

            return total;
        }

        private void Zip(string folder, string zipPath)
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using (FileStream stream = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                {
                    string entryName = file.Substring(folder.Length).TrimStart('/', '\\').Replace('\\', '/');
                    ZipArchiveEntry entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal);

                    using (Stream entryStream = entry.Open())
                    using (FileStream source = File.OpenRead(file))
                    {
                        source.CopyTo(entryStream);
                    }
                }
            }
        }

        private string Sanitize(string value)
        {
            string result = value ?? "";

            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(invalid, '_');
            }

            return result.Replace(' ', '_');
        }
    }
}
