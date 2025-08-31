using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Core.DomainObjects
{
    public class Account
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
    }
}
