using Fashia.Application.Common.Models;
using Fashia.Application.Products.Queries.Common;
using Fashia.Application.Products.Queries.GetProductById;
using Fashia.Application.Products.Queries.GetProducts;

namespace Fashia.Application.Common.Interfaces;

public interface IProductReadService
{
    Task<PaginatedList<ProductListItemDto>> GetProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken
    );

    Task<ProductDetailDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
}
