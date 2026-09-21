using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class SearchField : VisualElement, ILocalizedElement
    {
        public const int DefaultWidth = 220;
        private const int IconSize = 14;
        private const string ClassName = "jtl-search";
        private const string FocusClass = "jtl-field-box--focus";

        public new class UxmlFactory : UxmlFactory<SearchField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _placeholderKey = new UxmlStringAttributeDescription { name = "placeholder-key" };
            private readonly UxmlStringAttributeDescription _value = new UxmlStringAttributeDescription { name = "value" };
            private readonly UxmlIntAttributeDescription _width = new UxmlIntAttributeDescription { name = "width", defaultValue = DefaultWidth };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                SearchField field = (SearchField)element;
                field.PlaceholderKey = _placeholderKey.GetValueFromBag(attributes, context);
                field.Value = _value.GetValueFromBag(attributes, context);
                field.style.width = _width.GetValueFromBag(attributes, context);
            }
        }

        private readonly Icon _icon = new Icon("search", IconSize, "muted");
        private readonly TextField _input = new TextField();
        private readonly Label _placeholder = new Label();

        public SearchField()
        {
            AddToClassList(ClassName);
            AddToClassList("jtl-field-box");
            _icon.AddToClassList("jtl-search__icon");
            _input.AddToClassList("jtl-search__input");
            _placeholder.AddToClassList("jtl-search__placeholder");
            _placeholder.pickingMode = PickingMode.Ignore;
            Add(_icon);
            Add(_input);
            Add(_placeholder);
            _input.RegisterValueChangedCallback(OnValueChanged);
            _input.RegisterCallback<FocusInEvent>(OnFocusChanged);
            _input.RegisterCallback<FocusOutEvent>(OnFocusChanged);
            UpdatePlaceholder();
        }

        public string PlaceholderKey { get; set; }

        public string Value
        {
            get => _input.value;
            set
            {
                _input.SetValueWithoutNotify(value ?? string.Empty);
                UpdatePlaceholder();
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(PlaceholderKey) == false)
            {
                _placeholder.text = localization.Get(PlaceholderKey);
            }
        }

        private void UpdatePlaceholder()
        {
            bool empty = string.IsNullOrEmpty(_input.value);
            _placeholder.style.display = empty ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnValueChanged(ChangeEvent<string> changeEvent)
        {
            UpdatePlaceholder();
        }

        private void OnFocusChanged(EventBase focusEvent)
        {
            EnableInClassList(FocusClass, focusEvent is FocusInEvent);
            UpdatePlaceholder();
        }
    }
}
