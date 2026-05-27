using Fashia.Application.TodoItems.Commands.CreateTodoItem;
using Fashia.Application.TodoItems.Commands.UpdateTodoItem;
using Fashia.Application.TodoLists.Commands.CreateTodoList;
using Fashia.Domain.Entities;

namespace Fashia.Application.FunctionalTests.TodoItems.Commands;

public class UpdateTodoItemTests : TestBase
{
    [Test]
    public async Task ShouldRequireValidTodoItemId()
    {
        var command = new UpdateTodoItemCommand { Id = 99, Name = "New Name" };
        await Should.ThrowAsync<NotFoundException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateTodoItem()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        var listId = await TestApp.SendAsync(new CreateTodoListCommand
        {
            Name = "New List"
        });

        var itemId = await TestApp.SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Name = "New Item"
        });

        var command = new UpdateTodoItemCommand
        {
            Id = itemId,
            Name = "Updated Item Name"
        };

        await TestApp.SendAsync(command);

        var item = await TestApp.FindAsync<TodoItem>(itemId);

        item.ShouldNotBeNull();
        item!.Name.ShouldBe(command.Name);
        item.LastModifiedBy.ShouldNotBeNull();
        item.LastModifiedBy.ShouldBe(userId);
        item.LastModified.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
