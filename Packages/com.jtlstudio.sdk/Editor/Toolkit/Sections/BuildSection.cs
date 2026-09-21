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
    public class BuildSection : ToolkitSection
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
        private const string TargetName = "WebGL";
        private const string CellPrefix = "jtl-build-history__cell--";

        private readonly SdkBuildService _builds = new SdkBuildService();
        private readonly PlayerSettingsPresetService _presets = new PlayerSettingsPresetService();
        private readonly PlatformId[] _platforms = { PlatformId.YandexGames, PlatformId.YouTubePlayables };
        private int _tab = BuildSettingsTab;
        private bool _advanced;
        private BuildResult _lastResult;

        public BuildSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Build;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "BuildSection";

        private JTLSDKEditorSettings Settings => JTLSDKEditorSettings.instance;

        protected override void OnRendered()
        {
            VisualElement body = Require<VisualElement>("build-body");
            VisualElement main = Column(12);
            main.AddToClassList("jtl-build__main");
            main.Add(CreateHero());
            main.Add(CreateRecentBuilds());
            SdkConfiguration active = Context.Project.Active;
            main.Add(CreateChecksCard("build.preChecks", _builds.PreChecks(active)));

            if (_lastResult != null && _lastResult.PostChecks.Count > 0)
            {
                main.Add(CreateChecksCard("build.postChecks", new List<BuildCheck>(_lastResult.PostChecks)));
            }

            if (_lastResult != null && _lastResult.IsSuccess == false)
            {
                main.Add(new InlineMessage { Variant = InlineMessage.ErrorVariant, Text = FailureText(_lastResult) });
            }

            body.Add(main);

            VisualElement aside = CreateAside();
            body.Add(aside);
            body.RegisterCallback<GeometryChangedEvent>(geometryEvent => ArrangeColumns(body, main, aside));
        }

        private void ArrangeColumns(VisualElement body, VisualElement main, VisualElement aside)
        {
            bool stacked = body.resolvedStyle.width < MinimumMainWidth + ColumnGap + AsideWidth;
            FlexDirection direction = stacked ? FlexDirection.Column : FlexDirection.Row;
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
            hero.AddToClassList("jtl-build-hero");
            VisualElement back = Decoration("jtl-build-hero__glass", "jtl-build-hero__glass--back");
            VisualElement front = Decoration("jtl-build-hero__glass", "jtl-build-hero__glass--front");
            hero.Add(back);
            hero.Add(front);

            VisualElement content = new VisualElement();
            content.AddToClassList("jtl-build-hero__content");

            VisualElement copy = new VisualElement();
            copy.AddToClassList("jtl-build-hero__copy");
            copy.Add(Localized("build.heroTitle", "jtl-build-hero__title"));
            copy.Add(Localized("build.heroAccent", "jtl-build-hero__title", "jtl-build-hero__title--accent"));
            copy.Add(Localized("build.heroDescription", "jtl-build-hero__description"));
            ToolkitButton start = Button("build.start", ToolkitButton.PrimaryVariant, "", () => NavigateTo(ToolkitSectionId.Configurations));
            start.AddToClassList("jtl-build-hero__start");
            copy.Add(start);
            content.Add(copy);

            VisualElement platforms = new VisualElement();
            platforms.AddToClassList("jtl-build-hero__platforms");
            VisualElement tiles = Row(10);
            tiles.AddToClassList("jtl-row--start");

            foreach (PlatformId platform in _platforms)
            {
                tiles.Add(PlatformTile(platform));
            }

            platforms.Add(tiles);
            VisualElement tagline = new VisualElement();
            tagline.AddToClassList("jtl-build-hero__tagline");

            foreach (string line in Context.Localization.GetList("build.tagline"))
            {
                tagline.Add(TextLabel(line, "jtl-build-hero__tagline-line"));
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
            tile.AddToClassList("jtl-build-tile");
            tile.Add(new PortalMark(Context.Platforms.PortalMark(platform), PortalSize));
            tile.Add(TextLabel(Context.Platforms.DisplayName(platform), "jtl-build-tile__name"));
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
            Card card = new Card { TitleKey = "build.recentBuilds", Spacing = 0 };
            card.AddToClassList("jtl-build-history");

            IReadOnlyList<BuildRecord> records = BuildHistory.instance.Records;

            if (records.Count == 0)
            {
                card.Add(Localized("build.noBuilds", "jtl-text--secondary", "jtl-build-history__empty"));
                return card;
            }

            VisualElement head = BuildRow("jtl-build-history__head");
            head.Add(Cell("name", Localized("build.columnName", "jtl-build-history__heading")));
            head.Add(Cell("platform", Localized("build.columnPlatform", "jtl-build-history__heading")));
            head.Add(Cell("size", Localized("build.columnSize", "jtl-build-history__heading")));
            head.Add(Cell("status", Localized("build.columnStatus", "jtl-build-history__heading")));
            head.Add(Cell("date", Localized("build.columnDate", "jtl-build-history__heading")));
            head.Add(Cell("action", new VisualElement()));
            card.Add(head);

            card.RegisterCallback<GeometryChangedEvent>(geometryEvent => card.EnableInClassList("jtl-build-history--compact", geometryEvent.newRect.width < CompactBuildsWidth));
            int count = Math.Min(RecentBuildCount, records.Count);

            for (int index = 0; index < count; index++)
            {
                card.Add(RecordRow(records[index], index == 0));
            }

            return card;
        }

        private VisualElement RecordRow(BuildRecord record, bool first)
        {
            VisualElement row = BuildRow("jtl-build-history__row");
            row.EnableInClassList("jtl-build-history__row--first", first);

            Label name = TextLabel(record.Name, "jtl-build-history__text");
            name.tooltip = record.Path;
            row.Add(Cell("name", name));

            VisualElement platform = Row(9);
            platform.Add(new PortalMark(Context.Platforms.PortalMark(record.Platform), RowPortalSize));
            platform.Add(TextLabel(Context.Platforms.DisplayName(record.Platform), "jtl-build-history__text"));
            row.Add(Cell("platform", platform));

            string size = record.IsSuccess ? Context.Text("build.size", (record.Bytes / SdkBuildService.BytesPerMegabyte).ToString("0.0", CultureInfo.InvariantCulture)) : "—";
            row.Add(Cell("size", TextLabel(size, "jtl-build-history__text", "jtl-text--secondary")));

            VisualElement status = Row(7);
            VisualElement dot = new VisualElement();
            dot.AddToClassList("jtl-build-history__dot");
            dot.AddToClassList(record.IsSuccess ? "jtl-build-history__dot--success" : "jtl-build-history__dot--error");
            status.Add(dot);
            status.Add(Localized(record.IsSuccess ? "build.statusSuccess" : "build.statusFailed", "jtl-build-history__text"));
            row.Add(Cell("status", status));

            row.Add(Cell("date", TextLabel(FormatTime(record.Time), "jtl-build-history__text", "jtl-text--secondary")));

            IconButton reveal = new IconButton("folder", ActionSize) { Variant = IconButton.GhostVariant, tooltip = Context.Text("build.reveal") };
            reveal.clicked += () => EditorUtility.RevealInFinder(record.Path);
            reveal.SetEnabled(File.Exists(record.Path) || Directory.Exists(record.Path));
            row.Add(Cell("action", reveal));
            return row;
        }

        private VisualElement BuildRow(string className)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-build-history__line");
            row.AddToClassList(className);
            return row;
        }

        private VisualElement Cell(string column, VisualElement content)
        {
            VisualElement cell = new VisualElement();
            cell.AddToClassList("jtl-build-history__cell");
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
                return Context.Text("build.today", clock);
            }

            if (time.Date == today.AddDays(-1))
            {
                return Context.Text("build.yesterday", clock);
            }

            return time.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) + ", " + clock;
        }

        private VisualElement CreateChecksCard(string titleKey, List<BuildCheck> checks)
        {
            int passed = checks.FindAll(check => check.Passed).Count;
            Card card = new Card { TitleKey = titleKey, Spacing = 8 };
            card.Header.Add(new Badge { Text = passed + " / " + checks.Count, Variant = passed == checks.Count ? Badge.SuccessVariant : Badge.ErrorVariant });

            foreach (BuildCheck check in checks)
            {
                card.Add(new CheckRow { Text = Context.Text(check.Key, check.Arguments), State = check.Passed ? CheckRow.PassState : CheckRow.FailState });
            }

            return card;
        }

        private string FailureText(BuildResult result)
        {
            if (string.IsNullOrEmpty(result.Error) == false)
            {
                return result.Error;
            }

            foreach (BuildCheck check in result.PostChecks)
            {
                if (check.Passed == false)
                {
                    return Context.Text(check.Key, check.Arguments);
                }
            }

            return Context.Text("build.statusFailed");
        }

        private VisualElement CreateAside()
        {
            VisualElement aside = new VisualElement();
            aside.AddToClassList("jtl-build-aside");
            SdkConfiguration active = Context.Project.Active;

            aside.Add(Localized("build.activeTarget", "jtl-build-aside__title"));
            aside.Add(TargetButton(active));

            if (active == null)
            {
                aside.Add(Localized("build.noTarget", "jtl-build-aside__note"));
                aside.Add(Button("build.createConfiguration", ToolkitButton.SecondaryVariant, "plus", () => NavigateTo(ToolkitSectionId.Configurations)));
                return aside;
            }

            SegmentedControl tabs = new SegmentedControl { ChoicesKey = "build.tabs" };
            tabs.AddToClassList("jtl-build-tabs");
            tabs.SetChoices(Context.Localization.GetList("build.tabs"));
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

            ToolkitButton advanced = Button("build.advanced", ToolkitButton.SecondaryVariant, "", () => OpenAdvanced(active));
            advanced.AddToClassList("jtl-build-aside__advanced");
            advanced.TrailingIconName = _tab == BuildSettingsTab && _advanced ? "chevron-down" : "chevron-right";
            aside.Add(advanced);

            List<BuildCheck> checks = _builds.PreChecks(active);
            BuildCheck failed = checks.Find(check => check.Passed == false);
            ToolkitButton build = Button("build.buildProject", ToolkitButton.PrimaryVariant, "build", () => RunBuild(active));
            build.AddToClassList("jtl-build-aside__build");
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
            button.AddToClassList("jtl-build-aside__target");

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
            platform.Add(Localized("build.platformConfiguration", "jtl-build-aside__title"));
            platform.Add(AsideField("build.buildTarget", ReadOnlyBox(TargetName)));

            WebCompression compression = preset.ApplyCompression ? preset.Compression : _presets.CurrentCompression();
            bool compressionInvalid = active.Platform == PlatformId.YouTubePlayables && compression != WebCompression.Disabled;
            platform.Add(AsideField("build.compression", EnumDropdown(compression, compressionInvalid, value => ChangePreset(active, () =>
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
            separator.AddToClassList("jtl-build-aside__separator");
            aside.Add(separator);
            aside.Add(Localized("build.outputSettings", "jtl-build-aside__title"));

            VisualElement folder = Column(8);
            folder.Add(Localized("build.buildFolder", "jtl-build-aside__label"));
            VisualElement folderLine = Row(8);
            folderLine.style.flexWrap = Wrap.NoWrap;
            folderLine.Add(TextInput(Settings.BuildPath, value => Settings.BuildPath = value));
            IconButton browse = new IconButton("folder", IconButton.DefaultSize) { Variant = IconButton.GhostVariant, tooltip = Context.Text("build.browse") };
            browse.clicked += Browse;
            folderLine.Add(browse);
            folder.Add(folderLine);
            VisualElement folderActions = Row(8);
            folderActions.Add(Button("build.openOutput", ToolkitButton.SecondaryVariant, "", OpenBuildFolder));
            folderActions.Add(Button("build.reset", ToolkitButton.GhostVariant, "", () => ChangeSettings(() => Settings.BuildPath = "")));
            folder.Add(folderActions);
            aside.Add(folder);

            VisualElement name = Column(8);
            name.Add(Localized("build.buildFileName", "jtl-build-aside__label"));
            VisualElement nameLine = Row(8);
            nameLine.style.flexWrap = Wrap.NoWrap;
            TextField nameField = TextInput(Settings.BuildNamePattern, value => Settings.BuildNamePattern = value);
            nameField.tooltip = _builds.ResolveName(Settings, active, Settings.BuildNumber + 1);
            nameLine.Add(nameField);
            Dropdown output = new Dropdown();
            output.AddToClassList("jtl-build-aside__output");
            output.choices = new List<string>(Context.Localization.GetList("build.outputs"));
            output.index = (int)Settings.BuildOutput;
            output.RegisterValueChangedCallback(changeEvent => ChangeSettings(() => Settings.BuildOutput = (BuildOutput)output.index));
            nameLine.Add(output);
            name.Add(nameLine);
            string outputPath = Path.Combine(Settings.BuildPath, _builds.ResolveName(Settings, active, Settings.BuildNumber + 1)) + (Settings.BuildOutput == BuildOutput.Zip ? ".zip" : "");
            Label outputLabel = TextLabel(outputPath, "jtl-build-aside__output-path", MonospaceFont.ClassName);
            outputLabel.tooltip = outputPath;
            name.Add(outputLabel);
            aside.Add(name);

            if (_advanced)
            {
                aside.Add(CreateAdvancedSettings());
            }
        }

        private VisualElement CreateAdvancedSettings()
        {
            VisualElement advanced = Column(4);
            Checkbox development = new Checkbox(Context.Text("build.development"), Settings.DevelopmentBuild);
            development.AddToClassList("jtl-build-aside__check");
            development.ValueChanged += value => ChangeSettings(() => Settings.DevelopmentBuild = value);
            advanced.Add(development);

            Checkbox openFolder = new Checkbox(Context.Text("build.openFolder"), Settings.OpenFolderAfterBuild);
            openFolder.AddToClassList("jtl-build-aside__check");
            openFolder.ValueChanged += value => ChangeSettings(() => Settings.OpenFolderAfterBuild = value);
            advanced.Add(openFolder);

            NumberFieldWithUnit number = new NumberFieldWithUnit { Width = 100 };
            number.Value = Settings.BuildNumber.ToString(CultureInfo.InvariantCulture);
            number.Input.RegisterCallback<FocusOutEvent>(focusEvent =>
            {
                bool valid = int.TryParse(number.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value >= 0;
                number.Error = valid == false;

                if (valid && value != Settings.BuildNumber)
                {
                    ChangeSettings(() => Settings.BuildNumber = value);
                }
            });
            VisualElement numberLine = Row(8);
            numberLine.Add(number);
            numberLine.Add(TextLabel("→ " + (Settings.BuildNumber + 1).ToString(CultureInfo.InvariantCulture), "jtl-text--secondary"));
            VisualElement numberRow = Row(10);
            numberRow.style.flexWrap = Wrap.NoWrap;
            numberRow.Add(Localized("build.number", "jtl-build-aside__label", "jtl-build-aside__field-label"));
            numberRow.Add(numberLine);
            advanced.Add(numberRow);
            return advanced;
        }

        private void AddPlayerSettings(VisualElement aside, SdkConfiguration active)
        {
            PlayerSettingsPreset preset = active.PlayerSettings;
            VisualElement product = Column(9);
            product.Add(Localized("build.productSettings", "jtl-build-aside__title"));
            product.Add(AsideField("build.productName", PlayerInput(PlayerSettings.productName, value => PlayerSettings.productName = value)));
            product.Add(AsideField("build.companyName", PlayerInput(PlayerSettings.companyName, value => PlayerSettings.companyName = value)));
            product.Add(AsideField("build.version", PlayerInput(PlayerSettings.bundleVersion, value => PlayerSettings.bundleVersion = value)));
            aside.Add(product);

            VisualElement runtime = Column(9);
            runtime.Add(Localized("build.runtimeSettings", "jtl-build-aside__title"));
            StrippingLevel stripping = preset.ApplyStripping ? preset.Stripping : _presets.CurrentStripping();
            runtime.Add(AsideField("build.stripping", EnumDropdown(stripping, false, value => ChangePreset(active, () =>
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
            row.Add(Localized(labelKey, "jtl-build-aside__label", "jtl-build-aside__field-label"));
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
            checkbox.AddToClassList("jtl-build-aside__check");
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
            string selected = EditorUtility.OpenFolderPanel(Context.Text("build.buildFolder"), Settings.BuildPath, "");

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
                _advanced = _advanced == false;
                Render();
                return;
            }

            Context.SelectedConfiguration = active;
            NavigateTo(ToolkitSectionId.ConfigurationDetails);
        }

        private void RunBuild(SdkConfiguration active)
        {
            _lastResult = _builds.Build(Settings, active);

            if (_lastResult.IsSuccess)
            {
                Context.Report(StatusKind.Success, "build.done", Settings.BuildNumber, (_lastResult.TotalBytes / SdkBuildService.BytesPerMegabyte).ToString("0.0", CultureInfo.InvariantCulture), _lastResult.OutputPath);
            }
            else
            {
                Context.Report(StatusKind.Error, "build.failed", FailureText(_lastResult));
            }

            Render();
        }
    }
}
