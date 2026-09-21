using System;
using System.Collections.Generic;
using JTLStudio.SDK.Editor.Toolkit.Components;
using JTLStudio.SDK.Prototype;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class LanguagesSection : ToolkitSection
    {
        private const int ColumnCount = 3;
        private const int SideColumnWidth = 470;

        private readonly PrototypeSimulationSettings _simulation = new PrototypeSimulationSettings();

        public LanguagesSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Languages;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "LanguagesSection";

        private JTLSDKSettings Settings => Context.Project.Settings;

        protected override void OnRendered()
        {
            _simulation.Load();
            VisualElement body = Require<VisualElement>("languages-body");
            VisualElement main = Column(12);
            main.AddToClassList("jtl-basis");
            main.Add(CreateProjectCard());
            main.Add(ProvidersCard("_languageProvider"));
            body.Add(main);

            VisualElement side = Column(12);
            side.style.width = SideColumnWidth;
            side.style.flexShrink = 0;
            side.Add(CreateReplacementsCard());
            side.Add(CreatePerConfigurationCard());
            side.Add(CreatePlayModeCard());
            body.Add(side);
        }

        private VisualElement CreateProjectCard()
        {
            Language[] all = (Language[])Enum.GetValues(typeof(Language));
            Card card = new Card { TitleKey = "languages.projectLanguages", Spacing = 12 };
            card.AddToClassList("jtl-basis");
            card.Header.Add(TextLabel(Context.Text("languages.selectedCountFormat", Settings.SupportedLanguages.Count, all.Length), "jtl-card__caption"));

            VisualElement grid = new VisualElement();
            grid.AddToClassList("jtl-language-grid");
            int perColumn = (all.Length + ColumnCount - 1) / ColumnCount;

            for (int column = 0; column < ColumnCount; column++)
            {
                VisualElement columnElement = new VisualElement();
                columnElement.AddToClassList("jtl-language-column");
                columnElement.EnableInClassList("jtl-language-column--last", column == ColumnCount - 1);

                for (int index = column * perColumn; index < Math.Min(all.Length, (column + 1) * perColumn); index++)
                {
                    Language language = all[index];
                    Checkbox checkbox = new Checkbox(Context.Text("language." + language), Settings.SupportedLanguages.Contains(language));
                    checkbox.ValueChanged += value => ToggleProjectLanguage(language, value);
                    columnElement.Add(checkbox);
                }

                grid.Add(columnElement);
            }

            card.Add(grid);

            FieldRow defaultRow = new FieldRow("languages.default", 150);
            Dropdown defaultLanguage = new Dropdown();
            defaultLanguage.style.width = 220;
            List<Language> supported = new List<Language>(Settings.SupportedLanguages);
            defaultLanguage.choices = LanguageNames(supported);
            defaultLanguage.index = supported.IndexOf(Settings.DefaultLanguage);
            defaultLanguage.RegisterValueChangedCallback(_ => ChangeDefault(supported, defaultLanguage.index));
            defaultRow.Add(defaultLanguage);
            card.Add(defaultRow);
            return card;
        }

        private VisualElement CreateReplacementsCard()
        {
            Card card = new Card { TitleKey = "languages.replacements" };
            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-table");
            List<LanguageReplacement> replacements = Settings.LanguageReplacements;

            for (int index = 0; index < replacements.Count; index++)
            {
                int captured = index;
                LanguageReplacement replacement = replacements[index];
                VisualElement row = new VisualElement();
                row.AddToClassList("jtl-table__row");
                row.EnableInClassList("jtl-table__row--head", index == 0);
                VisualElement from = Row(6);
                from.AddToClassList("jtl-grow");
                from.Add(new Icon("languages", 14, "muted"));
                from.Add(TextLabel(Context.Text("language." + replacement.From), "jtl-text"));
                row.Add(from);
                row.Add(TextLabel("→", "jtl-text", "jtl-text--muted"));
                Dropdown to = new Dropdown();
                to.style.width = 140;
                to.style.marginLeft = 8;
                List<Language> targets = new List<Language>(Settings.SupportedLanguages);
                to.choices = LanguageNames(targets);
                to.index = targets.IndexOf(replacement.To);
                to.RegisterValueChangedCallback(_ => ChangeReplacementTarget(captured, targets, to.index));
                row.Add(to);
                IconButton remove = new IconButton("close", 24);
                remove.style.marginLeft = 6;
                remove.clicked += () => RemoveReplacement(captured);
                row.Add(remove);
                table.Add(row);
            }

            if (replacements.Count > 0)
            {
                card.Add(table);
            }

            VisualElement actions = new VisualElement();
            actions.AddToClassList("jtl-row");
            ToolkitButton add = Button("languages.addReplacement", ToolkitButton.SecondaryVariant, "plus", ShowReplacementMenu);
            add.Compact = true;
            actions.Add(add);
            card.Add(actions);
            return card;
        }

        private VisualElement CreatePerConfigurationCard()
        {
            Card card = new Card { TitleKey = "languages.perConfiguration" };
            IReadOnlyList<SdkConfiguration> configurations = Context.Project.Configurations;

            if (configurations.Count == 0)
            {
                card.Add(Localized("languages.noConfigurations", "jtl-text--secondary"));
                return card;
            }

            foreach (SdkConfiguration configuration in configurations)
            {
                SdkConfiguration captured = configuration;
                VisualElement row = Row(8);
                row.AddToClassList("jtl-clickable-row");
                row.Add(new PortalMark(Context.Platforms.PortalMark(configuration.Platform), 16));
                row.Add(TextLabel(configuration.DisplayName, "jtl-text"));
                row.Add(Spacer());

                foreach (Language language in configuration.Languages)
                {
                    row.Add(new Badge { Text = Context.Text("language." + language), Variant = Settings.SupportedLanguages.Contains(language) ? Badge.NeutralVariant : Badge.WarningVariant });
                }

                row.RegisterCallback<ClickEvent>(_ => OpenConfiguration(captured));
                card.Add(row);
            }

            return card;
        }

        private VisualElement CreatePlayModeCard()
        {
            Card card = new Card { TitleKey = "languages.playMode" };
            FieldRow row = new FieldRow("languages.startLanguage", 150);
            Dropdown start = new Dropdown();
            start.style.width = 220;
            List<Language> supported = new List<Language>(Settings.SupportedLanguages);
            start.choices = LanguageNames(supported);
            start.index = supported.IndexOf(_simulation.StartLanguage);
            start.RegisterValueChangedCallback(_ => ChangeStartLanguage(supported, start.index));
            row.Add(start);
            card.Add(row);
            return card;
        }

        private List<string> LanguageNames(List<Language> languages)
        {
            List<string> names = new List<string>();

            foreach (Language language in languages)
            {
                names.Add(Context.Text("language." + language));
            }

            return names;
        }

        private void ToggleProjectLanguage(Language language, bool enabled)
        {
            JTLSDKSettings settings = Settings;

            if (enabled == false && settings.SupportedLanguages.Count == 1 && settings.SupportedLanguages.Contains(language))
            {
                Context.Report(StatusKind.Warning, "languages.lastLanguage");
                Render();
                return;
            }

            Context.Project.Modify(settings, "Change project languages", () =>
            {
                if (enabled && settings.SupportedLanguages.Contains(language) == false)
                {
                    settings.SupportedLanguages.Add(language);
                }
                else if (enabled == false)
                {
                    settings.SupportedLanguages.Remove(language);

                    if (settings.DefaultLanguage == language)
                    {
                        settings.DefaultLanguage = settings.SupportedLanguages[0];
                    }
                }
            });

            Render();
        }

        private void ChangeDefault(List<Language> supported, int index)
        {
            if (index < 0 || index >= supported.Count)
            {
                return;
            }

            JTLSDKSettings settings = Settings;
            Context.Project.Modify(settings, "Change default language", () => settings.DefaultLanguage = supported[index]);
        }

        private void ShowReplacementMenu()
        {
            GenericMenu menu = new GenericMenu();
            JTLSDKSettings settings = Settings;

            foreach (Language language in (Language[])Enum.GetValues(typeof(Language)))
            {
                bool used = settings.SupportedLanguages.Contains(language) || settings.LanguageReplacements.Exists(replacement => replacement.From == language);

                if (used)
                {
                    continue;
                }

                Language captured = language;
                menu.AddItem(new GUIContent(Context.Text("language." + language)), false, () => AddReplacement(captured));
            }

            menu.ShowAsContext();
        }

        private void AddReplacement(Language from)
        {
            JTLSDKSettings settings = Settings;
            Context.Project.Modify(settings, "Add language replacement", () => settings.LanguageReplacements.Add(new LanguageReplacement(from, settings.DefaultLanguage)));
            Render();
        }

        private void ChangeReplacementTarget(int index, List<Language> targets, int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= targets.Count)
            {
                return;
            }

            JTLSDKSettings settings = Settings;
            Language from = settings.LanguageReplacements[index].From;
            Context.Project.Modify(settings, "Change language replacement", () => settings.LanguageReplacements[index] = new LanguageReplacement(from, targets[targetIndex]));
        }

        private void RemoveReplacement(int index)
        {
            JTLSDKSettings settings = Settings;
            Context.Project.Modify(settings, "Remove language replacement", () => settings.LanguageReplacements.RemoveAt(index));
            Render();
        }

        private void ChangeStartLanguage(List<Language> supported, int index)
        {
            if (index < 0 || index >= supported.Count)
            {
                return;
            }

            _simulation.StartLanguage = supported[index];
            _simulation.Save();
            Context.Report(StatusKind.Success, "languages.startLanguageSaved", Context.Text("language." + supported[index]));
        }

        private void OpenConfiguration(SdkConfiguration configuration)
        {
            Context.SelectedConfiguration = configuration;
            NavigateTo(ToolkitSectionId.ConfigurationDetails);
        }
    }
}
