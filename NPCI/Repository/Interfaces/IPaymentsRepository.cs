using NPCI.Models;

namespace NPCI.Repository.Interfaces
{
    public interface IPaymentsRepository
    {
        Task<PaymentSaga> AddAsync(PaymentSaga payment);
        Task<PaymentSaga> GetPaymentByUtr(string utr);
        Task<PaymentSaga> UpdateStatus(string utr, string status);
    }
}
