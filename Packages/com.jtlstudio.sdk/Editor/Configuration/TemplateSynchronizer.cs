using UnityEditor;

namespace JTLStudio.SDK.Editor.Configuration
{
    [InitializeOnLoad]
    public static class TemplateSynchronizer
    {
        static TemplateSynchronizer()
        {
            EditorApplication.delayCall += Synchronize;
        }

        private static void Synchronize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            TemplateService template = new TemplateService();

            if (template.IsOutdated)
            {
                template.Update();
            }
        }
    }
}
