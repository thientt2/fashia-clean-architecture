using Fashia.Application.Common.Exceptions;
using Fashia.Application.TodoLists.Commands.CreateTodoList;
using Fashia.Application.TodoLists.Commands.UpdateTodoList;
using Fashia.Domain.Entities;

namespace Fashia.Application.FunctionalTests.TodoLists.Commands;

public class UpdateTodoListTests : TestBase
{
    [Test]
    public async Task ShouldRequireValidTodoListId()
    {
        var command = new UpdateTodoListCommand { Id = 99, Name = "New Name" };
        await Should.ThrowAsync<NotFoundException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireUniqueName()
    {
        var listId = await TestApp.SendAsync(new CreateTodoListCommand
        {
            Name = "New List"
        });

        await TestApp.SendAsync(new CreateTodoListCommand
        {
            Name = "Other List"
        });

        var command = new UpdateTodoListCommand
        {
            Id = listId,
            Name = "Other List"
        };

        var ex = await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(command));

        ex.Errors.ShouldContainKey("Name");
        ex.Errors["Name"].ShouldContain("'Name' must be unique.");
    }

    [Test]
    public async Task ShouldUpdateTodoList()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        var listId = await TestApp.SendAsync(new CreateTodoListCommand
        {
            Name = "New List"
        });

        var command = new UpdateTodoListCommand
        {
            Id = listId,
            Name = "Updated List Name"
        };

        await TestApp.SendAsync(command);

        var list = await TestApp.FindAsync<TodoList>(listId);

        list.ShouldNotBeNull();
        list!.Name.ShouldBe(command.Name);
        list.LastModifiedBy.ShouldNotBeNull();
        list.LastModifiedBy.ShouldBe(userId);
        list.LastModified.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
