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
            var item = await store.GetAsync;
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


        )

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ).ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapGet("/hello", () => "Hello");



app.Run();

public record Item { Guid Id; string Name; decimal Price; }
public record ItemDto { string Name; decimal Price; }



record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
