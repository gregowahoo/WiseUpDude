using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WiseUpDude.Client.Services;
using Microsoft.Extensions.DependencyInjection; // Add this using directive for AddHttpClient extension method

using Microsoft.Extensions.Logging;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Set up logging: log everything to the browser console
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddConsole();

//builder.Services.AddScoped<QuizApiService>();

//builder.Services.AddHttpClient<QuizApiService>(client =>
//{
//    //client.BaseAddress = new Uri("https://localhost:7201");
//    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7150/") });
//});


//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7199") });

await builder.Build().RunAsync();
