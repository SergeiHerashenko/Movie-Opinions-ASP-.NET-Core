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

            foreach (var step in steps.OrderBy(s => s.Order))
            {
                var result = await step.ExecuteAsync(context);

                if (!result.IsSuccess)
                {
                    await RollbackHistory(context.UserId, history);
                    return result;
                }

                history.Push(step);
            }

            return new ServiceResponse
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Message = "Успіх!"
            }; 
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
