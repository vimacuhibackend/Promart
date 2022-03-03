using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Promart.Api.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _logger.LogInformation("----- Controlador (Handling) de comando {CommandName} ({@Command})", typeof(TRequest).Name, request);
            var response = await next();
            _logger.LogInformation("----- Comando {CommandName} control - respuesta: {@Response}", typeof(TRequest).Name, response);

            return response;
        }
    }
}
