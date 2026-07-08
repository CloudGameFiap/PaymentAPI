namespace PaymentAPI.Models;

public sealed class Payment
{
    private Payment()
    {
    }

    private Payment(
        Guid orderId,
        int userId,
        int gameId,
        decimal price,
        string type,
        PaymentStatus status,
        int installments,
        DateTime paymentDate)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        UserId = userId;
        GameId = gameId;
        Price = price;
        Type = type;
        Status = status;
        Installments = installments;
        PaymentDate = paymentDate;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public int UserId { get; private set; }
    public int GameId { get; private set; }
    public decimal Price { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public int Installments { get; private set; }
    public DateTime PaymentDate { get; private set; }

    public static Payment Create(
        Guid orderId,
        int userId,
        int gameId,
        decimal price,
        string type,
        PaymentStatus status,
        int installments)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
        }

        if (gameId <= 0)
        {
            throw new ArgumentException("GameId must be greater than zero.", nameof(gameId));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        if (installments <= 0)
        {
            throw new ArgumentException("Installments must be greater than zero.", nameof(installments));
        }

        return new Payment(
            orderId,
            userId,
            gameId,
            price,
            type,
            status,
            installments,
            DateTime.UtcNow);
    }
}
