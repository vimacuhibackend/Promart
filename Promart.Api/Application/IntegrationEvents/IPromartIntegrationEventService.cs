using Sunedu.BuildingBlocks.EventBusBase.Events;
using System.Threading.Tasks;

namespace Promart.Api.Application.IntegrationEvents
{
    public interface IPromartIntegrationEventService
    {
        Task PublishEventsThroughEventBusAsync(IntegrationEvent evt);
        Task SaveEventAndAcademicoContextChangesAsync(IntegrationEvent evt);
    }
}
