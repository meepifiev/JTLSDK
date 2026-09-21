using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class BuildSection : ToolkitSection
    {
        private const string ReadyTime = "14:03";
        private const string BlockedTime = "14:11";
        private const string ShowBlockedKey = "build.showBlocked";
        private const string ShowReadyKey = "build.showReady";
        private const string PassedBadgeKey = "badge.passed4";
        private const string FailedBadgeKey = "badge.failed1";

        private bool _blocked;

        public BuildSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Build;

        public override ToolkitStatus Status => _blocked
            ? new ToolkitStatus(StatusKind.Error, "build.statusBlocked", BlockedTime)
            : new ToolkitStatus(StatusKind.Success, "build.status", ReadyTime);

        protected override string TemplateName => "BuildSection";

        protected override void OnRendered()
        {
            ToolkitButton toggle = Require<ToolkitButton>("toggle-state");
            toggle.TextKey = _blocked ? ShowReadyKey : ShowBlockedKey;
            toggle.clicked += OnToggleClicked;

            Card checks = Require<Card>("pre-build-card");
            checks.BadgeKey = _blocked ? FailedBadgeKey : PassedBadgeKey;
            checks.BadgeVariant = _blocked ? Badge.ErrorVariant : Badge.SuccessVariant;
            Require<CheckRow>("compression-check").State = _blocked ? CheckRow.FailState : CheckRow.PassState;
            Require<InlineMessage>("blocked-message").style.display = _blocked ? DisplayStyle.Flex : DisplayStyle.None;
            Require<ToolkitButton>("build-button").SetEnabled(_blocked == false);
        }

        private void OnToggleClicked()
        {
            _blocked = _blocked == false;
            Render();
            ShowStatus(Status);
        }
    }
}
