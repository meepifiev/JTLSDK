using JTLStudio.SDK.Editor.Toolkit.Components;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class ConfigurationDetailsSection : ToolkitSection
    {
        private const string StatusTime = "14:07";

        public ConfigurationDetailsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.ConfigurationDetails;

        public override ToolkitSectionId NavigationId => ToolkitSectionId.Configurations;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Error, "details.status", StatusTime);

        protected override string TemplateName => "ConfigurationDetailsSection";

        protected override void OnRendered()
        {
            Require<IconButton>("back-button").clicked += OnBackClicked;
        }

        private void OnBackClicked()
        {
            NavigateTo(ToolkitSectionId.Configurations);
        }
    }
}
