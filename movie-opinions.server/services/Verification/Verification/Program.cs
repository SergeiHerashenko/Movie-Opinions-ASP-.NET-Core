using Verification.DAL.Connect_Database;
using Verification.DAL.Interface;
using Verification.DAL.Repositories;
using Verification.Services.Implementations;
using Verification.Services.Interfaces;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddScoped<IVerificationService, VerificationService>();
        builder.Services.AddScoped<IVerificationRepositories, VerificationRepositories>();
        builder.Services.AddScoped<IConnectVerificationDb, ConnectVerificationDb>();

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