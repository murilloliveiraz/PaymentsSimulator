using GPay.PaymentInitiated;

namespace GPay
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly PaymentInitiatedConsumer _paymentInitiatedConsumer;

        public Worker(ILogger<Worker> logger, PaymentInitiatedConsumer paymentInitiatedConsumer)
        {
            _logger = logger;
            _paymentInitiatedConsumer = paymentInitiatedConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = new List<Task>
                {
                    _paymentInitiatedConsumer.Start(stoppingToken)
                };

                await Task.WhenAll(tasks);
            }
        }
    }
}
