using Fashia.Application.Uploads.Commands.UploadImage;
using Fashia.Domain.Constants;
using Fashia.Web.Endpoints.Uploads.Requests;
using Fashia.Web.Endpoints.Uploads.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints.Uploads;

public class Uploads : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder
            .MapPost(UploadImage, "images")
            .DisableAntiforgery()
            .RequireAuthorization(Policies.CanManageProducts);
    }

    [EndpointSummary("Upload a images")]
    [EndpointDescription("Uploads a image and returns the URL of the uploaded file.")]
    public static async Task<Ok<UploadImageResult>> UploadImage(
        ISender sender,
        IFormFile file,
        [FromForm] string folder,
        CancellationToken cancellationToken
    )
    {
        await using var stream = file.OpenReadStream();

        var result = await sender.Send(
            new UploadImageCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeInBytes = file.Length,
                Folder = folder,
            },
            cancellationToken
        );

        return TypedResults.Ok(result);
    }
}
