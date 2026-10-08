namespace PaymentAdapter.Payments;

/// <summary>Target interface – the only payment API the application knows about.</summary>
public interface IPaymentProcessor
{
    PaymentResult Charge(decimal amount, string currency, string customerRef);
    PaymentResult Refund(string transactionId, decimal amount);
}
