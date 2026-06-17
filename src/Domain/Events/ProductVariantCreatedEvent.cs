public sealed class ProductVariantCreatedEvent : BaseEvent
{
    public ProductVariantCreatedEvent(ProductVariant productVariant)
    {
        ProductVariant = productVariant;
    }

    public ProductVariant ProductVariant { get; }
}
