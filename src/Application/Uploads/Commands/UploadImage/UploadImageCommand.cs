using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Security;
using Fashia.Domain.Constants;
using Fashia.Domain.Entities;
using MediatR;

namespace Fashia.Application.Uploads.Commands.UploadImage;

[Authorize(Policy = Policies.ManageProducts)]
public record UploadImageCommand : IRequest<UploadImageResult>
{
    public Stream FileStream { get; init; } = default!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeInBytes { get; init; }
    public string Folder { get; init; } = string.Empty;
}

public record UploadImageResult(
    int Id,
    string Url,
    string PublicId,
    string FileName,
    string OriginalFileName
);

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, UploadImageResult>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UploadImageCommandHandler(
        IFileStorageService fileStorageService,
        IApplicationDbContext context,
        IUser user
    )
    {
        _fileStorageService = fileStorageService;
        _context = context;
        _user = user;
    }

    public async Task<UploadImageResult> Handle(
        UploadImageCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_user.Id is null)
        {
            throw new UnauthorizedAccessException();
        }

        var storageFolder = $"temporary/{request.Folder}";

        var uploaded = await _fileStorageService.UploadImageAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            storageFolder,
            cancellationToken
        );

        var uploadedFile = new UploadedFile(
            fileName: uploaded.FileName,
            originalFileName: request.FileName,
            contentType: request.ContentType,
            sizeInBytes: request.SizeInBytes,
            url: uploaded.Url,
            publicId: uploaded.PublicId,
            folder: storageFolder
        );

        _context.UploadedFiles.Add(uploadedFile);

        await _context.SaveChangesAsync(cancellationToken);

        return new UploadImageResult(
            uploadedFile.Id,
            uploadedFile.Url,
            uploadedFile.PublicId,
            uploadedFile.FileName,
            uploadedFile.OriginalFileName
        );
    }
}
