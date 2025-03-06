// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using BlogApi;

var httpClient = new HttpClient();
var apiBaseUrl = "http://localhost:5251";

var client = new BlogApiClient(apiBaseUrl,httpClient);


var blogs = await client.BlogsAllAsync();

foreach(var blog in blogs){
    Console.WriteLine ($"{blog.Title}: {blog.Body}");
}

await client.BlogsDELETEAsync(0);

var newBlog = new Blog{

    Title="My tittle",
    Body = "A new body"

};
await client.BlogsPOSTAsync(newBlog);


//para crear el client
//await new SwaggerClientGenerator().GenerateClient();

// using System.Text.Json;

// Console.WriteLine("Hello, World!");

// var httpClient = new HttpClient();
// var apiBaseUrl = "http://localhost:5251";

// var httpResults = await httpClient.GetAsync($"{apiBaseUrl}/Blogs");

// if(httpResults.StatusCode != System.Net.HttpStatusCode.OK){
//     Console.WriteLine("Failed to fetch blogs.");
// }

// var blogsStream = await httpResults.Content.ReadAsStreamAsync();
// var options = new System.Text.Json.JsonSerializerOptions{
//     PropertyNameCaseInsensitive = true
// };

// var blogs = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Blog>>(blogsStream,options);

// foreach(var blog in blogs){
//     Console.WriteLine ($"{blog.Title}: {blog.Body}");
// }



// public class Blog {

//     public required string Title {get;set;}
//     public required string Body {get;set;}

// }