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

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    //Add Google Authentication
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Missing Google ClientId in configuration.");
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Missing Google ClientSecret in configuration.");
        googleOptions.CallbackPath = new PathString("/signin-google");
    })
    .AddIdentityCookies();

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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

#endregion


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
builder.Services.AddHttpClient();


builder.Services.AddScoped<ContentFetchingService>();
//builder.Services.AddScoped<QuizBuilderService>();
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

builder.Services.AddScoped<IUserQuizAttemptRepository<UserQuizAttempt>, UserQuizAttemptRepository>();
builder.Services.AddScoped<IUserQuizAttemptQuestionRepository<UserQuizAttemptQuestion>, UserQuizAttemptQuestionRepository>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


//builder.Services.AddScoped<TopicService>();
//builder.Services.AddScoped<QuizFromTopicService>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


builder.Services.AddScoped<DashboardDataService>();

builder.Services.AddScoped<QuizApiService>();
builder.Services.AddScoped<UserQuizApiService>();
builder.Services.AddScoped<UserQuizAttemptApiService>();
builder.Services.AddScoped<UserQuizAttemptQuestionApiService>();

builder.Services.AddScoped<QuizAttemptService>();

//TODO: (Using this still?)
builder.Services.AddScoped<QuizState>();

#endregion


#region Api Support

var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
if (string.IsNullOrWhiteSpace(apiBaseAddress))
    throw new InvalidOperationException("ApiBaseAddress is not configured.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
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

app.UseCors("AllowClient");

app.Run();
