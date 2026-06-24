using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Application.Products.Queries.Common;
using Fashia.Application.Products.Queries.GetProductById;

namespace Fashia.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IRequest<ProductDetailDto>;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IProductReadService _productReadService;

    public GetProductByIdQueryHandler(IProductReadService productReadService)
    {
        _productReadService = productReadService;
    }

    public async Task<ProductDetailDto> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var product = await _productReadService.GetProductByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product", request.Id.ToString());
        }

        return product;
    }
}
