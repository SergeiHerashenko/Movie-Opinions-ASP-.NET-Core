using AuthService.DAL.Connect_Database;
using AuthService.Services.Interfaces;
using AuthService.Services.Implementations;
using AuthService.DAL.Interface;
using AuthService.DAL.Repositories;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSingleton<IConnectAuthDb, ConnectAuthDb>();
        builder.Services.AddScoped<IAuthService, AuthServiceImplementations>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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