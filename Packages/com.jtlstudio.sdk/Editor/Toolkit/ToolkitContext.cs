using System;
using JTLStudio.SDK.Editor.Toolkit.Localization;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public class ToolkitContext
    {
        public ToolkitContext(ToolkitLocalization localization, ToolkitAssets assets)
        {
            Localization = localization ?? throw new ArgumentNullException(nameof(localization));
            Assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        public event Action<ToolkitSectionId> NavigationRequested;

        public event Action<ToolkitStatus> StatusRequested;

        public ToolkitLocalization Localization { get; }

        public ToolkitAssets Assets { get; }

        public void Navigate(ToolkitSectionId sectionId)
        {
            NavigationRequested?.Invoke(sectionId);
        }

        public void ShowStatus(ToolkitStatus status)
        {
            StatusRequested?.Invoke(status);
        }
    }
}
