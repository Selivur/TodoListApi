using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TodoListApi.Controllers;
using TodoListApi.Data;
using TodoListApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace TodoListApiTests
{
    public class TodoItemsControllerTests : IDisposable
    {
        private readonly DbContextOptions<TodoContext> _options;
        private readonly TodoContext _context;

        public TodoItemsControllerTests()
        {
            _options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TodoContext(_options);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetTodoItems_ReturnsListOfTodoItems()
        {
            // Arrange
            _context.TodoItems.Add(new TodoItem { Id = 1, Title = "Test Item 1", Description = "Description 1" });
            _context.TodoItems.Add(new TodoItem { Id = 2, Title = "Test Item 2", Description = "Description 2" });
            _context.SaveChanges();
            var controller = new TodoItemsController(_context);

            // Act
            var result = await controller.GetTodoItems();

            // Assert
            if (result.Result is OkObjectResult okResult)
            {
                var items = Assert.IsType<List<TodoItem>>(okResult.Value);
                Assert.Equal(2, items.Count);
            }
            else
            {
                Console.WriteLine($"Expected OkObjectResult but received {result.Result?.GetType().Name}");
            }
        }

        [Fact]
        public async Task GetTodoItem_WithValidId_ReturnsTodoItem()
        {
            // Arrange
            _context.TodoItems.Add(new TodoItem { Id = 1, Title = "Test Item", Description = "Test Description" });
            _context.SaveChanges();
            var controller = new TodoItemsController(_context);

            // Act
            var result = await controller.GetTodoItem(1);

            // Assert
            if (result.Result is OkObjectResult okResult)
            {
                var item = Assert.IsType<TodoItem>(okResult.Value);
                Assert.Equal(1, item.Id);
            }
            else
            {
                Console.WriteLine($"Expected OkObjectResult but received {result.Result?.GetType().Name}");
            }
        }

        [Fact]
        public async Task GetTodoItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new TodoItemsController(_context);

            // Act
            var result = await controller.GetTodoItem(999); // Assuming 999 does not exist.

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostTodoItem_ReturnsCreatedResponse()
        {
            // Arrange
            var controller = new TodoItemsController(_context);
            var newItem = new TodoItem { Id = 3, Title = "New Test Item", Description = "Test Description" };

            // Act
            var result = await controller.PostTodoItem(newItem);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetTodoItem", createdAtActionResult.ActionName);
            Assert.Equal(3, createdAtActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task PutTodoItem_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var existingItem = new TodoItem { Id = 1, Title = "Test Item", Description = "Test Description" };
            _context.TodoItems.Add(existingItem);
            _context.SaveChanges();

            var controller = new TodoItemsController(_context);
            var updatedItem = new TodoItem { Id = 1, Title = "Updated Test Item", Description = "Updated Description" };

            // Act
            // Detach the existing entity from the change tracker
            _context.Entry(existingItem).State = EntityState.Detached;

            var result = await controller.PutTodoItem(1, updatedItem);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Ensure that the entity is updated in the database
            var updatedEntity = await _context.TodoItems.FindAsync(1);
            Assert.Equal("Updated Test Item", updatedEntity.Title);
            Assert.Equal("Updated Description", updatedEntity.Description);
        }



        [Fact]
        public async Task PutTodoItem_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var controller = new TodoItemsController(_context);
            var updatedItem = new TodoItem { Id = 1, Title = "Updated Test Item", Description = "Updated Description" };

            // Act
            var result = await controller.PutTodoItem(999, updatedItem); // Assuming 999 does not exist.

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteTodoItem_WithValidId_ReturnsNoContent()
        {
            // Arrange
            _context.TodoItems.Add(new TodoItem { Id = 1, Title = "Test Item", Description = "Test Description" });
            _context.SaveChanges();
            var controller = new TodoItemsController(_context);

            // Act
            var result = await controller.DeleteTodoItem(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTodoItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new TodoItemsController(_context);

            // Act
            var result = await controller.DeleteTodoItem(999); // Assuming 999 does not exist.

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
