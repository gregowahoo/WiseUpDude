# WiseUpDude - AI-Enhanced Learning and Quiz Platform

WiseUpDude is a comprehensive .NET 9.0 multi-project solution that provides an intelligent learning and quiz platform with AI integrations. The application uses Blazor WebAssembly, Entity Framework Core, ASP.NET Identity, and multiple AI services (Azure OpenAI, GitHub GPT-4o, Perplexity, Gemini).

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Bootstrap and Build the Repository
1. **Install .NET 9.0 SDK** (REQUIRED - .NET 8.0 is insufficient):
   ```bash
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 9.0 --install-dir ~/.dotnet
   export PATH="$HOME/.dotnet:$PATH"
   dotnet --version  # Should show 9.0.303 or higher
   ```

2. **Restore packages** - NEVER CANCEL: Takes 4-5 minutes on first run. Set timeout to 10+ minutes:
   ```bash
   dotnet restore
   ```

3. **Build the solution** - NEVER CANCEL: Takes 1-2 minutes (15-30 seconds after initial restore). Set timeout to 5+ minutes:
   ```bash
   dotnet build --no-restore
   ```
   - Expect ~139 warnings (this is normal)
   - Build should succeed despite warnings

4. **Complete workflow from clean state** - NEVER CANCEL: Takes ~40 seconds total. Set timeout to 2+ minutes:
   ```bash
   dotnet clean && dotnet restore && dotnet build
   ```

5. **Run tests** - NEVER CANCEL: Takes 6+ minutes. Set timeout to 15+ minutes:
   ```bash
   dotnet test --no-build --verbosity normal
   ```
   - Some database-related tests may fail (normal without proper DB setup)
   - Most tests should pass

### Run the Application
- **Main web application**:
  ```bash
  cd WiseUpDude
  dotnet run --no-build
  ```
  - Runs on https://localhost:7150
  - Will fail without proper database connection and API keys (expected)

- **Azure Functions (background tasks)** - Requires Azure Functions Core Tools:
  ```bash
  # Install Azure Functions Core Tools first (not included in this repo)
  # npm install -g azure-functions-core-tools@4 --unsafe-perm true
  cd WiseUpDude.Functions/ResourceCreatorFunction
  func start --dotnet-isolated
  ```
  - Alternative: Run directly with `dotnet run` (may have limitations)

### Install Additional Tools (if needed)
- **Entity Framework tools**:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Validation

### CRITICAL Timing and Timeout Requirements
- **NEVER CANCEL builds or long-running commands**
- **Package restore**: 4-5 minutes first time (set timeout: 10+ minutes)
- **Solution build**: 15-30 seconds after restore, 1-2 minutes from clean (set timeout: 5+ minutes) 
- **Complete workflow**: ~40 seconds from clean state (set timeout: 2+ minutes)
- **Test execution**: 6+ minutes (set timeout: 15+ minutes)
- **Always wait for completion** - builds may appear hung but are working

### Manual Validation Steps
- **Always build and test your changes** before committing
- **Database connection failures are expected** without proper configuration
- **API key missing warnings are normal** in development environment
- **~139 build warnings are normal** - focus only on errors
- **Some test failures are expected** due to database context issues

### Required Configuration (for full functionality)
The application requires several configuration items to run fully:
- SQL Server connection string (currently points to Azure SQL)
- OpenAI API key
- Azure OpenAI endpoint and key  
- GitHub AI endpoint and key
- Perplexity API key
- Google OAuth credentials
- Facebook OAuth credentials
- Tenor API key for GIFs
- Application Insights connection string

## Project Structure

### Key Projects
- **WiseUpDude**: Main web application (ASP.NET Core + Blazor)
- **WiseUpDude.Client**: Blazor WebAssembly client
- **WiseUpDude.Data**: Entity Framework data layer with repositories
- **WiseUpDude.Model**: Domain models and DTOs
- **WiseUpDude.Services**: Business logic and AI service integrations
- **WiseUpDude.Shared**: Shared components between server and client
- **WiseUpDude.Functions/ResourceCreatorFunction**: Azure Functions for background processing
- **Test Projects**: WiseUpDude.Test.Repositories, WiseUpDude.Tests.Controllers, WiseUpDude.Test.Shared

### Technology Stack
- **.NET 9.0** (minimum SDK version 9.0.303)
- **ASP.NET Core** with Blazor Server and WebAssembly modes
- **Entity Framework Core** with SQL Server
- **ASP.NET Identity** with external authentication (Google, Facebook)
- **AI Integrations**: Azure OpenAI, GitHub GPT-4o, Perplexity, Gemini APIs
- **Logging**: Serilog with Application Insights
- **Dependency Injection** throughout
- **Caching layer** for performance
- **Azure Functions** for background tasks

### Database
- **Entity Framework migrations** available in `WiseUpDude.Data/Migrations`
- **SQL scripts** available: `ApplyMigration.sql`, `QuizOfTheDayMigration.sql`, `UpdateSchema.sql`
- **Database initialization** happens automatically on startup (if connection available)

### AI Features
- **Quiz generation** from topics or prompts using multiple LLM providers
- **Content fetching** and processing from URLs
- **Learning mode** with context snippets and citations
- **Answer randomization** service
- **Multiple difficulty levels** (Easy, Medium, Hard)

## Common Tasks

### Development Workflow
1. **Always run the full build chain** before making changes:
   ```bash
   export PATH="$HOME/.dotnet:$PATH"
   dotnet restore  # 4-5 minutes
   dotnet build --no-restore  # 1-2 minutes  
   dotnet test --no-build --verbosity normal  # 6+ minutes
   ```

2. **For iterative development** (after initial build):
   ```bash
   dotnet build  # Only rebuild changed projects
   dotnet test --no-build  # Run tests on changes
   ```

3. **Run specific test projects**:
   ```bash
   dotnet test WiseUpDude.Test.Repositories --no-build
   dotnet test WiseUpDude.Tests.Controllers --no-build
   ```

### Code Navigation
- **Controllers**: `WiseUpDude/Controllers/` - API endpoints
- **Razor Components**: `WiseUpDude/Components/` - Blazor server components
- **Client Components**: `WiseUpDude.Client/` - WebAssembly components
- **Services**: `WiseUpDude.Services/` - Business logic and AI integrations
- **Repositories**: `WiseUpDude.Data/Repositories/` - Data access layer
- **Models**: `WiseUpDude.Model/` - Domain models and DTOs
- **AI Prompt Templates**: `WiseUpDude.Services/QuizPromptTemplates.cs`
- **Configuration**: `WiseUpDude/appsettings.json` and `Program.cs`

### Important Code Patterns
- **AI Service Integration**: Services are registered in DI container in `Program.cs`
- **Repository Pattern**: All data access goes through repository interfaces
- **DTO Mapping**: Entities are mapped to/from DTOs in repositories
- **Async/Await**: All data operations are asynchronous
- **Dependency Injection**: Used throughout for services and repositories
- **Learn Mode**: Special quiz mode with additional context and citations

### Troubleshooting
- **"Cannot target .NET 9.0" error**: Install .NET 9.0 SDK using provided script
- **Long restore/build times**: Normal - NEVER CANCEL, wait for completion
- **Database connection errors**: Expected without proper SQL Server setup
- **Missing API key warnings**: Normal in development without configured keys
- **Test failures**: Some database-related failures are expected
- **Build warnings (~139)**: Normal - focus only on actual errors

### Scripts and Utilities
- **`dotnet-install.sh`**: Install .NET 9.0 SDK
- **`find-duplicate-static-assets.ps1`**: Find duplicate static files
- **Launch settings**: `WiseUpDude/Properties/launchSettings.json` - configured for https://localhost:7150

## Common Reference Information

### Repository Structure (ls -la /)
```
.git
.gitattributes
.gitignore
.vscode/
ApplyMigration.sql
QuizOfTheDayMigration.sql
README.md
UpdateSchema.sql
WiseUpDude/                    # Main web application
WiseUpDude.Client/             # Blazor WebAssembly client
WiseUpDude.Data/               # Entity Framework data layer
WiseUpDude.Functions/          # Azure Functions
WiseUpDude.Model/              # Domain models and DTOs
WiseUpDude.Services/           # Business logic and AI services
WiseUpDude.Shared/             # Shared components
WiseUpDude.Test.Repositories/  # Repository tests
WiseUpDude.Test.Shared/        # Shared test utilities
WiseUpDude.Tests.Controllers/  # Controller tests
WiseUpDude.sln                 # Solution file
dotnet-install.sh             # .NET SDK installer
find-duplicate-static-assets.ps1
```

### Key Configuration Files
- `WiseUpDude/appsettings.json` - Main app configuration
- `WiseUpDude.Functions/ResourceCreatorFunction/appsettings.json` - Functions configuration
- `WiseUpDude/Properties/launchSettings.json` - Debug launch settings
- `WiseUpDude.Data/Migrations/` - Entity Framework migrations
- All `*.csproj` files - Project definitions and package references

## Validation Scenarios

After making changes, always test:

1. **Build validation**:
   ```bash
   dotnet restore && dotnet build --no-restore
   ```
   - Should complete without errors
   - Warnings are acceptable

2. **Test validation**:
   ```bash
   dotnet test --no-build --verbosity normal
   ```
   - Most tests should pass
   - Some database failures expected

3. **Application startup**:
   ```bash
   cd WiseUpDude && dotnet run --no-build
   ```
   - Should start and display startup logs
   - Database errors are expected without proper connection

4. **Key functionality areas to test**:
   - Quiz creation and management
   - User authentication flows
   - AI service integrations
   - Repository CRUD operations
   - Blazor component rendering

**CRITICAL REMINDERS:**
- **NEVER CANCEL** any build, restore, or test commands
- **Set appropriate timeouts** (10+ min for restore, 5+ min for build, 15+ min for tests)
- **Build warnings are normal** - focus on errors only
- **Some test failures are expected** without full environment setup
- **Always validate your changes** with the complete build and test cycle