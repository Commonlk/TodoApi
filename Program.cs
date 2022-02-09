using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.Select(x => new TodoItemDto(x)).ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDto(x)).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(new TodoItemDto(todo))
            : Results.NotFound());

app.MapPost("/todoitems", async (TodoItemDto todoItemDto, TodoDb db) =>
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDto.IsComplete,
        Name = todoItemDto.Name
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todoItem.Id}", todoItem);
});

app.MapPut("/todoitems/{id}", async (int id, TodoItemDto todoItemDto, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo == null) return Results.NotFound();

    todo.Name = todoItemDto.Name;
    todo.IsComplete = todoItemDto.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo == null) return Results.NotFound();

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();

    return Results.Ok(new TodoItemDto(todo));
});

app.Run();

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

class TodoItemDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public TodoItemDto() { }

    public TodoItemDto(Todo todoItem) =>
    (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}