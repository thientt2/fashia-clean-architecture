using Fashia.Domain.Entities;

namespace Fashia.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }
    
    public string? Title { get; init; }
    
    public string? Name { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TodoList, LookupDto>();
            CreateMap<TodoItem, LookupDto>();
            CreateMap<Category, LookupDto>();
            CreateMap<Brand, LookupDto>();
        }
    }
}
