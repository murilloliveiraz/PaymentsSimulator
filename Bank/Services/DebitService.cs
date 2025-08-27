using Bank.Context;
using Bank.Producers.DebitRequest;
using BuildingBlocks.Core.EventBus.Events;

namespace Bank.Services
{
    public class DebitService
    {
        private readonly BankContext _context;
        private readonly DebitRequestProducer _producer;

        public DebitService(BankContext context, DebitRequestProducer producer)
        {
            _context = context;
            _producer = producer;
        }

        public async Task HandleDebitRequest(DebitRequestEvent tx)
        {
            var payment = await _context.Transactions.FirstOrDefaultAsync(x => x.Utr == tx.Utr);
            if (payment == null || tx.Amount <= 0)
            {
                await _producer.ProduceDebitFailed(new DebitFailedEvent(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, PaymentStatuses.DebitFailed, DateTime.UtcNow));
                return;
            }

            if (payment.Status == PaymentStatuses.DebitSuccess)
                return;

            var payer = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == tx.SenderAccount);
            if (payer == null || payer.Balance < tx.Amount)
            {
                payment.Status = PaymentStatuses.DebitFailed;
                await _context.SaveChangesAsync();
                return;
            }

            payer.Balance -= tx.Amount;
            payment.Status = PaymentStatuses.DebitSuccess;

            await _context.SaveChangesAsync();
        }
    }
}