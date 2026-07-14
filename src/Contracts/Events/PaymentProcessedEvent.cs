namespace CloudGameCatalog.Consumer.Consumers.PaymentApi.PaymentProcessed;

public class PaymentProcessedEvent
{
    public int GameId { get; set; }

    public int UserId { get; set; }

    public PaymentProcessStatus Status { get; set; }
}

public enum PaymentProcessStatus
{
    WaitingPayment = 1,
    PaymentApproved = 2,
    PaymentRejected = 3,
}
