namespace CloudGame.Contracts.Events;

public sealed record PaymentProcessedEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public int UserId { get; init; }
    public int GameId { get; init; }
    public decimal Price { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}
