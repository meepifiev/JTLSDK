using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Net;
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
        private const string TemplatesRoot = "Assets/WebGLTemplates";
        private const string UnityDefaultTemplate = "APPLICATION:Default";
        private const string PackageTemplateFolder = "Editor/Template~/JTLSDK";
        public const string DefaultLogoPath = "Packages/com.jtlstudio.sdk/Editor/Toolkit/Icons/Brand/jtlsdk-template-logo.png";

        public const string BuildNumberKey = "JTLSDK.Template.BuildNumber";

        private const string DataFolder = "TemplateData";
        private const string IndexFile = "index.html";
        private const string LogoName = "logo";
        private const string LoaderBackgroundName = "loader-background";
        private const string PageBackgroundName = "page-background";
        private const string TokenMark = "%";
        private const string VariablePrefix = "JTLSDK_";
        private const string MetaExtension = ".meta";
        private const string YandexHead = "<script src=\"/sdk.js\"></script>";
        private const string YouTubeHead = "<script src=\"https://www.youtube.com/game_api/v1\"></script>";
        private const string CustomKeysProperty = "templateCustomKeys";
        private readonly string[] _imageExtensions = { ".png", ".jpg" };

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
            RemoveLegacyVariables();
        }

        public void Uninstall()
        {
            if (AssetDatabase.IsValidFolder(TemplateFolder))
            {
                AssetDatabase.DeleteAsset(TemplateFolder);
            }

            if (AssetDatabase.IsValidFolder(TemplatesRoot) && AssetDatabase.FindAssets("", new[] { TemplatesRoot }).Length == 0)
            {
                AssetDatabase.DeleteAsset(TemplatesRoot);
            }

            if (PlayerSettings.WebGL.template == TemplateSetting)
            {
                PlayerSettings.WebGL.template = UnityDefaultTemplate;
            }

            AssetDatabase.Refresh();
        }

        public bool IsSelected => IsInstalled && PlayerSettings.WebGL.template == TemplateSetting;

        public void PrepareAssets(JTLSDKEditorSettings settings)
        {
            string dataFolder = Path.Combine(TemplateFolder, DataFolder);
            PrepareImage(LogoTexture(settings), dataFolder, LogoName);
            PrepareImage(ImageOf(settings.LoaderBackground), dataFolder, LoaderBackgroundName);
            PrepareImage(ImageOf(settings.PageBackground), dataFolder, PageBackgroundName);
        }

        public Dictionary<string, string> Values(JTLSDKEditorSettings settings, SdkConfiguration configuration, int buildNumber, bool development)
        {
            PlatformId platform = configuration == null ? PlatformId.Editor : configuration.Platform;
            Texture2D logo = LogoTexture(settings);

            return new Dictionary<string, string>
            {
                { "JTLSDK_PLATFORM", PlatformKey(platform) },
                { "JTLSDK_PLATFORM_HEAD", PlatformHead(platform) },
                { "JTLSDK_PAGE_BACKGROUND", settings.PageBackground.ToCss(ImageFileName(ImageOf(settings.PageBackground), PageBackgroundName)) },
                { "JTLSDK_LOADER_BACKGROUND", settings.LoaderBackground.ToCss(ImageFileName(ImageOf(settings.LoaderBackground), LoaderBackgroundName)) },
                { "JTLSDK_LOGO_FILE", ImageFileName(logo, LogoName) },
                { "JTLSDK_LOGO_SIZE", settings.LogoSize.ToString(CultureInfo.InvariantCulture) },
                { "JTLSDK_LOGO_DISPLAY", logo != null ? "block" : "none" },
                { "JTLSDK_PROGRESS_FILL", Css(settings.ProgressFill) },
                { "JTLSDK_PROGRESS_FILL_TO", Css(settings.ProgressGradient ? settings.ProgressFillTo : settings.ProgressFill) },
                { "JTLSDK_PROGRESS_TRACK", Css(settings.ProgressTrack) },
                { "JTLSDK_PROGRESS_BORDER_WIDTH", settings.ProgressBorderWidth.ToString(CultureInfo.InvariantCulture) },
                { "JTLSDK_PROGRESS_BORDER_COLOR", Css(settings.ProgressBorderColor) },
                { "JTLSDK_PROGRESS_PADDING", settings.ProgressPadding.ToString(CultureInfo.InvariantCulture) },
                { "JTLSDK_PROGRESS_WIDTH", settings.ProgressWidthPercent.ToString(CultureInfo.InvariantCulture) },
                { "JTLSDK_PROGRESS_HEIGHT", settings.ProgressHeight.ToString(CultureInfo.InvariantCulture) },
                { "JTLSDK_PROGRESS_RADIUS", settings.ProgressRadius.ToString(CultureInfo.InvariantCulture) },
                { "JTLSDK_PROGRESS_POSITION", settings.ProgressAtBottom ? "bottom" : "logo" },
                { "JTLSDK_LOADING_TEXT", WebUtility.HtmlEncode(settings.LoadingText) },
                { "JTLSDK_ASPECT", settings.FixedAspect ? settings.AspectRatio : "free" },
                { "JTLSDK_ASPECT_MOBILE", settings.FixedAspect && settings.FreeAspectOnMobile ? "free" : "same" },
                { "JTLSDK_DPR_DESKTOP", PixelRatio(settings.DesktopPixelRatioMode, settings.DesktopPixelRatio) },
                { "JTLSDK_DPR_MOBILE", PixelRatio(settings.MobilePixelRatioMode, settings.MobilePixelRatio) },
                { "JTLSDK_DEV_BADGE", development ? "DEV · b" + buildNumber.ToString(CultureInfo.InvariantCulture) + " · " + PlatformName(platform) + " · v" + JTLSDK.Version : "" }
            };
        }

        public void Substitute(string outputFolder, Dictionary<string, string> values)
        {
            string index = Path.Combine(outputFolder, IndexFile);

            if (File.Exists(index) == false)
            {
                throw new FileNotFoundException(index);
            }

            string html = File.ReadAllText(index);

            foreach (KeyValuePair<string, string> value in values)
            {
                html = html.Replace(TokenMark + value.Key + TokenMark, value.Value ?? "");
            }

            File.WriteAllText(index, html);
        }

        private Texture2D ImageOf(TemplateBackground background)
        {
            return background.Kind == BackgroundKind.Image ? background.Image : null;
        }

        private void PrepareImage(Texture2D texture, string dataFolder, string baseName)
        {
            foreach (string extension in _imageExtensions)
            {
                string stale = Path.Combine(dataFolder, baseName + extension);

                if (File.Exists(stale))
                {
                    File.Delete(stale);
                }

                if (File.Exists(stale + MetaExtension))
                {
                    File.Delete(stale + MetaExtension);
                }
            }

            if (texture != null)
            {
                CopyTexture(texture, Path.Combine(dataFolder, ImageFileName(texture, baseName)));
            }
        }

        private string ImageFileName(Texture2D texture, string baseName)
        {
            string source = SourcePath(texture);
            string extension = Path.GetExtension(source).ToLowerInvariant();
            return baseName + (extension == ".jpg" || extension == ".jpeg" ? ".jpg" : ".png");
        }

        private string SourcePath(Texture2D texture)
        {
            if (texture == null)
            {
                return "";
            }

            string assetPath = AssetDatabase.GetAssetPath(texture);
            return string.IsNullOrEmpty(assetPath) ? "" : FileUtil.GetPhysicalPath(assetPath);
        }

        private void RemoveLegacyVariables()
        {
            PropertyInfo property = typeof(PlayerSettings).GetProperty(CustomKeysProperty, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || property.GetValue(null) is string[] keys == false)
            {
                return;
            }

            List<string> kept = new List<string>();

            foreach (string key in keys)
            {
                if (key.StartsWith(VariablePrefix) == false)
                {
                    kept.Add(key);
                }
            }

            if (kept.Count != keys.Length)
            {
                property.SetValue(null, kept.ToArray());
            }
        }

        private string Css(Color color)
        {
            return color.a >= 0.999f ? "#" + ColorUtility.ToHtmlStringRGB(color) : "#" + ColorUtility.ToHtmlStringRGBA(color);
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

        public Texture2D DefaultLogo => AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultLogoPath);

        public Texture2D LogoTexture(JTLSDKEditorSettings settings)
        {
            switch (settings.LogoMode)
            {
                case LogoMode.Custom:
                    return settings.Logo;

                case LogoMode.Default:
                    return DefaultLogo;

                default:
                    return null;
            }
        }

        private bool CopyTexture(Texture2D texture, string destination)
        {
            if (texture == null)
            {
                return false;
            }

            string source = SourcePath(texture);

            if (string.IsNullOrEmpty(source) || File.Exists(source) == false)
            {
                return false;
            }

            string extension = Path.GetExtension(source).ToLowerInvariant();

            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
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
