using Bank.Context;
using Bank.DTOs;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using System.Net;
using System.Text.Json;

namespace Bank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ApiController
    {
        private readonly BankContext _context;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IPaymentsRepository _paymentsRepository;

        public PaymentsController(BankContext context, IOutboxRepository outboxRepository, IPaymentsRepository paymentsRepository)
        {
            _context = context;
            _outboxRepository = outboxRepository;
            _paymentsRepository = paymentsRepository;
        }

        [HttpGet("{utr}")]
        public async Task<ActionResult> GetTransaction([FromRoute] string utr)
        {
            var payment = await _paymentsRepository.GetPaymentByUtr(utr);

            if (payment is null)
                return CustomResponse((int)HttpStatusCode.NotFound, false);

            return CustomResponse((int)HttpStatusCode.OK, true, payment);
        }

        [HttpPost("")]
        public async Task<ActionResult<Account>> CreateTransaction([FromBody] PaymentRequest payment)
        {
            var senderAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == payment.SenderAccount);
            if (senderAccount == null)
                return CustomResponse((int)HttpStatusCode.BadRequest, false, "Sender account not found");

            if (senderAccount.Balance < payment.Amount)
                return CustomResponse((int)HttpStatusCode.BadRequest, false, "Insufficient funds");

            var newPayment = new Transaction
            {
                SenderAccount = payment.SenderAccount,
                ReceiverAccount = payment.ReceiverAccount,
                Amount = payment.Amount,
                CreatedAt = DateTime.UtcNow,
                Status = PaymentStatuses.Initiated,
                Utr = Guid.NewGuid().ToString()
            };

            var response = await _paymentsRepository.AddAsync(newPayment);

            if (response is null)
                return CustomResponse((int)HttpStatusCode.BadRequest, false);

            var @event = new PaymentInitiatedEvent(response.TransactionId, response.Utr, response.SenderAccount, response.ReceiverAccount, response.Amount, response.Status, DateTime.UtcNow);
            
            var outboxEvent = new OutboxMessage
            {
                CorrelationId = @event.Utr,
                Topic = QueueNames.GPay.InitiatePayment,
                EventType = nameof(PaymentInitiatedEvent),
                Payload = JsonSerializer.Serialize(@event),
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _outboxRepository.AddAsync(outboxEvent);

            return CustomResponse((int)HttpStatusCode.OK, true, response);
        }
    }
}
