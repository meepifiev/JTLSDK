using System;
using System.Globalization;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class FlagsSection : SettingsListSection
    {
        private const string FlagsProperty = "_flags";

        public FlagsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Flags;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "FlagsSection";

        protected override void BuildContent()
        {
            VisualElement root = Require<VisualElement>("flags");
            SerializedProperty flags = Serialized.FindProperty(FlagsProperty);
            Card card = new Card { TitleKey = "flags.cardTitle", Spacing = 8 };

            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-table");
            VisualElement head = new VisualElement();
            head.AddToClassList("jtl-table__row");
            head.AddToClassList("jtl-table__row--head");
            LocalizedLabel keyHeading = Localized("flags.columnKey", "jtl-table__heading");
            keyHeading.style.width = 230;
            head.Add(keyHeading);
            LocalizedLabel typeHeading = Localized("flags.columnType", "jtl-table__heading");
            typeHeading.style.width = 130;
            head.Add(typeHeading);
            head.Add(Localized("flags.columnDefault", "jtl-table__heading", "jtl-grow"));
            table.Add(head);

            for (int index = 0; index < flags.arraySize; index++)
            {
                table.Add(CreateRow(flags, index));
            }

            card.Add(table);

            if (flags.arraySize == 0)
            {
                card.Add(Localized("flags.empty", "jtl-text--secondary"));
            }

            VisualElement actions = Row(10);
            ToolkitButton add = Button("flags.add", ToolkitButton.SecondaryVariant, "plus", AddFlag);
            add.Compact = true;
            actions.Add(add);
            card.Add(actions);
            card.Add(GenerateConstantsRow());
            root.Add(card);
            root.Add(ProvidersCard("_flags"));
        }

        private VisualElement CreateRow(SerializedProperty flags, int index)
        {
            SerializedProperty flag = flags.GetArrayElementAtIndex(index);
            SerializedProperty type = flag.FindPropertyRelative("_type");
            SerializedProperty defaultValue = flag.FindPropertyRelative("_defaultValue");
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-table__row");
            TextField key = TextInput(flag.FindPropertyRelative("_key"), true, 220);
            key.style.marginRight = 10;
            row.Add(key);
            Dropdown typeInput = EnumInput<FlagType>(type, "flags.types", 120);
            typeInput.style.marginRight = 10;
            row.Add(typeInput);

            VisualElement valueCell = new VisualElement();
            valueCell.AddToClassList("jtl-grow");
            valueCell.AddToClassList("jtl-row");
            valueCell.Add(DefaultValueInput((FlagType)type.enumValueIndex, defaultValue));
            row.Add(valueCell);

            IconButton delete = new IconButton("delete", 24);
            delete.clicked += () => RemoveElement(flags, index);
            row.Add(delete);
            return row;
        }

        private VisualElement DefaultValueInput(FlagType type, SerializedProperty defaultValue)
        {
            if (type == FlagType.Bool)
            {
                bool.TryParse(defaultValue.stringValue, out bool current);
                SwitchToggle toggle = new SwitchToggle(current);
                toggle.ValueChanged += value =>
                {
                    defaultValue.stringValue = value ? "true" : "false";
                    Apply(false);
                };
                return toggle;
            }

            if (type == FlagType.String)
            {
                return TextInput(defaultValue, false, 240);
            }

            NumberFieldWithUnit number = new NumberFieldWithUnit { Width = 120 };
            number.Value = defaultValue.stringValue;
            number.Input.RegisterCallback<FocusOutEvent>(focusEvent =>
            {
                bool valid = type == FlagType.Int
                    ? int.TryParse(number.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int _)
                    : float.TryParse(number.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float _);
                number.Error = valid == false;

                if (valid && defaultValue.stringValue != number.Value)
                {
                    defaultValue.stringValue = number.Value.Trim();
                    Apply(false);
                }
            });
            return number;
        }

        private void AddFlag()
        {
            SerializedProperty flags = Serialized.FindProperty(FlagsProperty);
            string key = UniqueId(flags, "_key", "flag");
            flags.arraySize++;
            SerializedProperty added = flags.GetArrayElementAtIndex(flags.arraySize - 1);
            added.FindPropertyRelative("_key").stringValue = key;
            added.FindPropertyRelative("_type").enumValueIndex = (int)FlagType.Bool;
            added.FindPropertyRelative("_defaultValue").stringValue = "false";
            Apply(true);
        }
    }
}
