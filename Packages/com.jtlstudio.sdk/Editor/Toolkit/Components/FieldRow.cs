using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class FieldRow : VisualElement, ILocalizedElement
    {
        public const int DefaultLabelWidth = 180;
        public const int CardLabelWidth = 150;
        private const int LabelGap = 12;
        private const int HelpIconSize = 13;
        private const string ClassName = "jtl-field-row";

        public new class UxmlFactory : UxmlFactory<FieldRow, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _labelKey = new UxmlStringAttributeDescription { name = "label-key" };
            private readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };
            private readonly UxmlIntAttributeDescription _labelWidth = new UxmlIntAttributeDescription { name = "label-width", defaultValue = DefaultLabelWidth };
            private readonly UxmlStringAttributeDescription _helpKey = new UxmlStringAttributeDescription { name = "help-key" };
            private readonly UxmlStringAttributeDescription _noteKey = new UxmlStringAttributeDescription { name = "note-key" };
            private readonly UxmlStringAttributeDescription _errorKey = new UxmlStringAttributeDescription { name = "error-key" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                FieldRow row = (FieldRow)element;
                row.LabelKey = _labelKey.GetValueFromBag(attributes, context);
                row.Label = _label.GetValueFromBag(attributes, context);
                row.LabelWidth = _labelWidth.GetValueFromBag(attributes, context);
                row.HelpKey = _helpKey.GetValueFromBag(attributes, context);
                row.NoteKey = _noteKey.GetValueFromBag(attributes, context);
                row.ErrorKey = _errorKey.GetValueFromBag(attributes, context);
            }
        }

        private readonly VisualElement _main = new VisualElement();
        private readonly VisualElement _labelCell = new VisualElement();
        private readonly Label _label = new Label();
        private readonly Icon _help = new Icon("help", HelpIconSize, "muted");
        private readonly VisualElement _control = new VisualElement();
        private readonly Label _note = new Label();
        private readonly InlineMessage _error = new InlineMessage();
        private int _labelWidth = DefaultLabelWidth;

        public FieldRow()
        {
            AddToClassList(ClassName);
            _main.AddToClassList("jtl-field-row__main");
            _labelCell.AddToClassList("jtl-field-row__label-cell");
            _label.AddToClassList("jtl-field-row__label");
            _help.AddToClassList("jtl-field-row__help");
            _help.pickingMode = PickingMode.Position;
            _control.AddToClassList("jtl-field-row__control");
            _control.AddToClassList("jtl-hstack-8");
            _note.AddToClassList("jtl-field-row__note");
            _error.AddToClassList("jtl-field-row__error");
            _error.Variant = InlineMessage.ErrorVariant;
            _help.style.display = DisplayStyle.None;
            _note.style.display = DisplayStyle.None;
            _error.style.display = DisplayStyle.None;
            _labelCell.Add(_label);
            _labelCell.Add(_help);
            _main.Add(_labelCell);
            _main.Add(_control);
            hierarchy.Add(_main);
            hierarchy.Add(_note);
            hierarchy.Add(_error);
            ApplyLabelWidth();
        }

        public FieldRow(string labelKey, int labelWidth) : this()
        {
            LabelKey = labelKey;
            LabelWidth = labelWidth;
        }

        public override VisualElement contentContainer => _control;

        public string LabelKey { get; set; }

        public string HelpKey { get; set; }

        public string NoteKey { get; set; }

        public string ErrorKey { get; set; }

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        public int LabelWidth
        {
            get => _labelWidth;
            set
            {
                _labelWidth = value;
                ApplyLabelWidth();
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(LabelKey) == false)
            {
                _label.text = localization.Get(LabelKey);
            }

            bool hasHelp = string.IsNullOrEmpty(HelpKey) == false;
            _help.style.display = hasHelp ? DisplayStyle.Flex : DisplayStyle.None;
            _help.tooltip = hasHelp ? localization.Get(HelpKey) : string.Empty;

            bool hasNote = string.IsNullOrEmpty(NoteKey) == false;
            _note.style.display = hasNote ? DisplayStyle.Flex : DisplayStyle.None;
            _note.text = hasNote ? localization.Get(NoteKey) : string.Empty;

            bool hasError = string.IsNullOrEmpty(ErrorKey) == false;
            _error.style.display = hasError ? DisplayStyle.Flex : DisplayStyle.None;
            _error.TextKey = ErrorKey;
            _error.ApplyLocalization(localization);
        }

        private void ApplyLabelWidth()
        {
            _labelCell.style.width = _labelWidth;
            _labelCell.style.minWidth = _labelWidth;
            _note.style.paddingLeft = _labelWidth + LabelGap;
            _error.style.paddingLeft = _labelWidth + LabelGap;
        }
    }
}
