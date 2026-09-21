namespace JTLStudio.SDK.Providers
{
    public readonly struct PlatformPurchase
    {
        public PlatformPurchase(string productId, string token)
        {
            ProductId = productId;
            Token = token;
        }

        public string ProductId { get; }
        public string Token { get; }
    }
}
