using Fashia.Domain.Entities;

namespace Fashia.Application.Products.Queries.Common;

public sealed class ProductVariantDto
{
    public int Id { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public long OriginalPrice { get; init; }

    public decimal DiscountPercentage { get; init; }

    public int StockQuantity { get; init; }

    public decimal SellingPrice { get; init; }

    public IReadOnlyCollection<ProductVariantAttributeValueDto> AttributeValues { get; init; } = [];
    public IReadOnlyCollection<ImageDto> ImageUrls { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(
                    dest => dest.OriginalPrice,
                    opt => opt.MapFrom(src => src.OriginalPrice.Amount)
                )
                .ForMember(
                    dest => dest.DiscountPercentage,
                    opt => opt.MapFrom(src => src.DiscountPercentage.Value)
                )
                .ForMember(
                    dest => dest.SellingPrice,
                    opt => opt.MapFrom(src => src.SellingPrice.Amount)
                )
                .ForMember(dest => dest.DisplayName, opt => opt.Ignore())
                .ForMember(dest => dest.StockQuantity, opt => opt.Ignore())
                .ForMember(
                    dest => dest.ImageUrls,
                    opt =>
                        opt.MapFrom(src =>
                            src.Images.Select(i => new ImageDto
                            {
                                UploadedFileId = i.UploadedFile.Id,
                                Url = i.UploadedFile.Url,
                                IsMain = i.IsMain,
                                DisplayOrder = i.DisplayOrder,
                            })
                        )
                );
        }
    }
}
