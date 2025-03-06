using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//solo permitir el trafico por ese puerto
//Configure the app to listen on HTTP only by setting up Kestrel to listen on localhost:5294.
builder.WebHost.ConfigureKestrel(options =>
{

    options.ListenLocalhost(5251); //http only

});

//añadiendo swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//clase de middleware
builder.Services.AddHttpLogging((o)=>{});
builder.Services.AddControllers();
//builder.Services.AddSwagger();
//builder.Services.AddSingleton<IMyService, MyService>();  // Singleton
//builder.Services.AddScoped<IMyService, MyService>();    // Scoped
builder.Services.AddTransient<IMyService, MyService>();   // Transient

// Add services to the container
builder.Services.AddControllers(); // Enables controller support

// Configure logging (optional)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var app = builder.Build();

//1. Security Event Logging: 
//Place this logging middleware at the beginning of the pipeline to capture any security-related events based on status codes from subsequent middleware.
// Middleware to log security events if response status indicates an issue
app.Use(async (context, next) =>
{
    await next(); // Run the next middleware first

    if (context.Response.StatusCode >= 400)
    {
        Console.WriteLine($"Security Event: {context.Request.Path} - Status Code: {context.Response.StatusCode}");
    }
});

//2. Simulated HTTPS Enforcement:
//This middleware simulates HTTPS enforcement by checking for a ?secure=true query parameter. If the parameter is missing, it blocks the request.
app.Use(async (context, next) =>
{
    if (context.Request.Query["secure"] != "true")
    {
        context.Response.StatusCode = 400; // 400 es el código de estado para solicitudes bloqueadas.
        await context.Response.WriteAsync("Simulated HTTPS Required");
        return;
    }

    await next();
});

//3. Input Validation:
//This middleware checks the input query parameter. Only alphanumeric characters are allowed, and it blocks any input containing <script>.
app.Use(async (context, next) =>
{
    var input = context.Request.Query["input"];
    if (!IsValidInput(input))
    {
        context.Response.StatusCode = 400; // Código de estado para entrada inválida.
        await context.Response.WriteAsync("Invalid Input");
        return;
    }

    await next();
});

// Helper method for input validation
static bool IsValidInput(string input)
{
    // Checks for any unsafe characters or patterns, including "<script>"
    return string.IsNullOrEmpty(input) || (input.All(char.IsLetterOrDigit) && !input.Contains("<script>"));
}

//4. Unauthorized Access
//This middleware checks if the path is /unauthorized. If so, it returns a 401 status with "Unauthorized Access" and blocks further processing.
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/unauthorized")
    {
        context.Response.StatusCode = 401; // Código de estado para acceso no autorizado.
        await context.Response.WriteAsync("Unauthorized Access");
        return;
    }

    await next();
});

//5. Simulated Authentication and Secure Cookies:
// Middleware for simulated authentication and secure cookies
app.Use(async (context, next) =>
{
    // Simulate authentication with a query parameter (e.g., "?authenticated=true")
    var isAuthenticated = context.Request.Query["authenticated"] == "true";
    if (!isAuthenticated)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access Denied");
        }
        return;
    }

    context.Response.Cookies.Append("SecureCookie", "SecureData", new CookieOptions
    {
        HttpOnly = true,
        Secure = true
    });

    await next();
});

//6. Asynchronous Processing: 
//This middleware simulates asynchronous processing by briefly delaying the response and adding "Processed Asynchronously" to the output.
// Middleware for asynchronous processing
app.Use(async (context, next) =>
{
    await Task.Delay(100); // Simulate async operation
    if (!context.Response.HasStarted)
    {
        await context.Response.WriteAsync("Processed Asynchronously\n");
    }
    await next();
});

//7. Final Response Middleware:
//This middleware provides a final response message for any request that successfully reaches the end of the pipeline.
// Final Response Middleware
app.Run(async (context) =>
{
    if (!context.Response.HasStarted)
    {
        await context.Response.WriteAsync("Final Response from Application\n");
    }
});

//swagger pages
if(app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

//clase de middleware
//app.UseHttpLogging();
//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();
if(!app.Environment.IsDevelopment()){
    app.UseExceptionHandler("/Inicio/Error");
}
//app.UseExceptionHandler();//pantalla de error agradable en toda la app middleware

//need to add this to appsettings:
//"Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware":"Information"

// Configure middleware for error handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Global exception caught: {ex.Message}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
    }
});

//Custom middleware 1
app.Use(async (context, next) => {
    Console.WriteLine("Custom middleware 1");
    var startTime = DateTime.UtcNow;
    await next.Invoke();
    var duration = DateTime.UtcNow - startTime;
    Console.WriteLine($"Duration: {duration}");
});

//Custom middleware 2
app.Use(async (context, next) => {
    Console.WriteLine("Custom middleware 2");
    Console.WriteLine(context.Request.Path);
    await next.Invoke();
    Console.WriteLine(context.Response.StatusCode);
});

//Custom middleware con condicional
//para pedir una password en la solicitud
// app.UseWhen(
//     context => context.Request.Method != "GET",
//     appBuilder => appBuilder.Use(async (context, next) => {
//         var extractedPassword = context.Request.Headers["X-Api-Key"];
//         if (extractedPassword == "thisIsABadPassword") {
//             await next.Invoke();
//         } else {
//             context.Response.StatusCode = 401;
//             await context.Response.WriteAsync("Invalid API Key");
//         }
//     })
// );


// Enable routing and map controller endpoints
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Maps attribute-defined routes in controllers
});

var blogs = new List<Blog>
{
    new Blog {Title = "My first post", Body = "This is my first post"},
    new Blog { Title =" My second post", Body ="This is my second post"}
};

app.MapGet("/", () => "Hello World! I'M GET");
app.MapPut("/", () => "Hello World! FROM PUT");
app.MapDelete("/", () => "Hello World! FROM DELETE");
app.MapPost("/", () => "Hello World! I'M POST");

app.Use(async (context,next)=>{
    Console.WriteLine("Logica Antes");
    await next.Invoke();
    Console.WriteLine("Logica Despues");
});


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


// Middleware to demonstrate lifecycle in multiple parts of the pipeline
app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("First Middleware"); 
    await next();
});

app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("Second Middleware"); 
    await next();
});

// Final endpoint to demonstrate service lifecycle in the request
app.MapGet("/service", (IMyService myService) =>
{
    myService.LogCreation("Root"); 
    return Results.Ok("Check the console for service creation logs.");
});


app.Run();


public class Blog{
    public required string Title {get;set;}
    public required string Body {get;set;}
}

public interface IMyService
{
    void LogCreation(string message);
}

public class MyService : IMyService
{
    private readonly int _serviceId;

    public MyService()
    {
        _serviceId = new Random().Next(100000, 999999); // Generate a random 6-digit number
    }

    public void LogCreation(string message)
    {
        Console.WriteLine($"{message} - Service ID: {_serviceId}");
    }
}
