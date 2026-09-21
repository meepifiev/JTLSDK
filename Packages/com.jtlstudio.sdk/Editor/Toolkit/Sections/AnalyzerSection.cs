using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using JTLStudio.SDK.Editor.Analyzer;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class AnalyzerSection : ToolkitSection
    {
        private const string FolderPreference = "JTLSDK.Analyzer.Folder";
        private const string ExcludePreference = "JTLSDK.Analyzer.Exclude";
        private const string DefaultFolder = "Assets";
        private const string DefaultExclude = "Assets/Plugins";

        private readonly ApiAnalyzer _analyzer = new ApiAnalyzer();
        private List<AnalyzerFinding> _findings;
        private double _scanSeconds;

        public AnalyzerSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Analyzer;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "AnalyzerSection";

        protected override void OnRendered()
        {
            VisualElement body = Require<VisualElement>("analyzer-body");
            body.Add(CreateScanCard());

            if (_findings == null)
            {
                return;
            }

            body.Add(CreateResultsCard());

            if (_findings.Exists(finding => finding.IsSimple))
            {
                VisualElement actions = Row(12);
                actions.Add(Spacer());
                actions.Add(Button("analyzer.replaceAll", ToolkitButton.SecondaryVariant, "refresh", ReplaceAllSimple));
                actions.Add(Localized("analyzer.replaceAllNote", "jtl-text--caption"));
                body.Add(actions);
            }
        }

        private VisualElement CreateScanCard()
        {
            Card card = new Card();
            VisualElement row = Row(12);
            row.AddToClassList("jtl-row--end");

            TextField folder = PathField("analyzer.folder", EditorPrefs.GetString(FolderPreference, DefaultFolder), FolderPreference, row);
            TextField exclude = PathField("analyzer.exclude", EditorPrefs.GetString(ExcludePreference, DefaultExclude), ExcludePreference, row);
            row.Add(Button("analyzer.scan", ToolkitButton.PrimaryVariant, "search", () => Scan(folder.value, exclude.value)));
            card.Add(row);
            return card;
        }

        private TextField PathField(string labelKey, string value, string preference, VisualElement row)
        {
            VisualElement column = Column(4);
            column.AddToClassList("jtl-basis");
            column.Add(Localized(labelKey, "jtl-text--secondary"));
            TextField field = new TextField { value = value };
            field.AddToClassList("jtl-field");
            field.AddToClassList(MonospaceFont.ClassName);
            field.RegisterCallback<FocusOutEvent>(_ => EditorPrefs.SetString(preference, field.value.Trim()));
            column.Add(field);
            row.Add(column);
            return field;
        }

        private VisualElement CreateResultsCard()
        {
            Card card = new Card { Title = Context.Text("analyzer.foundCount", _findings.Count), Spacing = 8 };
            card.Header.Add(TextLabel(Context.Text("analyzer.scannedFormat", _analyzer.ScannedFiles, _scanSeconds.ToString("0.0", CultureInfo.InvariantCulture)), "jtl-card__caption"));

            if (_findings.Count == 0)
            {
                card.Add(Localized("analyzer.nothingFound", "jtl-text--secondary"));
                return card;
            }

            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-table");

            foreach (AnalyzerFinding finding in _findings)
            {
                table.Add(CreateFindingRow(finding));
            }

            card.Add(table);
            return card;
        }

        private VisualElement CreateFindingRow(AnalyzerFinding finding)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-finding-row");

            VisualElement file = Row(6);
            file.AddToClassList("jtl-finding-row__file");
            file.Add(new Icon("search", 14, "muted"));
            file.Add(TextLabel(System.IO.Path.GetFileName(finding.Path) + ":" + finding.Line, "jtl-text--small"));
            file.tooltip = finding.Path;
            row.Add(file);

            Label call = TextLabel(finding.Code, "jtl-finding-row__call", MonospaceFont.ClassName);
            row.Add(call);
            row.Add(TextLabel("→", "jtl-finding-row__arrow"));
            Label replacement = TextLabel(finding.HasReplacement ? finding.Replacement : Context.Text("analyzer.manual"), "jtl-finding-row__replacement", MonospaceFont.ClassName);
            row.Add(replacement);

            VisualElement actions = Row(6);
            actions.AddToClassList("jtl-finding-row__actions");
            ToolkitButton open = Button("analyzer.open", ToolkitButton.GhostVariant, "", () => Open(finding));
            open.Compact = true;
            actions.Add(open);

            if (finding.HasReplacement)
            {
                ToolkitButton replace = Button("analyzer.replace", ToolkitButton.SecondaryVariant, "", () => ReplaceOne(finding));
                replace.Compact = true;
                actions.Add(replace);
            }

            row.Add(actions);
            return row;
        }

        private void Scan(string folder, string exclude)
        {
            EditorPrefs.SetString(FolderPreference, folder.Trim());
            EditorPrefs.SetString(ExcludePreference, exclude.Trim());
            List<string> excluded = new List<string>(exclude.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
            Stopwatch stopwatch = Stopwatch.StartNew();
            _findings = _analyzer.Scan(folder.Trim(), excluded);
            stopwatch.Stop();
            _scanSeconds = stopwatch.Elapsed.TotalSeconds;
            Context.Report(StatusKind.Info, "analyzer.scanDone", _findings.Count, folder.Trim());
            Render();
        }

        private void Open(AnalyzerFinding finding)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<MonoScript>(finding.Path);

            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, finding.Line);
            }
        }

        private void ReplaceOne(AnalyzerFinding finding)
        {
            if (_analyzer.Replace(finding))
            {
                _findings.Remove(finding);
                AssetDatabase.ImportAsset(finding.Path);
                Context.Report(StatusKind.Success, "analyzer.replaced", System.IO.Path.GetFileName(finding.Path), finding.Line);
            }
            else
            {
                Context.Report(StatusKind.Error, "analyzer.replaceFailed", System.IO.Path.GetFileName(finding.Path), finding.Line);
            }

            Render();
        }

        private void ReplaceAllSimple()
        {
            List<AnalyzerFinding> simple = _findings.FindAll(finding => finding.IsSimple);
            HashSet<string> files = new HashSet<string>();

            foreach (AnalyzerFinding finding in simple)
            {
                files.Add(finding.Path);
            }

            bool confirmed = EditorUtility.DisplayDialog(Context.Text("analyzer.replaceAll"), Context.Text("analyzer.replaceAllConfirm", simple.Count, files.Count), Context.Text("analyzer.replace"), Context.Text("details.cancel"));

            if (confirmed == false)
            {
                return;
            }

            int replaced = 0;

            foreach (AnalyzerFinding finding in simple)
            {
                if (_analyzer.Replace(finding))
                {
                    replaced++;
                    _findings.Remove(finding);
                }
            }

            AssetDatabase.Refresh();
            Context.Report(StatusKind.Success, "analyzer.replacedCount", replaced);
            Render();
        }
    }
}
