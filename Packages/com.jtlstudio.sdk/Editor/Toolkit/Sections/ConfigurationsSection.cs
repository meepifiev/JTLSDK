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

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "ConfigurationsSection";

        protected override void OnRendered()
        {
            ToolkitButton newConfiguration = Require<ToolkitButton>("new-configuration");
            newConfiguration.clicked += OnNewConfigurationClicked;
            newConfiguration.SetEnabled(Context.Project.Find(PlatformId.YandexGames) == null || Context.Project.Find(PlatformId.YouTubePlayables) == null);
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
            Card card = new Card { Active = active };
            card.AddToClassList("jtl-basis");

            VisualElement header = Row(10);
            header.AddToClassList("jtl-clickable-row");
            header.Add(new PortalMark(Context.Platforms.PortalMark(configuration.Platform), 24));
            header.Add(TextLabel(configuration.DisplayName, "jtl-text", "jtl-text--title"));
            header.RegisterCallback<ClickEvent>(_ => OpenConfiguration(configuration));
            card.Add(header);

            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-table");
            table.Add(CreateDefineRow(configuration));
            card.Add(table);

            ToolkitButton action = active
                ? CreateButton("configurations.openConfiguration", ToolkitButton.SecondaryVariant, () => OpenConfiguration(configuration))
                : CreateButton("configurations.makeActive", ToolkitButton.PrimaryVariant, () => ActivateConfiguration(configuration));
            action.Block = true;
            card.Add(action);
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
            AddPlatformItem(menu, PlatformId.YandexGames);
            AddPlatformItem(menu, PlatformId.YouTubePlayables);
            menu.ShowAsContext();
        }

        private void AddPlatformItem(GenericMenu menu, PlatformId platform)
        {
            GUIContent content = new GUIContent(Context.Platforms.DisplayName(platform));

            if (Context.Project.Find(platform) != null)
            {
                menu.AddDisabledItem(content);
                return;
            }

            menu.AddItem(content, false, () => CreateConfiguration(platform));
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
