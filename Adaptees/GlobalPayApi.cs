using System.Collections.Concurrent;

namespace PaymentAdapter.Adaptees;

public record GlobalPayRequest(double Total, string Ccy, string Payer);
public record GlobalPayReply(bool Ok, string Ref, string Msg);

/// <summary>Adaptee: modern gateway with its own request/reply objects and naming.</summary>
public class GlobalPayApi
{
    private readonly ConcurrentDictionary<string, double> _authorizations = new();

    public GlobalPayReply Authorize(GlobalPayRequest request)
    {
        if (request.Total <= 0) return new(false, "", "Total must be positive");

        var reference = $"GP-{Guid.NewGuid():N}"[..11];
        _authorizations[reference] = request.Total;
        return new(true, reference, "Authorized");
    }

    public GlobalPayReply Void(string reference, double total)
    {
        if (!_authorizations.TryGetValue(reference, out var original))
            return new(false, reference, "Reference not found");
        if (total <= 0 || total > original)
            return new(false, reference, "Invalid void amount");

        _authorizations[reference] = original - total;
        return new(true, reference, "Voided");
    }
}
