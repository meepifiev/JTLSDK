using JTLStudio.SDK.Editor.Configuration;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace JTLStudio.SDK.Editor.Build
{
    public class TemplateBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private readonly TemplateService _template = new TemplateService();
        private readonly SettingsAssetService _settings = new SettingsAssetService();
        private readonly PlatformBridgeFilter _bridges = new PlatformBridgeFilter();

        private const string OutdatedMessage = "JTL SDK: the WebGL template is outdated. Open JTL SDK › Toolkit › Template and press Reinstall.";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            _bridges.Register();

            if (_template.IsSelected && _template.IsOutdated)
            {
                throw new BuildFailedException(OutdatedMessage);
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL || _template.IsSelected == false)
            {
                return;
            }

            JTLSDKSettings settings = _settings.Find();
            SdkConfiguration configuration = settings == null ? null : settings.ActiveConfiguration;
            int buildNumber = SessionState.GetInt(TemplateService.BuildNumberKey, JTLSDKEditorSettings.instance.BuildNumber);
            bool development = JTLSDKEditorSettings.instance.DevelopmentBuild;
            _template.CopyImages(JTLSDKEditorSettings.instance, report.summary.outputPath);
            _template.Substitute(report.summary.outputPath, _template.Values(JTLSDKEditorSettings.instance, configuration, buildNumber, development));
        }
    }
}
