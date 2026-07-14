using CloudGameCatalog.Consumer.Consumers.PaymentApi.PaymentProcessed;
using PaymentAPI.Dtos;
using PaymentAPI.Models;

namespace PaymentAPI.Services;

public sealed record PaymentProcessResult(
    Payment Payment,
    PaymentResponse Response,
    PaymentProcessedEvent Event);
