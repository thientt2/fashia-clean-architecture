using Fashia.Application.Common.Exceptions;
using Fashia.Application.TodoLists.Commands.CreateTodoList;
using Fashia.Domain.Entities;

namespace Fashia.Application.FunctionalTests.TodoLists.Commands;

public class CreateTodoListTests : TestBase
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new CreateTodoListCommand();
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireUniqueName()
    {
        await TestApp.SendAsync(new CreateTodoListCommand
        {
            Name = "Shopping"
        });

        var command = new CreateTodoListCommand
        {
            Name = "Shopping"
        };

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldCreateTodoList()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        var command = new CreateTodoListCommand
        {
            Name = "Tasks"
        };

        var id = await TestApp.SendAsync(command);

        var list = await TestApp.FindAsync<TodoList>(id);

        list.ShouldNotBeNull();
        list!.Name.ShouldBe(command.Name);
        list.CreatedBy.ShouldBe(userId);
        list.Created.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
