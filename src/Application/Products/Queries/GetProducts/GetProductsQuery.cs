using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Application.Products.Queries.Common;

namespace Fashia.Application.Products.Queries.GetProducts;

using Fashia.Application.Common.Models;

public sealed record GetProductsQuery : IRequest<PaginatedList<ProductListItemDto>>
{
    public string? Search { get; init; }

    public int? BrandId { get; init; }

    public int? CategoryId { get; init; }

    public long? MinPrice { get; init; }

    public long? MaxPrice { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, PaginatedList<ProductListItemDto>>
{
    private readonly IProductReadService _productReadService;

    public GetProductsQueryHandler(IProductReadService productReadService)
    {
        _productReadService = productReadService;
    }

    public Task<PaginatedList<ProductListItemDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken
    )
    {
        return _productReadService.GetProductsAsync(request, cancellationToken);
    }
}
