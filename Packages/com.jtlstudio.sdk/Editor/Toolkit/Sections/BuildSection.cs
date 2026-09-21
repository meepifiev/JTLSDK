using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using JTLStudio.SDK.Editor.Build;
using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class BuildSection : ToolkitSection
    {
        private const int LabelWidth = 170;

        private readonly SdkBuildService _builds = new SdkBuildService();
        private BuildResult _lastResult;

        public BuildSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Build;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "BuildSection";

        private JTLSDKEditorSettings Settings => JTLSDKEditorSettings.instance;

        protected override void OnRendered()
        {
            SdkConfiguration active = Context.Project.Active;
            List<BuildCheck> checks = _builds.PreChecks(active);
            bool blocked = checks.Exists(check => check.Passed == false);

            VisualElement body = Require<VisualElement>("build-body");
            VisualElement columns = Row(12);
            columns.AddToClassList("jtl-row--start");
            VisualElement left = Column(12);
            left.AddToClassList("jtl-basis");
            left.Add(CreateSettingsCard(active));
            columns.Add(left);

            VisualElement right = Column(12);
            right.style.width = 380;
            right.style.flexShrink = 0;
            right.Add(CreateChecksCard("build.preChecks", checks));

            if (_lastResult != null && _lastResult.PostChecks.Count > 0)
            {
                right.Add(CreateChecksCard("build.postChecks", new List<BuildCheck>(_lastResult.PostChecks)));
            }

            columns.Add(right);
            body.Add(columns);

            VisualElement footer = Row(12);
            int next = Settings.BuildNumber + 1;
            string output = Path.Combine(Settings.BuildPath, _builds.ResolveName(Settings, active, next)) + (Settings.BuildOutput == BuildOutput.Zip ? ".zip" : "");
            Label outputLabel = TextLabel(output, "jtl-text--caption", MonospaceFont.ClassName);
            outputLabel.tooltip = output;
            outputLabel.style.flexShrink = 1;
            outputLabel.style.minWidth = 0;
            outputLabel.style.overflow = Overflow.Hidden;
            outputLabel.style.whiteSpace = WhiteSpace.NoWrap;
            outputLabel.style.textOverflow = TextOverflow.Ellipsis;
            footer.Add(outputLabel);
            footer.Add(Spacer());
            ToolkitButton build = Button("build.build", ToolkitButton.PrimaryVariant, "build", () => RunBuild(active));
            build.style.flexShrink = 0;
            build.SetEnabled(blocked == false);
            footer.Add(build);
            body.Add(footer);

            if (_lastResult != null && _lastResult.IsSuccess == false)
            {
                body.Add(new InlineMessage { Variant = InlineMessage.ErrorVariant, Text = FailureText(_lastResult) });
            }
        }

        private VisualElement CreateSettingsCard(SdkConfiguration active)
        {
            Card card = new Card { TitleKey = "build.settings", Spacing = 10 };

            FieldRow configuration = new FieldRow("build.configuration", LabelWidth);
            VisualElement box = Row(6);
            box.AddToClassList("jtl-field-box");
            box.AddToClassList("jtl-read-only");
            box.style.maxWidth = 260;
            box.style.flexGrow = 1;
            box.style.flexShrink = 1;
            box.style.minWidth = 0;

            if (active != null)
            {
                box.Add(new PortalMark(Context.Platforms.PortalMark(active.Platform), 16));
            }

            box.Add(TextLabel(active == null ? Context.Text("topbar.noConfiguration") : active.DisplayName, "jtl-read-only__text"));
            configuration.Add(box);
            card.Add(configuration);

            FieldRow development = new FieldRow("build.development", LabelWidth);
            SwitchToggle developmentToggle = new SwitchToggle(Settings.DevelopmentBuild);
            developmentToggle.ValueChanged += value => Change(() => Settings.DevelopmentBuild = value);
            development.Add(developmentToggle);
            card.Add(development);

            FieldRow output = new FieldRow("build.output", LabelWidth);
            RadioGroup outputGroup = new RadioGroup();
            outputGroup.SetChoices(Context.Localization.GetList("build.outputs"));
            outputGroup.Index = (int)Settings.BuildOutput;
            outputGroup.IndexChanged += index => Change(() => Settings.BuildOutput = (BuildOutput)index);
            output.Add(outputGroup);
            card.Add(output);

            FieldRow path = new FieldRow("build.path", LabelWidth);
            VisualElement pathLine = Row(8);
            pathLine.style.flexGrow = 1;
            pathLine.style.flexShrink = 1;
            pathLine.style.minWidth = 0;
            pathLine.style.flexWrap = Wrap.NoWrap;
            TextField pathField = TextInput(Settings.BuildPath, value => Settings.BuildPath = value);
            pathField.style.flexBasis = 0;
            pathField.style.flexShrink = 1;
            pathLine.Add(pathField);
            ToolkitButton browse = Button("build.browse", ToolkitButton.SecondaryVariant, "folder", Browse);
            browse.style.flexShrink = 0;
            pathLine.Add(browse);
            path.Add(pathLine);
            card.Add(path);

            FieldRow name = new FieldRow("build.namePattern", LabelWidth);
            TextField nameField = TextInput(Settings.BuildNamePattern, value => Settings.BuildNamePattern = value);
            nameField.AddToClassList(MonospaceFont.ClassName);
            name.Add(nameField);
            card.Add(name);

            FieldRow number = new FieldRow("build.number", LabelWidth);
            NumberFieldWithUnit numberField = new NumberFieldWithUnit { Width = 100 };
            numberField.Value = Settings.BuildNumber.ToString(CultureInfo.InvariantCulture);
            numberField.Input.RegisterCallback<FocusOutEvent>(focusEvent =>
            {
                bool valid = int.TryParse(numberField.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value >= 0;
                numberField.Error = valid == false;

                if (valid && value != Settings.BuildNumber)
                {
                    Change(() => Settings.BuildNumber = value);
                }
            });
            number.Add(numberField);
            number.Add(TextLabel("→ " + (Settings.BuildNumber + 1).ToString(CultureInfo.InvariantCulture), "jtl-text--secondary", "jtl-ml-8"));
            card.Add(number);

            FieldRow after = new FieldRow("build.after", LabelWidth);
            Checkbox openFolder = new Checkbox(Context.Text("build.openFolder"), Settings.OpenFolderAfterBuild);
            openFolder.ValueChanged += value => Change(() => Settings.OpenFolderAfterBuild = value);
            after.Add(openFolder);
            card.Add(after);
            return card;
        }

        private VisualElement CreateChecksCard(string titleKey, List<BuildCheck> checks)
        {
            int passed = checks.FindAll(check => check.Passed).Count;
            Card card = new Card { TitleKey = titleKey, Spacing = 8 };
            bool allPassed = passed == checks.Count;
            card.Header.Add(new Badge { Text = passed + " / " + checks.Count, Variant = allPassed ? Badge.SuccessVariant : Badge.ErrorVariant });

            foreach (BuildCheck check in checks)
            {
                card.Add(new CheckRow { Text = Context.Text(check.Key, check.Arguments), State = check.Passed ? CheckRow.PassState : CheckRow.FailState });
            }

            return card;
        }

        private TextField TextInput(string value, Action<string> assign)
        {
            TextField field = new TextField { value = value };
            field.AddToClassList("jtl-field");
            field.AddToClassList("jtl-grow");
            field.style.minWidth = 0;
            field.RegisterCallback<FocusOutEvent>(focusEvent => Change(() => assign(field.value)));
            return field;
        }

        private void Browse()
        {
            string selected = EditorUtility.OpenFolderPanel(Context.Text("build.path"), Settings.BuildPath, "");

            if (string.IsNullOrEmpty(selected))
            {
                return;
            }

            string project = Path.GetFullPath(".").Replace('\\', '/') + "/";
            string normalized = selected.Replace('\\', '/');
            string value = normalized.StartsWith(project) ? normalized.Substring(project.Length) : normalized;
            Change(() => Settings.BuildPath = value);
        }

        private void Change(Action change)
        {
            change();
            Settings.Persist();
            Render();
        }

        private string FailureText(BuildResult result)
        {
            if (string.IsNullOrEmpty(result.Error) == false)
            {
                return result.Error;
            }

            BuildCheck failed = null;

            foreach (BuildCheck check in result.PostChecks)
            {
                if (check.Passed == false)
                {
                    failed = check;
                    break;
                }
            }

            return failed == null ? "" : Context.Text(failed.Key, failed.Arguments);
        }

        private void RunBuild(SdkConfiguration active)
        {
            _lastResult = _builds.Build(Settings, active);

            if (_lastResult.IsSuccess)
            {
                Context.Report(StatusKind.Success, "build.done", Settings.BuildNumber, (_lastResult.TotalBytes / 1048576f).ToString("0.0", CultureInfo.InvariantCulture), _lastResult.OutputPath);
            }
            else
            {
                Context.Report(StatusKind.Error, "build.failed", FailureText(_lastResult));
            }

            Render();
        }
    }
}
