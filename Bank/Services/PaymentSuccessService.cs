using Bank.Context;
using Bank.Models.EventsResponse;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;

namespace Bank.Services
{
    public class PaymentSuccessService
    {
        private readonly BankContext _context;
        private readonly IPaymentsRepository _paymentsRepository;

        public PaymentSuccessService(IPaymentsRepository paymentsRepository, BankContext context)
        {
            _paymentsRepository = paymentsRepository;
            _context = context;
        }

        public async Task<PaymentSuccessResult> HandlePaymentSuccess(PaymentSuccessEvent tx)
        {
            var payment = await _paymentsRepository.GetPaymentByUtr(tx.Utr);
            if (payment == null)
            {
                return PaymentSuccessResult.Failed(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount, "Pagamento inexistente");
            }

            if (payment.Status == PaymentStatuses.PaymentSuccess)
                return PaymentSuccessResult.Success(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount);

            using var transaction = await _context.Database.BeginTransactionAsync();

            payment.Status = PaymentStatuses.PaymentSuccess;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return PaymentSuccessResult.Success(tx.TransactionId, tx.Utr, tx.SenderAccount, tx.ReceiverAccount, tx.Amount);
        }
    }
}