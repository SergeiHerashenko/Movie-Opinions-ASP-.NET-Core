using Authorization.DAL.Connect_Database;
using Authorization.DAL.Interface;
using Authorization.DAL.Repositories;
using Authorization.Services.Implementations;
using Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        builder.Services.AddSingleton<IConnectAuthorizationDb, ConnectAuthorizationDb>();
        builder.Services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

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
}