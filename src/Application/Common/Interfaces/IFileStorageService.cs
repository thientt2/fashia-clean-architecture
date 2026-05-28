namespace Fashia.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken);
}