
using System.Collections.Concurrent;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IItemStore, InMemoryItemStore>();



var app = builder.Build();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/items", async (IItemStore store) =>
        {
            var items = await store.GetAllAsync();
            return Results.Ok(items);
        });

app.MapGet("/items/{id:guid}", async (Guid id, IItemStore store) =>
        {
            var item = await store.GetAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }
        );

app.MapPost("/items", async (ItemDto dto, IItemStore store, HttpContext ctx) =>
        {
            var error = Validate(dto);
            if (error is not null) return Results.BadRequest(new { error });

            var item = new Item
            {
                Id = Guid.NewGuid(),
                Name = dto.Name!,
                Price = dto.Price
            };
            await store.AddAsync(item);
            var location = $"/items/{item.Id}";
            return Results.Created(location, item);
        });

app.MapPut("/item/{id:guid}", async (Guid id, ItemDto dto, IItemStore store) =>
        {

            var error = Validate(dto);
            if (error is not null) return Results.BadRequest(new { error });

            var exists = await store.GetAsync(id);
            if (exists is null) return Results.NotFound();
            var updated = exists with { Name = dto.Name!, Price = dto.Price };
            await store.UpdateAsync(updated);
            return Results.NoContent();
        });

app.MapDelete("/item/{id:guid}", async (Guid id, IItemStore store) =>
        {
            var removed = await store.DeleteAsync(id);
            return removed ? Results.NoContent() : Results.NotFound();
        });


app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapGet("/hello", () => "Hello");



app.Run();

static string? Validate(ItemDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return "Name can't be empty";
    if (dto.Price < 0) return "Price can't be below 0.";
    return null;
}

public record Item
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
}

public record ItemDto(string? Name, decimal Price);


public interface IItemStore
{
    Task<IReadOnlyCollection<Item>> GetAllAsync();
    Task<Item?> GetAsync(Guid id);
    Task AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task<bool> DeleteAsync(Guid id);
}

public sealed class InMemoryItemStore : IItemStore
{
    private readonly ConcurrentDictionary<Guid, Item> _data = new();

    public Task<IReadOnlyCollection<Item>> GetAllAsync()
        => Task.FromResult((IReadOnlyCollection<Item>)_data.Values);

    public Task<Item?> GetAsync(Guid id)
        => Task.FromResult(_data.TryGetValue(id, out var it) ? it : null);

    public Task AddAsync(Item item)
    {
        _data[item.Id] = item;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Item item)
    {
        _data[item.Id] = item;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id)
        => Task.FromResult(_data.TryRemove(id, out _));
}









record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
