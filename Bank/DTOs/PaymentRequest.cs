namespace Bank.DTOs
{
    public class PaymentRequest
    {
        public string SenderAccount { get; set; }
        public string ReceiverAccount { get; set; }
        public decimal Amount { get; set; }
    }
}
