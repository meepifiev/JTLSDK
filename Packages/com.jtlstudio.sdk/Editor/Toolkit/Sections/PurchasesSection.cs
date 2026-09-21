using System;
using JTLStudio.SDK.Editor.Toolkit.Components;
using UnityEditor;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Sections
{
    public class PurchasesSection : SettingsListSection
    {
        private const string ProductsProperty = "_products";

        public PurchasesSection(ToolkitContext context) : base(context)
        {
        }

        public override ToolkitSectionId Id => ToolkitSectionId.Purchases;

        public override ToolkitStatus Status => new ToolkitStatus(StatusKind.Info, "purchases.ready", DateTime.Now.ToString("HH:mm"));

        protected override string TemplateName => "PurchasesSection";

        protected override void BuildContent()
        {
            Require<ToolkitButton>("add-product").clicked += AddProduct;
            VisualElement list = Require<VisualElement>("products");
            SerializedProperty products = Serialized.FindProperty(ProductsProperty);

            if (products.arraySize == 0)
            {
                EmptyState empty = new EmptyState { TitleKey = "purchases.emptyTitle", DescriptionKey = "purchases.emptyDescription", IconName = "purchases" };
                empty.Add(Button("purchases.addProduct", ToolkitButton.SecondaryVariant, "plus", AddProduct));
                list.Add(empty);
            }

            for (int index = 0; index < products.arraySize; index++)
            {
                list.Add(CreateProductCard(products, index));
            }

            if (Context.Project.Active != null && Context.Providers.IsUnsupported(Context.Project.Active.Payments == null ? null : Context.Project.Active.Payments.GetType()))
            {
                list.Add(new InlineMessage { Variant = InlineMessage.WarningVariant, Text = Context.Text("purchases.unsupportedWarning", Context.Project.Active.DisplayName) });
            }

            list.Add(GenerateConstantsRow());
        }

        private VisualElement CreateProductCard(SerializedProperty products, int index)
        {
            SerializedProperty product = products.GetArrayElementAtIndex(index);
            SerializedProperty id = product.FindPropertyRelative("_id");
            Card card = new Card { Spacing = 12 };

            VisualElement header = Row(10);
            header.Add(TextLabel(string.IsNullOrEmpty(id.stringValue) ? "—" : id.stringValue, "jtl-text", "jtl-text--title", MonospaceFont.ClassName));
            header.Add(Spacer());
            IconButton delete = new IconButton("delete", 26);
            delete.clicked += () => RemoveElement(products, index);
            header.Add(delete);
            card.Add(header);

            VisualElement columns = Row(16);
            columns.AddToClassList("jtl-row--start");

            VisualElement left = Column(8);
            left.AddToClassList("jtl-basis");
            FieldRow idRow = new FieldRow("purchases.productId", 120);
            idRow.Add(TextInput(id, true, 0));
            left.Add(idRow);
            FieldRow typeRow = new FieldRow("purchases.type", 120);
            typeRow.Add(EnumInput<ProductType>(product.FindPropertyRelative("_type"), "purchases.types", 200));
            left.Add(typeRow);
            columns.Add(left);

            VisualElement right = Column(8);
            right.AddToClassList("jtl-basis");
            FieldRow yandexRow = new FieldRow("purchases.yandexId", 130) { NoteKey = "purchases.yandexIdNote" };
            yandexRow.Add(TextInput(PlatformIdProperty(product.FindPropertyRelative("_platformIds"), PlatformId.YandexGames), true, 0));
            right.Add(yandexRow);
            FieldRow priceRow = new FieldRow("purchases.testPrice", 130);
            priceRow.Add(FloatInput(product.FindPropertyRelative("_testPrice"), 90));
            TextField currency = TextInput(product.FindPropertyRelative("_testCurrency"), false, 80);
            currency.style.marginLeft = 8;
            priceRow.Add(currency);
            right.Add(priceRow);
            columns.Add(right);

            card.Add(columns);
            return card;
        }

        private void AddProduct()
        {
            SerializedProperty products = Serialized.FindProperty(ProductsProperty);
            string id = UniqueId(products, "_id", "product");
            products.arraySize++;
            SerializedProperty added = products.GetArrayElementAtIndex(products.arraySize - 1);
            added.FindPropertyRelative("_id").stringValue = id;
            added.FindPropertyRelative("_type").enumValueIndex = (int)ProductType.NonConsumable;
            added.FindPropertyRelative("_platformIds").arraySize = 0;
            added.FindPropertyRelative("_testPrice").floatValue = 1f;
            added.FindPropertyRelative("_testCurrency").stringValue = "YAN";
            Apply(true);
            Context.Report(StatusKind.Success, "purchases.added", id);
        }
    }
}
