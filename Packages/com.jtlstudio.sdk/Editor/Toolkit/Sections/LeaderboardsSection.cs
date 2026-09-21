using System;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class LeaderboardsSection : SettingsListSection
    {
        private const string LeaderboardsProperty = "_leaderboards";

        public LeaderboardsSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Leaderboards;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "", "");

        protected override string TemplateName => "LeaderboardsSection";

        protected override void BuildContent()
        {
            VisualElement root = Require<VisualElement>("leaderboards");
            SerializedProperty leaderboards = Serialized.FindProperty(LeaderboardsProperty);
            Card card = new Card { TitleKey = "leaderboards.cardTitle", Spacing = 8 };

            VisualElement table = new VisualElement();
            table.AddToClassList("jtl-table");
            VisualElement head = new VisualElement();
            head.AddToClassList("jtl-table__row");
            head.AddToClassList("jtl-table__row--head");
            head.Add(Heading("leaderboards.columnId", 220));
            head.Add(Heading("leaderboards.columnYandex", 0));
            head.Add(Heading("leaderboards.columnYoutube", 164));
            table.Add(head);

            for (int index = 0; index < leaderboards.arraySize; index++)
            {
                table.Add(CreateRow(leaderboards, index));
            }

            card.Add(table);

            if (leaderboards.arraySize == 0)
            {
                card.Add(Localized("leaderboards.empty", "jtl-text--secondary"));
            }

            VisualElement actions = Row(10);
            ToolkitButton add = Button("leaderboards.add", ToolkitButton.SecondaryVariant, "plus", AddLeaderboard);
            add.Compact = true;
            actions.Add(add);
            card.Add(actions);
            card.Add(GenerateConstantsRow());
            root.Add(card);
            root.Add(ProvidersCard("_leaderboards"));
        }

        private VisualElement CreateRow(SerializedProperty leaderboards, int index)
        {
            SerializedProperty leaderboard = leaderboards.GetArrayElementAtIndex(index);
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-table__row");
            TextField id = TextInput(leaderboard.FindPropertyRelative("_id"), true, 210);
            id.style.marginRight = 10;
            row.Add(id);
            TextField yandex = PlatformIdInput(leaderboard.FindPropertyRelative("_platformIds"), PlatformId.YandexGames);
            yandex.style.marginRight = 10;
            yandex.style.minWidth = 0;
            yandex.style.flexShrink = 1;
            row.Add(yandex);
            Label youtube = TextLabel(Context.Text("leaderboards.singleBoard"), "jtl-text--secondary");
            youtube.style.width = 130;
            youtube.style.flexShrink = 0;
            row.Add(youtube);
            IconButton delete = new IconButton("delete", 24);
            delete.clicked += () => RemoveElement(leaderboards, index);
            row.Add(delete);
            return row;
        }

        private Label Heading(string key, int width)
        {
            LocalizedLabel label = Localized(key, "jtl-table__heading");

            if (width > 0)
            {
                label.style.width = width;
            }
            else
            {
                label.AddToClassList("jtl-grow");
            }

            return label;
        }

        private void AddLeaderboard()
        {
            SerializedProperty leaderboards = Serialized.FindProperty(LeaderboardsProperty);
            string id = UniqueId(leaderboards, "_id", "leaderboard");
            leaderboards.arraySize++;
            SerializedProperty added = leaderboards.GetArrayElementAtIndex(leaderboards.arraySize - 1);
            added.FindPropertyRelative("_id").stringValue = id;
            added.FindPropertyRelative("_platformIds").arraySize = 0;
            Apply(true);
        }
    }
}
