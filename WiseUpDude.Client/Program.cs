using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection; // Add this using directive for AddHttpClient extension method  
using Microsoft.Extensions.Logging;
using WiseUpDude.Shared.Services;
using WiseUpDude.Shared.State;
using Blazored.LocalStorage;
using WiseUpDude.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Set up logging: log everything to the browser console  
builder.Logging.SetMinimumLevel(LogLevel.Trace);
//builder.Logging.AddConsole();  


string? baseAddress = builder.Configuration["ApiBaseAddress"];

if (string.IsNullOrWhiteSpace(baseAddress))
{
    var environment = builder.HostEnvironment; // Use HostEnvironment property
    baseAddress = environment.IsDevelopment()
        ? "https://localhost:7150/"
        : "https://wiseupdude.com/";
}

// Log the base address being used
Console.WriteLine($"HttpClient BaseAddress set to: {baseAddress}");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

builder.Services.AddScoped<QuizApiService>();
builder.Services.AddScoped<UserQuizApiService>();

builder.Services.AddScoped<LearningTrackQuizApiService>();
builder.Services.AddScoped<LearningTrackQuizAttemptApiService>();

// In both Server and WASM Program.cs  
builder.Services.AddScoped<QuizState>();

builder.Services.AddScoped<IUserQuizAttemptApiService, UserQuizAttemptApiService>();
builder.Services.AddScoped<WiseUpDude.Shared.Services.UrlMetaClient>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<WiseUpDude.Client.Services.JwtAuthService>();
builder.Services.AddScoped<WiseUpDude.Shared.Services.IAssignmentTypeService, WiseUpDude.Shared.Services.AssignmentTypeService>();

await builder.Build().RunAsync();
