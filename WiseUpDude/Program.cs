using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using WiseUpDude.Client.Pages;
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


#region MyBits

//Brought over from Original WUD
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITopicsCacheService<Topic>, TopicsCacheService<Topic>>();
builder.Services.AddHttpClient();


builder.Services.AddScoped<ContentFetchingService>();
//builder.Services.AddScoped<QuizBuilderService>();
builder.Services.AddScoped<QuizStateService>();
//builder.Services.AddScoped<IQuizFromPromptService, QuizFromPromptService>();
builder.Services.AddScoped<AnswerRandomizerService>();


builder.Services.AddScoped<IRepository<Quiz>, QuizRepository>();
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

#endregion


var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
if (string.IsNullOrWhiteSpace(apiBaseAddress))
    throw new InvalidOperationException("ApiBaseAddress is not configured.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddScoped<QuizApiService>();
// In both Server and WASM Program.cs
builder.Services.AddScoped<QuizState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.MapControllers();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(WiseUpDude.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
