using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Fashia.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Fashia.Infrastructure.Storage;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryStorageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary cloud name is not configured.");

        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary API key is not configured.");

        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary API secret is not configured.");

        var account = new Account(cloudName, apiKey, apiSecret);

        _cloudinary = new Cloudinary(account)
        {
            Api =
            {
                Secure = true
            }
        };
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken)
    {
        if (stream.Length == 0)
            throw new ArgumentException("File is empty.", nameof(stream));

        var extension = Path.GetExtension(fileName);
        var publicId = $"{Path.GetFileNameWithoutExtension(fileName)}-{Guid.NewGuid():N}";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            PublicId = publicId,
            UseFilename = false,
            UniqueFilename = false,
            Overwrite = false,
            AllowedFormats = new[] { "jpg", "jpeg", "png", "webp" }
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
            throw new InvalidOperationException(result.Error.Message);

        return result.SecureUrl?.ToString()
            ?? throw new InvalidOperationException("Cloudinary did not return a secure URL.");
    }

    public async Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return;

        var uri = new Uri(fileUrl);

        var segments = uri.AbsolutePath.Split('/');

        var uploadIndex = Array.IndexOf(segments, "upload");

        if (uploadIndex == -1)
            return;

        var publicIdSegments = segments
            .Skip(uploadIndex + 2)
            .ToArray();

        var publicIdWithExtension = string.Join('/', publicIdSegments);

        var publicId = Path.Combine(
                Path.GetDirectoryName(publicIdWithExtension) ?? string.Empty,
                Path.GetFileNameWithoutExtension(publicIdWithExtension))
            .Replace("\\", "/");

        var deleteParams = new DeletionParams(publicId);

        await _cloudinary.DestroyAsync(deleteParams);
    }
}