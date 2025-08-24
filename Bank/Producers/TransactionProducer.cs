namespace Bank.Producers
{
    public class TransactionProducer
    {
        public async Task ProduceNewPayment(Transaction transaction)
        {
            using (var dispatcher = new KafkaDispatcher<string, Transaction>(Serializers.Utf8, new JsonSerializer<Transaction>()))
            {
                await dispatcher.SendAsync(QueueNames.GPay.InitiatePayment, transaction.Utr, transaction);
            }
        }
    }
}
