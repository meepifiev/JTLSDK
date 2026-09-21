using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JTLStudio.SDK.Editor.Toolkit;
using JTLStudio.SDK.Editor.Toolkit.Localization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace JTLStudio.SDK.Tests.Toolkit
{
    public class ToolkitWindowTests
    {
        private const int NavigationItemCount = 18;
        private const string RussianConfigurations = "Конфигурации";
        private const string EnglishConfigurations = "Configurations";
        private const string KeyAttributePattern = "(?:^|\\s)(?:[a-z]+-)?key=\"([^\"]+)\"";

        private ToolkitWindow _window;

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            _window = ScriptableObject.CreateInstance<ToolkitWindow>();
            _window.EnsureBuilt();
            LogAssert.ignoreFailingMessages = false;
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = true;

            if (_window != null)
            {
                UnityEngine.Object.DestroyImmediate(_window);
            }

            LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void SidebarContainsEveryNavigationItem()
        {
            Assert.AreEqual(NavigationItemCount, _window.NavigationItems.Count);
        }

        [Test]
        public void EverySectionRendersWithChildren()
        {
            foreach (ToolkitSectionId sectionId in Enum.GetValues(typeof(ToolkitSectionId)))
            {
                Assert.DoesNotThrow(() => _window.Navigate(sectionId), sectionId.ToString());
                Assert.AreEqual(sectionId, _window.CurrentSectionId);
                Assert.Greater(_window.CurrentSectionRoot.childCount, 0, sectionId.ToString());
            }
        }

        [Test]
        public void LanguageToggleRerendersShellAndSection()
        {
            _window.Navigate(ToolkitSectionId.Configurations);
            _window.SetLanguage(ToolkitLanguage.Russian);
            Assert.AreEqual(ToolkitLanguage.Russian, _window.Language);
            Assert.AreEqual(RussianConfigurations, _window.NavigationItems[0].Text);
            Assert.Greater(_window.CurrentSectionRoot.childCount, 0);
            _window.SetLanguage(ToolkitLanguage.English);
            Assert.AreEqual(ToolkitLanguage.English, _window.Language);
            Assert.AreEqual(EnglishConfigurations, _window.NavigationItems[0].Text);
        }

        [Test]
        public void EveryKeyReferencedInTemplatesIsLocalized()
        {
            ToolkitLocalization localization = new ToolkitLocalization();
            Regex pattern = new Regex(KeyAttributePattern);
            List<string> missing = new List<string>();

            foreach (string path in Directory.GetFiles(ToolkitAssets.Root, "*.uxml", SearchOption.AllDirectories))
            {
                foreach (Match match in pattern.Matches(File.ReadAllText(path)))
                {
                    string key = match.Groups[1].Value;

                    if (localization.Contains(key) == false)
                    {
                        missing.Add(Path.GetFileName(path) + ": " + key);
                    }
                }
            }

            Assert.IsEmpty(missing, string.Join(", ", missing));
        }
    }
}
