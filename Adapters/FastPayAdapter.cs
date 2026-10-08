using System.Xml.Linq;
using PaymentAdapter.Adaptees;
using PaymentAdapter.Payments;

namespace PaymentAdapter.Adapters;

/// <summary>Adapter: converts dollars to cents and parses the XML replies of FastPay.</summary>
public class FastPayAdapter(FastPayLegacyApi api) : IPaymentProcessor
{
    private const string Provider = "FastPay";

    public PaymentResult Charge(decimal amount, string currency, string customerRef) =>
        Parse(api.MakePayment(ToCents(amount), currency, customerRef));

    public PaymentResult Refund(string transactionId, decimal amount) =>
        Parse(api.ReverseTransaction(transactionId, ToCents(amount)), transactionId);

    private static int ToCents(decimal amount) => (int)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

    private static PaymentResult Parse(string xml, string fallbackTxn = "")
    {
        var root = XElement.Parse(xml);
        var ok = root.Element("status")?.Value == "APPROVED";
        var txn = root.Element("txn")?.Value ?? fallbackTxn;
        var message = ok ? "Approved" : root.Element("reason")?.Value ?? "Declined";
        return new(ok, txn, Provider, message);
    }
}
