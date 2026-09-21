using System;
using System.Collections.Generic;
using JTLStudio.SDK.Editor.Toolkit.Components;
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
        private const string VersionPrefix = "v";
        private const string WindowTemplate = "ToolkitWindow.uxml";
        private const string TokensStyleSheet = "Styles/Tokens.uss";
        private const string ToolkitStyleSheet = "Styles/Toolkit.uss";
        private const string ComponentsStyleSheet = "Styles/Components.uss";

        private readonly ToolkitAssets _assets = new ToolkitAssets();
        private readonly ToolkitLocalization _localization = new ToolkitLocalization();
        private readonly Dictionary<ToolkitSectionId, ToolkitSection> _sections = new Dictionary<ToolkitSectionId, ToolkitSection>();
        private readonly List<NavigationItem> _navigationItems = new List<NavigationItem>();
        private ToolkitContext _context;
        private VisualElement _root;
        private VisualElement _sidebar;
        private VisualElement _topBar;
        private ScrollView _content;
        private StatusBar _statusBar;
        private SegmentedControl _languageSwitch;
        private ToolkitSection _currentSection;

        [MenuItem("Window/JTL SDK/Toolkit", false, 0)]
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
            _context = new ToolkitContext(_localization, _assets);
            _context.NavigationRequested += OnNavigationRequested;
            _context.StatusRequested += OnStatusRequested;
            rootVisualElement.styleSheets.Add(_assets.LoadStyleSheet(TokensStyleSheet));
            rootVisualElement.styleSheets.Add(_assets.LoadStyleSheet(ToolkitStyleSheet));
            rootVisualElement.styleSheets.Add(_assets.LoadStyleSheet(ComponentsStyleSheet));
            _assets.LoadTemplate(WindowTemplate).CloneTree(rootVisualElement);
            _root = rootVisualElement.Q<VisualElement>("root");
            _sidebar = rootVisualElement.Q<VisualElement>("sidebar");
            _topBar = rootVisualElement.Q<VisualElement>("top-bar");
            _content = rootVisualElement.Q<ScrollView>("content");
            _statusBar = rootVisualElement.Q<StatusBar>("status-bar");
            _languageSwitch = rootVisualElement.Q<SegmentedControl>("language-switch");
            _languageSwitch.Index = (int)_localization.Language;
            _languageSwitch.IndexChanged += OnLanguageIndexChanged;
            rootVisualElement.Q<Label>("version").text = VersionPrefix + ReadPackageVersion();
            _navigationItems.Clear();
            _navigationItems.AddRange(rootVisualElement.Query<NavigationItem>().ToList());

            foreach (NavigationItem item in _navigationItems)
            {
                item.Clicked += OnNavigationItemClicked;
            }

            CreateSections();
            _root.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
            LocalizeShell();
            Navigate(ToolkitSectionId.Configurations);
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
        }

        private void Register(ToolkitSection section)
        {
            _sections[section.Id] = section;
        }

        private void LocalizeShell()
        {
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
