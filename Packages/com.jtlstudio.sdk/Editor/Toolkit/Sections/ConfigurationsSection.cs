using System;
using System.Collections.Generic;
using System.Globalization;
using JTLStudio.SDK.Editor.Toolkit.Components;
using JTLStudio.SDK.Editor.Toolkit.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class ConfigurationsSection : ToolkitSection
    {
        private const int CardsPerRow = 2;
        private const float MinimumSeconds = 0f;

        private readonly LogLevel[] _logLevels = { LogLevel.None, LogLevel.Errors, LogLevel.ErrorsAndWarnings, LogLevel.All };

        public ConfigurationsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Configurations;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "configurations.ready", DateTime.Now.ToString("HH:mm"));

        protected override string TemplateName => "ConfigurationsSection";

        protected override void OnRendered()
        {
            Require<ToolkitButton>("new-configuration").clicked += OnNewConfigurationClicked;
            BuildCards(Require<VisualElement>("configuration-cards"));
            BindGeneralSettings();
        }

        private void BuildCards(VisualElement container)
        {
            IReadOnlyList<SdkConfiguration> configurations = Context.Project.Configurations;

            if (configurations.Count == 0)
            {
                EmptyState empty = new EmptyState
                {
                    TitleKey = "configurations.emptyTitle",
                    DescriptionKey = "configurations.emptyDescription",
                    IconName = "configurations"
                };
                empty.Add(CreateButton("configurations.createYandex", ToolkitButton.SecondaryVariant, () => CreateConfiguration(PlatformId.YandexGames)));
                empty.Add(CreateButton("configurations.createYouTube", ToolkitButton.SecondaryVariant, () => CreateConfiguration(PlatformId.YouTubePlayables)));
                container.Add(empty);
                return;
            }

            VisualElement row = null;

            for (int index = 0; index < configurations.Count; index++)
            {
                if (index % CardsPerRow == 0)
                {
                    row = new VisualElement();
                    row.AddToClassList("jtl-row");
                    row.AddToClassList("jtl-row--stretch");
                    row.AddToClassList("jtl-hstack-12");
                    row.AddToClassList("jtl-configuration-grid__row");
                    container.Add(row);
                }

                row.Add(CreateCard(configurations[index]));
            }

            if (configurations.Count % CardsPerRow != 0)
            {
                VisualElement filler = new VisualElement();
                filler.AddToClassList("jtl-basis");
                row.Add(filler);
            }
        }

        private Card CreateCard(SdkConfiguration configuration)
        {
            bool active = Context.Project.Active == configuration;
            PlatformPresentation platforms = Context.Platforms;
            Card card = new Card { Active = active };
            card.AddToClassList("jtl-basis");

            VisualElement header = new VisualElement();
            header.AddToClassList("jtl-row");
            header.AddToClassList("jtl-hstack-10");
            header.Add(new PortalMark(platforms.PortalMark(configuration.Platform), 24));
            Label name = new Label(configuration.DisplayName);
            name.AddToClassList("jtl-text");
            name.AddToClassList("jtl-text--title");
            header.Add(name);
            header.Add(new Badge(active ? "badge.active" : "badge.inactive", active ? Badge.SuccessVariant : Badge.NeutralVariant));
            card.Add(header);

            LocalizedLabel description = new LocalizedLabel(platforms.DescriptionKey(configuration.Platform));
            description.AddToClassList("jtl-text--secondary");
            description.AddToClassList("jtl-text--wrap");
            card.Add(description);

            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-table");
            table.Add(CreateDefineRow(configuration));
            table.Add(CreateValueRow("configurations.compressionFormat", CompressionText(configuration)));
            table.Add(CreateValueRow("configurations.languages", Context.Text("configurations.languagesCount", configuration.Languages.Count, Context.Project.Settings.SupportedLanguages.Count)));
            card.Add(table);

            VisualElement actions = new VisualElement();
            actions.AddToClassList("jtl-row");
            actions.AddToClassList("jtl-hstack-8");
            ToolkitButton open = CreateButton("configurations.openConfiguration", ToolkitButton.SecondaryVariant, () => OpenConfiguration(configuration));
            open.Block = true;
            actions.Add(open);

            if (active == false)
            {
                ToolkitButton activate = CreateButton("configurations.makeActive", ToolkitButton.PrimaryVariant, () => ActivateConfiguration(configuration));
                activate.Block = true;
                actions.Add(activate);
            }

            card.Add(actions);
            return card;
        }

        private VisualElement CreateDefineRow(SdkConfiguration configuration)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-table__row");
            row.AddToClassList("jtl-table__row--head");
            LocalizedLabel label = new LocalizedLabel("configurations.defineSymbol");
            label.AddToClassList("jtl-text--secondary");
            label.AddToClassList("jtl-grow");
            row.Add(label);
            Label value = new Label(string.IsNullOrEmpty(configuration.DefineSymbol) ? "—" : configuration.DefineSymbol);
            value.AddToClassList("jtl-text--small");
            value.AddToClassList(MonospaceFont.ClassName);
            row.Add(value);
            IconButton copy = new IconButton("copy", 22);
            copy.AddToClassList("jtl-ml-4");
            copy.clicked += () => CopyDefineSymbol(configuration.DefineSymbol);
            row.Add(copy);
            return row;
        }

        private VisualElement CreateValueRow(string labelKey, string valueText)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-table__row");
            LocalizedLabel label = new LocalizedLabel(labelKey);
            label.AddToClassList("jtl-text--secondary");
            label.AddToClassList("jtl-grow");
            row.Add(label);
            Label value = new Label(valueText);
            value.AddToClassList("jtl-text");
            row.Add(value);
            return row;
        }

        private string CompressionText(SdkConfiguration configuration)
        {
            PlayerSettingsPreset preset = configuration.PlayerSettings;
            return preset.ApplyCompression ? preset.Compression.ToString() : Context.Text("configurations.notApplied");
        }

        private ToolkitButton CreateButton(string textKey, string variant, Action onClick)
        {
            ToolkitButton button = new ToolkitButton(textKey, variant);
            button.clicked += onClick;
            return button;
        }

        private void BindGeneralSettings()
        {
            JTLSDKSettings settings = Context.Project.Settings;
            NumberFieldWithUnit timeout = Require<NumberFieldWithUnit>("initialization-timeout");
            NumberFieldWithUnit autosave = Require<NumberFieldWithUnit>("autosave-delay");
            Dropdown logging = Require<Dropdown>("logging-level");

            timeout.Value = settings.InitializationTimeoutSeconds.ToString(CultureInfo.InvariantCulture);
            autosave.Value = settings.AutosaveDelaySeconds.ToString(CultureInfo.InvariantCulture);
            timeout.Input.RegisterValueChangedCallback(changeEvent => ApplySeconds(changeEvent.newValue, timeout, seconds => settings.InitializationTimeoutSeconds = seconds));
            autosave.Input.RegisterValueChangedCallback(changeEvent => ApplySeconds(changeEvent.newValue, autosave, seconds => settings.AutosaveDelaySeconds = seconds));

            logging.choices = new List<string>(Context.Localization.GetList("configurations.loggingLevels"));
            logging.index = Array.IndexOf(_logLevels, settings.LogLevel);
            logging.RegisterValueChangedCallback(_ => ApplyLogLevel(logging.index));
        }

        private void ApplySeconds(string text, NumberFieldWithUnit field, Action<float> assign)
        {
            bool valid = float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float seconds) && seconds >= MinimumSeconds;
            field.Error = valid == false;

            if (valid == false)
            {
                return;
            }

            Context.Project.Modify(Context.Project.Settings, "Change JTL SDK settings", () => assign(seconds));
        }

        private void ApplyLogLevel(int index)
        {
            if (index < 0 || index >= _logLevels.Length)
            {
                return;
            }

            JTLSDKSettings settings = Context.Project.Settings;
            Context.Project.Modify(settings, "Change JTL SDK logging level", () => settings.LogLevel = _logLevels[index]);
        }

        private void OnNewConfigurationClicked()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent(Context.Platforms.DisplayName(PlatformId.YandexGames)), false, () => CreateConfiguration(PlatformId.YandexGames));
            menu.AddItem(new GUIContent(Context.Platforms.DisplayName(PlatformId.YouTubePlayables)), false, () => CreateConfiguration(PlatformId.YouTubePlayables));
            menu.ShowAsContext();
        }

        private void CreateConfiguration(PlatformId platform)
        {
            SdkConfiguration configuration = Context.Project.Create(platform);
            Context.Report(StatusKind.Success, "configurations.created", configuration.DisplayName);
            OpenConfiguration(configuration);
        }

        private void OpenConfiguration(SdkConfiguration configuration)
        {
            Context.SelectedConfiguration = configuration;
            NavigateTo(ToolkitSectionId.ConfigurationDetails);
        }

        private void ActivateConfiguration(SdkConfiguration configuration)
        {
            Context.Project.Activate(configuration);
            Context.Report(StatusKind.Success, "configurations.activated", configuration.DisplayName);
            Render();
        }

        private void CopyDefineSymbol(string defineSymbol)
        {
            EditorGUIUtility.systemCopyBuffer = defineSymbol;
            Context.Report(StatusKind.Info, "configurations.copied", defineSymbol);
        }
    }
}
