using Backend_APIs.Models;
using Backend_APIs.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Backend_APIs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var railwayPort = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrWhiteSpace(railwayPort) &&
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
            {
                builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
            }

            // Add MySQL DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            builder.Services.AddDbContext<MediaidbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.Parse("8.0.36-mysql")));

            // Add Services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IGeminiAiService, GeminiAiService>();

            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("Jwt:Key is not configured.");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            builder.Services.AddControllers();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                    return new BadRequestObjectResult(new DTOs.ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Data = null,
                        Errors = errors
                    });
                };
            });

            // Configure Swagger with JWT support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MediAI Backend API",
                    Version = "v1",
                    Description = "Healthcare Management System API"
                });

                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by a space and your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var response = new DTOs.ApiResponse<object>
                    {
                        Success = false,
                        Message = "An unexpected error occurred",
                        Data = null,
                        Errors = null
                    };

                    await context.Response.WriteAsJsonAsync(response);
                });
            });
            // REMOVE the if (app.Environment.IsDevelopment()) wrapper
            // REMOVE the if (app.Environment.IsDevelopment()) wrapper
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
              options.SwaggerEndpoint("/swagger/v1/swagger.json", "MediAI API v1");
              options.RoutePrefix = string.Empty; // This makes Swagger show up at https://mediaibackendrailway-production.up.railway.app/
            });
            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
            // {
                // app.UseSwagger();
                // app.UseSwaggerUI();
            // }

            //app.UseHttpsRedirection();

            // Enable serving static files (for profile photos)
            app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
