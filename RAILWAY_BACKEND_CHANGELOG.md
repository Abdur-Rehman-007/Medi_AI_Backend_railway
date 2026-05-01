# Railway Backend Changes

This file summarizes the backend changes made to prepare the Medi-AI API for Railway deployment.

## What changed

### 1. Railway port support
- The backend now reads the Railway `PORT` environment variable.
- If `ASPNETCORE_URLS` is not already set, the app binds to `http://0.0.0.0:<PORT>` so Railway can route traffic correctly.

### 2. Safer database configuration
- The `DbContext` no longer overrides the injected connection string when it is already configured by ASP.NET Core.
- This prevents the local scaffolded MySQL connection from replacing the Railway connection string.

### 3. Secret cleanup for GitHub
- Checked-in `appsettings.json` and `appsettings.Development.json` no longer contain real secrets.
- They now use placeholder values so the repo is safe to push to GitHub.

### 4. Swagger behavior
- Swagger still works locally.
- It can also be enabled from configuration with `EnableSwagger`.

## Files changed

### Backend startup
- [Backend/Medi-AI_backend-main/Backend-APIs/Program.cs](Backend/Medi-AI_backend-main/Backend-APIs/Program.cs)

### Database context
- [Backend/Medi-AI_backend-main/Backend-APIs/Models/MediaidbContext.cs](Backend/Medi-AI_backend-main/Backend-APIs/Models/MediaidbContext.cs)

### Configuration
- [Backend/Medi-AI_backend-main/Backend-APIs/appsettings.json](Backend/Medi-AI_backend-main/Backend-APIs/appsettings.json)
- [Backend/Medi-AI_backend-main/Backend-APIs/appsettings.Development.json](Backend/Medi-AI_backend-main/Backend-APIs/appsettings.Development.json)

## Validation

- `dotnet build` passed successfully after the changes.

## Railway deployment values to set

Set these environment variables in Railway:

```text
ConnectionStrings__DefaultConnection=<your-railway-mysql-connection-string>
Jwt__Key=<your-strong-jwt-secret>
Jwt__Issuer=MediAI-Backend
Jwt__Audience=MediAI-Users
Jwt__ExpiryInHours=24
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SenderEmail=<your-sender-email>
EmailSettings__SenderName=MediAI Healthcare
EmailSettings__Username=<your-sender-email>
EmailSettings__Password=<your-email-app-password>
EmailSettings__EnableSsl=true
EmailSettings__UseConsoleForDevelopment=false
```

## Next step

Push the backend folder to GitHub, then connect Railway to the repo and import `database/mediaidb.sql` into the Railway MySQL database.