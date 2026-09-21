using System;
using System.Globalization;
using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public abstract class SettingsListSection : ToolkitSection
    {
        private readonly ConstantsGenerator _generator = new ConstantsGenerator();
        private SerializedObject _serialized;

        protected SettingsListSection(ToolkitContext context) : base(context)
        {
        }

        protected SerializedObject Serialized => _serialized;

        protected JTLSDKSettings Settings => Context.Project.Settings;

        protected override void OnRendered()
        {
            _serialized = new SerializedObject(Settings);
            BuildContent();
        }

        protected abstract void BuildContent();

        protected TextField TextInput(SerializedProperty property, bool monospace, int width)
        {
            TextField field = new TextField { value = property.stringValue };
            field.AddToClassList("jtl-field");

            if (monospace)
            {
                field.AddToClassList(MonospaceFont.ClassName);
            }

            if (width > 0)
            {
                field.style.width = width;
            }
            else
            {
                field.AddToClassList("jtl-grow");
                field.style.minWidth = 0;
                field.style.flexShrink = 1;
            }

            field.RegisterCallback<FocusOutEvent>(_ => CommitString(property, field.value));
            field.RegisterCallback<KeyDownEvent>(keyEvent =>
            {
                if (keyEvent.keyCode == UnityEngine.KeyCode.Return || keyEvent.keyCode == UnityEngine.KeyCode.KeypadEnter)
                {
                    CommitString(property, field.value);
                }
            });
            return field;
        }

        protected Dropdown EnumInput<T>(SerializedProperty property, string choicesKey, int width) where T : Enum
        {
            Dropdown dropdown = new Dropdown();
            if (width > 0)
            {
                dropdown.style.width = width;
                dropdown.style.minWidth = width;
            }
            else
            {
                dropdown.style.flexGrow = 1;
                dropdown.style.minWidth = 0;
            }
            dropdown.choices = new System.Collections.Generic.List<string>(Context.Localization.GetList(choicesKey));
            dropdown.index = property.enumValueIndex;
            dropdown.RegisterValueChangedCallback(_ =>
            {
                if (dropdown.index >= 0 && dropdown.index != property.enumValueIndex)
                {
                    property.enumValueIndex = dropdown.index;
                    Apply(true);
                }
            });
            return dropdown;
        }

        protected NumberFieldWithUnit FloatInput(SerializedProperty property, int width)
        {
            NumberFieldWithUnit field = new NumberFieldWithUnit { Width = width };
            field.Value = property.floatValue.ToString(CultureInfo.InvariantCulture);
            field.Input.RegisterCallback<FocusOutEvent>(_ =>
            {
                bool valid = float.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) && value >= 0f;
                field.Error = valid == false;

                if (valid && Math.Abs(property.floatValue - value) > float.Epsilon)
                {
                    property.floatValue = value;
                    Apply(false);
                }
            });
            return field;
        }

        protected TextField PlatformIdInput(SerializedProperty platformIds, PlatformId platform)
        {
            SerializedProperty existing = FindPlatformId(platformIds, platform);
            TextField field = new TextField { value = existing == null ? "" : existing.stringValue };
            field.AddToClassList("jtl-field");
            field.AddToClassList(MonospaceFont.ClassName);
            field.AddToClassList("jtl-grow");
            field.style.minWidth = 0;
            field.style.flexShrink = 1;
            field.RegisterCallback<FocusOutEvent>(_ => CommitPlatformId(platformIds, platform, field.value));
            return field;
        }

        protected string UniqueId(SerializedProperty array, string relativeName, string prefix)
        {
            for (int number = 1; number <= array.arraySize + 1; number++)
            {
                string candidate = prefix + "_" + number.ToString(CultureInfo.InvariantCulture);
                bool used = false;

                for (int index = 0; index < array.arraySize; index++)
                {
                    if (array.GetArrayElementAtIndex(index).FindPropertyRelative(relativeName).stringValue == candidate)
                    {
                        used = true;
                    }
                }

                if (used == false)
                {
                    return candidate;
                }
            }

            return prefix + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        protected void RemoveElement(SerializedProperty array, int index)
        {
            array.DeleteArrayElementAtIndex(index);
            Apply(true);
        }

        protected void Apply(bool rerender)
        {
            _serialized.ApplyModifiedProperties();
            Context.Project.Save(Settings);
            Context.Project.NotifyChanged();

            if (rerender)
            {
                Render();
            }
        }

        protected VisualElement GenerateConstantsRow()
        {
            VisualElement row = Row(10);
            row.Add(Button("common.generateConstants", ToolkitButton.SecondaryVariant, "refresh", GenerateConstants));
            row.Add(TextLabel(ConstantsGenerator.DefaultPath, "jtl-text--caption", MonospaceFont.ClassName));
            return row;
        }

        private SerializedProperty FindPlatformId(SerializedProperty platformIds, PlatformId platform)
        {
            for (int index = 0; index < platformIds.arraySize; index++)
            {
                SerializedProperty element = platformIds.GetArrayElementAtIndex(index);

                if (element.FindPropertyRelative("_platform").enumValueIndex == (int)platform)
                {
                    return element.FindPropertyRelative("_id");
                }
            }

            return null;
        }

        private void CommitPlatformId(SerializedProperty platformIds, PlatformId platform, string value)
        {
            string trimmed = value == null ? "" : value.Trim();
            SerializedProperty existing = FindPlatformId(platformIds, platform);

            if (existing == null && trimmed.Length == 0)
            {
                return;
            }

            if (existing == null)
            {
                platformIds.arraySize++;
                SerializedProperty added = platformIds.GetArrayElementAtIndex(platformIds.arraySize - 1);
                added.FindPropertyRelative("_platform").enumValueIndex = (int)platform;
                existing = added.FindPropertyRelative("_id");
                existing.stringValue = "";
            }

            if (existing.stringValue == trimmed)
            {
                return;
            }

            existing.stringValue = trimmed;
            Apply(false);
        }

        private void CommitString(SerializedProperty property, string value)
        {
            string trimmed = value == null ? "" : value.Trim();

            if (property.stringValue == trimmed)
            {
                return;
            }

            property.stringValue = trimmed;
            Apply(false);
        }

        private void GenerateConstants()
        {
            string path = _generator.Generate(Settings, ConstantsGenerator.DefaultPath);
            Context.Report(StatusKind.Success, "common.constantsGenerated", path);
        }
    }
}
