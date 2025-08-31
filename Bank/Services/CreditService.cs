using Bank.Context;
using Bank.Models.EventsResponse;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;

namespace Bank.Services
{
    public class CreditService
    {
        private readonly BankContext _context;
        private readonly IPaymentsRepository _paymentsRepository;

        public CreditService(IPaymentsRepository paymentsRepository, BankContext context)
        {
            _paymentsRepository = paymentsRepository;
            _context = context;
        }

        public async Task<CreditResult> HandleCreditRequest(CreditRequestEvent tx)
        {
            var payment = await _paymentsRepository.GetPaymentByUtr(tx.Utr);
            if (payment == null || tx.Amount <= 0)
            {
                return CreditResult.Failed(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, "Pagamento inexistente ou valor inválido");
            }

            if (payment.Status == PaymentStatuses.CreditSuccess)
                return CreditResult.Success(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount);

            if (payment.Status == PaymentStatuses.CreditFailed)
                return CreditResult.Failed(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, "Pagamento já falhou anteriormente");

            var receiver = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == tx.ReceiverAccount);
            if (receiver == null)
            {
                payment.Status = PaymentStatuses.CreditFailed;
                await _context.SaveChangesAsync();
                return CreditResult.Failed(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, "Recebedor não existe");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            receiver.Balance += tx.Amount;
            payment.Status = PaymentStatuses.CreditSuccess;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreditResult.Success(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount);
        }
    }
}