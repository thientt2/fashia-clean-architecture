using Fashia.Application.TodoLists.Queries.GetTodos;
using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;

namespace Fashia.Application.FunctionalTests.TodoLists.Queries;

public class GetTodosTests : TestBase
{
    [Test]
    public async Task ShouldReturnPriorityLevels()
    {
        await TestApp.RunAsDefaultUserAsync();

        var query = new GetTodosQuery();

        var result = await TestApp.SendAsync(query);

        result.PriorityLevels.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnAllListsAndItems()
    {
        await TestApp.RunAsDefaultUserAsync();

        await TestApp.AddAsync(new TodoList
        {
            Name = "Shopping",
            Colour = Colour.Blue,
            Items =
                {
                    new TodoItem { Name = "Apples", Done = true },
                    new TodoItem { Name = "Milk", Done = true },
                    new TodoItem { Name = "Bread", Done = true },
                    new TodoItem { Name = "Toilet paper" },
                    new TodoItem { Name = "Pasta" },
                    new TodoItem { Name = "Tissues" },
                    new TodoItem { Name = "Tuna" }
                }
        });

        var query = new GetTodosQuery();

        var result = await TestApp.SendAsync(query);

        result.Lists.Count.ShouldBe(1);
        result.Lists.First().Items.Count.ShouldBe(7);
    }

    [Test]
    public async Task ShouldDenyAnonymousUser()
    {
        var query = new GetTodosQuery();

        var action = () => TestApp.SendAsync(query);

        await Should.ThrowAsync<UnauthorizedAccessException>(action);
    }
}
