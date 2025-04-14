using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.ClientModel;
using WiseUpDude.Components;
using WiseUpDude.Components.Account;
using WiseUpDude.Data;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Model;
using WiseUpDude.Services;
using WiseUpDude.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Google;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.WriteTo.Console()
                 .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));

// Add logging region
builder.Logging.AddConsole();
builder.Logging.AddDebug();

#region Configuration

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

#endregion

#region Chat Client Configuration

// 1) Create an Azure-based chat client for GPT-3.5
var innerChatClientAzure = new AzureOpenAIClient(
    new Uri(builder.Configuration["AI:Endpoint"] ?? throw new InvalidOperationException("Missing AI:Endpoint")),
    new ApiKeyCredential(builder.Configuration["AI:Key"] ?? throw new InvalidOperationException("Missing AI:Key")))
    .AsChatClient("gpt-35-turbo");

// 2) Create an Github chat client for ???
var innerChatClientGithub = new AzureOpenAIClient(
    new Uri(builder.Configuration["GithubAI:Endpoint"] ?? throw new InvalidOperationException("Missing GithubAI:Endpoint")),
    new ApiKeyCredential(builder.Configuration["GithubAI:Key"] ?? throw new InvalidOperationException("Missing GithubAI:Key")))
    .AsChatClient("gpt-4o");

// 2) Create a third chat client for OpenAI gpt-4o-mini
var innerChatClientOpenAI = new OpenAI.Chat.ChatClient("gpt-4o-mini",
    builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing OpenAI:ApiKey"))
    .AsChatClient();

builder.Services.AddChatClient(innerChatClientAzure);             // Azure-based GPT-3.5
//builder.Services.AddChatClient(innerChatClientGithub);              // Azure-based GPT-3.5
//builder.Services.AddChatClient(innerChatClientOpenAI);            // “gpt-4o-mini” from OpenAI

#endregion

#region Services

builder.Services.AddMemoryCache();

// Register TopicsCacheService as a singleton
builder.Services.AddSingleton<ITopicsCacheService<TopicItem>, TopicsCacheService<TopicItem>>();

// Razor Components with Interactivity
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// HTTP Client (for external API calls)
builder.Services.AddHttpClient();

// Application Services
builder.Services.AddScoped<ContentFetchingService>();
builder.Services.AddScoped<QuizBuilderService>();
builder.Services.AddScoped<QuizStateService>();
builder.Services.AddScoped<IRepository<Quiz>, QuizRepository>();
builder.Services.AddScoped<IRepository<QuizQuestion>, QuizQuestionRepository>();
builder.Services.AddScoped<QuizTopicService>();
builder.Services.AddScoped<QuizQuestionsFromTopic>();

// Identity
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// Email Sender (No-Op)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Missing Google ClientId in configuration.");
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Missing Google ClientSecret in configuration.");
        googleOptions.CallbackPath = new PathString("/signin-google"); // Default redirect URI
    });

// EF Core and Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

#endregion

var app = builder.Build();

#region Middleware

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();

// Static assets & Razor Components
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Identity UI endpoints
app.MapAdditionalIdentityEndpoints();

// Middleware to set the X-Content-Type-Options header on every response
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    await next();
});

#endregion

app.Run();
