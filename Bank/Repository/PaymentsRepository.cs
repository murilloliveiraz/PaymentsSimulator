using Bank.Context;
using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bank.Repository
{
    public class PaymentsRepository: IPaymentsRepository
    {
        protected readonly BankContext db;

        public PaymentsRepository(BankContext _db)
        {
            db = _db;
        }

        public async Task<Transaction> AddAsync(Transaction payment)
        {
            if (payment is null)
                throw new ArgumentNullException(nameof(payment));

            await db.Transactions.AddAsync(payment);
            await db.SaveChangesAsync();
            return payment;
        }

        public async Task<Transaction> GetPaymentByUtr(string utr)
        {
            var payment = await db.Transactions.FirstOrDefaultAsync(pay => pay.Utr == utr);
            if (payment is null)
                throw new InvalidOperationException("payment not found");
            return payment;

        }

        public async Task<Transaction> UpdateStatus(string utr, string status)
        {
            var payment = await db.Transactions.FirstOrDefaultAsync(pay => pay.Utr == utr);
            if (payment is null)
                throw new InvalidOperationException("message not found");

            payment.Status = status;
            await db.SaveChangesAsync();
            return payment;
        }
    }
}
