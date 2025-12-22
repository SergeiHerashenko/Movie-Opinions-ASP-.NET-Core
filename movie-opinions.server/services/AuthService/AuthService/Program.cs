using AuthService.DAL.Connect_Database;
using AuthService.DAL.Interface;
using AuthService.DAL.Repositories;
using AuthService.Services.Implementations;
using AuthService.Services.Interfaces;
using System.Net.Http.Headers;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var profileServiceUrl = builder.Configuration["ServiceUrls:ProfileService"];
        if (string.IsNullOrEmpty(profileServiceUrl))
        {
            throw new Exception("Критична помилка: Не знайдено URL для ProfileService у конфігурації!");
        }

        builder.Services.AddSingleton<IConnectAuthDb, ConnectAuthDb>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();

        builder.Services.AddHttpClient<IAuthService, AuthServiceImplementations>(client =>
        {
            client.BaseAddress = new Uri(profileServiceUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}