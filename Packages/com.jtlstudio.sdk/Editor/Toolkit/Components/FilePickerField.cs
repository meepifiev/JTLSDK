using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class FilePickerField : VisualElement
    {
        private const int IconSize = 14;
        private const string ClassName = "jtl-picker";

        public new class UxmlFactory : UxmlFactory<FilePickerField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _value = new UxmlStringAttributeDescription { name = "value" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                ((FilePickerField)element).Value = _value.GetValueFromBag(attributes, context);
            }
        }

        private readonly Icon _folder = new Icon("folder", IconSize, "muted");
        private readonly Label _value = new Label();
        private readonly Icon _chevron = new Icon("chevron-right", IconSize, "muted");

        public FilePickerField()
        {
            AddToClassList(ClassName);
            AddToClassList("jtl-field-box");
            _folder.AddToClassList("jtl-picker__icon");
            _value.AddToClassList("jtl-picker__value");
            _chevron.AddToClassList("jtl-picker__chevron");
            Add(_folder);
            Add(_value);
            Add(_chevron);
            focusable = true;
        }

        public string Value
        {
            get => _value.text;
            set => _value.text = value;
        }
    }
}
