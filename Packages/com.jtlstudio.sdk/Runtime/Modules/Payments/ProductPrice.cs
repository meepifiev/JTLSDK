namespace JTLStudio.SDK
{
    public readonly struct ProductPrice
    {
        public ProductPrice(decimal value, string currencyCode, string formatted)
        {
            Value = value;
            CurrencyCode = currencyCode;
            Formatted = formatted;
        }

        public decimal Value { get; }
        public string CurrencyCode { get; }
        public string Formatted { get; }
    }
}
