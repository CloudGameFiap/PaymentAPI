namespace PaymentAPI.Dtos;

public sealed record ProcessPaymentRequest(
    int UserId,
    int GameId,
    decimal Price,
    string? Type,
    int Installments,
    Guid? OrderId = null);
