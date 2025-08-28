using BuildingBlocks.Core.DomainObjects;

namespace BuildingBlocks.Core.Interfaces
{
    public interface IOutboxRepository
    {
        Task<OutboxMessage> AddAsync(OutboxMessage message);
        Task<IEnumerable<OutboxMessage>> GetMessagesByCorrelationIdAndStatus(string correlationId, string status);
        Task<IEnumerable<OutboxMessage>> GetPendingMessages();
        Task<OutboxMessage> MarkAsPublishedAsync(string correlationId);
    }
}
