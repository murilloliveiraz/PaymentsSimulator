namespace Bank.Repository.Interfaces
{
    public interface IRefundRepository
    {
        Task<Refund> AddAsync(Refund refund);
        Task<Refund> GetRefundByUtr(string utr);
        Task<IEnumerable<Refund>> GetAllPendingRefunds();
        Task<Refund> MarkAsDone(string utr);
        Task IncrementRetry(string utr, string? errorMessage);
        Task MarkAsDlq(string utr, string? errorMessage);
    }
}
