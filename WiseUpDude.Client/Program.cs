using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection; // Add this using directive for AddHttpClient extension method
using Microsoft.Extensions.Logging;
using WiseUpDude.Shared.Services;
using WiseUpDude.Shared.State;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Set up logging: log everything to the browser console
builder.Logging.SetMinimumLevel(LogLevel.Trace);
//builder.Logging.AddConsole();

var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
if (string.IsNullOrWhiteSpace(apiBaseAddress))
    throw new InvalidOperationException("ApiBaseAddress is not configured.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

builder.Services.AddScoped<QuizApiService>();
builder.Services.AddScoped<UserQuizApiService>();

builder.Services.AddScoped<LearningTrackQuizApiService>();
builder.Services.AddScoped<LearningTrackQuizAttemptApiService>();

// In both Server and WASM Program.cs
builder.Services.AddScoped<QuizState>();

builder.Services.AddScoped<IUserQuizAttemptApiService, UserQuizAttemptApiService>();

await builder.Build().RunAsync();
