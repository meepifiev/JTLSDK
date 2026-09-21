using System;
using System.Collections.Generic;
using JTLStudio.SDK.Editor.Toolkit.Data;
using JTLStudio.SDK.Editor.Toolkit.Localization;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public class ToolkitContext
    {
        private const string TimeFormat = "HH:mm";

        public ToolkitContext(ToolkitLocalization localization, ToolkitAssets assets, ToolkitProject project)
        {
            Localization = localization ?? throw new ArgumentNullException(nameof(localization));
            Assets = assets ?? throw new ArgumentNullException(nameof(assets));
            Project = project ?? throw new ArgumentNullException(nameof(project));
        }

        public event Action<ToolkitSectionId> NavigationRequested;

        public event Action<ToolkitStatus> StatusRequested;

        public ToolkitLocalization Localization { get; }

        public ToolkitAssets Assets { get; }

        public ToolkitProject Project { get; }

        public ProviderCatalog Providers { get; } = new ProviderCatalog();

        public ModuleSlots Modules { get; } = new ModuleSlots();

        public PlatformPresentation Platforms { get; } = new PlatformPresentation();

        public SdkConfiguration SelectedConfiguration { get; set; }

        public HashSet<string> ExpandedProviders { get; } = new HashSet<string>();

        public void Navigate(ToolkitSectionId sectionId)
        {
            NavigationRequested?.Invoke(sectionId);
        }

        public void ShowStatus(ToolkitStatus status)
        {
            StatusRequested?.Invoke(status);
        }

        public void Report(StatusKind kind, string messageKey, params object[] arguments)
        {
            string text = string.Format(Localization.Get(messageKey), arguments);
            StatusRequested?.Invoke(new ToolkitStatus(kind, messageKey, DateTime.Now.ToString(TimeFormat), text));
        }

        public string Text(string key, params object[] arguments)
        {
            return arguments.Length == 0 ? Localization.Get(key) : string.Format(Localization.Get(key), arguments);
        }
    }
}
