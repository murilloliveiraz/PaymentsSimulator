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
            public const string DebitFailed = "NPCI.payment.debit.failed";
            public const string CreditSuccess = "NPCI.payment.credit.success";
            public const string CreditFailed = "NPCI.payment.credit.failed";
        }

        public static class Bank
        {
            public const string DebitSuccess = "Bank.payment.debit.success";
            public const string DebitFailed = "Bank.payment.debit.failed";
            public const string CreditSuccess = "Bank.payment.credit.success";
            public const string CreditFailed = "Bank.payment.credit.failed";
            public const string RefundSuccess = "Bank.payment.refund.success";
            public const string RefundFailed = "Bank.payment.refund.failed";
            public const string DebitRequest = "Bank.payment.debit.request";
            public const string CreditRequest = "Bank.payment.credit.request";
            public const string RefundRequest = "Bank.payment.refund.request";
        }

        public static class Refund
        {
            public const string RefundRequest = "Refund.refund.request";
            public const string PaymentFailed = "Refund.payment.failed";
            public const string RefundFailed = "Refund.refund.failed";
        }
    }
}
