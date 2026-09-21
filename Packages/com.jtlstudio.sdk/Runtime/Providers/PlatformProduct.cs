namespace JTLStudio.SDK.Providers
{
    public readonly struct PlatformProduct
    {
        public PlatformProduct(string id, ProductPrice price)
        {
            Id = id;
            Price = price;
        }

        public string Id { get; }
        public ProductPrice Price { get; }
    }
}
