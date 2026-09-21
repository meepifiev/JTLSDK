using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class LanguagesSection : ToolkitSection
    {
        private const string StatusTime = "14:06";
        private const string LanguageKeyPrefix = "language.";

        private readonly Language[][] _columns =
        {
            new[] { Language.English, Language.German, Language.Portuguese, Language.Ukrainian, Language.Uzbek, Language.Georgian, Language.Hindi, Language.Vietnamese, Language.Korean },
            new[] { Language.Russian, Language.French, Language.Italian, Language.Belarusian, Language.Azerbaijani, Language.Hebrew, Language.Indonesian, Language.ChineseSimplified, Language.Romanian },
            new[] { Language.Turkish, Language.Spanish, Language.Polish, Language.Kazakh, Language.Armenian, Language.Arabic, Language.Thai, Language.Japanese }
        };

        private readonly Language[] _selected = { Language.English, Language.Russian, Language.Turkish };

        public LanguagesSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Languages;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Success, "languages.status", StatusTime);

        protected override string TemplateName => "LanguagesSection";

        protected override void OnRendered()
        {
            VisualElement grid = Require<VisualElement>("language-grid");

            for (int columnIndex = 0; columnIndex < _columns.Length; columnIndex++)
            {
                VisualElement column = new VisualElement();
                column.AddToClassList("jtl-language-column");
                column.EnableInClassList("jtl-language-column--last", columnIndex == _columns.Length - 1);

                foreach (Language language in _columns[columnIndex])
                {
                    Checkbox checkbox = new Checkbox();
                    checkbox.LabelKey = LanguageKeyPrefix + language;
                    checkbox.Value = IsSelected(language);
                    column.Add(checkbox);
                }

                grid.Add(column);
            }
        }

        private bool IsSelected(Language language)
        {
            foreach (Language selected in _selected)
            {
                if (selected == language)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
