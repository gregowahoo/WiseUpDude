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
using WiseUpDude.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);


#region Logging Configuration

// Update the logging configuration to include Azure Web App Diagnostics  
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddAzureWebAppDiagnostics();

// Configure Serilog (this replaces default logging)
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("/home/LogFiles/serilog.log", rollingInterval: RollingInterval.Day); // ✅ Azure Linux-friendly path
});

// Optional: if you want to add App Service diagnostics *in addition to* Serilog
// Uncomment this if you still want it
builder.Logging.AddAzureWebAppDiagnostics();

builder.Logging.AddConsole();
builder.Logging.AddDebug();
#endregion

#region Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
#endregion

#region Chat Client Configuration
// Chat client setup for Azure, GitHub, and OpenAI
var innerChatClientAzure = new AzureOpenAIClient(
    new Uri(builder.Configuration["AI:Endpoint"] ?? throw new InvalidOperationException("Missing AI:Endpoint")),
    new ApiKeyCredential(builder.Configuration["AI:Key"] ?? throw new InvalidOperationException("Missing AI:Key")))
    .AsChatClient("gpt-35-turbo");

var innerChatClientGithub = new AzureOpenAIClient(
    new Uri(builder.Configuration["GithubAI:Endpoint"] ?? throw new InvalidOperationException("Missing GithubAI:Endpoint")),
    new ApiKeyCredential(builder.Configuration["GithubAI:Key"] ?? throw new InvalidOperationException("Missing GithubAI:Key")))
    .AsChatClient("gpt-4o");

var innerChatClientOpenAI = new OpenAI.Chat.ChatClient("gpt-4.1",
    builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing OpenAI:ApiKey"))
    .AsChatClient();

//builder.Services.AddChatClient(innerChatClientAzure); // Azure-based GPT-3.5
//builder.Services.AddChatClient(innerChatClientGithub); // Azure-based GPT-3.5
builder.Services.AddChatClient(innerChatClientOpenAI); // “gpt-4o-mini” from OpenAI
#endregion

#region Services
// General service registrations
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
        ?? throw new InvalidOperationException("Missing Application Insights ConnectionString in configuration.");
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITopicsCacheService<WiseUpDude.Model.Topic>, TopicsCacheService<WiseUpDude.Model.Topic>>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ContentFetchingService>();

builder.Services.AddScoped<QuizBuilderService>();           //TODO:Deterine if this is needed

builder.Services.AddScoped<QuizStateService>();
builder.Services.AddScoped<IQuizFromPromptService, QuizFromPromptService>();
builder.Services.AddScoped<AnswerRandomizerService>();

builder.Services.AddScoped<IRepository<Quiz>, QuizRepository>();
builder.Services.AddScoped<IQuizQuestionRepository<QuizQuestion>, QuizQuestionRepository>();
builder.Services.AddScoped<ITopicRepository<Topic>, TopicRepository>();

builder.Services.AddScoped<IUserRepository<Quiz>, UserQuizRepository>();
builder.Services.AddScoped<IUserRepository<QuizQuestion>, UserQuizQuestionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<TopicService>();
builder.Services.AddScoped<QuizFromTopicService>();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

#endregion

#region Authentication
// Authentication setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Missing Google ClientId in configuration.");
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Missing Google ClientSecret in configuration.");
        googleOptions.CallbackPath = new PathString("/signin-google"); // Default redirect URI
    });
#endregion

#region EF Core and Identity
// EF Core and Identity setup
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

#region Database Initialization
// Auto-apply any pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
#endregion

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

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("WISE UP LOG: Greg, this log entry should be visible in Azure at {Time}", DateTime.UtcNow);


app.Run();
