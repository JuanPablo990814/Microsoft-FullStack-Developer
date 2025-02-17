using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var blogs = new List<Blog>
{
    new Blog {Title = "My first post", Body = "This is my first post"},
    new Blog { Title =" My second post", Body ="This is my second post"}
};

app.MapGet("/", () => "Hello World! I'M GET");
app.MapPut("/", () => "Hello World! FROM PUT");
app.MapDelete("/", () => "Hello World! FROM DELETE");
app.MapPost("/", () => "Hello World! I'M POST");
app.MapGet("/Prueba", () => "Probanding! I'M GET");
app.MapGet("/Blogs", () => {return blogs;});
app.MapGet("/Blogs/{id}", (int id) => 
{
    if (id < 0 || id >= blogs.Count())

        return Results.NotFound();
    else
        return Results.Ok(blogs[id]);

});

app.MapPost("/Blogs", (Blog blog) => {

    blogs.Add(blog);
    return Results.Created($"/Blogs/{blogs.Count-1}",blog);
    
});

app.MapDelete("/Blogs/{id}", (int id) => 
{
    if (id < 0 || id >= blogs.Count())
    {
        return Results.NotFound();
    }
    else
    {
        blogs.RemoveAt(id);
        return Results.NoContent();
    }
});

app.MapPut("/Blogs/{id}", (int id,Blog blog) => 
{
    if (id < 0 || id >= blogs.Count())
    {
        return Results.NotFound();
    }
    else
    {
        blogs[id] = blog;
        return Results.Ok(blog);
    }
});

app.Run();


public class Blog{
    public required string Title {get;set;}
    public required string Body {get;set;}
}