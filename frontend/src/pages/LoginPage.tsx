import { useState, type FormEvent, type ReactNode } from 'react'
import { Link, Navigate, useLocation, useNavigate, type Location } from 'react-router-dom'
import { fieldErrorsOf } from '../api/client'
import { homePathFor, useAuth } from '../auth/authContext'
import { ErrorBanner } from '../components/ErrorBanner'
import { Field } from '../components/Field'

export function AuthCard({ title, subtitle, children }: { title: string; subtitle: string; children: ReactNode }) {
  return (
    <div className="auth-page">
      <div className="card auth-card">
        <div className="brand auth-brand">
          <img src="/favicon.svg" alt="" width={32} height={32} />
          <span>BookCenter</span>
        </div>
        <h1>{title}</h1>
        <p className="muted auth-subtitle">{subtitle}</p>
        {children}
      </div>
    </div>
  )
}

export function LoginPage() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: Location } | null)?.from

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<unknown>(null)
  const [submitting, setSubmitting] = useState(false)

  if (user) return <Navigate to={homePathFor(user)} replace />

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      const signedIn = await login({ email: email.trim(), password })
      navigate(from ? `${from.pathname}${from.search}` : homePathFor(signedIn), { replace: true })
    } catch (err) {
      setError(err)
      setSubmitting(false)
    }
  }

  const errors = fieldErrorsOf(error)

  return (
    <AuthCard title="Sign in" subtitle="Welcome back to the library.">
      <form onSubmit={onSubmit} noValidate>
        <ErrorBanner error={error} />
        <Field label="Email" errors={errors.email}>
          <input className="input" type="email" autoComplete="username" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </Field>
        <Field label="Password" errors={errors.password}>
          <input className="input" type="password" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </Field>
        <button type="submit" className="button button--primary button--block" disabled={submitting}>
          {submitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
      <p className="auth-switch muted">
        New reader? <Link to="/register">Create an account</Link>
      </p>
    </AuthCard>
  )
}
