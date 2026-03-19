using Microsoft.IdentityModel.Tokens;
using Profile.Application;
using Profile.Infrastructure;
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
            Log.Information("Запуск застосунку Profile Service...");

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["JwtService:Issuer"],
                        ValidAudience = builder.Configuration["JwtService:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtService:Key"] ?? throw new Exception("JWT Key missing")))
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ServiceProfileCreate", policy =>
                {
                    policy.RequireClaim("token_type", "service");
                    policy.RequireClaim("service", "authorization-service");
                    policy.RequireClaim("permission", "profile:create");
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Не вдалося запустити застосунок!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}