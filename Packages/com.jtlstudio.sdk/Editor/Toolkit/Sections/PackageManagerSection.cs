using System;
using System.Collections.Generic;
using System.IO;
using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Toolkit.Components;
using JTLStudio.SDK.Editor.Updates;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class PackageManagerSection : ToolkitSection
    {
        private const string SdkRepository = "meepifiev/JTLSDK";
        private const string PackagePath = "Packages/com.jtlstudio.sdk";
        private const string PackageGitUrl = "https://github.com/meepifiev/JTLSDK.git?path=Packages/com.jtlstudio.sdk#";
        private const string PreReleasePreference = "JTLSDK.Package.ShowPreReleases";

        private readonly GitHubReleases _github = new GitHubReleases();
        private readonly ModuleCatalog _catalog = new ModuleCatalog();
        private readonly TemplateService _template = new TemplateService();
        private ReleaseCheckResult _sdkReleases;
        private ModuleCatalogResult _modules;
        private bool _checking;
        private string _checkedAt;

        public PackageManagerSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.PackageManager;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "PackageManagerSection";

        private string InstalledVersion
        {
            get
            {
                PackageInfo package = PackageInfo.FindForAssetPath(PackagePath);
                return package == null ? "0.0.0" : package.version;
            }
        }

        private bool ShowPreReleases => EditorPrefs.GetBool(PreReleasePreference, false);

        protected override void OnRendered()
        {
            if (_sdkReleases == null && _checking == false)
            {
                Check();
            }

            VisualElement body = Require<VisualElement>("package-body");
            body.Add(CreateSdkCard());
            body.Add(CreateTemplateCard());
            body.Add(CreateModulesCard());

            VisualElement footer = Row(10);
            footer.Add(TextLabel(_checking ? Context.Text("package.checking") : Context.Text("package.checkedAtFormat", _checkedAt ?? "—"), "jtl-text--caption"));
            footer.Add(Spacer());
            footer.Add(Button("package.checkNow", ToolkitButton.GhostVariant, "refresh", Check));
            body.Add(footer);
        }

        private VisualElement CreateSdkCard()
        {
            Card card = new Card();
            VisualElement header = Row(14);
            VisualElement logo = new VisualElement();
            logo.AddToClassList("jtl-brand__logo");
            logo.AddToClassList("jtl-brand__logo--package");
            header.Add(logo);

            ReleaseInfo latest = Latest(_sdkReleases);
            bool updateAvailable = latest != null && _github.CompareVersions(latest.Version, InstalledVersion) > 0;
            header.Add(TextLabel(SdkStateText(latest, updateAvailable), "jtl-text--caption"));
            header.Add(Spacer());

            if (updateAvailable)
            {
                header.Add(new Badge("badge.updateAvailable", Badge.AccentVariant));
                ToolkitButton update = new ToolkitButton { Label = Context.Text("package.updateToFormat", latest.Version), Variant = ToolkitButton.PrimaryVariant };
                update.clicked += () => UpdateSdk(latest);
                header.Add(update);
            }

            card.Add(header);

            VisualElement preReleaseRow = Row(10);
            preReleaseRow.Add(Localized("package.showPreReleases", "jtl-text--secondary"));
            SwitchToggle preRelease = new SwitchToggle(ShowPreReleases);
            preRelease.ValueChanged += value =>
            {
                EditorPrefs.SetBool(PreReleasePreference, value);
                Render();
            };
            preReleaseRow.Add(preRelease);
            card.Add(preReleaseRow);

            if (_sdkReleases != null && _sdkReleases.IsSuccess)
            {
                VisualElement list = Column(8);
                list.AddToClassList("jtl-divider-top");
                list.AddToClassList("jtl-pt-4");

                foreach (ReleaseInfo release in _sdkReleases.Releases)
                {
                    if (release.IsPreRelease && ShowPreReleases == false)
                    {
                        continue;
                    }

                    list.Add(CreateReleaseRow(release));
                }

                if (list.childCount > 0)
                {
                    card.Add(list);
                }
            }

            return card;
        }

        private VisualElement CreateReleaseRow(ReleaseInfo release)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-release");
            VisualElement version = new VisualElement();
            version.AddToClassList("jtl-release__version");
            version.Add(TextLabel(release.Version, "jtl-text"));
            version.Add(TextLabel(release.PublishedAt, "jtl-text--caption", "jtl-mt-2"));
            row.Add(version);

            VisualElement notes = Column(2);

            foreach (string note in release.Notes)
            {
                notes.Add(TextLabel("•  " + note, "jtl-text--secondary", "jtl-text--wrap"));
            }

            if (release.IsPreRelease)
            {
                notes.Add(new Badge("badge.preRelease", Badge.WarningVariant));
            }

            row.Add(notes);
            return row;
        }

        private VisualElement CreateTemplateCard()
        {
            Card card = new Card { Spacing = 8 };
            VisualElement row = Row(10);
            row.Add(new Icon("template", 20, "secondary"));
            VisualElement text = Column(2);
            text.Add(TextLabel(Context.Text("package.webglTemplate"), "jtl-text"));
            bool installed = _template.IsInstalled;
            text.Add(TextLabel(installed ? Context.Text("package.templateInstalled", TemplateService.TemplateFolder) : Context.Text("package.templateMissing"), "jtl-text--caption"));
            row.Add(text);
            row.Add(Spacer());
            ToolkitButton install = new ToolkitButton { Label = Context.Text(installed ? "template.reinstall" : "package.install"), Variant = ToolkitButton.SecondaryVariant };
            install.clicked += () =>
            {
                _template.Install();
                Context.Report(StatusKind.Success, "template.installedTo", TemplateService.TemplateFolder);
                Render();
            };
            row.Add(install);

            if (installed)
            {
                ToolkitButton remove = new ToolkitButton { Label = Context.Text("template.remove"), Variant = ToolkitButton.GhostVariant };
                remove.clicked += () =>
                {
                    if (Context.Confirm("template.removeTitle", "template.removeMessage", "template.remove") == false)
                    {
                        return;
                    }

                    _template.Uninstall();
                    Context.Report(StatusKind.Info, "template.removed");
                    Render();
                };
                row.Add(remove);
            }
            card.Add(row);
            return card;
        }

        private VisualElement CreateModulesCard()
        {
            Card card = new Card { TitleKey = "package.modules", Spacing = 8 };

            if (_modules == null)
            {
                card.Add(Localized("package.checking", "jtl-text--secondary"));
                return card;
            }

            if (_modules.IsSuccess == false)
            {
                card.Add(TextLabel(Context.Text("package.modulesFailed", _modules.Error), "jtl-text--secondary"));
                return card;
            }

            if (_modules.Modules.Count == 0)
            {
                card.Add(Localized("package.noModules", "jtl-text--secondary"));
                return card;
            }

            foreach (ModuleDefinition module in _modules.Modules)
            {
                card.Add(CreateModuleRow(module));
            }

            return card;
        }

        private VisualElement CreateModuleRow(ModuleDefinition module)
        {
            VisualElement row = Row(10);
            row.Add(new Icon("package", 20, "secondary"));
            VisualElement text = Column(2);
            text.Add(TextLabel(module.Name, "jtl-text"));
            PackageInfo installed = PackageInfo.FindForAssetPath("Packages/" + module.Package);
            bool compatible = string.IsNullOrEmpty(module.Requires) || _github.CompareVersions(InstalledVersion, module.Requires) >= 0;
            string state = installed != null ? Context.Text("package.installedFormat", installed.version) : Context.Text("package.notInstalled");

            if (compatible == false)
            {
                state += " · " + Context.Text("package.requiresFormat", module.Requires);
            }

            List<string> platforms = new List<string>();

            foreach (string platform in module.Platforms)
            {
                platforms.Add(Enum.TryParse(platform, out PlatformId id) ? Context.Platforms.DisplayName(id) : platform);
            }

            if (platforms.Count > 0)
            {
                state += " · " + string.Join(", ", platforms);
            }

            text.Add(TextLabel(state, "jtl-text--caption"));
            row.Add(text);
            row.Add(Spacer());

            if (installed != null)
            {
                bool embedded = installed.source == PackageSource.Embedded || installed.source == PackageSource.Local;
                ToolkitButton remove = new ToolkitButton { Label = Context.Text("package.remove"), Variant = ToolkitButton.GhostVariant };
                remove.SetEnabled(embedded == false);
                remove.clicked += () => RemoveModule(module);
                row.Add(remove);
                return row;
            }

            ToolkitButton install = new ToolkitButton { Label = Context.Text("package.install"), Variant = ToolkitButton.SecondaryVariant };
            install.SetEnabled(compatible);
            install.clicked += () => InstallModule(module);
            row.Add(install);
            return row;
        }

        private string SdkStateText(ReleaseInfo latest, bool updateAvailable)
        {
            string installed = Context.Text("package.installedFormat", InstalledVersion);

            if (_checking)
            {
                return installed + " · " + Context.Text("package.checking");
            }

            if (_sdkReleases == null)
            {
                return installed;
            }

            if (_sdkReleases.IsSuccess == false)
            {
                return installed + " · " + Context.Text(_sdkReleases.IsRepositoryMissing ? "package.repositoryMissing" : "package.checkFailed");
            }

            if (latest == null)
            {
                return installed + " · " + Context.Text("package.noReleases");
            }

            return installed + " · " + (updateAvailable ? Context.Text("package.availableFormat", latest.Version) : Context.Text("package.upToDate"));
        }

        private ReleaseInfo Latest(ReleaseCheckResult result)
        {
            if (result == null || result.IsSuccess == false)
            {
                return null;
            }

            ReleaseInfo latest = null;

            foreach (ReleaseInfo release in result.Releases)
            {
                if (release.IsPreRelease && ShowPreReleases == false)
                {
                    continue;
                }

                if (latest == null || _github.CompareVersions(release.Version, latest.Version) > 0)
                {
                    latest = release;
                }
            }

            return latest;
        }

        private void Check()
        {
            _checking = true;
            int pending = 2;

            void Finish()
            {
                pending--;

                if (pending > 0)
                {
                    return;
                }

                _checking = false;
                _checkedAt = DateTime.Now.ToString("HH:mm");
                Render();
            }

            _github.Fetch(SdkRepository, result =>
            {
                _sdkReleases = result;
                Finish();
            });
            _catalog.Fetch(result =>
            {
                _modules = result;
                Finish();
            });
        }

        private void InstallModule(ModuleDefinition module)
        {
            ReleaseInfo latest = module.Repository == SdkRepository ? Latest(_sdkReleases) : null;
            Client.Add(_catalog.GitUrl(module, latest == null ? "" : latest.Tag));
            Context.Report(StatusKind.Info, "package.installing", module.Name);
        }

        private void RemoveModule(ModuleDefinition module)
        {
            bool confirmed = EditorUtility.DisplayDialog(module.Name, Context.Text("package.removeMessage", module.Name), Context.Text("package.remove"), Context.Text("details.cancel"));

            if (confirmed == false)
            {
                return;
            }

            Client.Remove(module.Package);
            Context.Report(StatusKind.Info, "package.removing", module.Name);
        }

        private void UpdateSdk(ReleaseInfo release)
        {
            bool confirmed = EditorUtility.DisplayDialog(Context.Text("package.updateTitle"), Context.Text("package.updateMessage", release.Version), Context.Text("package.update"), Context.Text("details.cancel"));

            if (confirmed == false)
            {
                return;
            }

            Client.Add(PackageGitUrl + release.Tag);
            Context.Report(StatusKind.Info, "package.updating", release.Version);
        }
    }
}
