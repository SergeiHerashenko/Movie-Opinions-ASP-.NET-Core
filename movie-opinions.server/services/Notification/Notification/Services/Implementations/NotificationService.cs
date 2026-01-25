using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;
using MovieOpinions.Contracts.Models.ServiceResult;
using Notification.DAL.Interface;
using Notification.Models.Enum;
using Notification.Models.Notification;
using Notification.Models.Request;
using Notification.Models.Responses;
using Notification.Services.Interfaces;
using System.Text.Json;

namespace Notification.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IEnumerable<ISender> _senders;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationService(IEnumerable<ISender> senders, INotificationRepository notificationRepository, IHttpClientFactory httpClientFactory)
        {
            _senders = senders;
            _notificationRepository = notificationRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ServiceResponse<StatusCode>> SendAsync(NotificationRequest notification)
        {
            var sender = _senders.FirstOrDefault(s => s.Channel == notification.Channel);

            if(sender == null)
            {
                var errorSender = new NotificationEntity()
                {
                    Id = Guid.NewGuid(),
                    Destination = notification.Destination,
                    Channel = notification.Channel,
                    NameTemplate = notification.TemplateName,
                    Status = Models.Enum.NotificationStatus.Failed,
                    CreatedAt = DateTime.UtcNow,
                    ErrorMessage = "Неіснуючий спосіб сповіщення!"
                };

                var saveErrorSender = await _notificationRepository.CreateAsync(errorSender);

                return new ServiceResponse<StatusCode>()
                {
                    IsSuccess = false,
                    StatusCode = saveErrorSender.StatusCode,
                    Message = saveErrorSender.Message
                };
            }

            try
            {
                var templateRequest = new InternalRequest<object>()
                {
                    ClientName = "TemplateClient",
                    Endpoint = $"api/template/templates/{notification.TemplateName}",
                    Method = HttpMethod.Get
                };

                var templateResponse = await SendInternalRequest<object, ServiceResponse<NotificationContent>>(templateRequest);

                if (!templateResponse.IsSuccess)
                {
                    return new ServiceResponse<StatusCode>()
                    {
                        IsSuccess = false,
                        Message = $"Помилка шаблону {templateResponse.Message}"
                    };
                }

                var template = templateResponse.Data.Data;

                switch (notification.Channel)
                {
                    case NotificationChannel.Email:
                        var verificationRequest = new InternalRequest<Guid>()
                        {
                            ClientName = "VerificationClient",
                            Endpoint = "api/verification/token",
                            Method = HttpMethod.Post,
                            Body = notification.IdUser
                        };

                        var verificationResponse = await SendInternalRequest<Guid, VerificationResponse>(verificationRequest);

                        if (!verificationResponse.IsSuccess)
                        {
                            return new ServiceResponse<StatusCode>()
                            {
                                IsSuccess = false,
                                Message = $"Помилка верифікації {verificationResponse.Message}"
                            };
                        }

                        var verificationToken = verificationResponse.Data;

                        notification.TemplateData["URL"] = verificationToken.Data;

                        break;

                    case NotificationChannel.SMS:
                        // Логіка смс
                        break;

                    case NotificationChannel.Viber:
                        // Логіка Viber
                        break;

                    case NotificationChannel.Telegram:
                        // Логіка Telegram
                        break;
                }

                string? processedSubject = null;

                if (notification.Channel == NotificationChannel.Email)
                {
                    processedSubject = ReplacePlaceholders(template.Subject, notification.TemplateData);
                }

                string processedBody = ReplacePlaceholders(template.Body, notification.TemplateData);

                var contentText = new NotificationContent()
                {
                    Subject = processedSubject,
                    Body = processedBody
                };

                await sender.SendAsync(notification.Destination, contentText);

                return new ServiceResponse<StatusCode>
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Message = "Повідомлення відправлено!"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StatusCode>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Непередбачувальна помилка!" + ex.Message
                };
            }
        }

        private string ReplacePlaceholders(string text, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            foreach (var item in data)
            {
                text = text.Replace($"{{{item.Key}}}", item.Value);
            }
            return text;
        }

        private async Task<ServiceResponse<TResponse>> SendInternalRequest<TBody, TResponse>(InternalRequest<TBody> internalReques)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(internalReques.ClientName);
                HttpResponseMessage response;

                if (internalReques.Method == HttpMethod.Post)
                {
                    response = await client.PostAsJsonAsync(internalReques.Endpoint, internalReques.Body);
                }
                else
                {
                    response = await client.GetAsync(internalReques.Endpoint);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TResponse>();

                    return new ServiceResponse<TResponse>
                    {
                        IsSuccess = true,
                        Data = result,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var errorData = await response.Content.ReadFromJsonAsync<ServiceResult<object>>();

                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = errorData?.Message ?? response.ReasonPhrase,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = $"Критична помилка: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
