using BuildingBlocks.Core.DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace Bank.Context
{
    public class BankContext: DbContext
    {
        public BankContext(DbContextOptions<BankContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<OutboxMessage> Outbox { get; set; }
    }
}
