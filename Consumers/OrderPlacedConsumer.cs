using CloudGame.Contracts.Events;
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
        var message = context.Message;

        logger.LogInformation(
            "Processing order {OrderId} payment for user {UserId} and game {GameId}",
            message.OrderId,
            message.UserId,
            message.GameId);

        var request = new ProcessPaymentRequest(
            message.UserId,
            message.GameId,
            message.Price,
            null,
            1,
            message.OrderId);

        var result = await processor.ProcessAsync(request, context.CancellationToken);
        await publishEndpoint.Publish(result.Event, context.CancellationToken);

        logger.LogInformation(
            "Payment {PaymentId} processed with status {Status}",
            result.Payment.Id,
            result.Payment.Status);
    }
}
