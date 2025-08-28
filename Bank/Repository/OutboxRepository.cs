using Bank.Context;
using BuildingBlocks.Core.Interfaces;

namespace NPCI.Repository
{
    public class OutboxRepository: IOutboxRepository
    {
        protected readonly BankContext db;

        public OutboxRepository(BankContext _db)
        {
            db = _db;
        }

        public async Task<OutboxMessage> AddAsync(OutboxMessage message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            await db.Outbox.AddAsync(message);
            await db.SaveChangesAsync();
            return message;
        }

        public async Task<IEnumerable<OutboxMessage>> GetMessagesByCorrelationIdAndStatus(string correlationId, string status)
        {
            var messages = await db.Outbox.Where(me => me.CorrelationId == correlationId).ToListAsync();
            return messages;

        }

        public async Task<IEnumerable<OutboxMessage>> GetPendingMessages()
        {
            return await db.Outbox.Where(me => me.Status == OutboxStatus.Pending).ToListAsync();
        }

        public async Task<OutboxMessage> MarkAsPublishedAsync(string messageId)
        {
            var message = await db.Outbox.FirstOrDefaultAsync(me => me.MessageId == messageId);
            if (message is null)
                throw new InvalidOperationException("message not found");

            message.Status = OutboxStatus.Sent;
            message.PublishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return message;
        }
    }
}
