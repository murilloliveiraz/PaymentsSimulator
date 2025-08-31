using Bank.Context;
using Bank.Models.EventsResponse;
using Bank.Repository.Interfaces;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bank.Services
{
    public class RefundService
    {
        private readonly BankContext _context;
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IRefundRepository _refundRepository;
        private readonly IOutboxRepository _outboxRepository;

        public RefundService(IPaymentsRepository paymentsRepository, BankContext context, IRefundRepository refundRepository, IOutboxRepository outboxRepository)
        {
            _paymentsRepository = paymentsRepository;
            _context = context;
            _refundRepository = refundRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task<Refund> HandleRefundRequest(RefundRequestEvent @event)
        {
            var payment = await _paymentsRepository.GetPaymentByUtr(@event.Utr);

            var existingRefund = await _refundRepository.GetRefundByUtr(@event.Utr);
            if (existingRefund != null)
            {
                return existingRefund;
            }

            var refund = new Refund
            {
                Utr = @event.Utr,
                TransactionId = @event.TransactionId,
                Status = RefundStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            payment.Status = PaymentStatuses.RefundRequested;

            await _refundRepository.AddAsync(refund);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return refund;
        }

        public async Task<RefundResult> HandleRefund(Refund refund)
        {
            var payment = await _paymentsRepository.GetPaymentByUtr(refund.Utr);
            if (payment == null)
            {
                return RefundResult.Failed(refund.RefundId, refund.TransactionId, refund.Utr, "Pagamento inexistente");
            }

            if (payment.Status == PaymentStatuses.RefundSuccess)
                return RefundResult.Success(refund.RefundId, refund.TransactionId, refund.Utr);

            var payer = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == payment.SenderAccount);
            var receiver = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == payment.ReceiverAccount);

            if (payer == null || receiver == null)
            {
                return RefundResult.Failed(refund.RefundId, refund.TransactionId, refund.Utr, "Recebedor ou Pagador não existem");
            }

            var debitEvent = await _outboxRepository.GetMessagesByCorrelationIdAndStatus(refund.Utr, PaymentStatuses.DebitSuccess);
            var creditEvent = await _outboxRepository.GetMessagesByCorrelationIdAndStatus(refund.Utr, PaymentStatuses.CreditSuccess);

            using var transaction = await _context.Database.BeginTransactionAsync();

            if(debitEvent != null)
            {
                payer.Balance += payment.Amount;
            }
            if (creditEvent != null)
            {
                receiver.Balance -= payment.Amount;
            }

            payment.Status = PaymentStatuses.RefundSuccess;
            refund.Status = RefundStatus.Done;

            await _refundRepository.MarkAsDone(payment.Utr);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RefundResult.Success(refund.RefundId, refund.TransactionId, refund.Utr);
        }
    }
}