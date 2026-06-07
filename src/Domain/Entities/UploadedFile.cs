namespace Fashia.Domain.Entities;

public class UploadedFile : BaseAuditableEntity
{
    private UploadedFile() { }

    public UploadedFile(
        string fileName,
        string originalFileName,
        string contentType,
        long sizeInBytes,
        string url,
        string publicId,
        string folder
    )
    {
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        Url = url;
        PublicId = publicId;
        Folder = folder;
        IsUsed = false;
    }

    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }

    public string Url { get; private set; } = string.Empty;
    public string PublicId { get; private set; } = string.Empty;
    public string Folder { get; private set; } = string.Empty;

    public bool IsUsed { get; private set; }

    public void MarkAsUsed()
    {
        IsUsed = true;
    }
}
