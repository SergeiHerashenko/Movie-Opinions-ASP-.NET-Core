using Authorization.Application.Interfaces.Infrastructure;
using Authorization.Domain.Enums;
using Contracts.Models.Response;
using Contracts.Models.Status;

namespace Authorization.Infrastructure.Identity
{
    public class ContactTypeDetector : IContactTypeDetector
    {
        public Result<LoginType> GetLoginType(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return new Result<LoginType>()
                {
                    IsSuccess = false,
                    Message = "Поле пусте!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }
            
            if (login.Contains("@"))
            {
                return new Result<LoginType>()
                {
                    IsSuccess = true,
                    Message = $"Тип {login}: {LoginType.Email}",
                    StatusCode = StatusCode.General.Ok,
                    Data = LoginType.Email
                };
            }

            if (long.TryParse(login.Replace("+", ""), out _))
            {
                return new Result<LoginType>()
                {
                    IsSuccess = true,
                    Message = $"Тип {login}: {LoginType.Phone}",
                    StatusCode = StatusCode.General.Ok,
                    Data = LoginType.Phone
                };
            }

            return new Result<LoginType>()
            {
                IsSuccess = false,
                Message = $"Контак типу {login}, не валідний!",
                StatusCode = StatusCode.General.BadRequest
            };
        }
    }
}
