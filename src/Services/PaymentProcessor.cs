using CloudGameCatalog.Consumer.Consumers.PaymentApi.PaymentProcessed;
using MassTransit;
using Microsoft.Extensions.Options;
using PaymentAPI.Data;
using PaymentAPI.Dtos;
using PaymentAPI.Models;
using PaymentAPI.Options;
using static MassTransit.ValidationResultExtensions;

namespace PaymentAPI.Services;

public sealed class PaymentProcessor(
    IPublishEndpoint publishEndpoint,
    PaymentDbContext dbContext,
    IOptions<PaymentProcessingOptions> options)
{
    private readonly PaymentProcessingOptions _options = options.Value;

    public async Task<PaymentProcessResult> ProcessAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid();
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

        var result = BuildResult(payment);

        await publishEndpoint.Publish(result.Event, cancellationToken);

        return result;
    }

    private static PaymentProcessResult BuildResult(Payment payment)
    {
        return new PaymentProcessResult(
            payment,
            PaymentResponse.FromPayment(payment),
            new PaymentProcessedEvent
            {                
                UserId = payment.UserId,
                GameId = payment.GameId,                
                Status = payment.Status == PaymentStatus.Approved ? PaymentProcessStatus.PaymentApproved : PaymentProcessStatus.PaymentRejected,                
            });
    }
}
