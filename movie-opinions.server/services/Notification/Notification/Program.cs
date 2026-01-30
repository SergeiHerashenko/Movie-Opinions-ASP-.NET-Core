using Notification.DAL.Connect_Database;
using Notification.DAL.Interface;
using Notification.DAL.Repositories;
using Notification.Services.Implementations;
using Notification.Services.Interfaces;
using Notification.Services.Senders;
using System.Text.Json.Serialization;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var templateServiceUrl = builder.Configuration["ServiceUrls:TemplateService"];
        var verificationServiceUrl = builder.Configuration["ServiceUrls:VerificationService"];

        if (string.IsNullOrEmpty(templateServiceUrl) || string.IsNullOrEmpty(verificationServiceUrl))
        {
            throw new Exception("Критична помилка: Не знайдено URL сервісів у конфігурації!");
        }

        // Клієнт для шаблонів
        builder.Services.AddHttpClient("TemplateClient", client =>
        {
            client.BaseAddress = new Uri(templateServiceUrl);
        });

        // Клієнт для верифікації
        builder.Services.AddHttpClient("VerificationClient", client =>
        {
            client.BaseAddress = new Uri(verificationServiceUrl);
        });

        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<IConnectNotificationDb, ConnectNotificationDb>();
        builder.Services.AddScoped<ISender, EmailSender>();
        builder.Services.AddScoped<ISender, SmsSender>();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

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