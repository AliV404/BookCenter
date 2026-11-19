import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { fieldErrorsOf } from '../api/client'
import { homePathFor, useAuth } from '../auth/authContext'
import { ErrorBanner } from '../components/ErrorBanner'
import { Field } from '../components/Field'
import { AuthCard } from './LoginPage'

export function RegisterPage() {
  const { user, register } = useAuth()
  const navigate = useNavigate()

  const [form, setForm] = useState({ name: '', surname: '', email: '', phone: '', password: '' })
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm({ ...form, [key]: e.target.value })
  const [error, setError] = useState<unknown>(null)
  const [submitting, setSubmitting] = useState(false)

  if (user) return <Navigate to={homePathFor(user)} replace />

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      const signedIn = await register({
        name: form.name.trim(),
        surname: form.surname.trim(),
        email: form.email.trim(),
        phone: form.phone.trim(),
        password: form.password,
      })
      navigate(homePathFor(signedIn), { replace: true })
    } catch (err) {
      setError(err)
      setSubmitting(false)
    }
  }

  const errors = fieldErrorsOf(error)

  return (
    <AuthCard
      title="Create an account"
      subtitle="Already have a reader card? Use the same email and we'll link it."
    >
      <form onSubmit={onSubmit} noValidate>
        <ErrorBanner error={error} />
        <div className="form-row">
          <Field label="First name" errors={errors.name}>
            <input className="input" autoComplete="given-name" value={form.name} onChange={set('name')} maxLength={100} required />
          </Field>
          <Field label="Last name" errors={errors.surname}>
            <input className="input" autoComplete="family-name" value={form.surname} onChange={set('surname')} maxLength={100} required />
          </Field>
        </div>
        <Field label="Email" errors={errors.email}>
          <input className="input" type="email" autoComplete="email" value={form.email} onChange={set('email')} maxLength={256} required />
        </Field>
        <Field label="Phone" errors={errors.phone}>
          <input className="input" type="tel" autoComplete="tel" value={form.phone} onChange={set('phone')} maxLength={30} required />
        </Field>
        <Field label="Password" errors={errors.password} hint="At least 8 characters, with a letter and a digit.">
          <input className="input" type="password" autoComplete="new-password" value={form.password} onChange={set('password')} maxLength={128} required />
        </Field>
        <button type="submit" className="button button--primary button--block" disabled={submitting}>
          {submitting ? 'Creating account…' : 'Create account'}
        </button>
      </form>
      <p className="auth-switch muted">
        Already registered? <Link to="/login">Sign in</Link>
      </p>
    </AuthCard>
  )
}
