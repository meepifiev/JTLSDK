using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class NumberFieldWithUnit : VisualElement, ILocalizedElement
    {
        public const int DefaultWidth = 110;
        private const string ClassName = "jtl-number-field";
        private const string ErrorClass = "jtl-number-field--error";
        private const string DisabledClass = "jtl-number-field--disabled";
        private const string FocusClass = "jtl-field-box--focus";

        public new class UxmlFactory : UxmlFactory<NumberFieldWithUnit, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _value = new UxmlStringAttributeDescription { name = "value" };
            private readonly UxmlStringAttributeDescription _unitKey = new UxmlStringAttributeDescription { name = "unit-key" };
            private readonly UxmlStringAttributeDescription _unit = new UxmlStringAttributeDescription { name = "unit" };
            private readonly UxmlIntAttributeDescription _width = new UxmlIntAttributeDescription { name = "width", defaultValue = DefaultWidth };
            private readonly UxmlBoolAttributeDescription _error = new UxmlBoolAttributeDescription { name = "error" };
            private readonly UxmlBoolAttributeDescription _disabled = new UxmlBoolAttributeDescription { name = "disabled" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                NumberFieldWithUnit field = (NumberFieldWithUnit)element;
                field.Value = _value.GetValueFromBag(attributes, context);
                field.Unit = _unit.GetValueFromBag(attributes, context);
                field.UnitKey = _unitKey.GetValueFromBag(attributes, context);
                field.Width = _width.GetValueFromBag(attributes, context);
                field.Error = _error.GetValueFromBag(attributes, context);
                field.Disabled = _disabled.GetValueFromBag(attributes, context);
            }
        }

        private readonly TextField _input = new TextField();
        private readonly Label _unit = new Label();
        private int _width = DefaultWidth;

        public NumberFieldWithUnit()
        {
            AddToClassList(ClassName);
            AddToClassList("jtl-field-box");
            _input.AddToClassList("jtl-number-field__input");
            _unit.AddToClassList("jtl-number-field__unit");
            _unit.pickingMode = PickingMode.Ignore;
            _unit.style.display = DisplayStyle.None;
            Add(_input);
            Add(_unit);
            _input.RegisterCallback<FocusInEvent>(OnFocusIn);
            _input.RegisterCallback<FocusOutEvent>(OnFocusOut);
            ApplyWidth();
        }

        public string UnitKey { get; set; }

        public TextField Input => _input;

        public string Value
        {
            get => _input.value;
            set => _input.SetValueWithoutNotify(value ?? string.Empty);
        }

        public string Unit
        {
            get => _unit.text;
            set
            {
                _unit.text = value;
                _unit.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                ApplyWidth();
            }
        }

        public bool Error
        {
            get => ClassListContains(ErrorClass);
            set => EnableInClassList(ErrorClass, value);
        }

        public bool Disabled
        {
            get => ClassListContains(DisabledClass);
            set
            {
                EnableInClassList(DisabledClass, value);
                _input.isReadOnly = value;
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(UnitKey) == false)
            {
                Unit = localization.Get(UnitKey);
            }
        }

        private void ApplyWidth()
        {
            style.width = _width;
            style.minWidth = _width;
        }

        private void OnFocusIn(FocusInEvent focusEvent)
        {
            AddToClassList(FocusClass);
        }

        private void OnFocusOut(FocusOutEvent focusEvent)
        {
            RemoveFromClassList(FocusClass);
        }
    }
}
