using BuildingBlocks.Core.DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace Bank.Context
{
    public class NFCIContext: DbContext
    {
        public NFCIContext(DbContextOptions<NFCIContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Payments { get; set; }
        public DbSet<OutboxMessage> Outbox { get; set; }
    }
}
