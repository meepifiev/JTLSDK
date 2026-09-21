using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using JTLStudio.SDK.Editor.Build;
using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class HomeSection : ToolkitSection
    {
        private const int RecentBuildCount = 5;
        private const int BuildSettingsTab = 0;
        private const int PortalSize = 40;
        private const int RowPortalSize = 22;
        private const int ActionSize = 26;
        private const float AsideWidth = 306f;
        private const float ColumnGap = 12f;
        private const float MinimumMainWidth = 460f;
        private const float MinimumHeroWidthForPlatforms = 560f;
        private const float CompactBuildsWidth = 640f;
        private const float Megabyte = 1048576f;
        private const string BuildTarget = "WebGL";
        private const string CellPrefix = "jtl-home-builds__cell--";

        private readonly SdkBuildService _builds = new SdkBuildService();
        private readonly PlayerSettingsPresetService _presets = new PlayerSettingsPresetService();
        private readonly PlatformId[] _platforms = { PlatformId.YandexGames, PlatformId.YouTubePlayables };
        private int _tab = BuildSettingsTab;

        public HomeSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Home;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "HomeSection";

        private JTLSDKEditorSettings Settings => JTLSDKEditorSettings.instance;

        protected override void OnRendered()
        {
            VisualElement body = Require<VisualElement>("home-body");
            VisualElement main = Column(12);
            main.AddToClassList("jtl-home__main");
            main.Add(CreateHero());
            main.Add(CreateRecentBuilds());
            body.Add(main);

            VisualElement aside = CreateAside();
            body.Add(aside);
            body.RegisterCallback<GeometryChangedEvent>(geometryEvent => ArrangeColumns(body, main, aside));
        }

        private void ArrangeColumns(VisualElement body, VisualElement main, VisualElement aside)
        {
            bool stacked = body.resolvedStyle.width < MinimumMainWidth + ColumnGap + AsideWidth;
            FlexDirection direction = stacked ? FlexDirection.Column : FlexDirection.Row;

            if (body.resolvedStyle.flexDirection == direction)
            {
                return;
            }

            body.style.flexDirection = direction;
            main.style.flexBasis = stacked ? new StyleLength(StyleKeyword.Auto) : new StyleLength(0f);
            main.style.flexGrow = stacked ? 0 : 1;
            aside.style.width = stacked ? new StyleLength(StyleKeyword.Auto) : new StyleLength(AsideWidth);
            aside.style.marginLeft = stacked ? 0 : ColumnGap;
            aside.style.marginTop = stacked ? ColumnGap : 0;
        }

        private VisualElement CreateHero()
        {
            VisualElement hero = new VisualElement();
            hero.AddToClassList("jtl-home-hero");
            VisualElement back = Decoration("jtl-home-hero__glass", "jtl-home-hero__glass--back");
            VisualElement front = Decoration("jtl-home-hero__glass", "jtl-home-hero__glass--front");
            hero.Add(back);
            hero.Add(front);

            VisualElement content = new VisualElement();
            content.AddToClassList("jtl-home-hero__content");

            VisualElement copy = new VisualElement();
            copy.AddToClassList("jtl-home-hero__copy");
            copy.Add(Localized("home.title", "jtl-home-hero__title"));
            copy.Add(Localized("home.titleAccent", "jtl-home-hero__title", "jtl-home-hero__title--accent"));
            copy.Add(Localized("home.description", "jtl-home-hero__description"));
            ToolkitButton start = Button("home.start", ToolkitButton.PrimaryVariant, "", () => NavigateTo(ToolkitSectionId.Configurations));
            start.AddToClassList("jtl-home-hero__start");
            copy.Add(start);
            content.Add(copy);

            VisualElement platforms = new VisualElement();
            platforms.AddToClassList("jtl-home-hero__platforms");
            VisualElement tiles = Row(10);
            tiles.AddToClassList("jtl-row--start");

            foreach (PlatformId platform in _platforms)
            {
                tiles.Add(PlatformTile(platform));
            }

            platforms.Add(tiles);
            VisualElement tagline = new VisualElement();
            tagline.AddToClassList("jtl-home-hero__tagline");

            foreach (string line in Context.Localization.GetList("home.tagline"))
            {
                tagline.Add(TextLabel(line, "jtl-home-hero__tagline-line"));
            }

            platforms.Add(tagline);
            content.Add(platforms);
            hero.Add(content);
            hero.RegisterCallback<GeometryChangedEvent>(geometryEvent =>
            {
                DisplayStyle display = geometryEvent.newRect.width < MinimumHeroWidthForPlatforms ? DisplayStyle.None : DisplayStyle.Flex;
                platforms.style.display = display;
                back.style.display = display;
                front.style.display = display;
            });
            return hero;
        }

        private VisualElement Decoration(params string[] classNames)
        {
            VisualElement element = new VisualElement { pickingMode = PickingMode.Ignore };

            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }

        private VisualElement PlatformTile(PlatformId platform)
        {
            VisualElement tile = new VisualElement();
            tile.AddToClassList("jtl-home-tile");
            tile.Add(new PortalMark(Context.Platforms.PortalMark(platform), PortalSize));
            tile.Add(TextLabel(Context.Platforms.DisplayName(platform), "jtl-home-tile__name"));
            tile.AddManipulator(new Clickable(() => OpenPlatform(platform)));
            return tile;
        }

        private void OpenPlatform(PlatformId platform)
        {
            SdkConfiguration configuration = Context.Project.Find(platform);

            if (configuration == null)
            {
                NavigateTo(ToolkitSectionId.Configurations);
                return;
            }

            Context.SelectedConfiguration = configuration;
            NavigateTo(ToolkitSectionId.ConfigurationDetails);
        }

        private VisualElement CreateRecentBuilds()
        {
            Card card = new Card { TitleKey = "home.recentBuilds", Spacing = 0 };
            card.AddToClassList("jtl-home-builds");
            LocalizedLabel open = Localized("home.openBuild", "jtl-home-link");
            open.AddManipulator(new Clickable(() => NavigateTo(ToolkitSectionId.Build)));
            card.Header.Add(open);

            IReadOnlyList<BuildRecord> records = BuildHistory.instance.Records;

            if (records.Count == 0)
            {
                card.Add(Localized("home.noBuilds", "jtl-text--secondary", "jtl-home-builds__empty"));
                return card;
            }

            VisualElement head = BuildRow("jtl-home-builds__head");
            head.Add(Cell("name", Localized("home.columnName", "jtl-home-builds__heading")));
            head.Add(Cell("platform", Localized("home.columnPlatform", "jtl-home-builds__heading")));
            head.Add(Cell("size", Localized("home.columnSize", "jtl-home-builds__heading")));
            head.Add(Cell("status", Localized("home.columnStatus", "jtl-home-builds__heading")));
            head.Add(Cell("date", Localized("home.columnDate", "jtl-home-builds__heading")));
            head.Add(Cell("action", new VisualElement()));
            card.Add(head);

            card.RegisterCallback<GeometryChangedEvent>(geometryEvent => card.EnableInClassList("jtl-home-builds--compact", geometryEvent.newRect.width < CompactBuildsWidth));
            int count = Math.Min(RecentBuildCount, records.Count);

            for (int index = 0; index < count; index++)
            {
                card.Add(RecordRow(records[index], index == 0));
            }

            return card;
        }

        private VisualElement RecordRow(BuildRecord record, bool first)
        {
            VisualElement row = BuildRow("jtl-home-builds__row");
            row.EnableInClassList("jtl-home-builds__row--first", first);

            Label name = TextLabel(record.Name, "jtl-home-builds__text");
            name.tooltip = record.Path;
            row.Add(Cell("name", name));

            VisualElement platform = Row(9);
            platform.Add(new PortalMark(Context.Platforms.PortalMark(record.Platform), RowPortalSize));
            platform.Add(TextLabel(Context.Platforms.DisplayName(record.Platform), "jtl-home-builds__text"));
            row.Add(Cell("platform", platform));

            string size = record.IsSuccess ? Context.Text("home.size", (record.Bytes / Megabyte).ToString("0.0", CultureInfo.InvariantCulture)) : "—";
            row.Add(Cell("size", TextLabel(size, "jtl-home-builds__text", "jtl-text--secondary")));

            VisualElement status = Row(7);
            VisualElement dot = new VisualElement();
            dot.AddToClassList("jtl-home-builds__dot");
            dot.AddToClassList(record.IsSuccess ? "jtl-home-builds__dot--success" : "jtl-home-builds__dot--error");
            status.Add(dot);
            status.Add(Localized(record.IsSuccess ? "home.success" : "home.failed", "jtl-home-builds__text"));
            row.Add(Cell("status", status));

            row.Add(Cell("date", TextLabel(FormatTime(record.Time), "jtl-home-builds__text", "jtl-text--secondary")));

            IconButton reveal = new IconButton("folder", ActionSize) { Variant = IconButton.GhostVariant, tooltip = Context.Text("home.reveal") };
            reveal.clicked += () => EditorUtility.RevealInFinder(record.Path);
            reveal.SetEnabled(File.Exists(record.Path) || Directory.Exists(record.Path));
            row.Add(Cell("action", reveal));
            return row;
        }

        private VisualElement BuildRow(string className)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-home-builds__line");
            row.AddToClassList(className);
            return row;
        }

        private VisualElement Cell(string column, VisualElement content)
        {
            VisualElement cell = new VisualElement();
            cell.AddToClassList("jtl-home-builds__cell");
            cell.AddToClassList(CellPrefix + column);
            cell.Add(content);
            return cell;
        }

        private string FormatTime(DateTime time)
        {
            string clock = time.ToString("HH:mm", CultureInfo.InvariantCulture);
            DateTime today = DateTime.Today;

            if (time.Date == today)
            {
                return Context.Text("home.today", clock);
            }

            if (time.Date == today.AddDays(-1))
            {
                return Context.Text("home.yesterday", clock);
            }

            return time.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) + ", " + clock;
        }

        private VisualElement CreateAside()
        {
            VisualElement aside = new VisualElement();
            aside.AddToClassList("jtl-home-aside");
            SdkConfiguration active = Context.Project.Active;

            aside.Add(Localized("home.activeTarget", "jtl-home-aside__title"));
            aside.Add(TargetButton(active));

            if (active == null)
            {
                aside.Add(Localized("home.noTarget", "jtl-home-aside__note"));
                aside.Add(Button("home.createConfiguration", ToolkitButton.SecondaryVariant, "plus", () => NavigateTo(ToolkitSectionId.Configurations)));
                return aside;
            }

            SegmentedControl tabs = new SegmentedControl { ChoicesKey = "home.tabs" };
            tabs.AddToClassList("jtl-home-tabs");
            tabs.SetChoices(Context.Localization.GetList("home.tabs"));
            tabs.Index = _tab;
            tabs.IndexChanged += index =>
            {
                _tab = index;
                Render();
            };
            aside.Add(tabs);

            if (_tab == BuildSettingsTab)
            {
                AddBuildSettings(aside, active);
            }
            else
            {
                AddPlayerSettings(aside, active);
            }

            aside.Add(Spacer());

            ToolkitButton advanced = Button("home.advanced", ToolkitButton.SecondaryVariant, "", () => OpenAdvanced(active));
            advanced.AddToClassList("jtl-home-aside__advanced");
            advanced.TrailingIconName = "chevron-right";
            aside.Add(advanced);

            List<BuildCheck> checks = _builds.PreChecks(active);
            BuildCheck failed = checks.Find(check => check.Passed == false);
            ToolkitButton build = Button("home.buildProject", ToolkitButton.PrimaryVariant, "build", () => RunBuild(active));
            build.AddToClassList("jtl-home-aside__build");
            build.SetEnabled(failed == null);

            if (failed != null)
            {
                build.tooltip = Context.Text(failed.Key, failed.Arguments);
            }

            aside.Add(build);
            return aside;
        }

        private VisualElement TargetButton(SdkConfiguration active)
        {
            Button button = new Button(ShowTargetMenu);
            button.AddToClassList("jtl-button");
            button.AddToClassList("jtl-button--dropdown");
            button.AddToClassList("jtl-home-aside__target");

            if (active != null)
            {
                PortalMark mark = new PortalMark(Context.Platforms.PortalMark(active.Platform), 16);
                mark.AddToClassList("jtl-mr-8");
                button.Add(mark);
            }

            Label name = TextLabel(active == null ? Context.Text("topbar.noConfiguration") : active.DisplayName, "jtl-button__label");
            button.Add(name);
            Icon chevron = new Icon("chevron-down", Icon.DefaultSize, "muted");
            chevron.AddToClassList("jtl-button__trailing");
            button.Add(chevron);
            return button;
        }

        private void ShowTargetMenu()
        {
            GenericMenu menu = new GenericMenu();
            SdkConfiguration active = Context.Project.Active;

            foreach (SdkConfiguration configuration in Context.Project.Configurations)
            {
                SdkConfiguration captured = configuration;
                menu.AddItem(new UnityEngine.GUIContent(configuration.DisplayName), configuration == active, () => Activate(captured));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new UnityEngine.GUIContent(Context.Text("topbar.manageConfigurations")), false, () => NavigateTo(ToolkitSectionId.Configurations));
            menu.ShowAsContext();
        }

        private void Activate(SdkConfiguration configuration)
        {
            if (Context.Project.Active == configuration)
            {
                return;
            }

            Context.Project.Activate(configuration);
            Context.Report(StatusKind.Success, "configurations.activated", configuration.DisplayName);
            Render();
        }

        private void AddBuildSettings(VisualElement aside, SdkConfiguration active)
        {
            PlayerSettingsPreset preset = active.PlayerSettings;
            VisualElement platform = Column(9);
            platform.Add(Localized("home.platformConfiguration", "jtl-home-aside__title"));
            platform.Add(AsideField("home.buildTarget", ReadOnlyBox(BuildTarget)));

            WebCompression compression = preset.ApplyCompression ? preset.Compression : _presets.CurrentCompression();
            bool compressionInvalid = active.Platform == PlatformId.YouTubePlayables && compression != WebCompression.Disabled;
            platform.Add(AsideField("home.compression", EnumDropdown(compression, compressionInvalid, value => ChangePreset(active, () =>
            {
                preset.Compression = value;
                preset.ApplyCompression = true;
            }))));
            aside.Add(platform);

            VisualElement checks = new VisualElement();
            checks.AddToClassList("jtl-column");
            checks.Add(CheckLine("Name Files As Hashes", PlayerSettings.WebGL.nameFilesAsHashes, value =>
            {
                PlayerSettings.WebGL.nameFilesAsHashes = value;
                AssetDatabase.SaveAssets();
            }));
            checks.Add(CheckLine("Data Caching", preset.ApplyDataCaching ? preset.DataCaching : PlayerSettings.WebGL.dataCaching, value => ChangePreset(active, () =>
            {
                preset.DataCaching = value;
                preset.ApplyDataCaching = true;
            })));
            checks.Add(CheckLine("Debug Symbols", preset.ApplyDebugSymbols ? preset.DebugSymbols : PlayerSettings.WebGL.debugSymbolMode != WebGLDebugSymbolMode.Off, value => ChangePreset(active, () =>
            {
                preset.DebugSymbols = value;
                preset.ApplyDebugSymbols = true;
            })));
            checks.Add(CheckLine("Decompression Fallback", preset.ApplyDecompressionFallback ? preset.DecompressionFallback : PlayerSettings.WebGL.decompressionFallback, value => ChangePreset(active, () =>
            {
                preset.DecompressionFallback = value;
                preset.ApplyDecompressionFallback = true;
            })));
            aside.Add(checks);

            VisualElement separator = new VisualElement();
            separator.AddToClassList("jtl-home-aside__separator");
            aside.Add(separator);
            aside.Add(Localized("home.outputSettings", "jtl-home-aside__title"));

            VisualElement folder = Column(8);
            folder.Add(Localized("home.buildFolder", "jtl-home-aside__label"));
            VisualElement folderLine = Row(8);
            folderLine.style.flexWrap = Wrap.NoWrap;
            folderLine.Add(TextInput(Settings.BuildPath, value => Settings.BuildPath = value));
            IconButton browse = new IconButton("folder", IconButton.DefaultSize) { Variant = IconButton.GhostVariant, tooltip = Context.Text("build.browse") };
            browse.clicked += Browse;
            folderLine.Add(browse);
            folder.Add(folderLine);
            VisualElement folderActions = Row(8);
            folderActions.Add(Button("home.openFolder", ToolkitButton.SecondaryVariant, "", OpenBuildFolder));
            folderActions.Add(Button("home.reset", ToolkitButton.GhostVariant, "", () => ChangeSettings(() => Settings.BuildPath = "")));
            folder.Add(folderActions);
            aside.Add(folder);

            VisualElement name = Column(8);
            name.Add(Localized("home.buildFileName", "jtl-home-aside__label"));
            VisualElement nameLine = Row(8);
            nameLine.style.flexWrap = Wrap.NoWrap;
            TextField nameField = TextInput(Settings.BuildNamePattern, value => Settings.BuildNamePattern = value);
            nameField.tooltip = _builds.ResolveName(Settings, active, Settings.BuildNumber + 1);
            nameLine.Add(nameField);
            Dropdown output = new Dropdown();
            output.AddToClassList("jtl-home-aside__output");
            output.choices = new List<string>(Context.Localization.GetList("home.outputs"));
            output.index = (int)Settings.BuildOutput;
            output.RegisterValueChangedCallback(changeEvent => ChangeSettings(() => Settings.BuildOutput = (BuildOutput)output.index));
            nameLine.Add(output);
            name.Add(nameLine);
            aside.Add(name);
        }

        private void AddPlayerSettings(VisualElement aside, SdkConfiguration active)
        {
            PlayerSettingsPreset preset = active.PlayerSettings;
            VisualElement product = Column(9);
            product.Add(Localized("home.productSettings", "jtl-home-aside__title"));
            product.Add(AsideField("home.productName", PlayerInput(PlayerSettings.productName, value => PlayerSettings.productName = value)));
            product.Add(AsideField("home.companyName", PlayerInput(PlayerSettings.companyName, value => PlayerSettings.companyName = value)));
            product.Add(AsideField("home.version", PlayerInput(PlayerSettings.bundleVersion, value => PlayerSettings.bundleVersion = value)));
            aside.Add(product);

            VisualElement runtime = Column(9);
            runtime.Add(Localized("home.runtimeSettings", "jtl-home-aside__title"));
            StrippingLevel stripping = preset.ApplyStripping ? preset.Stripping : _presets.CurrentStripping();
            runtime.Add(AsideField("home.stripping", EnumDropdown(stripping, false, value => ChangePreset(active, () =>
            {
                preset.Stripping = value;
                preset.ApplyStripping = true;
            }))));
            aside.Add(runtime);

            VisualElement checks = new VisualElement();
            checks.AddToClassList("jtl-column");
            checks.Add(CheckLine("Run In Background", preset.ApplyRunInBackground ? preset.RunInBackground : PlayerSettings.runInBackground, value => ChangePreset(active, () =>
            {
                preset.RunInBackground = value;
                preset.ApplyRunInBackground = true;
            })));
            aside.Add(checks);
        }

        private VisualElement AsideField(string labelKey, VisualElement control)
        {
            VisualElement row = Row(10);
            row.style.flexWrap = Wrap.NoWrap;
            row.Add(Localized(labelKey, "jtl-home-aside__label", "jtl-home-aside__field-label"));
            Fill(control);
            row.Add(control);
            return row;
        }

        private VisualElement ReadOnlyBox(string text)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("jtl-field-box");
            box.AddToClassList("jtl-read-only");
            box.Add(TextLabel(text, "jtl-read-only__text"));
            return box;
        }

        private Dropdown EnumDropdown<T>(T value, bool error, Action<T> change) where T : Enum
        {
            Dropdown dropdown = new Dropdown { Error = error };
            dropdown.choices = new List<string>(Enum.GetNames(typeof(T)));
            dropdown.index = Convert.ToInt32(value);
            dropdown.RegisterValueChangedCallback(changeEvent =>
            {
                if (dropdown.index >= 0)
                {
                    change((T)Enum.ToObject(typeof(T), dropdown.index));
                }
            });
            return dropdown;
        }

        private VisualElement CheckLine(string text, bool value, Action<bool> change)
        {
            Checkbox checkbox = new Checkbox(text, value);
            checkbox.AddToClassList("jtl-home-aside__check");
            checkbox.ValueChanged += change;
            return checkbox;
        }

        private TextField TextInput(string value, Action<string> assign)
        {
            TextField field = new TextField { value = value };
            field.AddToClassList("jtl-field");
            Fill(field);
            field.RegisterCallback<FocusOutEvent>(focusEvent => ChangeSettings(() => assign(field.value)));
            return field;
        }

        private void Fill(VisualElement control)
        {
            control.style.flexGrow = 1;
            control.style.flexShrink = 1;
            control.style.flexBasis = 0;
            control.style.minWidth = 0;
        }

        private TextField PlayerInput(string value, Action<string> assign)
        {
            TextField field = new TextField { value = value };
            field.AddToClassList("jtl-field");
            field.RegisterCallback<FocusOutEvent>(focusEvent =>
            {
                assign(field.value);
                AssetDatabase.SaveAssets();
            });
            return field;
        }

        private void ChangePreset(SdkConfiguration active, Action change)
        {
            Context.Project.Modify(active, "Change player settings", change);
            Context.Project.ApplyPreset(active);
            Render();
        }

        private void ChangeSettings(Action change)
        {
            change();
            Settings.Persist();
            Render();
        }

        private void Browse()
        {
            string selected = EditorUtility.OpenFolderPanel(Context.Text("home.buildFolder"), Settings.BuildPath, "");

            if (string.IsNullOrEmpty(selected))
            {
                return;
            }

            string project = Path.GetFullPath(".").Replace('\\', '/') + "/";
            string normalized = selected.Replace('\\', '/');
            string value = normalized.StartsWith(project) ? normalized.Substring(project.Length) : normalized;
            ChangeSettings(() => Settings.BuildPath = value);
        }

        private void OpenBuildFolder()
        {
            string folder = Path.GetFullPath(Settings.BuildPath);
            Directory.CreateDirectory(folder);
            EditorUtility.RevealInFinder(folder);
        }

        private void OpenAdvanced(SdkConfiguration active)
        {
            if (_tab == BuildSettingsTab)
            {
                NavigateTo(ToolkitSectionId.Build);
                return;
            }

            Context.SelectedConfiguration = active;
            NavigateTo(ToolkitSectionId.ConfigurationDetails);
        }

        private void RunBuild(SdkConfiguration active)
        {
            BuildResult result = _builds.Build(Settings, active);

            if (result.IsSuccess)
            {
                Context.Report(StatusKind.Success, "build.done", Settings.BuildNumber, (result.TotalBytes / Megabyte).ToString("0.0", CultureInfo.InvariantCulture), result.OutputPath);
            }
            else
            {
                Context.Report(StatusKind.Error, "build.failed", string.IsNullOrEmpty(result.Error) ? Context.Text("home.failed") : result.Error);
            }

            Render();
        }
    }
}
