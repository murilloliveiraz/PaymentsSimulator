using Bank.Context;
using Bank.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bank.Repository
{
    public class RefundRepository: IRefundRepository
    {
        protected readonly BankContext db;

        public RefundRepository(BankContext _db)
        {
            db = _db;
        }

        public async Task<Refund> AddAsync(Refund refund)
        {
            if (refund is null)
                throw new ArgumentNullException(nameof(refund));

            await db.Refund.AddAsync(refund);
            await db.SaveChangesAsync();
            return refund;
        }

        public async Task<Refund> GetRefundByUtr(string utr)
        {
            var refund = await db.Refund.FirstOrDefaultAsync(re => re.Utr == utr);
            return refund;

        }
        
        public async Task<IEnumerable<Refund>> GetAllPendingRefunds()
        {
            return await db.Refund
                .Where(r => r.Status == RefundStatus.Pending && r.NextAttemptAt <= DateTime.UtcNow && r.RetryCount < 3)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetPendingMessages()
        {
            return await db.Refund.Where(re => re.Status == RefundStatus.Pending).ToListAsync();
        }

        public async Task<Refund> MarkAsDone(string utr)
        {
            var refund = await db.Refund.FirstOrDefaultAsync(re => re.Utr == utr);
            if (refund is null)
                throw new InvalidOperationException("refund not found");

            refund.Status = RefundStatus.Done;
            refund.RefundedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return refund;
        }

        public async Task IncrementRetry(string utr, string? errorMessage)
        {
            var refund = await db.Refund.FirstOrDefaultAsync(r => r.Utr == utr);
            if (refund == null)
                return;

            refund.RetryCount += 1;
            refund.LastAttemptAt = DateTime.UtcNow;
            refund.ErrorMessage = errorMessage;

            var baseDelay = 5;
            var delayInSeconds = Math.Pow(2, refund.RetryCount) * baseDelay;
            refund.NextAttemptAt = DateTime.UtcNow.AddSeconds(delayInSeconds);

            await db.SaveChangesAsync();
        }

        public async Task MarkAsDlq(string utr, string? errorMessage)
        {
            var refund = await db.Refund.FirstOrDefaultAsync(r => r.Utr == utr);
            if (refund == null)
                return;

            refund.Status = RefundStatus.DLQ;
            refund.ErrorMessage = errorMessage;
            refund.LastAttemptAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }

    }
}
