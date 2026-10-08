async function request(path, options) {
  const res = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })
  const text = await res.text()
  let body = text
  try { body = text ? JSON.parse(text) : null } catch { /* plain-text error */ }
  if (!res.ok) {
    // Declined payments come back as a PaymentResult; validation errors as plain text.
    const message = typeof body === 'string' ? body : body?.message
    throw new Error(message || `Request failed (${res.status})`)
  }
  return body
}

export const getProviders = () => request('/api/payments/providers')

export const charge = (provider, amount, currency, customerRef) =>
  request(`/api/payments/${provider}/charge`, {
    method: 'POST',
    body: JSON.stringify({ amount, currency, customerRef }),
  })

export const refund = (provider, transactionId, amount) =>
  request(`/api/payments/${provider}/refund`, {
    method: 'POST',
    body: JSON.stringify({ transactionId, amount }),
  })
