using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Toolkit.Components;
using JTLStudio.SDK.Editor.Toolkit.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class ConfigurationDetailsSection : ToolkitSection
    {
        private const string PresetProperty = "_playerSettings";
        private const int ValueWidth = 240;
        private const int MinimumMemory = 32;

        private readonly ConfigurationValidator _validator = new ConfigurationValidator();
        private SdkConfiguration _configuration;
        private SerializedObject _serialized;

        public ConfigurationDetailsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.ConfigurationDetails;

        public override ToolkitSectionId NavigationId => ToolkitSectionId.Configurations;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "ConfigurationDetailsSection";

        protected override void OnRendered()
        {
            VisualElement root = Require<VisualElement>("details-root");
            _configuration = Context.SelectedConfiguration != null ? Context.SelectedConfiguration : Context.Project.Active;

            if (_configuration == null)
            {
                EmptyState empty = new EmptyState { TitleKey = "details.noConfigurationTitle", IconName = "configurations" };
                ToolkitButton back = new ToolkitButton("details.backToList", ToolkitButton.SecondaryVariant);
                back.clicked += OnBackClicked;
                empty.Add(back);
                root.Add(empty);
                return;
            }

            _serialized = new SerializedObject(_configuration);
            root.Add(CreateHeader());
            root.Add(CreatePresetCard());
            root.Add(CreateModulesCard());
            root.Add(CreatePauseCard());
            root.Add(CreateLanguagesCard());
        }

        private VisualElement CreateHeader()
        {
            bool active = Context.Project.Active == _configuration;
            VisualElement header = Row(12);

            IconButton back = new IconButton("chevron-left", 28) { Variant = IconButton.SecondaryVariant };
            back.clicked += OnBackClicked;
            header.Add(back);
            header.Add(new PortalMark(Context.Platforms.PortalMark(_configuration.Platform), 28));

            VisualElement titleColumn = new VisualElement();
            titleColumn.AddToClassList("jtl-column");
            VisualElement titleRow = Row(8);
            Label title = new Label(_configuration.DisplayName);
            title.AddToClassList("jtl-section-header__title");
            titleRow.Add(title);
            titleRow.Add(new Badge(active ? "badge.active" : "badge.inactive", active ? Badge.SuccessVariant : Badge.NeutralVariant));
            titleColumn.Add(titleRow);
            Label platform = new Label(Context.Text("details.portal", Context.Platforms.DisplayName(_configuration.Platform)));
            platform.AddToClassList("jtl-text--secondary");
            titleColumn.Add(platform);
            header.Add(titleColumn);

            header.Add(Spacer());

            VisualElement chip = new VisualElement();
            chip.AddToClassList("jtl-chip");
            Label define = new Label(string.IsNullOrEmpty(_configuration.DefineSymbol) ? "—" : _configuration.DefineSymbol);
            define.AddToClassList("jtl-chip__text");
            define.AddToClassList(MonospaceFont.ClassName);
            chip.Add(define);
            IconButton copy = new IconButton("copy", 20);
            copy.clicked += OnCopyDefineClicked;
            chip.Add(copy);
            header.Add(chip);

            if (active == false)
            {
                ToolkitButton activate = new ToolkitButton("configurations.makeActive", ToolkitButton.PrimaryVariant);
                activate.clicked += OnActivateClicked;
                header.Add(activate);
            }

            IconButton delete = new IconButton("delete", 28) { Variant = IconButton.SecondaryVariant };
            delete.tooltip = Context.Text("details.delete");
            delete.clicked += OnDeleteClicked;
            header.Add(delete);
            return header;
        }

        private VisualElement CreatePresetCard()
        {
            Card card = new Card { TitleKey = "details.projectSettings" };
            SerializedProperty preset = _serialized.FindProperty(PresetProperty);
            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-column");

            VisualElement head = new VisualElement();
            head.AddToClassList("jtl-setting-head");
            head.Add(Heading("details.columnSetting", "jtl-setting-head__setting"));
            head.Add(Heading("details.columnValue", "jtl-setting-head__value"));
            head.Add(Spacer());
            head.Add(Heading("details.columnApply", "jtl-setting-head__apply"));
            table.Add(head);

            table.Add(SettingRow("Compression Format", EnumControl<WebCompression>(preset.FindPropertyRelative("_compression"), IsCompressionInvalid()), preset.FindPropertyRelative("_applyCompression")));

            foreach (string issue in _validator.Validate(_configuration))
            {
                InlineMessage message = new InlineMessage { Variant = InlineMessage.ErrorVariant, Text = issue };
                message.AddToClassList("jtl-setting-error");
                table.Add(message);
            }

            table.Add(SettingRow("Decompression Fallback", SwitchControl(preset.FindPropertyRelative("_decompressionFallback")), preset.FindPropertyRelative("_applyDecompressionFallback")));
            table.Add(SettingRow("Data Caching", SwitchControl(preset.FindPropertyRelative("_dataCaching")), preset.FindPropertyRelative("_applyDataCaching")));
            table.Add(SettingRow("Managed Stripping Level", EnumControl<StrippingLevel>(preset.FindPropertyRelative("_stripping"), false), preset.FindPropertyRelative("_applyStripping")));
            table.Add(SettingRow("Run In Background", SwitchControl(preset.FindPropertyRelative("_runInBackground")), preset.FindPropertyRelative("_applyRunInBackground")));
            table.Add(SettingRow("Debug Symbols", SwitchControl(preset.FindPropertyRelative("_debugSymbols")), preset.FindPropertyRelative("_applyDebugSymbols")));
            table.Add(SettingRow("Memory Size", MemoryControl(preset.FindPropertyRelative("_memorySizeMegabytes")), preset.FindPropertyRelative("_applyMemorySize")));
            card.Add(table);
            return card;
        }

        private VisualElement CreateModulesCard()
        {
            Card card = new Card { TitleKey = "details.modules", Spacing = 0 };
            VisualElement list = new VisualElement();
            list.AddToClassList("jtl-column");

            foreach (ModuleSlot slot in Context.Modules.All)
            {
                list.Add(new ProviderRow(Context, _configuration, slot, Localized(slot.NameKey), Render));
            }

            card.Add(list);
            return card;
        }

        private VisualElement CreatePauseCard()
        {
            Card card = new Card { TitleKey = "details.pause" };
            FieldRow focus = new FieldRow("details.pauseOnFocusLoss", FieldRow.DefaultLabelWidth);
            focus.Add(SwitchControl(_serialized.FindProperty("_pauseOnFocusLoss")));
            card.Add(focus);
            return card;
        }

        private VisualElement CreateLanguagesCard()
        {
            Card card = new Card { TitleKey = "details.languages" };
            List<Language> projectLanguages = Context.Project.Settings.SupportedLanguages;

            if (projectLanguages.Count == 0)
            {
                card.Add(new InlineMessage("details.noProjectLanguages", InlineMessage.WarningVariant));
                return card;
            }

            VisualElement grid = new VisualElement();
            grid.AddToClassList("jtl-row");
            grid.AddToClassList("jtl-wrap");

            foreach (Language language in projectLanguages)
            {
                Language captured = language;
                Checkbox checkbox = new Checkbox(language.ToString(), _configuration.Languages.Contains(language));
                checkbox.AddToClassList("jtl-language-check");
                checkbox.ValueChanged += value => ToggleLanguage(captured, value);
                grid.Add(checkbox);
            }

            card.Add(grid);
            return card;
        }

        private VisualElement SettingRow(string name, VisualElement control, SerializedProperty applyProperty)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-setting-row");
            Label label = new Label(name);
            label.AddToClassList("jtl-setting-row__name");
            row.Add(label);
            VisualElement value = new VisualElement();
            value.AddToClassList("jtl-setting-row__value");
            value.Add(control);
            row.Add(value);
            row.Add(Spacer());
            VisualElement apply = new VisualElement();
            apply.AddToClassList("jtl-setting-row__apply");
            Checkbox checkbox = new Checkbox(string.Empty, applyProperty.boolValue);
            checkbox.ValueChanged += enabled => ApplyBool(applyProperty, enabled);
            apply.Add(checkbox);
            row.Add(apply);
            return row;
        }

        private VisualElement EnumControl<T>(SerializedProperty property, bool error) where T : Enum
        {
            Dropdown dropdown = new Dropdown { Error = error };
            dropdown.style.width = ValueWidth;
            dropdown.style.minWidth = ValueWidth;
            dropdown.choices = new List<string>(Enum.GetNames(typeof(T)));
            dropdown.index = property.enumValueIndex;
            dropdown.RegisterValueChangedCallback(_ => ApplyEnum(property, dropdown.index));
            return dropdown;
        }

        private VisualElement SwitchControl(SerializedProperty property)
        {
            SwitchToggle toggle = new SwitchToggle(property.boolValue);
            toggle.ValueChanged += value => ApplyBool(property, value);
            return toggle;
        }

        private VisualElement MemoryControl(SerializedProperty property)
        {
            NumberFieldWithUnit field = new NumberFieldWithUnit { UnitKey = "unit.mb", Width = 120 };
            field.Value = property.intValue.ToString(CultureInfo.InvariantCulture);
            field.Input.RegisterValueChangedCallback(changeEvent =>
            {
                bool valid = int.TryParse(changeEvent.newValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int megabytes) && megabytes >= MinimumMemory;
                field.Error = valid == false;

                if (valid)
                {
                    property.intValue = megabytes;
                    ApplySerialized(false);
                }
            });
            return field;
        }

        private bool IsCompressionInvalid()
        {
            PlayerSettingsPreset preset = _configuration.PlayerSettings;
            return _configuration.Platform == PlatformId.YouTubePlayables && preset.ApplyCompression && preset.Compression != WebCompression.Disabled;
        }

        private Label Heading(string key, string className)
        {
            LocalizedLabel label = new LocalizedLabel(key);
            label.AddToClassList("jtl-table__heading");
            label.AddToClassList(className);
            return label;
        }

        private void ToggleLanguage(Language language, bool enabled)
        {
            Context.Project.Modify(_configuration, "Change configuration languages", () =>
            {
                if (enabled && _configuration.Languages.Contains(language) == false)
                {
                    _configuration.Languages.Add(language);
                }
                else if (enabled == false)
                {
                    _configuration.Languages.Remove(language);
                }
            });
        }

        private void ApplyBool(SerializedProperty property, bool value)
        {
            property.boolValue = value;
            ApplySerialized(true);
        }

        private void ApplyString(SerializedProperty property, string value)
        {
            if (property.stringValue == value)
            {
                return;
            }

            property.stringValue = value;
            ApplySerialized(false);
        }

        private void ApplyEnum(SerializedProperty property, int index)
        {
            if (index < 0 || property.enumValueIndex == index)
            {
                return;
            }

            property.enumValueIndex = index;
            ApplySerialized(true);
        }

        private void ApplySerialized(bool rerender)
        {
            _serialized.ApplyModifiedProperties();
            SaveConfiguration();

            if (rerender)
            {
                Render();
            }
        }

        private void SaveConfiguration()
        {
            Context.Project.Save(_configuration);
            Context.Project.NotifyChanged();
        }

        private void OnBackClicked()
        {
            NavigateTo(ToolkitSectionId.Configurations);
        }

        private void OnActivateClicked()
        {
            Context.Project.Activate(_configuration);
            Context.Report(StatusKind.Success, "configurations.activated", _configuration.DisplayName);
            Render();
        }

        private void OnCopyDefineClicked()
        {
            EditorGUIUtility.systemCopyBuffer = _configuration.DefineSymbol;
            Context.Report(StatusKind.Info, "configurations.copied", _configuration.DefineSymbol);
        }

        private void OnDeleteClicked()
        {
            bool confirmed = EditorUtility.DisplayDialog(Context.Text("details.deleteTitle"), Context.Text("details.deleteMessage", _configuration.DisplayName), Context.Text("details.delete"), Context.Text("details.cancel"));

            if (confirmed == false)
            {
                return;
            }

            string name = _configuration.DisplayName;
            Context.Project.Delete(_configuration);
            Context.SelectedConfiguration = null;
            Context.Report(StatusKind.Warning, "details.deleted", name);
            NavigateTo(ToolkitSectionId.Configurations);
        }
    }
}
