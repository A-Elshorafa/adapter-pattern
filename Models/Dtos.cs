namespace PaymentAdapter.Models;

public record ChargeRequest(decimal Amount, string Currency, string CustomerRef);
public record RefundRequest(string TransactionId, decimal Amount);
