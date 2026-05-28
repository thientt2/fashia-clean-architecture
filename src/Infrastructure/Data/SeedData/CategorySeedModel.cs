namespace Fashia.Infrastructure.Data.SeedData;

public sealed class CategorySeedModel
{
    public string Name { get; set; } = string.Empty;

    public List<CategorySeedModel> Children { get; set; } = [];
}
