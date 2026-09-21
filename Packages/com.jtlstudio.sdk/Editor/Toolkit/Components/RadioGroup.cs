using System;
using System.Collections.Generic;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class RadioGroup : VisualElement, ILocalizedElement
    {
        public const int DefaultGap = 18;
        private const string ClassName = "jtl-radio-group";
        private const string OptionClass = "jtl-radio";
        private const string SelectedClass = "jtl-radio--selected";

        public new class UxmlFactory : UxmlFactory<RadioGroup, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _choicesKey = new UxmlStringAttributeDescription { name = "choices-key" };
            private readonly UxmlIntAttributeDescription _index = new UxmlIntAttributeDescription { name = "index" };
            private readonly UxmlIntAttributeDescription _gap = new UxmlIntAttributeDescription { name = "gap", defaultValue = DefaultGap };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                RadioGroup group = (RadioGroup)element;
                group.ChoicesKey = _choicesKey.GetValueFromBag(attributes, context);
                group.Index = _index.GetValueFromBag(attributes, context);
                group.Gap = _gap.GetValueFromBag(attributes, context);
            }
        }

        private readonly List<VisualElement> _options = new List<VisualElement>();
        private int _index;
        private int _gap = DefaultGap;

        public RadioGroup()
        {
            AddToClassList(ClassName);
        }

        public event Action<int> IndexChanged;

        public string ChoicesKey { get; set; }

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                ApplySelection();
            }
        }

        public int Gap
        {
            get => _gap;
            set
            {
                _gap = value;
                ApplyGap();
            }
        }

        public void SetChoices(IReadOnlyList<string> choices)
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            Clear();
            _options.Clear();

            for (int optionIndex = 0; optionIndex < choices.Count; optionIndex++)
            {
                VisualElement option = CreateOption(choices[optionIndex], optionIndex);
                _options.Add(option);
                Add(option);
            }

            ApplyGap();
            ApplySelection();
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(ChoicesKey) == false)
            {
                SetChoices(localization.GetList(ChoicesKey));
            }
        }

        private VisualElement CreateOption(string label, int optionIndex)
        {
            VisualElement option = new VisualElement();
            option.AddToClassList(OptionClass);
            option.focusable = true;
            VisualElement circle = new VisualElement();
            circle.AddToClassList("jtl-radio__circle");
            circle.pickingMode = PickingMode.Ignore;
            VisualElement dot = new VisualElement();
            dot.AddToClassList("jtl-radio__dot");
            dot.pickingMode = PickingMode.Ignore;
            circle.Add(dot);
            Label text = new Label(label);
            text.AddToClassList("jtl-radio__label");
            text.pickingMode = PickingMode.Ignore;
            option.Add(circle);
            option.Add(text);
            option.AddManipulator(new Clickable(() => OnOptionClicked(optionIndex)));
            return option;
        }

        private void ApplySelection()
        {
            for (int optionIndex = 0; optionIndex < _options.Count; optionIndex++)
            {
                _options[optionIndex].EnableInClassList(SelectedClass, optionIndex == _index);
            }
        }

        private void ApplyGap()
        {
            for (int optionIndex = 0; optionIndex < _options.Count; optionIndex++)
            {
                bool last = optionIndex == _options.Count - 1;
                _options[optionIndex].style.marginRight = last ? 0 : _gap;
            }
        }

        private void OnOptionClicked(int optionIndex)
        {
            if (optionIndex == _index)
            {
                return;
            }

            Index = optionIndex;
            IndexChanged?.Invoke(_index);
        }
    }
}
