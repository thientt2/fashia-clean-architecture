namespace Fashia.Application.Common.Models;

public sealed class FileUpload
{
    public FileUpload(
        Stream stream,
        string fileName,
        string contentType)
    {
        Stream = stream;
        FileName = fileName;
        ContentType = contentType;
    }

    public Stream Stream { get; }

    public string FileName { get; }

    public string ContentType { get; }
}