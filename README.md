# WiseUpDude

An intelligent learning and quiz platform that allows users to create and take quizzes from various content sources.

## Authentication Setup

WiseUpDude supports external authentication via Google and Facebook. Follow the setup instructions below to configure these providers.

### Facebook Authentication Setup

#### 1. Create a Facebook App

1. Go to the [Facebook for Developers](https://developers.facebook.com/) website
2. Click "My Apps" and then "Create App"
3. Select "Consumer" as the app type
4. Fill in the required information:
   - **App Name**: WiseUpDude (or your preferred name)
   - **App Contact Email**: Your email address
5. Click "Create App"

#### 2. Configure Facebook Login

1. In your Facebook app dashboard, click "Add Product"
2. Find "Facebook Login" and click "Set Up"
3. Select "Web" as the platform
4. Enter your site URL:
   - **Development**: `https://localhost:7150`
   - **Production**: `https://your-domain.com`
5. In the left sidebar, go to "Facebook Login" > "Settings"
6. Add the following redirect URIs:
   - **Development**: `https://localhost:7150/signin-facebook`
   - **Production**: `https://your-domain.com/signin-facebook`

#### 3. Get App Credentials

1. In the left sidebar, go to "Settings" > "Basic"
2. Copy the **App ID** and **App Secret**
3. Note: You may need to verify your app or add a privacy policy URL for production use

#### 4. Configure WiseUpDude

Add the Facebook credentials to your configuration:

**For Development (User Secrets):**
```bash
dotnet user-secrets set "Authentication:Facebook:AppId" "your-facebook-app-id"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "your-facebook-app-secret"
```

**For Production (Environment Variables):**
```bash
Authentication__Facebook__AppId=your-facebook-app-id
Authentication__Facebook__AppSecret=your-facebook-app-secret
```

**Or update appsettings.json (not recommended for production):**
```json
{
  "Authentication": {
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-app-secret"
    }
  }
}
```

### Google Authentication Setup

Follow similar steps for Google authentication:

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Create OAuth 2.0 credentials
5. Configure the redirect URIs:
   - **Development**: `https://localhost:7150/signin-google`
   - **Production**: `https://your-domain.com/signin-google`
6. Add the credentials to your configuration using the same pattern as Facebook

### Testing Authentication

1. Start the application: `dotnet run`
2. Navigate to the login page
3. Click "Login with Facebook" or "Login with Google"
4. Complete the OAuth flow
5. Verify that the user account is created and you're logged in

### Troubleshooting

**Common Issues:**

1. **Invalid redirect URI**: Ensure the redirect URIs in your Facebook/Google app match exactly
2. **App not approved**: Facebook apps may require approval for certain permissions in production
3. **HTTPS required**: OAuth providers require HTTPS. Use `dotnet dev-certs https --trust` for development
4. **Configuration not found**: Verify your app secrets are properly configured

**Debug Mode:**
Enable detailed logging to troubleshoot authentication issues:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```