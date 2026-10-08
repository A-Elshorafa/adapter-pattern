import React, { useEffect, useState } from 'react'
import { getProviders, charge, refund } from './api.js'

const inputCls =
  'w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-base focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200'
// Native selects ignore padding in Safari; drop the native look and draw our own chevron.
const selectCls = `${inputCls} appearance-none bg-no-repeat pr-10`
const chevron = {
  backgroundImage:
    "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3E%3Cpath stroke='%2364748b' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3E%3C/svg%3E\")",
  backgroundPosition: 'right 0.75rem center',
  backgroundSize: '1.25rem',
}
const btnCls =
  'w-full rounded-lg bg-indigo-600 px-4 py-2.5 font-medium text-white hover:bg-indigo-700 disabled:opacity-50 sm:w-auto'

function Field({ label, children }) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-medium text-slate-600">{label}</span>
      {children}
    </label>
  )
}

function Card({ title, subtitle, children }) {
  return (
    <section className="rounded-xl bg-white p-4 shadow-sm ring-1 ring-slate-200 sm:p-6">
      <h2 className="text-lg font-semibold text-slate-800">{title}</h2>
      <p className="mb-4 text-sm text-slate-500">{subtitle}</p>
      {children}
    </section>
  )
}

export default function App() {
  const [providers, setProviders] = useState([])
  const [provider, setProvider] = useState('')
  const [charging, setCharging] = useState({ amount: '12.50', currency: 'USD', customerRef: 'cust1' })
  const [refunding, setRefunding] = useState({ transactionId: '', amount: '' })
  const [busy, setBusy] = useState(false)
  const [history, setHistory] = useState([])

  useEffect(() => {
    getProviders()
      .then((p) => { setProviders(p); setProvider(p[0] ?? '') })
      .catch((e) => log('error', 'Load providers', e.message))
  }, [])

  const log = (status, action, message, result) =>
    setHistory((h) => [{ id: crypto.randomUUID(), status, action, message, result, provider }, ...h])

  const run = async (action, fn, after) => {
    setBusy(true)
    try {
      const result = await fn()
      log('ok', action, result.message, result)
      after?.(result)
    } catch (e) {
      log('error', action, e.message)
    } finally {
      setBusy(false)
    }
  }

  const onCharge = (e) => {
    e.preventDefault()
    run('Charge', () => charge(provider, Number(charging.amount), charging.currency, charging.customerRef),
      (r) => setRefunding((f) => ({ ...f, transactionId: r.transactionId })))
  }

  const onRefund = (e) => {
    e.preventDefault()
    run('Refund', () => refund(provider, refunding.transactionId, Number(refunding.amount)))
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-6 sm:py-10">
      <header className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900 sm:text-3xl">Payment Adapter Tester</h1>
        <p className="text-sm text-slate-500">
          Same <code>IPaymentProcessor</code> interface, different gateways behind adapters.
        </p>
      </header>

      <div className="mb-6 flex flex-wrap gap-2" role="radiogroup" aria-label="Provider">
        {providers.map((p) => (
          <button
            key={p}
            role="radio"
            aria-checked={p === provider}
            onClick={() => setProvider(p)}
            className={`flex-1 rounded-lg px-4 py-2.5 text-sm font-medium ring-1 sm:flex-none ${
              p === provider ? 'bg-indigo-600 text-white ring-indigo-600' : 'bg-white text-slate-700 ring-slate-300 hover:bg-slate-50'
            }`}
          >
            {p}
          </button>
        ))}
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card title="Charge" subtitle="POST /api/payments/{provider}/charge">
          <form onSubmit={onCharge} className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <Field label="Amount">
                <input className={inputCls} type="number" step="0.01" min="0" inputMode="decimal" required
                  value={charging.amount} onChange={(e) => setCharging({ ...charging, amount: e.target.value })} />
              </Field>
              <Field label="Currency">
                <select className={selectCls} style={chevron} value={charging.currency}
                  onChange={(e) => setCharging({ ...charging, currency: e.target.value })}>
                  <option>USD</option><option>EUR</option><option>EGP</option>
                </select>
              </Field>
            </div>
            <Field label="Customer reference">
              <input className={inputCls} required value={charging.customerRef}
                onChange={(e) => setCharging({ ...charging, customerRef: e.target.value })} />
            </Field>
            <button className={btnCls} disabled={busy || !provider}>Charge with {provider}</button>
          </form>
        </Card>

        <Card title="Refund" subtitle="POST /api/payments/{provider}/refund">
          <form onSubmit={onRefund} className="space-y-3">
            <Field label="Transaction ID">
              <input className={inputCls} required placeholder="filled after a charge" value={refunding.transactionId}
                onChange={(e) => setRefunding({ ...refunding, transactionId: e.target.value })} />
            </Field>
            <Field label="Amount">
              <input className={inputCls} type="number" step="0.01" min="0" inputMode="decimal" required
                value={refunding.amount} onChange={(e) => setRefunding({ ...refunding, amount: e.target.value })} />
            </Field>
            <button className={btnCls} disabled={busy || !provider}>Refund with {provider}</button>
          </form>
        </Card>
      </div>

      <section className="mt-6">
        <h2 className="mb-3 text-lg font-semibold text-slate-800">Results</h2>
        {history.length === 0 && <p className="text-sm text-slate-500">No requests yet.</p>}
        <ul className="space-y-2">
          {history.map((h) => (
            <li key={h.id}
              className={`rounded-lg border-l-4 bg-white p-3 text-sm shadow-sm ${h.status === 'ok' ? 'border-emerald-500' : 'border-rose-500'}`}>
              <div className="flex flex-wrap items-center gap-x-2">
                <span className="font-semibold">{h.action}</span>
                <span className="rounded bg-slate-100 px-1.5 py-0.5 text-xs text-slate-600">{h.provider}</span>
                <span className={h.status === 'ok' ? 'text-emerald-700' : 'text-rose-700'}>{h.message}</span>
              </div>
              {h.result && (
                <pre className="mt-2 overflow-x-auto rounded bg-slate-50 p-2 text-xs text-slate-700">
                  {JSON.stringify(h.result, null, 2)}
                </pre>
              )}
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}
