namespace CloudGame.Contracts.Events;

public sealed record OrderPlacedEvent
{
    public Guid OrderId { get; init; } = Guid.NewGuid();
    public int UserId { get; init; }
    public int GameId { get; init; }
    public decimal Price { get; init; }
    public DateTime PlacedAt { get; init; } = DateTime.UtcNow;
}
