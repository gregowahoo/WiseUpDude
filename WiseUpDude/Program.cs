using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Serilog;
using Serilog.Events;
using System.ClientModel;
using WiseUpDude.Components;
using WiseUpDude.Components.Account;
using WiseUpDude.Data;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
using WiseUpDude.Services;
using WiseUpDude.Services.Interfaces;
using WiseUpDude.Shared.Services;
using WiseUpDude.Shared.State;

Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"[Serilog SelfLog] {msg}"));

var builder = WebApplication.CreateBuilder(args);

#region Logging Configuration

var isAzure = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") != null;
var logPath = isAzure
    ? @"D:\\home\\LogFiles\\serilog.log" // Azure App Service (Windows)
    : Path.Combine("Logs", "log-.txt"); // Local dev logs

if (!isAzure && !Directory.Exists("Logs"))
    Directory.CreateDirectory("Logs");

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
});

// Optional: Also enable Azure Web App Diagnostics if needed for Log Stream
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Logging.AddAzureWebAppDiagnostics();


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
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
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

builder.Services.AddScoped<WiseUpDude.Services.ITokenValidationService, WiseUpDude.Services.TokenValidationService>();

#endregion


#region Api Support

var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
if (string.IsNullOrWhiteSpace(apiBaseAddress))
    throw new InvalidOperationException("ApiBaseAddress is not configured.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("https://wiseupdude-fzauhwdug7cma9fb.centralus-01.azurewebsites.net/", "https://localhost:7150")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

#endregion


//Build the pipline

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
//{
//app.UseSwagger();
//app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(WiseUpDude.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.UseCors("AllowBlazorClient");


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

app.Run();
