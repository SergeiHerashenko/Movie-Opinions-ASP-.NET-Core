using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Notification.DAL.Connect_Database;
using Notification.DAL.Interface;
using Notification.Models.Enum;
using Notification.Models.Notification;
using Npgsql;

namespace Notification.DAL.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IConnectNotificationDb _connectNotificationDb; 

        public NotificationRepository(IConnectNotificationDb connectNotificationDb)
        {
            _connectNotificationDb = connectNotificationDb;
        }

        public async Task<RepositoryResponse<NotificationEntity>> CreateAsync(NotificationEntity entity)
        {
            await using (var conn = new NpgsqlConnection(_connectNotificationDb.GetConnectNotificationDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var transaction = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            await InsertNotificationTableAsync(conn, transaction, entity);

                            await transaction.CommitAsync();

                            return new RepositoryResponse<NotificationEntity>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Create.Created,
                                Message = "Запис створено!",
                                Data = entity
                            };
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();

                            return new RepositoryResponse<NotificationEntity>()
                            {
                                IsSuccess = false,
                                StatusCode = StatusCode.General.InternalError,
                                Message = ex.Message,
                            };
                        }
                    }
                }
                catch(Exception ex)
                {
                    return new RepositoryResponse<NotificationEntity>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<NotificationEntity>> DeleteAsync(Guid id)
        {
            await using (var conn = new NpgsqlConnection(_connectNotificationDb.GetConnectNotificationDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var deleteNotification = new NpgsqlCommand(
                        "DELETE FROM " +
                            "Notification_Table " +
                        "WHERE " +
                            "id_notification = @ID RETURNING *", conn))
                    {
                        deleteNotification.Parameters.AddWithValue("@ID", id);

                        await using (var readerInformation = await deleteNotification.ExecuteReaderAsync())
                        {
                            if (await readerInformation.ReadAsync())
                            {
                                var notificationEntity = MapReaderToNotification(readerInformation);

                                return new RepositoryResponse<NotificationEntity>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Сповіщення видалено!",
                                    Data = notificationEntity
                                };
                            }
                        }
                    }

                    return new RepositoryResponse<NotificationEntity>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Сповіщення не знайдено, видалення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<NotificationEntity>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public Task<RepositoryResponse<IEnumerable<NotificationEntity>>> GetByFilterAsync<TValue>(string columnName, TValue value)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse<NotificationEntity>> GetNotificationById(Guid idNotification)
        {
            await using (var conn = new NpgsqlConnection(_connectNotificationDb.GetConnectNotificationDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getNotificationCommand = new NpgsqlCommand(
                        "SELECT " +
                            "id_notification, destination, channel_notification, template_name, status, created_at, error_message, content_type " +
                        "FROM " +
                            "Notification_Table " +
                        "WHERE " +
                            "id_notification = @IdNotification", conn))
                    {
                        getNotificationCommand.Parameters.AddWithValue("@IdNotification", idNotification);

                        await using (var readerInformationNotification = await getNotificationCommand.ExecuteReaderAsync())
                        {
                            if (await readerInformationNotification.ReadAsync())
                            {
                                var notificationEntity = MapReaderToNotification(readerInformationNotification);

                                return new RepositoryResponse<NotificationEntity>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Сповіщення знайдено!",
                                    Data = notificationEntity
                                };
                            }
                        }
                    }

                    return new RepositoryResponse<NotificationEntity>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Сповіщення не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<NotificationEntity>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public Task<RepositoryResponse<NotificationEntity>> UpdateAsync(NotificationEntity entity)
        {
            throw new NotImplementedException();
        }

        private async Task InsertNotificationTableAsync(NpgsqlConnection conn, NpgsqlTransaction transaction, NotificationEntity entity)
        {
            await using (var insertUserTable = new NpgsqlCommand(
                "INSERT INTO " +
                    "Notification_Table (id_notification, destination, channel_notification, template_name, status, created_at, error_message, content_type) " +
                "VALUES " +
                    "(@Id, @Destination, @ChannelNotification, @TemplateName, @Status, NOW(), @ErrorMessage, @ContentType);", conn, transaction))
            {
                insertUserTable.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.Id;
                insertUserTable.Parameters.AddWithValue("@Destination", entity.Destination);
                insertUserTable.Parameters.AddWithValue("@ChannelNotification", entity.Channel);
                insertUserTable.Parameters.AddWithValue("@TemplateName", entity.NameTemplate);
                insertUserTable.Parameters.AddWithValue("@Status", entity.Status);
                insertUserTable.Parameters.AddWithValue("@ErrorMessage", entity.ErrorMessage ?? (object)DBNull.Value);
                insertUserTable.Parameters.AddWithValue("@ContentType", entity.ContentType);

                await insertUserTable.ExecuteNonQueryAsync();
            }
        }

        private NotificationEntity MapReaderToNotification(NpgsqlDataReader reader)
        {
            return new NotificationEntity()
            {
                Id = reader.GetGuid(reader.GetOrdinal("id_notification")),
                Destination = reader["destination"].ToString(),
                Channel = (NotificationChannel)reader["channel_notification"],
                ContentType = (ContentType)reader["content_type"],
                NameTemplate = reader["template_name"].ToString(),
                Status = (NotificationStatus)reader["status"],
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                ErrorMessage = reader["error_message"] == DBNull.Value ? null : reader["error_message"].ToString()
            };
        }
    }
}
