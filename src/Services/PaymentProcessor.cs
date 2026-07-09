using CloudGame.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentAPI.Data;
using PaymentAPI.Dtos;
using PaymentAPI.Models;
using PaymentAPI.Options;

namespace PaymentAPI.Services;

public sealed class PaymentProcessor(
    PaymentDbContext dbContext,
    IOptions<PaymentProcessingOptions> options)
{
    private readonly PaymentProcessingOptions _options = options.Value;

    public async Task<PaymentProcessResult> ProcessAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var orderId = request.OrderId ?? Guid.NewGuid();
        //var existingPayment = await dbContext.Payments
        //    .AsNoTracking()
        //    .FirstOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);

        //if (existingPayment is not null)
        //{
        //    return BuildResult(existingPayment);
        //}

        var status = Random.Shared.Next(100) < _options.ApprovalPercentage
            ? PaymentStatus.Approved
            : PaymentStatus.Rejected;

        var payment = Payment.Create(
            orderId,
            request.UserId,
            request.GameId,
            request.Price,
            string.IsNullOrWhiteSpace(request.Type) ? _options.DefaultPaymentType : request.Type,
            status,
            request.Installments);

        //dbContext.Payments.Add(payment);
        //await dbContext.SaveChangesAsync(cancellationToken);

        return BuildResult(payment);
    }

    private static PaymentProcessResult BuildResult(Payment payment)
    {
        return new PaymentProcessResult(
            payment,
            PaymentResponse.FromPayment(payment),
            new PaymentProcessedEvent
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Price = payment.Price,
                Status = payment.Status.ToString(),
                ProcessedAt = payment.PaymentDate
            });
    }
}
