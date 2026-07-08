using PaymentAPI.Models;

namespace PaymentAPI.Dtos;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    int UserId,
    int GameId,
    decimal Price,
    string Type,
    PaymentStatus Status,
    int Installments,
    DateTime PaymentDate)
{
    public static PaymentResponse FromPayment(Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.UserId,
            payment.GameId,
            payment.Price,
            payment.Type,
            payment.Status,
            payment.Installments,
            payment.PaymentDate);
    }
}
