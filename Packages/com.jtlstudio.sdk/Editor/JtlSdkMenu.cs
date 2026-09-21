using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Editor
{
    public static class JtlSdkMenu
    {
        private const string Root = "JTL SDK/";

        [MenuItem(Root + "Create Settings", false, 20)]
        private static void CreateSettings()
        {
            JTLSDKSettings settings = new SettingsAssetService().GetOrCreate();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        [MenuItem(Root + "Development/Create Demo Settings", false, 100)]
        private static void CreateDemoSettings()
        {
            JTLSDKSettings settings = new DemoSettingsSetup().Create();
            Selection.activeObject = settings;
            Debug.Log("[JTL SDK] Demo settings are ready: " + AssetDatabase.GetAssetPath(settings));
        }

        [MenuItem(Root + "Development/Import Demo Sample", false, 101)]
        private static void ImportDemoSample()
        {
            SampleSync sync = new SampleSync();
            sync.ImportToProject();
            Debug.Log("[JTL SDK] Demo sample imported to " + sync.ImportedSamplePath);
        }

        [MenuItem(Root + "Development/Sync Demo To Package", false, 102)]
        private static void SyncDemoToPackage()
        {
            SampleSync sync = new SampleSync();
            sync.SyncToPackage();
            Debug.Log("[JTL SDK] Demo sample synced to " + sync.PackageSamplePath);
        }
    }
}
