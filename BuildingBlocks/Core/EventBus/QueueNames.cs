namespace BuildingBlocks.Core.EventBus
{
    public static class QueueNames
    {
        public static class GPay
        {
            public const string InitiatePayment = "GPay.payment.initiated";
            public const string PaymentFailed = "GPay.failed";
            public const string RefundSuccess = "GPay.refund.success";
        }

        public static class NPCI
        {
            public const string DebitRequest = "NPCI.payment.debit.request";
            public const string CreditRequest = "NPCI.payment.credit.request";
            public const string PaymentFailed = "NPCI.payment.failed";
            public const string PaymentSuccess = "NPCI.payment.success";
            public const string PaymentInitiated = "NPCI.payment.initiated";
            public const string DebitSuccess = "NPCI.payment.debit.success";
            public const string CreditSuccess = "NPCI.payment.credit.success";
        }

        public static class Bank
        {
            public const string DebitRequest = "Bank.payment.debit.request";
            public const string CreditRequest = "Bank.payment.credit.request";
            public const string RefundRequest = "Bank.payment.refund.request";
        }

        public static class Refund
        {
            public const string RefundRequest = "Refund.refund.request";
            public const string PaymentFailed = "Refund.payment.failed";
        }
    }
}
