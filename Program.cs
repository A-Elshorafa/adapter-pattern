using PaymentAdapter.Adaptees;
using PaymentAdapter.Adapters;
using PaymentAdapter.Models;
using PaymentAdapter.Payments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FastPayLegacyApi>();
builder.Services.AddSingleton<GlobalPayApi>();
builder.Services.AddSingleton<FastPayAdapter>();
builder.Services.AddSingleton<GlobalPayAdapter>();
builder.Services.AddSingleton<PaymentProcessorFactory>();
builder.Services.AddSingleton<CheckoutService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(o => o.RoutePrefix = "swagger");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapGet("/api/payments/providers", () => Enum.GetNames<ProviderType>())
    .WithSummary("List supported payment providers");

app.MapPost("/api/payments/{provider}/charge", (ProviderType provider, ChargeRequest req, CheckoutService checkout) =>
{
    try
    {
        var result = checkout.Charge(provider, req.Amount, req.Currency, req.CustomerRef);
        return result.Success ? Results.Created($"/api/payments/{provider}/{result.TransactionId}", result)
                              : Results.BadRequest(result);
    }
    catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
})
.WithSummary("Charge a customer through the chosen provider (via its adapter)");

app.MapPost("/api/payments/{provider}/refund", (ProviderType provider, RefundRequest req, CheckoutService checkout) =>
{
    try
    {
        var result = checkout.Refund(provider, req.TransactionId, req.Amount);
        if (result.Success) return Results.Ok(result);
        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
               || result.Message.Contains("Unknown", StringComparison.OrdinalIgnoreCase)
            ? Results.NotFound(result) : Results.BadRequest(result);
    }
    catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
})
.WithSummary("Refund a previous transaction through the chosen provider");

app.Run();
