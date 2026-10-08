using PaymentAdapter.Adaptees;
using PaymentAdapter.Payments;

namespace PaymentAdapter.Adapters;

/// <summary>Adapter: maps our method signatures to GlobalPay's request/reply objects.</summary>
public class GlobalPayAdapter(GlobalPayApi api) : IPaymentProcessor
{
    private const string Provider = "GlobalPay";

    public PaymentResult Charge(decimal amount, string currency, string customerRef) =>
        Map(api.Authorize(new GlobalPayRequest((double)amount, currency, customerRef)));

    public PaymentResult Refund(string transactionId, decimal amount) =>
        Map(api.Void(transactionId, (double)amount));

    private static PaymentResult Map(GlobalPayReply r) => new(r.Ok, r.Ref, Provider, r.Msg);
}
