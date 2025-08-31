using BuildingBlocks.Core.DomainObjects;

namespace BuildingBlocks.Core.Interfaces
{
    public interface IPaymentsRepository
    {
        Task<Transaction> AddAsync(Transaction payment);
        Task<Transaction> GetPaymentByUtr(string utr);
        Task<Transaction> UpdateStatus(string utr, string status);
    }
}
