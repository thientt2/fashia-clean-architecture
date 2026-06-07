using System.Text.Json.Serialization;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;

namespace Fashia.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand : IRequest
{
    [JsonIgnore]
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int BrandId { get; init; }
    public List<int> NewUploadedImageIds { get; init; } = [];
}

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context
            .Products.Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id.ToString());

        product.Rename(request.Name);
        product.UpdateDescription(request.Description);
        product.ChangeCategory(request.CategoryId);
        product.ChangeBrand(request.BrandId);

        // Thêm ảnh mới — không xóa ảnh cũ
        if (request.NewUploadedImageIds.Count > 0)
        {
            var uploadedFiles = await _context
                .UploadedFiles.Where(x => request.NewUploadedImageIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var currentMaxOrder = product.Images.Any()
                ? product.Images.Max(i => i.DisplayOrder)
                : -1;

            var order = currentMaxOrder + 1;
            foreach (var imageId in request.NewUploadedImageIds)
            {
                product.AddImage(imageId, isMain: false, displayOrder: order);
                uploadedFiles[imageId].MarkAsUsed();
                order++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
