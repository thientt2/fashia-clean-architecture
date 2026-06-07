using Microsoft.AspNetCore.Http;

namespace Fashia.Web.Endpoints.Uploads.Requests;

public sealed class UploadFileRequest
{
    public IFormFile File { get; init; } = default!;
    public string Folder { get; init; } = string.Empty;
}
