using System.Text;
using Dapper;
using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Application.Products.Queries.Common;
using Fashia.Application.Products.Queries.GetProductById;
using Fashia.Application.Products.Queries.GetProducts;
using Fashia.Infrastructure.Data;

namespace Fashia.Infrastructure.ReadServices;

public sealed class ProductReadService : IProductReadService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ProductReadService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PaginatedList<ProductListItemDto>> GetProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken
    )
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var where = new StringBuilder();
        where.AppendLine(
            """
            WHERE 1 = 1
            """
        );

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            where.AppendLine(
                """
                AND (
                    p."Name" ILIKE @Search
                    OR p."Description" ILIKE @Search
                )
                """
            );

            parameters.Add("Search", $"%{query.Search.Trim()}%");
        }

        if (query.BrandId.HasValue)
        {
            where.AppendLine("""AND p."BrandId" = @BrandId""");
            parameters.Add("BrandId", query.BrandId.Value);
        }

        if (query.CategoryId.HasValue)
        {
            where.AppendLine("""AND p."CategoryId" = @CategoryId""");
            parameters.Add("CategoryId", query.CategoryId.Value);
        }

        if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
        {
            where.AppendLine(
                """
                AND EXISTS (
                    SELECT 1
                    FROM "ProductVariants" pv_price
                    WHERE pv_price."ProductId" = p."Id"
                """
            );

            if (query.MinPrice.HasValue)
            {
                where.AppendLine(
                    """
                      AND CAST(
                            pv_price."OriginalPrice" * (1 - pv_price."DiscountPercentage" / 10000.0)
                            AS bigint
                          ) >= @MinPrice
                    """
                );

                parameters.Add("MinPrice", query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                where.AppendLine(
                    """
                      AND CAST(
                            pv_price."OriginalPrice" * (1 - pv_price."DiscountPercentage" / 10000.0)
                            AS bigint
                          ) <= @MaxPrice
                    """
                );

                parameters.Add("MaxPrice", query.MaxPrice.Value);
            }

            where.AppendLine(
                """
                )
                """
            );
        }

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;
        var offset = (pageNumber - 1) * pageSize;

        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var orderBy = BuildProductOrderBy(query.SortBy, query.SortDirection);

        var countSql = $"""
            SELECT COUNT(*)
            FROM "Products" p
            INNER JOIN "Brands" b ON b."Id" = p."BrandId"
            INNER JOIN "Categories" c ON c."Id" = p."CategoryId"
            {where};
            """;

        var dataSql = $"""
            SELECT
                p."Id",
                p."Name",
                p."Description",
                b."Name" AS "BrandName",
                c."Name" AS "CategoryName",

                COALESCE(MIN(v."OriginalPrice"), 0) AS "MinOriginalPrice",
                COALESCE(MAX(v."OriginalPrice"), 0) AS "MaxOriginalPrice",

                COALESCE(MIN(v."DiscountPercentage") / 100.0, 0) AS "MinDiscountPercentage",
                COALESCE(MAX(v."DiscountPercentage") / 100.0, 0) AS "MaxDiscountPercentage",

                COALESCE(
                    MIN(
                        CAST(
                            v."OriginalPrice" * (1 - v."DiscountPercentage" / 10000.0)
                            AS bigint
                        )
                    ),
                    0
                ) AS "MinFinalPrice",

                COALESCE(
                    MAX(
                        CAST(
                            v."OriginalPrice" * (1 - v."DiscountPercentage" / 10000.0)
                            AS bigint
                        )
                    ),
                    0
                ) AS "MaxFinalPrice",

                thumbnail."Url" AS "ThumbnailImageUrl",
                COUNT(v."Id") AS "VariantCount"
            FROM "Products" p
            INNER JOIN "Brands" b ON b."Id" = p."BrandId"
            INNER JOIN "Categories" c ON c."Id" = p."CategoryId"
            LEFT JOIN LATERAL (
                SELECT uf."Url"
                FROM "ProductImages" img
                INNER JOIN "UploadedFiles" uf
                    ON uf."Id" = img."UploadedFileId"
                WHERE img."ProductId" = p."Id"
                ORDER BY
                    img."IsMain" DESC,
                    img."Id" ASC
                LIMIT 1
            ) thumbnail ON TRUE
            LEFT JOIN "ProductVariants" v
                ON v."ProductId" = p."Id"
            {where}
            GROUP BY
                p."Id",
                p."Name",
                p."Description",
                b."Name",
                c."Name",
                thumbnail."Url"
            {orderBy}
            OFFSET @Offset
            LIMIT @PageSize;
            """;

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken)
        );

        var items = await connection.QueryAsync<ProductListItemDto>(
            new CommandDefinition(dataSql, parameters, cancellationToken: cancellationToken)
        );

        return new PaginatedList<ProductListItemDto>(
            items.ToList(),
            totalCount,
            pageNumber,
            pageSize
        );
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(
        int productId,
        CancellationToken cancellationToken
    )
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT
                p."Id",
                p."Name",
                p."Description",
                b."Name" AS "BrandName",
                c."Name" AS "CategoryName"
            FROM "Products" p
            INNER JOIN "Brands" b
                ON b."Id" = p."BrandId"
            INNER JOIN "Categories" c
                ON c."Id" = p."CategoryId"
            WHERE p."Id" = @ProductId;

            SELECT
                v."Id" AS "VariantId",
                v."OriginalPrice",
                v."DiscountPercentage" / 100.0 AS "DiscountPercentage",
                CAST(
                    v."OriginalPrice" * (1 - v."DiscountPercentage" / 10000.0)
                    AS bigint
                ) AS "FinalPrice",

                COALESCE(inv."StockQuantity", 0) AS "Quantity",
                COALESCE(inv."ReservedQuantity", 0) AS "ReservedQuantity",

                a."Id" AS "AttributeId",
                a."Name" AS "AttributeName",
                av."Id" AS "AttributeValueId",
                av."Value" AS "AttributeValue"
            FROM "ProductVariants" v
            LEFT JOIN (
                SELECT
                    "ProductVariantId",
                    SUM("StockQuantity") AS "StockQuantity",
                    SUM("ReservedQuantity") AS "ReservedQuantity"
                FROM "BranchVariantInventories"
                GROUP BY "ProductVariantId"
            ) inv
                ON inv."ProductVariantId" = v."Id"
            LEFT JOIN "VariantAttributeValues" vav
                ON vav."ProductVariantId" = v."Id"
            LEFT JOIN "AttributeValues" av
                ON av."Id" = vav."AttributeValueId"
            LEFT JOIN "Attributes" a
                ON a."Id" = av."AttributeId"
            WHERE v."ProductId" = @ProductId
            ORDER BY v."Id", a."Id", av."Id";

            SELECT
                img."Id",                
                img."IsMain",
                uf."Url" AS "ImageUrl"
            FROM "ProductImages" img
            LEFT JOIN "UploadedFiles" uf ON uf."Id" = img."UploadedFileId"
            WHERE img."ProductId" = @ProductId
            ORDER BY img."IsMain" DESC, img."Id";
            """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { ProductId = productId },
                cancellationToken: cancellationToken
            )
        );

        var product = await multi.ReadSingleOrDefaultAsync<ProductDetailDto>();

        if (product is null)
        {
            return null;
        }

        var variantRows = (await multi.ReadAsync<ProductVariantAttributeRow>()).ToList();

        product.Variants = variantRows
            .GroupBy(x => new
            {
                x.VariantId,
                x.OriginalPrice,
                x.DiscountPercentage,
                x.FinalPrice,
                x.Quantity,
                x.ReservedQuantity,
            })
            .Select(g => new ProductVariantDto
            {
                Id = g.Key.VariantId,
                OriginalPrice = g.Key.OriginalPrice,
                DiscountPercentage = g.Key.DiscountPercentage,
                FinalPrice = g.Key.FinalPrice,
                Quantity = g.Key.Quantity,
                ReservedQuantity = g.Key.ReservedQuantity,

                AttributeValueIds = g.Where(x => x.AttributeValueId.HasValue)
                    .Select(x => x.AttributeValueId!.Value)
                    .Distinct()
                    .ToList(),

                AttributeValues = g.Where(x =>
                        x.AttributeId.HasValue && x.AttributeValueId.HasValue
                    )
                    .Select(x => new VariantAttributeValueDto
                    {
                        AttributeId = x.AttributeId!.Value,
                        AttributeName = x.AttributeName ?? string.Empty,
                        AttributeValueId = x.AttributeValueId!.Value,
                        Value = x.AttributeValue ?? string.Empty,
                    })
                    .DistinctBy(x => x.AttributeValueId)
                    .OrderBy(x => x.AttributeName)
                    .ThenBy(x => x.Value)
                    .ToList(),
            })
            .ToList();

        product.VariantOptions = variantRows
            .Where(x => x.AttributeId.HasValue && x.AttributeValueId.HasValue)
            .GroupBy(x => new
            {
                AttributeId = x.AttributeId!.Value,
                AttributeName = x.AttributeName ?? string.Empty,
            })
            .Select(g => new ProductVariantOptionDto
            {
                AttributeId = g.Key.AttributeId,
                AttributeName = g.Key.AttributeName,
                Values = g.GroupBy(x => new
                    {
                        AttributeValueId = x.AttributeValueId!.Value,
                        Value = x.AttributeValue ?? string.Empty,
                    })
                    .Select(vg => new ProductVariantOptionValueDto
                    {
                        AttributeValueId = vg.Key.AttributeValueId,
                        Value = vg.Key.Value,
                    })
                    .OrderBy(x => x.Value)
                    .ToList(),
            })
            .OrderBy(x => x.AttributeName)
            .ToList();

        product.Images = (await multi.ReadAsync<ProductImageDto>()).ToList();

        return product;
    }

    private static string BuildProductOrderBy(string? sortBy, string? sortDirection)
    {
        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        return sortBy?.ToLowerInvariant() switch
        {
            "name" => $"""ORDER BY p."Name" {direction}""",
            "price" => $"""ORDER BY "MinFinalPrice" {direction}""",
            "created" => $"""ORDER BY p."Created" {direction}""",
            _ => """ORDER BY p."Name" ASC""",
        };
    }

    private sealed class ProductVariantAttributeRow
    {
        public int VariantId { get; init; }

        public long OriginalPrice { get; init; }

        public decimal DiscountPercentage { get; init; }

        public long FinalPrice { get; init; }

        public int Quantity { get; init; }

        public int ReservedQuantity { get; init; }

        public int? AttributeId { get; init; }

        public string? AttributeName { get; init; }

        public int? AttributeValueId { get; init; }

        public string? AttributeValue { get; init; }
    }
}
