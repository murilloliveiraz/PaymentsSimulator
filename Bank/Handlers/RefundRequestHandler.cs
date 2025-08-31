using Bank.Repository.Interfaces;
using Bank.Services;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bank.Consumers
{
    public class RefundRequestHandler : IEventHandler<RefundRequestEvent>
    {
        private readonly ILogger<RefundRequestEvent> _logger;
        private readonly RefundService _refundService;

        public RefundRequestHandler(ILogger<RefundRequestEvent> logger, RefundService refundService)
        {
            _logger = logger;
            _refundService = refundService;
        }

        public async Task HandleAsync(RefundRequestEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received  RefundRequestEvent [TransactionId={TransactionId}, UTR={UTR}]", @event.TransactionId, @event.Utr);

            var result = await _refundService.HandleRefundRequest(@event);
        }
    }
}
