using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Promart.Infrastructure;
using Sunedu.BuildingBlocks.EventBusBase.Abstractions;
using Sunedu.BuildingBlocks.EventBusBase.Events;
using Sunedu.BuildingBlocks.IntegrationEventLogEF;
using Sunedu.BuildingBlocks.IntegrationEventLogEF.Services;
using Sunedu.BuildingBlocks.IntegrationEventLogEF.Utilities;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Promart.Api.Application.IntegrationEvents
{
    public class PromartIntegrationEventService : IPromartIntegrationEventService
    {
        private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        private readonly PromartContext _academicoContext;
        private readonly IntegrationEventLogContext _eventLogContext;
        private readonly IIntegrationEventLogService _eventLogService;
        private readonly ILogger<PromartIntegrationEventService> _logger;

        public PromartIntegrationEventService(IEventBus eventBus,
            PromartContext academicoContext,
            IntegrationEventLogContext eventLogContext,
            Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory,
            ILogger<PromartIntegrationEventService> logger)
        {
            _academicoContext = academicoContext ?? throw new ArgumentNullException(nameof(academicoContext));
            _eventLogContext = eventLogContext ?? throw new ArgumentNullException(nameof(eventLogContext));
            _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = _integrationEventLogServiceFactory(_academicoContext.Database.GetDbConnection());
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishEventsThroughEventBusAsync(IntegrationEvent evt)
        {
            try
            {
                _logger.LogInformation("----- Publishing integration event: {IntegrationEventId_published} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);

                await _eventLogService.MarkEventAsInProgressAsync(evt.Id);
                _eventBus.Publish(evt);
                await _eventLogService.MarkEventAsPublishedAsync(evt.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
                await _eventLogService.MarkEventAsFailedAsync(evt.Id);
            }
        }

        public async Task SaveEventAndAcademicoContextChangesAsync(IntegrationEvent evt)
        {
            _logger.LogInformation("----- SoporteIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);

            //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
            //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency            
            await ResilientTransaction.New(_academicoContext).ExecuteAsync(async () =>
            {
                // Achieving atomicity between original catalog database operation and the IntegrationEventLog thanks to a local transaction
                await _academicoContext.SaveChangesAsync();
                await _eventLogService.SaveEventAsync(evt, _academicoContext.Database.CurrentTransaction.GetDbTransaction());
            });

        }
    }
}
