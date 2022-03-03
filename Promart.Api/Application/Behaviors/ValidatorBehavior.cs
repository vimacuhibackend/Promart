using Promart.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBusBase.Extensions;

namespace Promart.Api.Application.Behaviors
{
    public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;
        private readonly IValidator<TRequest>[] _validators;

        public ValidatorBehavior(IValidator<TRequest>[] validators, ILogger<ValidatorBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var typeName = typeof(TRequest).Name;

            _logger.LogInformation("----- Validando el commando {CommandType}", typeName);

            var failures = _validators
                .Select(v => v.Validate(request))
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (failures.Any())
            {
                _logger.LogWarning("Validación de errores - {CommandType} - Command: {@Command} - Errors: {@ValidationErrors}", typeName, request, failures);

                throw new ClienteDomainException(
                    $"Commando de Validación por tipo de Errores {typeof(TRequest).Name}", new ValidationException("Validation exception", failures));
            }

            return await next();
        }
    }
}
