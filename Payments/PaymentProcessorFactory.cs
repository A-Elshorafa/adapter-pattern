using PaymentAdapter.Adapters;

namespace PaymentAdapter.Payments;

public class PaymentProcessorFactory(FastPayAdapter fastPay, GlobalPayAdapter globalPay)
{
    public IPaymentProcessor Get(ProviderType provider) => provider switch
    {
        ProviderType.FastPay => fastPay,
        ProviderType.GlobalPay => globalPay,
        _ => throw new ArgumentOutOfRangeException(nameof(provider))
    };
}
