using Fashia.Domain.Entities;

namespace Fashia.Application.Products.Queries;
public sealed class ProductVariantAttributeValueDto
{
    public int Id { get; init; }

    public string Value { get; init; } = string.Empty;

    public string? HexValue { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProductVariantAttributeValue, ProductVariantAttributeValueDto>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.AttributeValue.Value))
                .ForMember(dest => dest.HexValue, opt => opt.MapFrom(src => src.AttributeValue.HexValue));
        }
    }
}