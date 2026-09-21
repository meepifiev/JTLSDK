using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using JTLStudio.SDK.Editor.Toolkit.Components;
using JTLStudio.SDK.Prototype;
using JTLStudio.SDK.Services.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class SavesSection : ToolkitSection
    {
        private const string ValuesKey = "values";
        private const string RevisionKey = "revision";
        private const string FormatKey = "format";
        private const string SavedAtKey = "savedAt";
        private const int DocumentFormat = 1;
        private const float BytesPerKilobyte = 1024f;

        private readonly JsonParser _parser = new JsonParser();
        private readonly JsonWriter _writer = new JsonWriter();
        private Dictionary<string, object> _document;
        private Dictionary<string, object> _values;
        private string _filter = "";

        public SavesSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Saves;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "SavesSection";

        protected override void OnRendered()
        {
            Load();
            VisualElement body = Require<VisualElement>("saves-body");
            body.Add(CreateToolbar());

            if (_values.Count == 0)
            {
                EmptyState empty = new EmptyState { TitleKey = "saves.emptyTitle", IconName = "saves" };
                empty.Add(Button("saves.addKey", ToolkitButton.SecondaryVariant, "plus", AddKey));
                body.Add(empty);
            }
            else
            {
                body.Add(CreateTable());
            }

            body.Add(CreateFooter());
            body.Add(ProvidersCard("_data"));
        }

        private VisualElement CreateToolbar()
        {
            VisualElement toolbar = Row(10);
            string json = PlayerPrefs.GetString(PrototypeDataProvider.StorageKey, "");
            long revision = _document.TryGetValue(RevisionKey, out object value) && value is long number ? number : 0;
            float kilobytes = Encoding.UTF8.GetByteCount(json) / BytesPerKilobyte;
            SdkConfiguration active = Context.Project.Active;
            int limit = active != null && active.Data != null ? active.Data.MaxBytes : 0;
            string size = limit > 0
                ? Context.Text("saves.sizeWithLimit", kilobytes.ToString("0.0", CultureInfo.InvariantCulture), (limit / BytesPerKilobyte).ToString("0", CultureInfo.InvariantCulture))
                : Context.Text("saves.size", kilobytes.ToString("0.0", CultureInfo.InvariantCulture));
            toolbar.Add(TextLabel(Context.Text("saves.revision", revision), "jtl-text--secondary"));
            toolbar.Add(TextLabel("·", "jtl-text--muted"));
            toolbar.Add(TextLabel(size, "jtl-text--secondary"));
            toolbar.Add(new Badge(string.IsNullOrEmpty(json) ? "badge.empty" : "badge.loaded", string.IsNullOrEmpty(json) ? Badge.NeutralVariant : Badge.SuccessVariant));
            toolbar.Add(Spacer());

            TextField search = new TextField { value = _filter };
            search.AddToClassList("jtl-field");
            search.style.width = 200;
            search.RegisterValueChangedCallback(changeEvent =>
            {
                _filter = changeEvent.newValue ?? "";
                Render();
                Root.Q<TextField>(className: "jtl-saves-search")?.Focus();
            });
            search.AddToClassList("jtl-saves-search");
            toolbar.Add(search);
            toolbar.Add(Button("saves.addKey", ToolkitButton.SecondaryVariant, "plus", AddKey));
            return toolbar;
        }

        private VisualElement CreateTable()
        {
            Card card = new Card { Spacing = 0 };
            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-table");
            VisualElement head = new VisualElement();
            head.AddToClassList("jtl-table__row");
            head.AddToClassList("jtl-table__row--head");
            head.Add(Heading("saves.columnKey", 220));
            head.Add(Heading("saves.columnType", 90));
            head.Add(Heading("saves.columnValue", 0));
            table.Add(head);

            List<string> keys = new List<string>(_values.Keys);
            keys.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string key in keys)
            {
                if (_filter.Length > 0 && key.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                table.Add(CreateRow(key, _values[key]));
            }

            card.Add(table);
            return card;
        }

        private VisualElement CreateRow(string key, object value)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-table__row");
            Label keyLabel = TextLabel(key, "jtl-text", MonospaceFont.ClassName);
            keyLabel.style.width = 220;
            row.Add(keyLabel);
            Label typeLabel = TextLabel(TypeName(value), "jtl-text--secondary");
            typeLabel.style.width = 90;
            row.Add(typeLabel);

            VisualElement cell = new VisualElement();
            cell.AddToClassList("jtl-grow");
            cell.AddToClassList("jtl-row");
            cell.Add(ValueEditor(key, value));
            row.Add(cell);

            IconButton delete = new IconButton("delete", 24);
            delete.clicked += () => DeleteKey(key);
            row.Add(delete);
            return row;
        }

        private VisualElement ValueEditor(string key, object value)
        {
            if (value is bool flag)
            {
                SwitchToggle toggle = new SwitchToggle(flag);
                toggle.ValueChanged += changed => SetValue(key, changed);
                return toggle;
            }

            bool isObject = value is Dictionary<string, object> || value is List<object>;
            TextField field = new TextField { value = ValueText(value), multiline = isObject };
            field.AddToClassList("jtl-field");
            field.AddToClassList("jtl-grow");

            if (isObject || value is string == false)
            {
                field.AddToClassList(MonospaceFont.ClassName);
            }

            field.RegisterCallback<FocusOutEvent>(focusEvent => CommitValue(key, value, field.value));
            return field;
        }

        private VisualElement CreateFooter()
        {
            VisualElement footer = Row(8);
            footer.Add(Button("saves.resetAll", ToolkitButton.DangerVariant, "", ResetAll));
            footer.Add(Spacer());
            footer.Add(Button("saves.exportJson", ToolkitButton.SecondaryVariant, "", ExportJson));
            footer.Add(Button("saves.importJson", ToolkitButton.SecondaryVariant, "", ImportJson));
            footer.Add(Button("saves.copyJson", ToolkitButton.GhostVariant, "copy", CopyJson));
            return footer;
        }

        private Label Heading(string key, int width)
        {
            LocalizedLabel label = Localized(key, "jtl-table__heading");

            if (width > 0)
            {
                label.style.width = width;
            }
            else
            {
                label.AddToClassList("jtl-grow");
            }

            return label;
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(PrototypeDataProvider.StorageKey, "");
            _document = new Dictionary<string, object>();
            _values = new Dictionary<string, object>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                _document = _parser.Parse(json) as Dictionary<string, object> ?? new Dictionary<string, object>();
                _values = _document.TryGetValue(ValuesKey, out object values) && values is Dictionary<string, object> dictionary ? dictionary : new Dictionary<string, object>();
            }
            catch (FormatException)
            {
                Context.Report(StatusKind.Error, "saves.corrupted");
            }
        }

        private void Store()
        {
            long revision = _document.TryGetValue(RevisionKey, out object value) && value is long number ? number : 0;
            _document[FormatKey] = (long)DocumentFormat;
            _document[RevisionKey] = revision + 1;
            _document[SavedAtKey] = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            _document[ValuesKey] = _values;
            PlayerPrefs.SetString(PrototypeDataProvider.StorageKey, _writer.Write(_document));
            PlayerPrefs.Save();
        }

        private string TypeName(object value)
        {
            switch (value)
            {
                case bool _:
                    return "bool";

                case long _:
                    return "int";

                case double _:
                    return "float";

                case string _:
                    return "string";

                default:
                    return "object";
            }
        }

        private string ValueText(object value)
        {
            switch (value)
            {
                case string text:
                    return text;

                case long number:
                    return number.ToString(CultureInfo.InvariantCulture);

                case double number:
                    return number.ToString("R", CultureInfo.InvariantCulture);

                default:
                    return _writer.Write(value);
            }
        }

        private void CommitValue(string key, object previous, string text)
        {
            object parsed;

            switch (previous)
            {
                case long _:
                    if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer) == false)
                    {
                        Context.Report(StatusKind.Error, "saves.invalidValue", key);
                        return;
                    }

                    parsed = integer;
                    break;

                case double _:
                    if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) == false)
                    {
                        Context.Report(StatusKind.Error, "saves.invalidValue", key);
                        return;
                    }

                    parsed = number;
                    break;

                case string _:
                    parsed = text;
                    break;

                default:
                    try
                    {
                        parsed = _parser.Parse(text);
                    }
                    catch (FormatException)
                    {
                        Context.Report(StatusKind.Error, "saves.invalidValue", key);
                        return;
                    }

                    break;
            }

            if (ValueText(parsed) == ValueText(previous))
            {
                return;
            }

            SetValue(key, parsed);
        }

        private void SetValue(string key, object value)
        {
            _values[key] = value;
            Store();
            Context.Report(StatusKind.Success, "saves.valueSaved", key);
            Render();
        }

        private void AddKey()
        {
            string key = "key";
            int suffix = 1;

            while (_values.ContainsKey(key + suffix.ToString(CultureInfo.InvariantCulture)))
            {
                suffix++;
            }

            SetValue(key + suffix.ToString(CultureInfo.InvariantCulture), "");
        }

        private void DeleteKey(string key)
        {
            _values.Remove(key);
            Store();
            Context.Report(StatusKind.Warning, "saves.keyDeleted", key);
            Render();
        }

        private void ResetAll()
        {
            bool confirmed = EditorUtility.DisplayDialog(Context.Text("saves.resetTitle"), Context.Text("saves.resetMessage"), Context.Text("saves.resetAll"), Context.Text("details.cancel"));

            if (confirmed == false)
            {
                return;
            }

            PlayerPrefs.DeleteKey(PrototypeDataProvider.StorageKey);
            PlayerPrefs.Save();
            Context.Report(StatusKind.Warning, "saves.resetDone");
            Render();
        }

        private void ExportJson()
        {
            string path = EditorUtility.SaveFilePanel(Context.Text("saves.exportJson"), "", "save.json", "json");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllText(path, PlayerPrefs.GetString(PrototypeDataProvider.StorageKey, "{}"));
            Context.Report(StatusKind.Success, "saves.exported", path);
        }

        private void ImportJson()
        {
            string path = EditorUtility.OpenFilePanel(Context.Text("saves.importJson"), "", "json");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string json = File.ReadAllText(path);

            try
            {
                _parser.Parse(json);
            }
            catch (FormatException)
            {
                Context.Report(StatusKind.Error, "saves.corrupted");
                return;
            }

            PlayerPrefs.SetString(PrototypeDataProvider.StorageKey, json);
            PlayerPrefs.Save();
            Context.Report(StatusKind.Success, "saves.imported", path);
            Render();
        }

        private void CopyJson()
        {
            EditorGUIUtility.systemCopyBuffer = PlayerPrefs.GetString(PrototypeDataProvider.StorageKey, "");
            Context.Report(StatusKind.Info, "saves.copied");
        }
    }
}
