using Bank.Context;
using Bank.DTOs;
using Bank.Producers.PaymentInititated;
using BuildingBlocks.Core.DomainObjects;
using BuildingBlocks.Core.EventBus.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Bank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ApiController
    {
        private readonly BankContext _context;
        private readonly TransactionProducer _producer;

        public PaymentsController(BankContext context, TransactionProducer producer)
        {
            _context = context;
            _producer = producer;
        }

        [HttpGet("{utr}")]
        public async Task<ActionResult> GetTransaction([FromRoute] string utr)
        {
            var payment = await _context.Transactions.FirstOrDefaultAsync(pay => pay.Utr == utr);
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
                LastUpdated = DateTime.UtcNow,
                Status = PaymentStatuses.Initiated,
                Utr = Guid.NewGuid().ToString()
            };

            await _context.Transactions.AddAsync(newPayment);
            var response = await _context.SaveChangesAsync() > 0;

            if (!response)
                return CustomResponse((int)HttpStatusCode.BadRequest, false);

            var @event = new PaymentInitiatedEvent(newPayment.TransactionId, newPayment.Utr, newPayment.SenderAccount, newPayment.ReceiverAccount, newPayment.Amount, newPayment.Status, DateTime.UtcNow);

            await _producer.ProduceNewPayment(@event);

            return CustomResponse((int)HttpStatusCode.OK, true, newPayment);
        }
    }
}
