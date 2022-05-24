using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapPost("/products", (ProductRequest productRequest, ApplicationDbContext context) => 
{
    var category = context.Category.Where(c => c.Id == productRequest.CategoryId).First();
    var product = new Product
    {
        Code = productRequest.Code,
        Name = productRequest.Name,
        Description = productRequest.Description,
        Category = category
    };

    if(productRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach (var item in productRequest.Tags)
        {
            product.Tags.Add(new Tag { Name = item });
        }
    }
    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}", product.Id);
});

//api.app.com/user/{code} = Route Params
app.MapGet("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) => 
{
    var product = context.Products.Where(p => p.Id == id)
    .Include(p => p.Category)
    .Include(p => p.Tags).First();

    if (product != null)
        return Results.Ok(product);

    return Results.NotFound();
});

app.MapPut("/products", (Product product) => 
{
    var productSaved = ProductRepository.GetBy(product.Code);
    productSaved.Name = product.Name;
    return Results.Ok();
});

app.MapDelete("/products/{code}", ([FromRoute] string code) => 
{
    var productSaved = ProductRepository.GetBy(code);
    ProductRepository.Remove(productSaved);
    return Results.Ok();
});

app.MapGet("/configuration/database", (IConfiguration configuration) => 
{
    return Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}");
});

app.Run();
