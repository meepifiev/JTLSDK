using System;
using System.Collections.Generic;
using JTLStudio.SDK.Services.Json;
using UnityEditor;
using UnityEngine.Networking;

namespace JTLStudio.SDK.Editor.Updates
{
    public class ModuleCatalog
    {
        public const string CatalogUrl = "https://api.github.com/repos/meepifiev/JTLSDK/contents/modules.json?ref=main";

        private const string UserAgent = "JTLSDK-Toolkit";
        private const string RawContent = "application/vnd.github.raw";

        private readonly JsonParser _parser = new JsonParser();

        public void Fetch(Action<ModuleCatalogResult> onDone)
        {
            if (onDone == null)
            {
                throw new ArgumentNullException(nameof(onDone));
            }

            UnityWebRequest request = UnityWebRequest.Get(CatalogUrl);
            request.SetRequestHeader("User-Agent", UserAgent);
            request.SetRequestHeader("Accept", RawContent);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            void Poll()
            {
                if (operation.isDone == false)
                {
                    return;
                }

                EditorApplication.update -= Poll;
                ModuleCatalogResult result = request.result == UnityWebRequest.Result.Success
                    ? Parse(request.downloadHandler.text)
                    : new ModuleCatalogResult(false, request.error, new List<ModuleDefinition>());
                request.Dispose();
                onDone(result);
            }

            EditorApplication.update += Poll;
        }

        public ModuleCatalogResult Parse(string json)
        {
            List<ModuleDefinition> modules = new List<ModuleDefinition>();

            try
            {
                if (_parser.Parse(json) is Dictionary<string, object> root == false || root.TryGetValue("modules", out object list) == false || list is List<object> items == false)
                {
                    return new ModuleCatalogResult(false, "modules.json has no modules list.", modules);
                }

                foreach (object item in items)
                {
                    if (item is Dictionary<string, object> module == false)
                    {
                        continue;
                    }

                    string package = Text(module, "package");

                    if (string.IsNullOrEmpty(package))
                    {
                        continue;
                    }

                    modules.Add(new ModuleDefinition(Text(module, "id"), Text(module, "name"), package, Text(module, "repository"), Text(module, "path"), Text(module, "requires"), Strings(module, "platforms")));
                }
            }
            catch (FormatException exception)
            {
                return new ModuleCatalogResult(false, exception.Message, modules);
            }

            return new ModuleCatalogResult(true, "", modules);
        }

        public string GitUrl(ModuleDefinition module, string tag)
        {
            string url = "https://github.com/" + module.Repository + ".git";

            if (string.IsNullOrEmpty(module.Path) == false)
            {
                url += "?path=" + module.Path;
            }

            return string.IsNullOrEmpty(tag) ? url : url + "#" + tag;
        }

        private string Text(Dictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out object value) && value is string text ? text : "";
        }

        private List<string> Strings(Dictionary<string, object> values, string key)
        {
            List<string> result = new List<string>();

            if (values.TryGetValue(key, out object value) && value is List<object> items)
            {
                foreach (object item in items)
                {
                    if (item is string text)
                    {
                        result.Add(text);
                    }
                }
            }

            return result;
        }
    }
}
