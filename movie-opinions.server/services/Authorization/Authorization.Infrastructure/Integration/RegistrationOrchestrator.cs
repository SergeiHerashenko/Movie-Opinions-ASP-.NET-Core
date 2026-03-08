using Authorization.Application.DTO.Context;
using Authorization.Application.Interfaces.Integration;
using Contracts.Model.Response;
using Contracts.Models.Status;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Integration
{
    public class RegistrationOrchestrator(
        IEnumerable<IPostRegistrationStep> steps,
        ILogger<RegistrationOrchestrator> logger) : IRegistrationOrchestrator
    {
        public async Task<ServiceResponse> RunIntegrationsAsync(RegistrationContext context)
        {
            var history = new Stack<IPostRegistrationStep>();

            var finalResult = new ServiceResponse()
            {
                IsSuccess = true,
                Message = "Кроки відсутні",
                StatusCode = StatusCode.General.NotFound
            };

            foreach (var step in steps.OrderBy(s => s.Order))
            {
                finalResult = await step.ExecuteAsync(context);

                if (!finalResult.IsSuccess)
                {
                    await RollbackHistory(context.UserId, history);
                    return finalResult;
                }

                history.Push(step);
            }

            return finalResult;
        }

        private async Task RollbackHistory(Guid userId, Stack<IPostRegistrationStep> history)
        {
            while (history.Count > 0)
            {
                var step = history.Pop();

                try 
                { 
                    await step.RollbackAsync(userId); 
                }
                catch (Exception ex) 
                { 
                    logger.LogCritical(ex, "Відкат не вдався"); 
                }
            }
        }
    }
}
