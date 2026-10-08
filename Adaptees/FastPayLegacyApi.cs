using System.Collections.Concurrent;

namespace PaymentAdapter.Adaptees;

/// <summary>Adaptee: legacy gateway. Amounts in cents, responses are XML strings.</summary>
public class FastPayLegacyApi
{
    private readonly ConcurrentDictionary<string, int> _transactions = new();

    public string MakePayment(int amountInCents, string cur, string cardToken)
    {
        if (amountInCents <= 0)
            return "<response><status>DECLINED</status><reason>Invalid amount</reason></response>";

        var txnId = $"FP-{Guid.NewGuid():N}"[..11];
        _transactions[txnId] = amountInCents;
        return $"<response><status>APPROVED</status><txn>{txnId}</txn><cur>{cur}</cur></response>";
    }

    public string ReverseTransaction(string txnId, int amountInCents)
    {
        if (!_transactions.TryGetValue(txnId, out var original))
            return "<response><status>DECLINED</status><reason>Unknown transaction</reason></response>";
        if (amountInCents <= 0 || amountInCents > original)
            return "<response><status>DECLINED</status><reason>Invalid refund amount</reason></response>";

        _transactions[txnId] = original - amountInCents;
        return $"<response><status>APPROVED</status><txn>{txnId}</txn></response>";
    }
}
