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
            var messageEntity = new NotificationEntity()
            {
                Id = Guid.NewGuid(),
                Channel = notification.Channel,
                CreatedAt = DateTime.UtcNow,
                Destination = notification.Destination,
                NameTemplate = notification.TemplateName,
                Status = NotificationStatus.Pending,
                ContentType = notification.ContentType
            };

            var saveMessageEntity = await _notificationRepository.CreateAsync(messageEntity);

            if (!saveMessageEntity.IsSuccess)
            {
                return new ServiceResponse<StatusCode>()
                {
                    IsSuccess = false,
                    StatusCode = saveMessageEntity.StatusCode,
                    Message = "Помилка бази даних!" + saveMessageEntity.Message
                };
            }

            var sender = _senders.FirstOrDefault(s => s.Channel == notification.Channel);

            if(sender == null)
            {
                messageEntity.Status = NotificationStatus.Failed;
                messageEntity.ErrorMessage = "Неіснуючий спосіб сповіщення!";

                var saveErrorSender = await _notificationRepository.UpdateAsync(messageEntity);

                if (!saveErrorSender.IsSuccess)
                {
                    return new ServiceResponse<StatusCode>()
                    {
                        IsSuccess = false,
                        StatusCode = saveErrorSender.StatusCode,
                        Message = "Помилка бази даних!" + saveErrorSender.Message
                    };
                }

                return new ServiceResponse<StatusCode>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = "Спосіб сповіщення не знайдено!"
                };
            }

            try
            {
                var contentMessage = await PrepareNotificationContentAsync(notification, notification.IdUser);

                if(contentMessage.IsSuccess != true)
                {
                    messageEntity.Status = NotificationStatus.Failed;

                    var saveErrorMessqge = await _notificationRepository.UpdateAsync(messageEntity);

                    if (!saveErrorMessqge.IsSuccess)
                    {
                        return new ServiceResponse<StatusCode>()
                        {
                            IsSuccess = false,
                            StatusCode = saveErrorMessqge.StatusCode,
                            Message = "Помилка бази даних!" + saveErrorMessqge.Message
                        };
                    }

                    return new ServiceResponse<StatusCode>()
                    {
                        IsSuccess = false,
                        StatusCode = contentMessage.StatusCode,
                        Message = "Помилка!" + contentMessage.Message
                    };
                }

                await sender.SendAsync(notification.Destination, contentMessage.Data);

                messageEntity.Status = NotificationStatus.Sent;

                var saveSender = await _notificationRepository.UpdateAsync(messageEntity);

                if (!saveSender.IsSuccess)
                {
                    return new ServiceResponse<StatusCode>()
                    {
                        IsSuccess = false,
                        StatusCode = saveSender.StatusCode,
                        Message = "Помилка бази даних!" + saveSender.Message
                    };
                }

                return new ServiceResponse<StatusCode>
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Message = "Повідомлення відправлено!"
                };
            }
            catch (Exception ex)
            {
                messageEntity.Status = NotificationStatus.Failed;
                messageEntity.ErrorMessage = ex.Message;

                var saveExSender = await _notificationRepository.UpdateAsync(messageEntity);

                if (!saveExSender.IsSuccess)
                {
                    return new ServiceResponse<StatusCode>()
                    {
                        IsSuccess = false,
                        StatusCode = saveExSender.StatusCode,
                        Message = "Помилка бази даних!" + saveExSender.Message
                    };
                }
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

        private async Task<ServiceResponse<NotificationContent>> PrepareNotificationContentAsync(NotificationRequest notification, Guid idUser)
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
                return new ServiceResponse<NotificationContent>()
                {
                    IsSuccess = false,
                    Message = $"Помилка шаблону {templateResponse.Message}"
                };
            }

            var template = templateResponse.Data.Data;

            string endpoint = (notification.ContentType == ContentType.URL) ? "token" : "code";

            var verificationRequest = new InternalRequest<Guid>()
            {
                ClientName = "VerificationClient",
                Endpoint = $"api/verification/{endpoint}",
                Method = HttpMethod.Post,
                Body = idUser
            };

            var verificationResponse = await SendInternalRequest<Guid, VerificationResponse>(verificationRequest);

            if (!verificationResponse.IsSuccess)
            {
                return new ServiceResponse<NotificationContent>()
                {
                    IsSuccess = false,
                    Message = $"Помилка верифікації {verificationResponse.Message}"
                };
            }

            var verificationToken = verificationResponse.Data;

            notification.TemplateData[(notification.ContentType == ContentType.URL) ? "URL" : "Code"] = verificationToken.Data;

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

            return new ServiceResponse<NotificationContent>()
            {
                IsSuccess =true,
                Data = contentText,
                StatusCode = StatusCode.Create.Created,
                Message = "Повідомлення створене!"
            };
        }
    }
}
