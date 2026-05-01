# Medi-AI Backend API

**ASP.NET Core 8.0 API for Medi-AI Healthcare Platform - Optimized for Railway Deployment**

## 🏗️ Architecture

- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 9.0 + Pomelo MySQL
- **Database**: MySQL 8.0
- **Authentication**: JWT Bearer Tokens (24-hour expiry)
- **Email Service**: Gmail SMTP (MailKit)
- **API Docs**: Swagger/OpenAPI

## 📋 Prerequisites

### Local Development
- .NET 8.0 SDK or higher
- MySQL 8.0+ (localhost:3306)
- Visual Studio 2022 / VS Code

### Railway Deployment
- GitHub repository
- Railway account ([railway.app](https://railway.app))
- Gmail account with App Password configured

## 🚀 Quick Start (Local)

### 1. Clone Repository
```bash
git clone https://github.com/YOUR_USERNAME/Medi_AI_Backend_railway.git
cd Medi_AI_Backend_railway/Backend-APIs
```

### 2. Build & Run
```bash
dotnet build
dotnet run --launch-profile http
```

The API will be available at: `http://localhost:5281`
Swagger docs: `http://localhost:5281/swagger`

### 3. Import Database
```bash
mysql -u root -p < ../database/mediaidb.sql
```

## 🔐 Configuration

### Local Setup (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=mediaidb;user=root;password=YOUR_PASSWORD;"
  },
  "Jwt": {
    "Key": "your-secret-key-32-characters-minimum",
    "Issuer": "MediAI-Backend",
    "Audience": "MediAI-Users",
    "ExpiryInHours": 24
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "MediAI Healthcare",
    "Username": "your-email@gmail.com",
    "Password": "your-gmail-app-password",
    "EnableSsl": true,
    "UseConsoleForDevelopment": true
  }
}
```

### Railway Deployment (Environment Variables)
Set these in Railway dashboard:

```
ConnectionStrings__DefaultConnection=mysql://user:password@host:port/mediaidb
Jwt__Key=your-strong-32-char-secret
Jwt__Issuer=MediAI-Backend
Jwt__Audience=MediAI-Users
Jwt__ExpiryInHours=24
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SenderEmail=your-email@gmail.com
EmailSettings__SenderName=MediAI Healthcare
EmailSettings__Username=your-email@gmail.com
EmailSettings__Password=your-gmail-app-password
EmailSettings__EnableSsl=true
EmailSettings__UseConsoleForDevelopment=false
```

## 📚 API Endpoints

### Authentication
- `POST /api/Auth/register` - Register new user with OTP verification
- `POST /api/Auth/verify-otp` - Verify OTP
- `POST /api/Auth/login` - Login and get JWT token
- `GET /api/Auth/current-user` - Get authenticated user info
- `GET /api/health` - Health check

### Doctors
- `GET /api/Doctors` - List all doctors
- `GET /api/Doctors/{id}` - Get doctor details with schedules
- `GET /api/Doctors/search` - Search doctors
- `GET /api/Doctors/dashboard` - Doctor dashboard (authenticated)
- `GET /api/Doctors/today-appointments` - Today's appointments
- `GET /api/Doctors/upcoming-appointments` - Upcoming appointments

### Appointments
- `POST /api/Appointments` - Book appointment
- `GET /api/Appointments` - Get appointments (role-based)
- `PUT /api/Appointments/{id}/status` - Update appointment status
- `DELETE /api/Appointments/{id}` - Cancel appointment
- `POST /api/Appointments/{id}/prescription` - Add prescription

### Users
- `GET /api/Users/profile` - Get user profile
- `PUT /api/Users/profile` - Update profile
- `POST /api/Users/change-password` - Change password
- `POST /api/Users/upload-photo` - Upload profile photo

### Admin
- `GET /api/Admin/dashboard` - Admin dashboard statistics
- `GET /api/Admin/users` - List all users
- `DELETE /api/Admin/users/{id}` - Delete user
- `PUT /api/Admin/users/{id}/status` - Toggle user status

## 📦 Database Schema

The database includes:
- 20 tables (Users, Doctors, Appointments, etc.)
- 3 views for analytics
- 2 stored procedures for common operations
- 4 triggers for audit logging
- Seed data for testing

Import schema:
```bash
mysql -u root -p mediaidb < database/mediaidb.sql
```

## 🚄 Railway Deployment Steps

See [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md) for detailed steps.

### Quick Summary:
1. Push code to GitHub
2. Create Railway project
3. Add MySQL service
4. Connect GitHub repo to Railway
5. Set environment variables
6. Import database schema
7. Deploy and test

## 🧪 Testing

### Swagger UI
Navigate to `/swagger` on your running instance to test all endpoints interactively.

### Sample Test Flow
1. **Register**: `POST /api/Auth/register`
2. **Verify OTP**: Check email, use `POST /api/Auth/verify-otp`
3. **Login**: `POST /api/Auth/login`
4. **Use Token**: Include JWT in `Authorization: Bearer <token>` header

## 🔄 Hot Reload Development

```bash
dotnet watch run --launch-profile http
```

Press `Ctrl+R` to reload without restarting the app.

## 📋 Project Structure

```
Backend-APIs/
├── Controllers/          # API endpoint handlers
├── Models/              # EF Core DbContext & entities
├── Services/            # Business logic (Auth, Email, etc.)
├── DTOs/               # Data transfer objects
├── Properties/         # Launch settings
└── appsettings.*.json  # Configuration files
```

## 🆘 Troubleshooting

| Issue | Solution |
|-------|----------|
| Connection refused | Check MySQL is running and connection string |
| Invalid JWT key | Jwt__Key must be 32+ characters |
| Email not sending | Verify Gmail App Password (not regular password) |
| Port conflict | Use different port or restart service |
| Database not found | Import mediaidb.sql before running app |

## 📄 License

This project is part of the Medi-AI Healthcare Platform.

## 👨‍💻 Support

For deployment issues, refer to:
- [RAILWAY_BACKEND_CHANGELOG.md](./RAILWAY_BACKEND_CHANGELOG.md)
- [RAILWAY_DEPLOYMENT_GUIDE.md](./RAILWAY_DEPLOYMENT_GUIDE.md)

---

**Created**: May 2026 | **Last Updated**: May 2026 | **Status**: Ready for Railway Deployment
