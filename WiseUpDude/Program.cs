using Azure.AI.OpenAI;
using Blazored.LocalStorage;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.ClientModel;
using System.Text;
using WiseUpDude.Components;
using WiseUpDude.Components.Account;
using WiseUpDude.Data;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
using WiseUpDude.Services;
using WiseUpDude.Services.Interfaces;
using WiseUpDude.Shared.Services;
using WiseUpDude.Shared.Services.Interfaces;
using WiseUpDude.Shared.State;
using WiseUpDude.Services.CategoryArt;

// Enable Serilog's self-diagnostics to see if there are any issues with Serilog itself
Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"[Serilog SelfLog] {msg}"));

var builder = WebApplication.CreateBuilder(args);

// First configure Application Insights - this must come before Serilog configuration
// Only enable Application Insights in non-development environments
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddApplicationInsightsTelemetry(options => {
        // Force reinitialize with environment variable if available
        var envConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envConnectionString)) {
            options.ConnectionString = envConnectionString;
            Console.WriteLine($"[Startup] Using APPLICATIONINSIGHTS_CONNECTION_STRING from environment variables with length {envConnectionString.Length}");
        }
    });
}
else
{
    Console.WriteLine("[Startup] Application Insights disabled in Development environment");
}

#region Logging Configuration

var isAzure = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") != null;

var shouldUseFileLogging = !isAzure && builder.Environment.IsDevelopment();
var logPath = isAzure
    ? @"D:\\home\\LogFiles\\serilog.log"
    : Path.Combine("Logs", "log-.txt");

if (shouldUseFileLogging && !Directory.Exists("Logs"))
    Directory.CreateDirectory("Logs");

// Important: Let's completely rely on the TelemetryConfiguration from DI
builder.Host.UseSerilog((context, services, configuration) =>
{
    var loggerConfig = configuration
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "WiseUpDude")
        .WriteTo.Console();

    // Only configure Application Insights sink if not in development
    if (!builder.Environment.IsDevelopment())
    {
        // Get the TelemetryConfiguration from the DI container
        var telemetryConfiguration = services.GetRequiredService<TelemetryConfiguration>();
        
        // Use the TelemetryConfiguration directly - this is the most reliable method
        loggerConfig.WriteTo.ApplicationInsights(
            telemetryConfiguration,
            TelemetryConverter.Traces);
        
        Console.WriteLine("[Startup] Configured ApplicationInsights sink with TelemetryConfiguration from DI");
    }
    else
    {
        Console.WriteLine("[Startup] Application Insights sink disabled in Development environment");
    }

    if (shouldUseFileLogging || isAzure)
    {
        loggerConfig.WriteTo.File(logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            shared: true);
    }
});

if (isAzure)
{
    builder.Logging.AddAzureWebAppDiagnostics();
}

#endregion

// Log Perplexity API Key presence, length, and value for diagnostics (WARNING: do not log secrets in production)
var perplexityApiKey = builder.Configuration["Perplexity:ApiKey"];
if (string.IsNullOrEmpty(perplexityApiKey))
    Serilog.Log.Warning("Perplexity:ApiKey is missing or empty in configuration/environment.");
else {
    Serilog.Log.Information("Perplexity:ApiKey is present. Length: {Length}", perplexityApiKey.Length);
    Serilog.Log.Warning("Perplexity:ApiKey value: {ApiKey}", perplexityApiKey); // WARNING: Remove after debugging
}


// Add services to the container.
builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies");

builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents()
    .AddAuthenticationStateSerialization();

#region Identity and Login Configuration

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Add Google and Facebook Authentication directly
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Missing Google ClientId in configuration.");
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Missing Google ClientSecret in configuration.");
        googleOptions.CallbackPath = new PathString("/signin-google");
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? throw new InvalidOperationException("Missing Facebook AppId in configuration.");
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? throw new InvalidOperationException("Missing Facebook AppSecret in configuration.");
    });

// Configure persistent cookie settings for login persistence
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Persistent for 30 days
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

#endregion


#region Perplexity AI Configuration

builder.Services.AddHttpClient("PerplexityAI", client =>
{
    client.BaseAddress = new Uri("https://api.perplexity.ai/");
    //client.DefaultRequestHeaders.Add("Authorization", $"Bearer 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");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Perplexity:ApiKey"]}");
    client.DefaultRequestHeaders.Add("accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(5);
});

#endregion

// Configure HttpClient for Gemini API
builder.Services.AddHttpClient("GeminiAI", client =>
{
    // Use the specific API endpoint for generateContent
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    // API Key will be added as a query parameter per Google's recommended usage for simple HTTP calls
    // client.DefaultRequestHeaders.Add("x-goog-api-key", builder.Configuration["GeminiApiKey"]); // Not needed if appending to URL
});


#region Chat Client Configuration

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

builder.Services.AddChatClient(innerChatClientOpenAI);

#endregion


#region MyBits

//Brought over from Original WUD
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITopicsCacheService<Topic>, TopicsCacheService<Topic>>();
builder.Services.AddScoped<IPromptSuggestionCacheService, PromptSuggestionCacheService>();
builder.Services.AddScoped<IUrlSuggestionCacheService, UrlSuggestionCacheService>();
builder.Services.AddHttpClient();


builder.Services.AddScoped<ContentFetchingService>();
builder.Services.AddScoped<QuizBuilderService>();
builder.Services.AddScoped<QuizStateService>();
builder.Services.AddScoped<IQuizFromPromptService, QuizFromPromptService>();
builder.Services.AddScoped<AnswerRandomizerService>();

builder.Services.AddScoped<IRepository<Quiz>, QuizRepository>();
builder.Services.AddScoped<QuizRepository>();

builder.Services.AddScoped<UserQuizRepository>();

builder.Services.AddScoped<IQuizQuestionRepository<QuizQuestion>, QuizQuestionRepository>();
builder.Services.AddScoped<ITopicRepository<Topic>, TopicRepository>();

builder.Services.AddScoped<IUserQuizRepository<Quiz>, UserQuizRepository>();
builder.Services.AddScoped<IUserQuizQuestionRepository<QuizQuestion>, UserQuizQuestionRepository>();

builder.Services.AddScoped<IUserQuizAttemptRepository<WiseUpDude.Model.UserQuizAttempt>, UserQuizAttemptRepository>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<ITenorGifService, TenorGifService>();

//builder.Services.AddScoped<TopicService>();
//builder.Services.AddScoped<QuizFromTopicService>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


builder.Services.AddScoped<DashboardDataService>();

builder.Services.AddScoped<QuizApiService>();
builder.Services.AddScoped<UserQuizApiService>();

builder.Services.AddScoped<LearningTrackQuizApiService>();
builder.Services.AddScoped<LearningTrackQuizAttemptApiService>();

//TODO: (Using this still?)
builder.Services.AddScoped<QuizState>();

builder.Services.AddScoped<IUserQuizAttemptApiService, UserQuizAttemptApiService>();
builder.Services.AddScoped<ILearningTrackQuizAttemptRepository, LearningTrackQuizAttemptRepository>();

builder.Services.AddScoped<ILearningTrackRepository, LearningTrackRepository>();
builder.Services.AddScoped<ILearningTrackCategoryRepository, LearningTrackCategoryRepository>();
builder.Services.AddScoped<ILearningTrackSourceRepository, LearningTrackSourceRepository>();
builder.Services.AddScoped<ILearningTrackQuizRepository, LearningTrackQuizRepository>();
builder.Services.AddScoped<ILearningTrackQuizQuestionRepository, LearningTrackQuizQuestionRepository>();

builder.Services.AddScoped<WiseUpDude.Data.Repositories.Interfaces.ILearningTrackQuizAttemptRepository, WiseUpDude.Data.Repositories.LearningTrackQuizAttemptRepository>();
builder.Services.AddScoped<WiseUpDude.Data.Repositories.Interfaces.ILearningTrackQuizAttemptQuestionRepository, WiseUpDude.Data.Repositories.LearningTrackQuizAttemptQuestionRepository>();

builder.Services.AddScoped<UserQuizAttemptApiService>();

builder.Services.AddSingleton<WiseUpDude.Shared.Services.ToastService>();

// Register PerplexityService and its dependencies
builder.Services.AddScoped<PerplexityService>();

builder.Services.AddScoped<ContextualQuizService>();

builder.Services.AddScoped<GeminiService>(); 

builder.Services.AddScoped<LearningTrackQuizService>();

builder.Services.AddScoped<WiseUpDude.Services.UrlMetaService>();
builder.Services.AddScoped<WiseUpDude.Shared.Services.UrlMetaClient>();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<WiseUpDude.Services.ITokenValidationService, WiseUpDude.Services.TokenValidationService>();

#if BlazorWebAssembly
builder.Services.AddScoped<IAssignmentTypeService, WiseUpDude.Shared.Services.AssignmentTypeApiService>();
#else
builder.Services.AddScoped<IAssignmentTypeService, WiseUpDude.Services.AssignmentTypeDbService>();
#endif

builder.Services.AddScoped<SpecialQuizAssignmentService>();
builder.Services.AddScoped<AssignmentTypeRepository>();
builder.Services.AddScoped<SpecialQuizAssignmentRepository>();
builder.Services.AddScoped<AssignmentTypeDbService>();

// Category art generation services
builder.Services.Configure<OpenAiImageOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddSingleton<ICategoryArtService, CategoryArtService>();
#endregion

var app = builder.Build();

app.UseSerilogRequestLogging(); // Add this line to enable request logging

app.UseRouting();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(WiseUpDude.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Seed generated category art (admin only)
app.MapPost("/admin/seed-category-art", async (
    IWebHostEnvironment env,
    ICategoryArtService svc,
    ILoggerFactory loggerFactory,
    CancellationToken ct) =>
{
    var logger = loggerFactory.CreateLogger("CategoryArtSeed");
    var dir = Path.Combine(env.WebRootPath, "images", "categories");
    Directory.CreateDirectory(dir);

    logger.LogInformation("Seeding category art start. Directory={Dir}", dir);

    var generated = 0;
    var skipped = 0;
    var failed = new List<object>();

    foreach (var label in WiseUpDude.Services.CategoryArt.CategoryArtPrompts.Map.Keys)
    {
        var file = Path.Combine(dir, Slug(label) + ".png");
        if (System.IO.File.Exists(file))
        {
            skipped++;
            logger.LogInformation("Skip existing icon. Label={Label} File={File}", label, file);
            continue;
        }
        try
        {
            logger.LogInformation("Generating icon. Label={Label}", label);
            var prompt = WiseUpDude.Services.CategoryArt.CategoryArtPrompts.BuildPrompt(label);
            var bytes = await svc.GeneratePngAsync(prompt, ct);
            await File.WriteAllBytesAsync(file, bytes, ct);
            generated++;
            logger.LogInformation("Icon saved. Label={Label} File={File} Bytes={Bytes}", label, file, bytes.Length);
        }
        catch (System.ClientModel.ClientResultException cre)
        {
            logger.LogError(cre, "ClientResult failure for label {Label}. Status={Status} Message={Message}", label, (int?)cre.Status, cre.Message);
            failed.Add(new { label, error = "client_result", status = (int?)cre.Status, message = cre.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure for label {Label}", label);
            failed.Add(new { label, error = "exception", message = ex.Message });
        }
    }

    logger.LogInformation("Seeding category art done. Generated={Generated} Skipped={Skipped} Failed={Failed}", generated, skipped, failed.Count);

    return Results.Ok(new { generated, skipped, failed, path = "/images/categories" });

    static string Slug(string s)
    {
        s = s.Trim().ToLowerInvariant();
        return System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9]+", "-").Trim('-');
    }
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

// Regenerate a single category icon (admin only)
app.MapPost("/admin/category-art/{slug}/regenerate", async (
    string slug,
    IWebHostEnvironment env,
    ICategoryArtService svc,
    ILoggerFactory loggerFactory,
    CancellationToken ct) =>
{
    var logger = loggerFactory.CreateLogger("CategoryArtRegenerate");
    string? label = WiseUpDude.Services.CategoryArt.CategoryArtPrompts.Map.Keys
        .FirstOrDefault(k => System.Text.RegularExpressions.Regex.Replace(k.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-') == slug);

    if (string.IsNullOrWhiteSpace(label))
    {
        logger.LogWarning("Regenerate called with unknown slug: {Slug}", slug);
        return Results.NotFound(new { error = "unknown_slug", slug });
    }

    var dir = Path.Combine(env.WebRootPath, "images", "categories");
    Directory.CreateDirectory(dir);
    var file = Path.Combine(dir, slug + ".png");

    try
    {
        logger.LogInformation("Regenerating icon. Label={Label} File={File}", label, file);
        var prompt = WiseUpDude.Services.CategoryArt.CategoryArtPrompts.BuildPrompt(label);
        var bytes = await svc.GeneratePngAsync(prompt, ct);
        await File.WriteAllBytesAsync(file, bytes, ct);
        logger.LogInformation("Icon regenerated. Label={Label} Bytes={Bytes}", label, bytes.Length);
        return Results.Ok(new { label, file = $"/images/categories/{slug}.png", bytes = bytes.Length, v = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
    }
    catch (System.ClientModel.ClientResultException cre)
    {
        logger.LogError(cre, "ClientResult failure during regenerate for label {Label}. Status={Status} Message={Message}", label, (int?)cre.Status, cre.Message);
        return Results.Problem(title: "client_result", detail: cre.Message, statusCode: (int?)cre.Status ?? 500);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected failure during regenerate for label {Label}", label);
        return Results.Problem(title: "exception", detail: ex.Message, statusCode: 500);
    }
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Seed roles
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "FreeSubscriber", "PaidSubscriber", "EnterpriseSubscriber" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

var appInsightsTelemetryClient = builder.Services.BuildServiceProvider().GetService<TelemetryClient>();

// Ensure the app runs
app.Run();
