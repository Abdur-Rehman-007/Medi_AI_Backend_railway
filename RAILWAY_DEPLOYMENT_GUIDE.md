# Railway Deployment Guide for Medi-AI

## Short Answer
Yes, **Railway is a good choice for the backend** of this project.

It is a good fit for:
- ASP.NET Core Web API
- MySQL database
- Environment variables and secrets
- Quick deployment for demos, testing, and FYP submission

It is **not the full solution by itself** for the Flutter mobile app:
- Railway is great for the backend API and database
- The Flutter app still needs to be built for Android/iOS/Web separately
- If you want the app fully live for users, you usually deploy the backend on Railway and then publish the mobile app or Flutter web app on another platform

## What Railway Should Host
Use Railway for:
- `Backend/Medi-AI_backend-main/Backend-APIs`
- MySQL database
- SMTP-related environment variables
- JWT and application configuration values

Do not use Railway as the only host for:
- Flutter Android APK
- Flutter iOS app
- Desktop app

## Recommended Production Setup

### Option 1: Best for a mobile app
- Backend API: Railway
- Database: Railway MySQL or Railway-managed database
- Mobile app: APK/AAB published separately

### Option 2: Best for a web demo
- Backend API: Railway
- Database: Railway MySQL
- Flutter Web: Deploy to a static host such as Firebase Hosting, Netlify, or Vercel

## Zero-to-Live Steps

### 1. Prepare the project
Before deployment, make sure:
- The backend builds locally
- The app works with your local MySQL database
- The login, OTP, appointment, and dashboard APIs work
- Sensitive values are not hardcoded in code

### 2. Create a Railway account
- Go to Railway
- Sign in with GitHub or email
- Create a new project

### 3. Push your code to GitHub
Railway deploys best from a GitHub repository.

Make sure your repo includes:
- `Backend/Medi-AI_backend-main/Backend-APIs`
- Flutter frontend code
- Database scripts if you want to initialize manually

### 4. Add the backend as a Railway service
- In Railway, choose **Deploy from GitHub Repo**
- Select your repository
- Point Railway to the backend project folder if needed
- Build the ASP.NET Core project

If Railway asks for a start command, use:
```bash
dotnet Backend-APIs.dll
```

If it needs the project path during build, use the backend project:
```bash
Backend/Medi-AI_backend-main/Backend-APIs/Backend-APIs.csproj
```

### 5. Create a database on Railway
You have two choices:
- Use Railway MySQL
- Use an external MySQL provider

For FYP simplicity, Railway MySQL is easier.

After creating the database:
- Copy the connection string
- Put it into the backend environment variables

### 6. Set environment variables on Railway
Do not hardcode secrets in `appsettings.json` for production.
Use Railway variables instead.

Set these values:

```text
ConnectionStrings__DefaultConnection=<your-railway-mysql-connection-string>
Jwt__Key=<your-strong-jwt-key>
Jwt__Issuer=MediAI-Backend
Jwt__Audience=MediAI-Users
Jwt__ExpiryInHours=24
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SenderEmail=<your-email>
EmailSettings__SenderName=MediAI Healthcare
EmailSettings__Username=<your-email>
EmailSettings__Password=<your-email-app-password>
EmailSettings__EnableSsl=true
EmailSettings__UseConsoleForDevelopment=false
```

## Important Security Notes
- Use an email **app password**, not your normal email password
- Keep the JWT key strong and private
- Do not commit real secrets to GitHub
- If possible, store secrets only in Railway variables

### 7. Update the database schema
After the backend is deployed, import your database schema.

You can do this by:
- Running your SQL script in the Railway MySQL database
- Or restoring the schema from your `.sql` file

Make sure the schema includes:
- Users
- Doctors
- Appointments
- Email verification OTP tables
- Reminders and prescriptions tables

### 8. Verify the backend URL
Once Railway deploys the backend, you will get a public URL like:
```text
https://your-backend.up.railway.app
```

Check these endpoints:
- `/swagger`
- `/api/auth/login`
- `/api/auth/register`
- `/api/auth/verify-otp`

### 9. Update the Flutter app API URL
Change the frontend API base URL from localhost to the Railway backend URL.

Example:
```dart
static const String baseUrl = 'https://your-backend.up.railway.app/api';
```

If you also publish Flutter Web, this step is required.

### 10. Test the full flow
Test in this order:
- Register a new user
- Check OTP email delivery
- Verify OTP
- Login
- Load dashboard
- Book appointment
- Open doctor dashboard
- Confirm or cancel appointment

### 11. Deploy the frontend if needed
If your final product includes Flutter Web:
- Build the web app
- Deploy it to Firebase Hosting, Netlify, or Vercel

If your final product is mobile-only:
- Build APK or AAB from Flutter
- Install on Android devices or publish to Play Store later

## Is Railway Good for This Project?

### Yes, because:
- Simple setup
- Easy environment variable management
- Good for .NET APIs
- Easy MySQL hosting
- Good for project demonstrations
- Fast deployment for FYP presentations

### Not ideal if:
- You want a complete mobile app platform in one place
- You need advanced enterprise production scaling
- You need long-term free hosting with large traffic

## Suggested Final Architecture

```text
Flutter App (Android/iOS/Web)
        ↓
Railway ASP.NET Core Backend
        ↓
Railway MySQL Database
        ↓
SMTP Email Service for OTP
```

## Common Mistakes to Avoid
- Leaving `localhost` in the frontend base URL
- Using the wrong SMTP password
- Forgetting to set `Jwt__Key`
- Forgetting to import the database schema
- Deploying the Flutter app without updating the API URL
- Using `UseConsoleForDevelopment=true` in production

## Deployment Checklist
- [ ] Backend builds locally
- [ ] GitHub repository is ready
- [ ] Railway project created
- [ ] Backend service connected
- [ ] MySQL database created
- [ ] Environment variables added
- [ ] Database schema imported
- [ ] Public backend URL tested
- [ ] Flutter API URL updated
- [ ] OTP email tested
- [ ] Login and dashboard tested

## Final Recommendation
If your goal is to make the **backend live quickly for your FYP**, Railway is a **good choice**.

If your goal is to make the **entire project live**, use:
- Railway for backend and database
- Another host for Flutter Web, or
- Android/iOS build for mobile delivery

## Quick Command Reference

```bash
cd Backend/Medi-AI_backend-main/Backend-APIs
dotnet restore
dotnet build
dotnet run
```

```bash
flutter pub get
flutter run
```

---

If you want, I can also create a second guide that shows the **exact Railway environment variable names** and the **backend startup settings** you should use.
