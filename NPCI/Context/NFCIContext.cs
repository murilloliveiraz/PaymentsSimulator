using BuildingBlocks.Core.DomainObjects;
using Microsoft.EntityFrameworkCore;
using NPCI.Models;

namespace Bank.Context
{
    public class NFCIContext: DbContext
    {
        public NFCIContext(DbContextOptions<NFCIContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentSaga> Payments { get; set; }
        public DbSet<OutboxMessage> Outbox { get; set; }
    }
}
