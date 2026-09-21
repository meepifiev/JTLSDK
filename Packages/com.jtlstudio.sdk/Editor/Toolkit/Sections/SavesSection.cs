using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class SavesSection : ToolkitSection
    {
        private const string StatusTime = "14:10";
        private const string ShowEmptyKey = "saves.showEmpty";
        private const string ShowFilledKey = "saves.showFilled";

        private bool _empty;

        public SavesSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Saves;

        public override ToolkitStatus Status => _empty
            ? new ToolkitStatus(StatusKind.Pending, "saves.statusEmpty", StatusTime)
            : new ToolkitStatus(StatusKind.Success, "saves.status", StatusTime);

        protected override string TemplateName => "SavesSection";

        protected override void OnRendered()
        {
            Require<VisualElement>("filled").style.display = _empty ? DisplayStyle.None : DisplayStyle.Flex;
            Require<VisualElement>("empty").style.display = _empty ? DisplayStyle.Flex : DisplayStyle.None;
            ToolkitButton toggle = Require<ToolkitButton>("toggle-state");
            toggle.TextKey = _empty ? ShowFilledKey : ShowEmptyKey;
            toggle.clicked += OnToggleClicked;
        }

        private void OnToggleClicked()
        {
            _empty = _empty == false;
            Render();
            ShowStatus(Status);
        }
    }
}
