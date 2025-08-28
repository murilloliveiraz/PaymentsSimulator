using Bank.Context;
using Bank.Models.EventsResponse;
using BuildingBlocks.Core.EventBus.Events;

namespace Bank.Services
{
    public class DebitService
    {
        private readonly BankContext _context;

        public DebitService(BankContext context)
        {
            _context = context;
        }

        public async Task<DebitResult> HandleDebitRequest(DebitRequestEvent tx)
        {
            var payment = await _context.Transactions.FirstOrDefaultAsync(x => x.Utr == tx.Utr);
            if (payment == null || tx.Amount <= 0)
            {
                return DebitResult.Failed(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, "Pagamento inexistente ou valor inválido");
            }

            if (payment.Status == PaymentStatuses.DebitSuccess)
                return DebitResult.Success(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount);

            if (payment.Status == PaymentStatuses.DebitFailed)
                return DebitResult.Failed(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, "Pagamento já falhou anteriormente");

            var payer = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == tx.SenderAccount);
            if (payer == null || payer.Balance < tx.Amount)
            {
                payment.Status = PaymentStatuses.DebitFailed;
                await _context.SaveChangesAsync();
                return DebitResult.Failed(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, "Saldo insuficiente");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            payer.Balance -= tx.Amount;
            payment.Status = PaymentStatuses.DebitSuccess;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return DebitResult.Success(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount);
        }
    }
}