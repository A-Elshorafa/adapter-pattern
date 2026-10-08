# Adapter Pattern – Payment Gateway Example (.NET 10)

## What is the Adapter pattern?

An **Adapter** lets classes with incompatible interfaces work together. It wraps an existing class (the *adaptee*) and exposes the interface the client expects (the *target*), translating calls and data in between.

Here the app wants one `IPaymentProcessor`, but the two gateways have different APIs:

| | FastPay (legacy) | GlobalPay |
|---|---|---|
| Charge | `MakePayment(int cents, cur, cardToken)` | `Authorize(GlobalPayRequest)` |
| Refund | `ReverseTransaction(txnId, cents)` | `Void(ref, total)` |
| Amount | integer cents | `double` |
| Response | XML string | `GlobalPayReply` object |

## Structure

```
Endpoint (Program.cs)
      │
      ▼
CheckoutService ──► PaymentProcessorFactory ──► IPaymentProcessor   ◄── Target
                                                  ├── FastPayAdapter   ──wraps──► FastPayLegacyApi  (Adaptee)
                                                  └── GlobalPayAdapter ──wraps──► GlobalPayApi      (Adaptee)
```

The adaptees in `Adaptees/` are never modified (imagine they are third-party SDKs). Adapters use composition and do the translation (dollars ↔ cents, XML parsing, reply mapping).

## Endpoints (Swagger at `/swagger`, port 8193)

| Method | Route | Notes |
|---|---|---|
| GET | `/api/payments/providers` | `["FastPay","GlobalPay"]` |
| POST | `/api/payments/{provider}/charge` | `{ "amount": 12.50, "currency": "USD", "customerRef": "cust1" }` → 201 |
| POST | `/api/payments/{provider}/refund` | `{ "transactionId": "...", "amount": 5 }` → 200; unknown txn → 404 |

Both providers return the same `PaymentResult` shape, even though the underlying APIs differ.

```bash
curl -X POST http://localhost:8193/api/payments/FastPay/charge \
  -H 'Content-Type: application/json' \
  -d '{"amount":12.50,"currency":"USD","customerRef":"cust1"}'
```

## Adding a provider

1. Add the third-party class to `Adaptees/`.
2. Write an adapter implementing `IPaymentProcessor`.
3. Add a `ProviderType` value, register it in DI and in `PaymentProcessorFactory`. Nothing else changes.
# adapter-pattern
