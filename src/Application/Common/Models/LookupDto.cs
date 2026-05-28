using Fashia.Domain.Entities;

namespace Fashia.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }

    public string? Name { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, LookupDto>();
            CreateMap<Brand, LookupDto>();
        }
    }
}
