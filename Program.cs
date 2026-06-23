using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Consumers;
using PaymentAPI.Data;
using PaymentAPI.Dtos;
using PaymentAPI.Options;
using PaymentAPI.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<PaymentProcessingOptions>(builder.Configuration.GetSection(PaymentProcessingOptions.SectionName));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CloudGameDb");
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<PaymentProcessor>();

builder.Services.AddMassTransit(bus =>
{
    bus.SetKebabCaseEndpointNameFormatter();
    bus.AddConsumer<OrderPlacedConsumer>();

    bus.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqOptions = builder.Configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.VirtualHost, host =>
        {
            host.Username(rabbitMqOptions.Username);
            host.Password(rabbitMqOptions.Password);
        });

        cfg.ReceiveEndpoint(rabbitMqOptions.OrderPlacedQueue, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderPlacedConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health/live", () => Results.Ok(new { Status = "Alive" }));

app.MapGet("/health/ready", async (
    PaymentDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new { Status = "Healthy" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/health", () => Results.Ok(new { Status = "Alive" }));

app.MapGet("/", () => Results.Ok(new
{
    Service = "PaymentsAPI",
    Status = "Running"
}));

app.MapPost("/payments/process", async (
    ProcessPaymentRequest request,
    PaymentProcessor processor,
    IPublishEndpoint publishEndpoint,
    CancellationToken cancellationToken) =>
{
    if (request.UserId <= 0 || request.GameId <= 0 || request.Price <= 0 || request.Installments <= 0)
    {
        return Results.BadRequest(new
        {
            Message = "UserId, GameId, Price and Installments must be greater than zero."
        });
    }

    var result = await processor.ProcessAsync(request, cancellationToken);
    await publishEndpoint.Publish(result.Event, cancellationToken);

    return Results.Created($"/payments/{result.Payment.Id}", result.Response);
})
.WithName("ProcessPayment");

app.MapGet("/payments/{id:guid}", async (
    Guid id,
    PaymentDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var payment = await dbContext.Payments
        .AsNoTracking()
        .Where(payment => payment.Id == id)
        .Select(payment => new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.UserId,
            payment.GameId,
            payment.Price,
            payment.Type,
            payment.Status,
            payment.Installments,
            payment.PaymentDate))
        .FirstOrDefaultAsync(cancellationToken);

    return payment is null ? Results.NotFound() : Results.Ok(payment);
})
.WithName("GetPaymentById");

app.MapGet("/payments", async (
    PaymentDbContext dbContext,
    int? userId,
    CancellationToken cancellationToken) =>
{
    var query = dbContext.Payments.AsNoTracking();

    if (userId is > 0)
    {
        query = query.Where(payment => payment.UserId == userId);
    }

    var payments = await query
        .OrderByDescending(payment => payment.PaymentDate)
        .Select(payment => new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.UserId,
            payment.GameId,
            payment.Price,
            payment.Type,
            payment.Status,
            payment.Installments,
            payment.PaymentDate))
        .ToListAsync(cancellationToken);

    return Results.Ok(payments);
})
.WithName("ListPayments");

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await WaitForDatabaseAsync(dbContext);
}

app.Run();

static async Task WaitForDatabaseAsync(PaymentDbContext dbContext)
{
    const int maxAttempts = 10;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                return;
            }
        }
        catch when (attempt < maxAttempts)
        {
        }

        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}
