using CloudGameCatalog.Domain.Commom.Events;
using MassTransit;
using PaymentAPI.Dtos;
using PaymentAPI.Services;

namespace PaymentAPI.Consumers;

public sealed class OrderPlacedConsumer(
    PaymentProcessor processor,
    IPublishEndpoint publishEndpoint,
    ILogger<OrderPlacedConsumer> logger) : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        logger.LogInformation("OrderPlacedEvent received.");
        var message = context.Message;

        logger.LogInformation(
            "Processing payment for user {UserId} and game {GameId}",            
            message.UserId,
            message.GameId);

        var request = new ProcessPaymentRequest(
            message.UserId,
            message.GameId,
            message.Price,
            null,
            1);

        var result = await processor.ProcessAsync(request, context.CancellationToken);
        await publishEndpoint.Publish(result.Event, context.CancellationToken);

        logger.LogInformation(
            "Payment for user {UserId} and game {GameId} processed with status {Status}",
            message.UserId,
            message.GameId,
            result.Payment.Status);

        logger.LogInformation("OrderPlacedEvent processed.");
    }
}
