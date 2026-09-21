using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Editor.Configuration
{
    [FilePath("ProjectSettings/JTLSDKEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class JTLSDKEditorSettings : ScriptableSingleton<JTLSDKEditorSettings>
    {
        [SerializeField] private LogoMode _logoMode = LogoMode.Default;
        [SerializeField] private Texture2D _logo;
        [SerializeField] private int _logoSize = 160;
        [SerializeField] private TemplateBackground _loaderBackground = new TemplateBackground { Kind = BackgroundKind.Gradient, Radial = true, GradientFrom = new Color(0.063f, 0.133f, 0.247f), GradientTo = new Color(0.024f, 0.031f, 0.051f) };
        [SerializeField] private TemplateBackground _pageBackground = new TemplateBackground { Kind = BackgroundKind.Color, Color = new Color(0.024f, 0.031f, 0.051f) };
        [SerializeField] private Color _progressFill = new Color(0.184f, 0.549f, 1f);
        [SerializeField] private Color _progressTrack = new Color(0.106f, 0.141f, 0.2f);
        [SerializeField] private bool _progressGradient;
        [SerializeField] private Color _progressFillTo = new Color(0.482f, 0.247f, 0.949f);
        [SerializeField] private int _progressBorderWidth;
        [SerializeField] private Color _progressBorderColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private int _progressPadding;
        [SerializeField] private int _progressWidthPercent = 40;
        [SerializeField] private int _progressHeight = 6;
        [SerializeField] private int _progressRadius = 3;
        [SerializeField] private bool _progressAtBottom;
        [SerializeField] private string _loadingText = "";
        [SerializeField] private bool _fixedAspect;
        [SerializeField] private string _aspectRatio = "16/9";
        [SerializeField] private bool _freeAspectOnMobile = true;
        [SerializeField] private PixelRatioMode _desktopPixelRatioMode = PixelRatioMode.Fixed;
        [SerializeField] private float _desktopPixelRatio = 1f;
        [SerializeField] private PixelRatioMode _mobilePixelRatioMode = PixelRatioMode.Fixed;
        [SerializeField] private float _mobilePixelRatio = 1f;
        [SerializeField] private BuildOutput _buildOutput = BuildOutput.Folder;
        [SerializeField] private string _buildPath = "Builds";
        [SerializeField] private string _buildNamePattern = "{product}_{configuration}_b{build}";
        [SerializeField] private int _buildNumber;
        [SerializeField] private bool _developmentBuild;
        [SerializeField] private bool _openFolderAfterBuild = true;

        public LogoMode LogoMode { get => _logoMode; set => _logoMode = value; }
        public Texture2D Logo { get => _logo; set => _logo = value; }
        public int LogoSize { get => _logoSize; set => _logoSize = Mathf.Max(16, value); }
        public TemplateBackground LoaderBackground => _loaderBackground;
        public TemplateBackground PageBackground => _pageBackground;
        public Color ProgressFill { get => _progressFill; set => _progressFill = value; }
        public Color ProgressTrack { get => _progressTrack; set => _progressTrack = value; }
        public bool ProgressGradient { get => _progressGradient; set => _progressGradient = value; }
        public Color ProgressFillTo { get => _progressFillTo; set => _progressFillTo = value; }
        public int ProgressBorderWidth { get => _progressBorderWidth; set => _progressBorderWidth = Mathf.Clamp(value, 0, 16); }
        public Color ProgressBorderColor { get => _progressBorderColor; set => _progressBorderColor = value; }
        public int ProgressPadding { get => _progressPadding; set => _progressPadding = Mathf.Clamp(value, 0, 16); }
        public int ProgressWidthPercent { get => _progressWidthPercent; set => _progressWidthPercent = Mathf.Clamp(value, 5, 100); }
        public int ProgressHeight { get => _progressHeight; set => _progressHeight = Mathf.Clamp(value, 1, 96); }
        public int ProgressRadius { get => _progressRadius; set => _progressRadius = Mathf.Clamp(value, 0, 999); }
        public bool ProgressAtBottom { get => _progressAtBottom; set => _progressAtBottom = value; }
        public string LoadingText { get => _loadingText; set => _loadingText = value ?? ""; }
        public bool FixedAspect { get => _fixedAspect; set => _fixedAspect = value; }
        public string AspectRatio { get => _aspectRatio; set => _aspectRatio = string.IsNullOrWhiteSpace(value) ? "16/9" : value.Trim(); }
        public bool FreeAspectOnMobile { get => _freeAspectOnMobile; set => _freeAspectOnMobile = value; }
        public PixelRatioMode DesktopPixelRatioMode { get => _desktopPixelRatioMode; set => _desktopPixelRatioMode = value; }
        public float DesktopPixelRatio { get => _desktopPixelRatio; set => _desktopPixelRatio = Mathf.Clamp(value, 0.5f, 4f); }
        public PixelRatioMode MobilePixelRatioMode { get => _mobilePixelRatioMode; set => _mobilePixelRatioMode = value; }
        public float MobilePixelRatio { get => _mobilePixelRatio; set => _mobilePixelRatio = Mathf.Clamp(value, 0.5f, 4f); }
        public BuildOutput BuildOutput { get => _buildOutput; set => _buildOutput = value; }
        public string BuildPath { get => _buildPath; set => _buildPath = string.IsNullOrWhiteSpace(value) ? "Builds" : value.Trim(); }
        public string BuildNamePattern { get => _buildNamePattern; set => _buildNamePattern = string.IsNullOrWhiteSpace(value) ? "{product}_{configuration}_b{build}" : value.Trim(); }
        public int BuildNumber { get => _buildNumber; set => _buildNumber = Mathf.Max(0, value); }
        public bool DevelopmentBuild { get => _developmentBuild; set => _developmentBuild = value; }
        public bool OpenFolderAfterBuild { get => _openFolderAfterBuild; set => _openFolderAfterBuild = value; }

        public void Persist()
        {
            Save(true);
        }
    }
}
