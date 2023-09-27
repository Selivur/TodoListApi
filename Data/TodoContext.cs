using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TodoListApi.Models;

namespace TodoListApi.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
        }
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}
