namespace PaymentAdapter.Payments;

/// <summary>Client: depends only on the IPaymentProcessor target interface.</summary>
public class CheckoutService(PaymentProcessorFactory factory)
{
    public PaymentResult Charge(ProviderType provider, decimal amount, string currency, string customerRef)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        return factory.Get(provider).Charge(amount, currency, customerRef);
    }

    public PaymentResult Refund(ProviderType provider, string transactionId, decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        return factory.Get(provider).Refund(transactionId, amount);
    }
}
