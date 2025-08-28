using Bank.Context;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using NPCI.Models;
using NPCI.Repository.Interfaces;
using System;

namespace NPCI.Repository
{
    public class PaymentsRepository: IPaymentsRepository
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

        public async Task<PaymentSaga> GetPaymentByUtr(string utr)
        {
            var payment = await db.Payments.FirstOrDefaultAsync(pay => pay.Utr == utr);
            if (payment is null)
                throw new InvalidOperationException("payment not found");
            return payment;

        }

        public async Task<PaymentSaga> UpdateStatus(string utr, string status)
        {
            var payment = await db.Payments.FirstOrDefaultAsync(pay => pay.Utr == utr);
            if (payment is null)
                throw new InvalidOperationException("message not found");

            payment.Status = status;
            await db.SaveChangesAsync();
            return payment;
        }
    }
}
