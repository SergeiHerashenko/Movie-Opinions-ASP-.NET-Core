using Authorization.Application.Interfaces.Cookie;
using Authorization.Application.Interfaces.Identity;
using Authorization.Application.Interfaces.Integration;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Application.Services;
using Authorization.DAL.Context;
using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.DAL.Repositories;
using Authorization.Infrastructure.Cookie;
using Authorization.Infrastructure.Cryptography;
using Authorization.Infrastructure.ExternalServices;
using Authorization.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();

        try
        {
            Log.Information("Запуск сервісу авторизації...");

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Add services to the container.
            var profileServiceUrl = builder.Configuration["ServiceUrls:ProfileService"];
            var notificationServiceUrl = builder.Configuration["ServiceUrls:NotificationService"];
            if (string.IsNullOrEmpty(profileServiceUrl) || string.IsNullOrEmpty(notificationServiceUrl))
            {
                throw new Exception("Критична помилка: Не знайдено URL сервісів у конфігурації!");
            }

            // Клієнт для профілів
            builder.Services.AddHttpClient("ProfileClient", client =>
            {
                client.BaseAddress = new Uri(profileServiceUrl);
            });

            // Клієнт для сповіщень
            builder.Services.AddHttpClient("NotificationClient", client =>
            {
                client.BaseAddress = new Uri(notificationServiceUrl);
            });

            builder.Services.AddSingleton<IDbConnectionProvider, ConnectAuthorizationDb>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserRestrictionRepository, UserRestrictionRepository>();
            builder.Services.AddScoped<IUserDeletionRepository, UserDeletionRepository>();
            builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
            builder.Services.AddScoped<IUserPendingAccountChangesRepository, UserPendingAccountChangesRepository>();

            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
            builder.Services.AddScoped<IAccessService, AccessService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAccountService, AccountService>();

            builder.Services.AddScoped<ISendInternalRequest, SendInternalRequest>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtProvider, JwtProvider>();
            builder.Services.AddScoped<ICookieProvider, CookieProvider>();
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("jwt"))
                        {
                            context.Token = context.Request.Cookies["jwt"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("FrontendPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Сервіс несподівано зупинився!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}