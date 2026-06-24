using Fashia.Application.Vouchers.Commands.ChangeVoucherStatus;
using Fashia.Application.Vouchers.Commands.CreateVoucher;
using Fashia.Application.Vouchers.Commands.UpdateVoucher;
using Fashia.Application.Vouchers.Queries;
using Fashia.Domain.Constants;
using Fashia.Web.Endpoints.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fashia.Web.Endpoints;

public class Vouchers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetVouchers);
        groupBuilder.MapGet(GetVoucherById, "{id:int}");
        groupBuilder.MapGet(GetOrderVoucherUsages, "usages");
        groupBuilder.MapGet(GetVoucherUsages, "{id:int}/usages");

        groupBuilder.MapPost(CreateVoucher).RequireAuthorization();
        groupBuilder.MapPut(UpdateVoucher, "{id:int}").RequireAuthorization();
        groupBuilder.MapPatch(DeactivateVoucher, "deactivate/{id:int}").RequireAuthorization();
        groupBuilder.MapPatch(SuspendVoucher, "suspend/{id:int}").RequireAuthorization();
        groupBuilder.MapPatch(ReactivateVoucher, "reactivate/{id:int}").RequireAuthorization();
    }

    [EndpointSummary("Get all Vouchers")]
    [EndpointDescription("Retrieves all vouchers.")]
    public static async Task<Ok<IReadOnlyCollection<VoucherDto>>> GetVouchers(ISender sender)
    {
        var vouchers = await sender.Send(new GetVouchersQuery());

        return TypedResults.Ok(vouchers);
    }

    [EndpointSummary("Get Voucher by Id")]
    [EndpointDescription("Retrieves a voucher by id.")]
    public static async Task<Results<Ok<VoucherDto>, NotFound>> GetVoucherById(
        ISender sender,
        int id
    )
    {
        var voucher = await sender.Send(new GetVoucherByIdQuery(id));

        return voucher is null ? TypedResults.NotFound() : TypedResults.Ok(voucher);
    }

    [EndpointSummary("Get Voucher Usages")]
    [EndpointDescription("Retrieves order-voucher usage records.")]
    public static async Task<Ok<IReadOnlyCollection<OrderVoucherUsageDto>>> GetOrderVoucherUsages(
        ISender sender,
        int? orderId,
        int? voucherId
    )
    {
        var usages = await sender.Send(new GetOrderVoucherUsagesQuery(voucherId, orderId));

        return TypedResults.Ok(usages);
    }

    [EndpointSummary("Get Usages by Voucher")]
    [EndpointDescription("Retrieves orders that used a specific voucher.")]
    public static async Task<Ok<IReadOnlyCollection<OrderVoucherUsageDto>>> GetVoucherUsages(
        ISender sender,
        int id
    )
    {
        var usages = await sender.Send(new GetOrderVoucherUsagesQuery(VoucherId: id));

        return TypedResults.Ok(usages);
    }

    [EndpointSummary("Create Voucher")]
    [EndpointDescription("Creates a new voucher.")]
    public static async Task<Created<int>> CreateVoucher(
        ISender sender,
        CreateVoucherRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = await sender.Send(
            new CreateVoucherCommand
            {
                Code = request.Code,
                DiscountType = request.DiscountType,
                DiscountAmount = request.DiscountAmount,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                UsageLimit = request.UsageLimit,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                VoucherType = request.VoucherType,
                ProductId = request.ProductId,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Display = request.Display,
                QuantityPerUser = request.QuantityPerUser,
            },
            cancellationToken
        );

        return TypedResults.Created($"/api/vouchers/{id}", id);
    }

    [EndpointSummary("Update Voucher")]
    [EndpointDescription("Updates an existing voucher.")]
    public static async Task<NoContent> UpdateVoucher(
        ISender sender,
        int id,
        UpdateVoucherRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new UpdateVoucherCommand
            {
                Id = id,
                Code = request.Code,
                DiscountType = request.DiscountType,
                DiscountAmount = request.DiscountAmount,
                MinOrderAmount = request.MinOrderAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                UsageLimit = request.UsageLimit,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                VoucherType = request.VoucherType,
                ProductId = request.ProductId,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Display = request.Display,
                QuantityPerUser = request.QuantityPerUser,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Deactivate Voucher")]
    [EndpointDescription("Deactivates a voucher.")]
    public static async Task<NoContent> DeactivateVoucher(ISender sender, int id)
    {
        await sender.Send(new DeactivateVoucherCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Suspend Voucher")]
    [EndpointDescription("Suspends a voucher.")]
    public static async Task<NoContent> SuspendVoucher(ISender sender, int id)
    {
        await sender.Send(new SuspendVoucherCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Reactivate Voucher")]
    [EndpointDescription("Reactivates an inactive or suspended voucher.")]
    public static async Task<NoContent> ReactivateVoucher(ISender sender, int id)
    {
        await sender.Send(new ReactivateVoucherCommand(id));

        return TypedResults.NoContent();
    }
}
