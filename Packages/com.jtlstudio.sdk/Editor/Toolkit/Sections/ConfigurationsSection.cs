using JTLStudio.SDK.Editor.Toolkit.Components;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class ConfigurationsSection : ToolkitSection
    {
        private const string StatusTime = "14:03";

        public ConfigurationsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Configurations;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Success, "configurations.status", StatusTime);

        protected override string TemplateName => "ConfigurationsSection";

        protected override void OnRendered()
        {
            Require<ToolkitButton>("open-configuration").clicked += OnOpenConfigurationClicked;
        }

        private void OnOpenConfigurationClicked()
        {
            NavigateTo(ToolkitSectionId.ConfigurationDetails);
        }
    }
}
