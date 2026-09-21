using System.IO;
using UnityEditor;


namespace JTLStudio.SDK.Editor.Development
{
    public class SampleSync
    {
        private const string PackagePath = "Packages/com.jtlstudio.sdk";
        private const string SampleFolder = "Samples~/Demo";
        private const string ImportRoot = "Assets/Samples/JTL SDK";
        private const string SampleName = "Demo";

        public string PackageSamplePath => Path.Combine(PackagePath, SampleFolder);

        public string ImportedSamplePath
        {
            get
            {
                UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);
                string version = package == null ? "0.0.0" : package.version;
                return Path.Combine(ImportRoot, version, SampleName);
            }
        }

        public void ImportToProject()
        {
            CopyDirectory(PackageSamplePath, ImportedSamplePath);
            AssetDatabase.Refresh();
        }

        public void SyncToPackage()
        {
            if (Directory.Exists(ImportedSamplePath) == false)
            {
                throw new DirectoryNotFoundException(ImportedSamplePath);
            }

            CopyDirectory(ImportedSamplePath, PackageSamplePath);
        }

        private void CopyDirectory(string source, string destination)
        {
            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, true);
            }

            Directory.CreateDirectory(destination);

            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file) == ".gitkeep")
                {
                    continue;
                }

                string relative = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string target = Path.Combine(destination, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file, target, true);
            }
        }
    }
}
