using Authorization.Application.Interfaces.Services;
using Authorization.DAL.Interface;
using Authorization.Domain.DTO;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Services
{
    public class AccessService : IAccessService
    {
        private readonly IUserDeletionRepository _userDeletionRepository;
        private readonly IUserRestrictionRepository _userRestrictionRepository;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AccessService> _logger;

        public AccessService(IUserDeletionRepository userDeletionRepository, 
            IUserRestrictionRepository userRestrictionRepository,
            IServiceScopeFactory scopeFactory,
            ILogger<AccessService> logger)
        {
            _userDeletionRepository = userDeletionRepository;
            _userRestrictionRepository = userRestrictionRepository;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<ServiceResponse> CheckUserAccess(User entity)
        {
            if (entity.IsBlocked)
            {
                var blockDetails = await _userRestrictionRepository.GetActiveBanByUserIdAsync(entity.UserId);

                switch (blockDetails.StatusCode)
                {
                    case StatusCode.General.InternalError:

                        return new ServiceResponse()
                        {
                            IsSuccess = false,
                            StatusCode = StatusCode.General.InternalError,
                            Message = "Сервіс тимчасово не працює, спробуйте увійти пізніше!"
                        };

                    case StatusCode.General.Ok:

                        if (blockDetails.Data?.ExpiresAt == null)
                        {
                            return new ServiceResponse()
                            {
                                IsSuccess = false,
                                StatusCode = StatusCode.Auth.Locked,
                                Message = "Ваш акаунт заблоковано назавжди без права на розблокування."
                            };
                        }

                        if (blockDetails.Data?.ExpiresAt > DateTime.UtcNow)
                        {
                            return new ServiceResponse()
                            {
                                IsSuccess = false,
                                StatusCode = StatusCode.Auth.Locked,
                                Message = $"Користувач заблокований. Дата розблокування: {blockDetails.Data.ExpiresAt:dd.MM.yyyy HH:mm}"
                            };
                        }

                        _ = Task.Run(async () =>
                        {
                            // Створюємо окремий Scope для фонового потоку
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                // Отримуємо репозиторії
                                var scopedUserRepositorie = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                                var scopedRestrictionRepositorie = scope.ServiceProvider.GetRequiredService<IUserRestrictionRepository>();

                                try
                                {
                                    _logger.LogInformation("Фонове розблокування користувача {UserId} розпочато.", entity.UserId);

                                    // 1. Оновлюємо юзера
                                    entity.IsBlocked = false;
                                    await scopedUserRepositorie.UpdateAsync(entity);

                                    // 2. Деактивуємо бан (якщо він є)
                                    blockDetails.Data.IsActive = false;
                                    await scopedRestrictionRepositorie.UpdateAsync(blockDetails.Data);

                                    _logger.LogInformation("Фонове розблокування користувача {UserId} успішно завершено.", entity.UserId);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Помилка у фоновому потоці при розблокуванні {UserId}", entity.UserId);
                                }
                            }
                        });

                        return new ServiceResponse()
                        {
                            IsSuccess = true,
                            StatusCode = StatusCode.General.Ok,
                            Message = "Дійсний користувач!"
                        };

                    case StatusCode.General.NotFound:

                        _ = Task.Run(async () => {
                            try
                            {
                                using var scope = _scopeFactory.CreateScope();
                                var repositorie = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                                entity.IsBlocked = false;
                                await repositorie.UpdateAsync(entity);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Помилка при фоновому скиданні IsBlocked для {UserId}", entity.UserId);
                            }
                        });

                        return new ServiceResponse()
                        {
                            IsSuccess = true,
                            StatusCode = StatusCode.General.Ok,
                            Message = "Дійсний користувач!"
                        };

                }
            }

            if (entity.IsDeleted)
            {
                var deletedUser = await _userDeletionRepository.GetUserDeletionsByIdAsync(entity.UserId);

                if(deletedUser.StatusCode != StatusCode.General.Ok)
                {
                    return new ServiceResponse()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Сервіс тимчасово не працює, спробуйте увійти пізніше!"
                    };
                }

                _logger.LogInformation("Користувач з логіном {Email} видалено", deletedUser.Data.Email);

                return new ServiceResponse()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = $"Користувач {deletedUser.Data.Email} видалений! Причина видалення: {deletedUser.Data.Reason}"
                };
            }

            return new ServiceResponse()
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Message = "Дійсний користувач!"
            };
        }
    }
}
