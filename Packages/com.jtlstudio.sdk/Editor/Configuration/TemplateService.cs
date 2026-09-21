using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class TemplateService
    {
        public const string TemplateName = "JTLSDK";
        public const string TemplateFolder = "Assets/WebGLTemplates/JTLSDK";
        public const string TemplateSetting = "PROJECT:JTLSDK";

        private const string PackagePath = "Packages/com.jtlstudio.sdk";
        private const string PackageTemplateFolder = "Editor/Template~/JTLSDK";
        private const string LogoFile = "logo.png";
        private const string LoaderBackgroundFile = "loader-background.png";
        private const string PageBackgroundFile = "page-background.png";
        private const string YandexHead = "<script src=\"/sdk.js\"></script>";
        private const string YouTubeHead = "<script src=\"https://www.youtube.com/game_api/v1\"></script>";
        private const string CustomKeysProperty = "templateCustomKeys";
        private const string VariablePattern = "\\{\\{\\{\\s*(JTLSDK_[A-Z_]+)\\s*\\}\\}\\}";

        private readonly Regex _variables = new Regex(VariablePattern);

        public bool IsInstalled => File.Exists(Path.Combine(TemplateFolder, "index.html"));

        public void Install()
        {
            UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);
            string root = package == null ? Path.GetFullPath(PackagePath) : package.resolvedPath;
            string source = Path.Combine(root, PackageTemplateFolder);

            if (Directory.Exists(source) == false)
            {
                throw new DirectoryNotFoundException(PackageTemplateFolder);
            }

            Copy(source, TemplateFolder);
            AssetDatabase.Refresh();
            PlayerSettings.WebGL.template = TemplateSetting;
        }

        public void Apply(JTLSDKEditorSettings settings, SdkConfiguration configuration, int buildNumber, bool development)
        {
            if (IsInstalled == false)
            {
                Install();
            }

            PlayerSettings.WebGL.template = TemplateSetting;
            RegisterVariables();
            string dataFolder = Path.Combine(TemplateFolder, "TemplateData");
            bool hasLogo = CopyTexture(settings.Logo, Path.Combine(dataFolder, LogoFile));
            CopyTexture(settings.LoaderBackground.Image, Path.Combine(dataFolder, LoaderBackgroundFile));
            CopyTexture(settings.PageBackground.Image, Path.Combine(dataFolder, PageBackgroundFile));

            PlatformId platform = configuration == null ? PlatformId.Editor : configuration.Platform;
            Set("JTLSDK_PLATFORM", PlatformKey(platform));
            Set("JTLSDK_PLATFORM_HEAD", PlatformHead(platform));
            Set("JTLSDK_PAGE_BACKGROUND", settings.PageBackground.ToCss(PageBackgroundFile));
            Set("JTLSDK_LOADER_BACKGROUND", settings.LoaderBackground.ToCss(LoaderBackgroundFile));
            Set("JTLSDK_LOGO_SIZE", settings.LogoSize.ToString(CultureInfo.InvariantCulture));
            Set("JTLSDK_LOGO_DISPLAY", hasLogo ? "block" : "none");
            Set("JTLSDK_PROGRESS_FILL", "#" + ColorUtility.ToHtmlStringRGB(settings.ProgressFill));
            Set("JTLSDK_PROGRESS_TRACK", "#" + ColorUtility.ToHtmlStringRGB(settings.ProgressTrack));
            Set("JTLSDK_PROGRESS_WIDTH", settings.ProgressWidthPercent.ToString(CultureInfo.InvariantCulture));
            Set("JTLSDK_PROGRESS_HEIGHT", settings.ProgressHeight.ToString(CultureInfo.InvariantCulture));
            Set("JTLSDK_PROGRESS_RADIUS", settings.ProgressRadius.ToString(CultureInfo.InvariantCulture));
            Set("JTLSDK_PROGRESS_POSITION", settings.ProgressAtBottom ? "bottom" : "logo");
            Set("JTLSDK_LOADING_TEXT", settings.LoadingText);
            Set("JTLSDK_ASPECT", settings.FixedAspect ? settings.AspectRatio : "free");
            Set("JTLSDK_ASPECT_MOBILE", settings.FixedAspect && settings.FreeAspectOnMobile ? "free" : "same");
            Set("JTLSDK_DPR_DESKTOP", PixelRatio(settings.DesktopPixelRatioMode, settings.DesktopPixelRatio));
            Set("JTLSDK_DPR_MOBILE", PixelRatio(settings.MobilePixelRatioMode, settings.MobilePixelRatio));
            Set("JTLSDK_FULLSCREEN_BUTTON", settings.FullscreenButton ? "true" : "false");
            Set("JTLSDK_DEV_BADGE", development ? "DEV · b" + buildNumber.ToString(CultureInfo.InvariantCulture) + " · " + PlatformName(platform) + " · v" + JTLSDK.Version : "");
            AssetDatabase.Refresh();
        }

        private void Set(string name, string value)
        {
            string expected = value ?? "";
            PlayerSettings.SetTemplateCustomValue(name, expected);

            if (PlayerSettings.GetTemplateCustomValue(name) != expected)
            {
                throw new InvalidOperationException("WebGL template variable " + name + " is not registered. Reinstall the template.");
            }
        }

        private void RegisterVariables()
        {
            PropertyInfo property = typeof(PlayerSettings).GetProperty(CustomKeysProperty, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null)
            {
                throw new InvalidOperationException("PlayerSettings." + CustomKeysProperty + " is not available in this Unity version.");
            }

            List<string> keys = new List<string>((string[])property.GetValue(null) ?? Array.Empty<string>());

            foreach (string file in Directory.GetFiles(TemplateFolder, "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(file);

                if (extension != ".html" && extension != ".css" && extension != ".js")
                {
                    continue;
                }

                foreach (Match match in _variables.Matches(File.ReadAllText(file)))
                {
                    string key = match.Groups[1].Value;

                    if (keys.Contains(key) == false)
                    {
                        keys.Add(key);
                    }
                }
            }

            property.SetValue(null, keys.ToArray());
        }

        private string PlatformKey(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.YandexGames:
                    return "yandex";

                case PlatformId.YouTubePlayables:
                    return "youtube";

                default:
                    return "editor";
            }
        }

        private string PlatformName(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.YandexGames:
                    return "Yandex Games";

                case PlatformId.YouTubePlayables:
                    return "YouTube Playables";

                default:
                    return "Editor";
            }
        }

        private string PlatformHead(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.YandexGames:
                    return YandexHead;

                case PlatformId.YouTubePlayables:
                    return YouTubeHead;

                default:
                    return "";
            }
        }

        private string PixelRatio(PixelRatioMode mode, float value)
        {
            string number = value.ToString("0.##", CultureInfo.InvariantCulture);

            switch (mode)
            {
                case PixelRatioMode.Fixed:
                    return number;

                case PixelRatioMode.AutoWithLimit:
                    return "max:" + number;

                default:
                    return "auto";
            }
        }

        private bool CopyTexture(Texture2D texture, string destination)
        {
            if (texture == null)
            {
                return false;
            }

            string source = AssetDatabase.GetAssetPath(texture);

            if (string.IsNullOrEmpty(source) || File.Exists(source) == false)
            {
                return false;
            }

            if (source.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(source, destination, true);
                return true;
            }

            Texture2D readable = new Texture2D(2, 2);

            if (readable.LoadImage(File.ReadAllBytes(source)) == false)
            {
                UnityEngine.Object.DestroyImmediate(readable);
                return false;
            }

            File.WriteAllBytes(destination, readable.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(readable);
            return true;
        }

        private void Copy(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string relative = file.Substring(source.Length).TrimStart('/', '\\');
                string target = Path.Combine(destination, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file, target, true);
            }
        }
    }
}
