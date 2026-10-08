namespace PaymentAdapter.Payments;

public record PaymentResult(bool Success, string TransactionId, string Provider, string Message);
