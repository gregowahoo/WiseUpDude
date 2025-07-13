# Facebook Authentication Setup Guide

This guide explains how to configure Facebook authentication for the WiseUpDude application.

## Overview

Facebook authentication is already fully implemented in the application. You only need to configure the Facebook App credentials in the application settings.

## Prerequisites

1. A Facebook Developer account
2. A Facebook App configured for your application

## Step 1: Create a Facebook App

1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Sign in with your Facebook account
3. Click "Create App"
4. Choose "Build Connected Experiences" for the app type
5. Fill in your app details:
   - **App Name**: WiseUpDude (or your preferred name)
   - **App Contact Email**: Your email address
6. Click "Create App"

## Step 2: Configure Facebook Login

1. In your Facebook App dashboard, click "Add Product"
2. Find "Facebook Login" and click "Set Up"
3. Choose "Web" as the platform
4. Enter your site URL (e.g., `https://localhost:7150` for development)
5. In the left sidebar, click "Facebook Login" > "Settings"
6. Add your OAuth redirect URIs:
   - For development: `https://localhost:7150/signin-facebook`
   - For production: `https://yourdomain.com/signin-facebook`

## Step 3: Get Your App Credentials

1. In your Facebook App dashboard, go to "Settings" > "Basic"
2. Copy your **App ID**
3. Click "Show" next to **App Secret** and copy it

## Step 4: Configure Application Settings

### For Development (appsettings.Development.json)

Create or update `appsettings.Development.json`:

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

### For Production

Use environment variables or Azure App Settings:

- `Authentication__Facebook__AppId`
- `Authentication__Facebook__AppSecret`

### Using User Secrets (Recommended for Development)

```bash
dotnet user-secrets set "Authentication:Facebook:AppId" "your-facebook-app-id"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "your-facebook-app-secret"
```

## Step 5: Configure App Domain (Production)

1. In Facebook App Settings > Basic
2. Add your domain to "App Domains"
3. Set "Privacy Policy URL" and "Terms of Service URL"

## Step 6: Make Your App Live (Production)

1. Go to "App Review" in your Facebook App dashboard
2. Switch your app from "Development" to "Live" mode
3. Your app must comply with Facebook's policies and may require review

## Testing Authentication

1. Run your application
2. Navigate to the login page (`/Account/Login`)
3. Click "Sign in with Facebook"
4. You should be redirected to Facebook for authentication
5. After successful authentication, you'll be redirected back to your application

## Troubleshooting

### Common Issues

1. **"Invalid OAuth redirect URI"**
   - Ensure your redirect URI in Facebook App settings matches exactly: `https://yourdomain.com/signin-facebook`

2. **"App Not Setup: This app is still in development mode"**
   - Your Facebook App is in development mode and only works for developers/testers
   - Either switch to Live mode or add test users in App Roles

3. **Configuration errors**
   - Verify your AppId and AppSecret are correctly set
   - Check that the configuration keys match exactly

### Debug Mode

To see more detailed authentication errors, enable debug logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

## Security Considerations

1. **Never commit secrets to source control**
2. **Use User Secrets for development**
3. **Use secure configuration providers for production (Azure Key Vault, etc.)**
4. **Keep your Facebook App Secret confidential**
5. **Regularly rotate your App Secret**

## Additional Features

The application automatically:
- Creates user accounts for new Facebook users
- Associates Facebook logins with existing email accounts
- Assigns the "FreeSubscriber" role to new users
- Handles email confirmation requirements

## Support

For issues specific to Facebook Login setup, refer to:
- [Facebook Login Documentation](https://developers.facebook.com/docs/facebook-login/)
- [ASP.NET Core Facebook Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins)