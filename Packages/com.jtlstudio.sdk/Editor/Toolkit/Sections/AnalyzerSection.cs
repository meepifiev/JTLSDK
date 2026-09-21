using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class AnalyzerSection : ToolkitSection
    {
        private const string StatusTime = "14:12";
        private const int FileIconSize = 16;
        private const int ArrowIconSize = 14;
        private const int CompactButtonGap = 6;

        private readonly string[][] _findings =
        {
            new[] { "GlobalTimeScaler.cs:23", "Time.timeScale = 0.3f", "JTLSDK.Time.Scale = 0.3f" },
            new[] { "PauseMenu.cs:64", "Time.timeScale = 0f", "JTLSDK.Time.Scale = 0f" },
            new[] { "SettingHandler.cs:41", "AudioListener.volume = v", "JTLSDK.Audio.Volume = v" },
            new[] { "MusicPlayer.cs:18", "AudioListener.pause = true", "JTLSDK.Pause.Set(\"Music\", true)" },
            new[] { "SavesService.cs:88", "PlayerPrefs.GetInt(\"Level\")", "JTLSDK.Data.GetInt(\"Level\")" },
            new[] { "SavesService.cs:96", "PlayerPrefs.SetInt(\"Level\", n)", "JTLSDK.Data.SetInt(\"Level\", n)" },
            new[] { "CursorLock.cs:12", "Cursor.lockState = CursorLockMode.Locked", "JTLSDK.Device.CursorLock = CursorLockMode.Locked" }
        };

        public AnalyzerSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Analyzer;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Warning, "analyzer.status", StatusTime);

        protected override string TemplateName => "AnalyzerSection";

        protected override void OnRendered()
        {
            VisualElement findings = Require<VisualElement>("findings");

            for (int index = 0; index < _findings.Length; index++)
            {
                findings.Add(CreateFindingRow(_findings[index], index == 0));
            }
        }

        private VisualElement CreateFindingRow(string[] finding, bool first)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-finding-row");
            row.EnableInClassList("jtl-finding-row--first", first);

            VisualElement file = new VisualElement();
            file.AddToClassList("jtl-finding-row__file");
            file.Add(new Icon("analyzer", FileIconSize, "muted"));
            Label fileName = new Label(finding[0]);
            fileName.AddToClassList("jtl-text--small");
            file.Add(fileName);
            row.Add(file);

            Label call = new Label(finding[1]);
            call.AddToClassList("jtl-finding-row__call");
            call.AddToClassList("jtl-mono");
            row.Add(call);

            VisualElement arrow = new VisualElement();
            arrow.AddToClassList("jtl-finding-row__arrow");
            arrow.Add(new Icon("arrow-right", ArrowIconSize, "muted"));
            row.Add(arrow);

            Label replacement = new Label(finding[2]);
            replacement.AddToClassList("jtl-finding-row__replacement");
            replacement.AddToClassList("jtl-mono");
            row.Add(replacement);

            VisualElement actions = new VisualElement();
            actions.AddToClassList("jtl-finding-row__actions");
            ToolkitButton open = new ToolkitButton("analyzer.open", ToolkitButton.GhostVariant);
            open.Compact = true;
            open.style.marginRight = CompactButtonGap;
            ToolkitButton replace = new ToolkitButton("analyzer.replace", ToolkitButton.SecondaryVariant);
            replace.Compact = true;
            actions.Add(open);
            actions.Add(replace);
            row.Add(actions);
            return row;
        }
    }
}
