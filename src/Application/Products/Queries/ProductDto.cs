using Fashia.Application.Common.Models;
using Fashia.Domain.Entities;

namespace Fashia.Application.Products.Queries;
public sealed class ProductDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? ImageUrl { get; init; }

    public LookupDto Category { get; init; } = null!;
    public LookupDto Brand { get; init; } = null!;
    public string Status { get; init; } = string.Empty;

    public IReadOnlyCollection<ProductVariantDto> Variants { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new LookupDto { Id = src.Category.Id, Name = src.Category.Name }))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => new LookupDto { Id = src.Brand.Id, Name = src.Brand.Name }))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants.OrderBy(v => v.Id)));
        }
    }
}


