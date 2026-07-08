namespace PaymentAPI.Options;

public sealed class PaymentProcessingOptions
{
    public const string SectionName = "PaymentProcessing";

    public int ApprovalPercentage { get; init; } = 80;
    public string DefaultPaymentType { get; init; } = "CreditCard";
}
