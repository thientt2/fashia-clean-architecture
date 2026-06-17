using Fashia.Application.Common.Interfaces;
using Fashia.Application.Inventories.Queries.Common;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Inventories.Queries.GetInventoryTransactionHistory;

public sealed record GetInventoryTransactionHistoryQuery
    : IRequest<InventoryTransactionHistoryResult>
{
    public int? BranchId { get; init; }
    public int? ProductVariantId { get; init; }
    public InventoryTransactionType? Type { get; init; }
    public int? OrderId { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class GetInventoryTransactionHistoryQueryHandler
    : IRequestHandler<GetInventoryTransactionHistoryQuery, InventoryTransactionHistoryResult>
{
    private const int MaxPageSize = 100;

    private readonly IApplicationDbContext _context;

    public GetInventoryTransactionHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryTransactionHistoryResult> Handle(
        GetInventoryTransactionHistoryQuery request,
        CancellationToken cancellationToken
    )
    {
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var query = _context.InventoryTransactions.AsNoTracking();
        query = ApplyFilters(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Include(x => x.Branch)
            .Include(x => x.ProductVariant)
                .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.Created)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryTransactionDto
            {
                Id = x.Id,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                ProductId = x.ProductVariant.ProductId,
                ProductName = x.ProductVariant.Product.Name,
                ProductVariantId = x.ProductVariantId,
                Type = x.Type.ToString(),
                Quantity = x.Quantity,
                PreviousStockQuantity = x.PreviousStockQuantity,
                NewStockQuantity = x.NewStockQuantity,
                PreviousReservedQuantity = x.PreviousReservedQuantity,
                NewReservedQuantity = x.NewReservedQuantity,
                SourceBranchId = x.SourceBranchId,
                DestinationBranchId = x.DestinationBranchId,
                OrderId = x.OrderId,
                TransferCorrelationId = x.TransferCorrelationId,
                Note = x.Note,
                Created = x.Created,
            })
            .ToListAsync(cancellationToken);

        return new InventoryTransactionHistoryResult
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = transactions,
        };
    }

    private static IQueryable<InventoryTransaction> ApplyFilters(
        IQueryable<InventoryTransaction> query,
        GetInventoryTransactionHistoryQuery request
    )
    {
        if (request.BranchId is not null)
            query = query.Where(x => x.BranchId == request.BranchId.Value);

        if (request.ProductVariantId is not null)
            query = query.Where(x => x.ProductVariantId == request.ProductVariantId.Value);

        if (request.Type is not null)
            query = query.Where(x => x.Type == request.Type.Value);

        if (request.OrderId is not null)
            query = query.Where(x => x.OrderId == request.OrderId.Value);

        if (request.From is not null)
            query = query.Where(x => x.Created >= request.From.Value);

        if (request.To is not null)
            query = query.Where(x => x.Created <= request.To.Value);

        return query;
    }
}
