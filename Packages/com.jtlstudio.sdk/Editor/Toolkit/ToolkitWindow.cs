using System;
using System.Collections.Generic;
using JTLStudio.SDK.Editor.Toolkit.Components;
using JTLStudio.SDK.Editor.Toolkit.Data;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using JTLStudio.SDK.Editor.Toolkit.Sections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public class ToolkitWindow : EditorWindow
    {
        public const string Title = "JTL SDK";
        public const float CollapseWidth = 1040f;
        private const float MinimumWidth = 900f;
        private const float MinimumHeight = 600f;
        private const string CollapsedClass = "jtl-root--collapsed";
        private const string PackageManifestPath = "Packages/com.jtlstudio.sdk/package.json";
        private const string FallbackVersion = "0.1.0";
        private const string SupportUrl = "https://t.me/jtlstudio";
        private const string WindowTemplate = "ToolkitWindow.uxml";
        private const string TokensStyleSheet = "Styles/Tokens.uss";
        private const string ToolkitStyleSheet = "Styles/Toolkit.uss";
        private const string ComponentsStyleSheet = "Styles/Components.uss";
        private const int MaximumBuildAttempts = 100;

        private readonly ToolkitAssets _assets = new ToolkitAssets();
        private readonly ToolkitProject _project = new ToolkitProject();
        private readonly ToolkitLocalization _localization = new ToolkitLocalization();
        private readonly Dictionary<ToolkitSectionId, ToolkitSection> _sections = new Dictionary<ToolkitSectionId, ToolkitSection>();
        private readonly List<NavigationItem> _navigationItems = new List<NavigationItem>();
        [SerializeField] private ToolkitLanguage _language = ToolkitLanguage.English;
        [SerializeField] private ToolkitSectionId _section = ToolkitSectionId.Configurations;
        private int _buildAttempts;
        private ToolkitContext _context;
        private VisualElement _root;
        private VisualElement _sidebar;
        private VisualElement _topBar;
        private ScrollView _content;
        private StatusBar _statusBar;
        private SegmentedControl _languageSwitch;
        private ToolkitSection _currentSection;

        [MenuItem("JTL SDK/Toolkit", false, 0)]
        public static void Open()
        {
            ToolkitWindow window = GetWindow<ToolkitWindow>();
            window.Show();
        }

        public ToolkitLanguage Language => _localization.Language;

        public ToolkitSectionId CurrentSectionId => _currentSection == null ? ToolkitSectionId.Configurations : _currentSection.Id;

        public VisualElement CurrentSectionRoot => _currentSection == null ? null : _currentSection.Root;

        public IReadOnlyList<NavigationItem> NavigationItems => _navigationItems;

        public bool Collapsed => _root != null && _root.ClassListContains(CollapsedClass);

        private void OnEnable()
        {
            titleContent = new GUIContent(Title);
            minSize = new Vector2(MinimumWidth, MinimumHeight);
        }

        private void CreateGUI()
        {
            EnsureBuilt();
        }

        private void OnDisable()
        {
            if (_context == null)
            {
                return;
            }

            _context.NavigationRequested -= OnNavigationRequested;
            _context.StatusRequested -= OnStatusRequested;
            _project.Changed -= OnProjectChanged;
        }

        public void EnsureBuilt()
        {
            if (_root != null)
            {
                return;
            }

            Build();
        }

        public void Navigate(ToolkitSectionId sectionId)
        {
            EnsureBuilt();

            if (_sections.TryGetValue(sectionId, out ToolkitSection section) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionId));
            }

            _currentSection = section;
            _section = sectionId;
            section.Render();
            _content.Clear();
            _content.Add(section.Root);
            _content.scrollOffset = Vector2.zero;
            UpdateNavigationSelection(section.NavigationId);
            ShowStatus(section.Status);
        }

        public void SetLanguage(ToolkitLanguage language)
        {
            EnsureBuilt();

            if (_localization.Language == language)
            {
                return;
            }

            _localization.Language = language;
            _language = language;
            _languageSwitch.Index = (int)language;
            LocalizeShell();

            if (_currentSection != null)
            {
                _currentSection.Render();
                ShowStatus(_currentSection.Status);
            }
        }

        private void Build()
        {
            StyleSheet tokens = _assets.FindStyleSheet(TokensStyleSheet);
            StyleSheet components = _assets.FindStyleSheet(ComponentsStyleSheet);
            StyleSheet toolkit = _assets.FindStyleSheet(ToolkitStyleSheet);
            VisualTreeAsset template = _assets.FindTemplate(WindowTemplate);

            if (tokens == null || components == null || toolkit == null || template == null)
            {
                ScheduleBuild();
                return;
            }

            _buildAttempts = 0;
            _localization.Language = _language;
            _context = new ToolkitContext(_localization, _assets, _project);
            _project.Changed += OnProjectChanged;
            _context.NavigationRequested += OnNavigationRequested;
            _context.StatusRequested += OnStatusRequested;
            rootVisualElement.styleSheets.Add(tokens);
            rootVisualElement.styleSheets.Add(components);
            rootVisualElement.styleSheets.Add(toolkit);
            template.CloneTree(rootVisualElement);
            _root = rootVisualElement.Q<VisualElement>("root");
            _sidebar = rootVisualElement.Q<VisualElement>("sidebar");
            _topBar = rootVisualElement.Q<VisualElement>("top-bar");
            _content = rootVisualElement.Q<ScrollView>("content");
            _statusBar = rootVisualElement.Q<StatusBar>("status-bar");
            _languageSwitch = rootVisualElement.Q<SegmentedControl>("language-switch");
            _languageSwitch.Index = (int)_localization.Language;
            _languageSwitch.IndexChanged += OnLanguageIndexChanged;
            _navigationItems.Clear();
            _navigationItems.AddRange(rootVisualElement.Query<NavigationItem>().ToList());

            foreach (NavigationItem item in _navigationItems)
            {
                item.Clicked += OnNavigationItemClicked;
            }

            rootVisualElement.Q<Button>("configuration-button").clicked += OnConfigurationButtonClicked;
            rootVisualElement.Q<VisualElement>("documentation-link").AddManipulator(new Clickable(OpenSupport));
            rootVisualElement.Q<VisualElement>("support-link").AddManipulator(new Clickable(OpenSupport));
            CreateSections();
            _root.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
            LocalizeShell();
            Navigate(_sections.ContainsKey(_section) ? _section : ToolkitSectionId.Configurations);
        }

        private void ScheduleBuild()
        {
            _buildAttempts++;

            if (_buildAttempts > MaximumBuildAttempts)
            {
                throw new InvalidOperationException(WindowTemplate);
            }

            EditorApplication.delayCall -= BuildWhenAssetsReady;
            EditorApplication.delayCall += BuildWhenAssetsReady;
        }

        private void BuildWhenAssetsReady()
        {
            if (this == null)
            {
                return;
            }

            EnsureBuilt();
        }

        private void CreateSections()
        {
            _sections.Clear();
            Register(new ConfigurationsSection(_context));
            Register(new ConfigurationDetailsSection(_context));
            Register(new SimulationSection(_context));
            Register(new TemplateSection(_context));
            Register(new BuildSection(_context));
            Register(new PackageManagerSection(_context));
            Register(new AnalyzerSection(_context));
            Register(new LanguagesSection(_context));
            Register(new PurchasesSection(_context));
            Register(new LeaderboardsSection(_context));
            Register(new FlagsSection(_context));
            Register(new SavesSection(_context));
            Register(new ModuleSection(_context, ToolkitSectionId.Ads, "nav.ads", "_ads"));
            Register(new ModuleSection(_context, ToolkitSectionId.Player, "nav.player", "_player"));
            Register(new ModuleSection(_context, ToolkitSectionId.Time, "nav.time", "_timeProvider"));
            Register(new ModuleSection(_context, ToolkitSectionId.Gameplay, "nav.gameplay", "_gameplay"));
            Register(new ModuleSection(_context, ToolkitSectionId.Review, "nav.review", "_review"));
            Register(new ModuleSection(_context, ToolkitSectionId.Shortcut, "nav.shortcut", "_shortcut"));
            Register(new PauseSection(_context));
            Register(new ModuleSection(_context, ToolkitSectionId.Audio, "nav.audio", "_platformProvider"));
            Register(new ModuleSection(_context, ToolkitSectionId.Platform, "nav.platform", "_platformProvider"));
        }

        private void Register(ToolkitSection section)
        {
            _sections[section.Id] = section;
        }

        private void LocalizeShell()
        {
            RefreshTopBar();
            Localize(_sidebar);
            Localize(_topBar);
            _statusBar.ApplyLocalization(_localization);
        }

        private void Localize(VisualElement element)
        {
            List<VisualElement> elements = element.Query<VisualElement>().ToList();

            foreach (VisualElement candidate in elements)
            {
                if (candidate is ILocalizedElement localized)
                {
                    localized.ApplyLocalization(_localization);
                }
            }
        }

        private void ShowStatus(ToolkitStatus status)
        {
            _statusBar.Show(status);
            _statusBar.ApplyLocalization(_localization);
        }

        private void UpdateNavigationSelection(ToolkitSectionId sectionId)
        {
            foreach (NavigationItem item in _navigationItems)
            {
                item.Selected = item.Section == sectionId;
            }
        }

        private string ReadPackageVersion()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(PackageManifestPath);
            return package == null ? FallbackVersion : package.version;
        }

        private void RefreshTopBar()
        {
            SdkConfiguration active = _project.Active;
            PlatformPresentation platforms = _context.Platforms;
            PortalMark mark = rootVisualElement.Q<PortalMark>("configuration-mark");
            mark.style.display = active == null ? DisplayStyle.None : DisplayStyle.Flex;

            if (active != null)
            {
                mark.Portal = platforms.PortalMark(active.Platform);
            }

            rootVisualElement.Q<Label>("configuration-name").text = active == null ? _localization.Get("topbar.noConfiguration") : active.DisplayName;
            rootVisualElement.Q<Label>("package-pill").text = _localization.Get("topbar.packageVersion") + " " + ReadPackageVersion();
        }

        private void OnConfigurationButtonClicked()
        {
            GenericMenu menu = new GenericMenu();
            SdkConfiguration active = _project.Active;

            foreach (SdkConfiguration configuration in _project.Configurations)
            {
                SdkConfiguration captured = configuration;
                menu.AddItem(new GUIContent(configuration.DisplayName), configuration == active, () => ActivateFromTopBar(captured));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(_localization.Get("topbar.manageConfigurations")), false, () => Navigate(ToolkitSectionId.Configurations));
            menu.ShowAsContext();
        }

        private void ActivateFromTopBar(SdkConfiguration configuration)
        {
            if (_project.Active == configuration)
            {
                return;
            }

            _project.Activate(configuration);
            _context.Report(StatusKind.Success, "configurations.activated", configuration.DisplayName);

            if (_currentSection != null)
            {
                Navigate(_currentSection.Id);
            }
        }

        private void OpenSupport()
        {
            Application.OpenURL(SupportUrl);
        }

        private void OnProjectChanged()
        {
            if (_root != null)
            {
                RefreshTopBar();
            }
        }

        private void OnNavigationItemClicked(NavigationItem item)
        {
            Navigate(item.Section);
        }

        private void OnNavigationRequested(ToolkitSectionId sectionId)
        {
            Navigate(sectionId);
        }

        private void OnStatusRequested(ToolkitStatus status)
        {
            ShowStatus(status);
        }

        private void OnLanguageIndexChanged(int index)
        {
            SetLanguage((ToolkitLanguage)index);
        }

        private void OnRootGeometryChanged(GeometryChangedEvent geometryEvent)
        {
            bool collapsed = geometryEvent.newRect.width < CollapseWidth;
            _root.EnableInClassList(CollapsedClass, collapsed);
        }
    }
}
