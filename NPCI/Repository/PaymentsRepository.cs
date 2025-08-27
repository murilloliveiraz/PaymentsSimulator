using Bank.Context;
using Microsoft.EntityFrameworkCore;
using NPCI.Models;
using System;

namespace NPCI.Repository
{
    public class PaymentsRepository
    {
        protected readonly NFCIContext db;

        public PaymentsRepository(NFCIContext _db)
        {
            db = _db;
        }

        public async Task<PaymentSaga> AddAsync(PaymentSaga payment)
        {
            if (payment is null)
                throw new ArgumentNullException(nameof(payment));

            await db.Payments.AddAsync(payment);
            return payment;
        }

        public async Task<PaymentSaga> GetMemberById(string utr)
        {
            var payment = await db.Payments.FirstOrDefaultAsync(pay => pay.Utr == utr);
            if (payment is null)
                throw new InvalidOperationException("payment not found");
            return payment;

        }

        public void UpdatePayment(PaymentSaga payment)
        {
            if (payment is null)
                throw new ArgumentNullException(nameof(payment));

            db.Payments.Update(payment);
        }
    }
}
