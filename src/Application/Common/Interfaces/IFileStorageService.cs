namespace Fashia.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<UploadedFileResult> UploadImageAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken
    );

    Task DeleteAsync(string publicId, CancellationToken cancellationToken);
}

public sealed record UploadedFileResult(string FileName, string Url, string PublicId);
