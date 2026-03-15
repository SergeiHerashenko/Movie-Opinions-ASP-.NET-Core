using Authorization.Application.DTO.Integration;
using Authorization.Application.DTO.Integration.Responses;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Http;
using Contracts.Model.Response;
using Contracts.Models;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.ExternalServices
{
    public class ContactsIntegrationSender : IContactsSender
    {
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly ILogger<ContactsIntegrationSender> _logger;

        public ContactsIntegrationSender(ISendInternalRequest sendInternalRequest,
            ILogger<ContactsIntegrationSender> logger)
        {
            _sendInternalRequest = sendInternalRequest;
            _logger = logger;
        }

        public async Task<ServiceResponse<IEnumerable<ContactResponseDTO>>> GetUserChannelsAsync(Guid userId)
        {
            var contactsRequest = new InternalRequest<object>
            {
                ClientName = "ContactsClient",
                Endpoint = $"api/contact/get-all-contact/{userId}",
                Method = HttpMethod.Get
            };

            return await ExecuteContactsRequestAsync<object, IEnumerable<ContactResponseDTO>>(contactsRequest, userId, "Отримання контактів");
        }

        public async Task<ServiceResponse> SendCreateContactRequestAsync(ContactIntegrationDTO contactCreateIntegrationDTO)
        {
            var contactsRequest = new InternalRequest<ContactIntegrationDTO>
            {
                ClientName = "ContactsClient",
                Endpoint = "api/contact/create",
                Method = HttpMethod.Post,
                Body = contactCreateIntegrationDTO
            };
            
            return await ExecuteContactsRequestAsync<ContactIntegrationDTO, object>(contactsRequest, contactCreateIntegrationDTO.UserId, "Створення");
        }

        public async Task<ServiceResponse> SendDeleteContactRequestAsync(Guid userId)
        {
            var contactsRequest = new InternalRequest<object>
            {
                ClientName = "ContactsClient",
                Endpoint = $"api/contact/delete/{userId}",
                Method = HttpMethod.Delete
            };

            return await ExecuteContactsRequestAsync<object, object>(contactsRequest, userId, "Видалення");
        }

        public async Task<ServiceResponse> SendUpdateContactRequestAsync(ContactIntegrationDTO contactCreateIntegrationDTO)
        {
            var contactsRequest = new InternalRequest<ContactIntegrationDTO>
            {
                ClientName = "ContactsClient",
                Endpoint = "api/contact/update",
                Method = HttpMethod.Put,
                Body = contactCreateIntegrationDTO
            };

            return await ExecuteContactsRequestAsync<ContactIntegrationDTO, object>(contactsRequest, contactCreateIntegrationDTO.UserId, "Оновлення");
        }

        private async Task<ServiceResponse<TResponse>> ExecuteContactsRequestAsync<TBody, TResponse>(
            InternalRequest<TBody> request,
            Guid userId,
            string actionName)
        {
            var responseContacts = await _sendInternalRequest.SendAsync<TBody, TResponse>(request);

            if (!responseContacts.IsSuccess)
            {
                _logger.LogError("Помилка інтеграції ({Action}) для користувача {UserId}. Статус: {StatusCode}",
                    actionName,
                    userId,
                    responseContacts.StatusCode);

                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    StatusCode = responseContacts.StatusCode,
                    Message = responseContacts.Message
                };
            }

            return new ServiceResponse<TResponse>
            {
                IsSuccess = true,
                StatusCode = responseContacts.StatusCode,
                Message = $"Операція {actionName} успішна!",
                Data = responseContacts.Data
            };
        }
    }
}
