using Microsoft.EntityFrameworkCore;
using PizzaStore.Models;
using Microsoft.OpenApi.Models;


const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";

builder.Services.AddEndpointsApiExplorer();

//Nas linhas abaixo temos a adição do BD SQLite e configurações do Swagger
builder.Services.AddSqlite<PizzaDb>(connectionString);
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo {
         Title = "PizzaStoreDonaMariza API",
         Description = "Desenvolvendo um mundo melhor : )",
         Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
      builder =>
      {
          builder.WithOrigins("*");
      });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

app.UseCors(MyAllowSpecificOrigins);

//Adição das rotas
app.MapGet("/", () => "Salve!");

app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());

app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});

app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatepizza, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;
    pizza.Price = updatepizza.Price;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
  var pizza = await db.Pizzas.FindAsync(id);
  if (pizza is null)
  {
    return Results.NotFound();
  }
  db.Pizzas.Remove(pizza);
  await db.SaveChangesAsync();
  return Results.Ok();
});

// 3) use the capability
app.UseCors(MyAllowSpecificOrigins);
app.Run();