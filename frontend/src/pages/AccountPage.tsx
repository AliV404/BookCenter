import { useMutation } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { fieldErrorsOf } from '../api/client'
import { useAuth } from '../auth/authContext'
import { ErrorBanner } from '../components/ErrorBanner'
import { Field } from '../components/Field'
import { PageHeader } from '../components/PageHeader'
import { useToast } from '../lib/toast'

export function AccountPage() {
  const { user, isAdmin } = useAuth()
  if (!user) return null

  return (
    <>
      <PageHeader title="Your account" />

      <section className="card panel">
        <h2>Profile</h2>
        <dl className="details">
          <dt>Name</dt>
          <dd>{user.fullName}</dd>
          <dt>Email</dt>
          <dd>{user.email}</dd>
          <dt>Role</dt>
          <dd>{isAdmin ? 'Admin' : 'Reader'}</dd>
          <dt>Reader card</dt>
          <dd>{user.readerId !== null ? `#${user.readerId}` : <span className="muted">None</span>}</dd>
        </dl>
        <p className="muted panel-note">To change your name or email, ask library staff: your email is also your reader card's.</p>
      </section>

      <ChangePasswordForm />
    </>
  )
}

function ChangePasswordForm() {
  const { changePassword } = useAuth()
  const notify = useToast()
  const empty = { currentPassword: '', newPassword: '', confirm: '' }
  const [form, setForm] = useState(empty)
  const [mismatch, setMismatch] = useState(false)

  const save = useMutation({
    mutationFn: () => changePassword({ currentPassword: form.currentPassword, newPassword: form.newPassword }),
    onSuccess: () => {
      setForm(empty)
      notify('Password changed. You were signed out on other devices.')
    },
  })
  const errors = fieldErrorsOf(save.error)

  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm({ ...form, [key]: e.target.value })

  const submit = (event: FormEvent) => {
    event.preventDefault()
    const doesNotMatch = form.newPassword !== form.confirm
    setMismatch(doesNotMatch)
    if (!doesNotMatch) save.mutate()
  }

  return (
    <section className="card panel">
      <h2>Change password</h2>
      <form onSubmit={submit} noValidate>
        <ErrorBanner error={save.error} />
        <Field label="Current password" errors={errors.currentPassword}>
          <input
            className="input"
            type="password"
            autoComplete="current-password"
            value={form.currentPassword}
            onChange={set('currentPassword')}
            required
          />
        </Field>
        <Field label="New password" errors={errors.newPassword} hint="At least 8 characters, with a letter and a digit.">
          <input
            className="input"
            type="password"
            autoComplete="new-password"
            value={form.newPassword}
            onChange={set('newPassword')}
            maxLength={128}
            required
          />
        </Field>
        <Field label="Repeat new password" errors={mismatch ? ['The passwords don’t match.'] : undefined}>
          <input
            className="input"
            type="password"
            autoComplete="new-password"
            value={form.confirm}
            onChange={set('confirm')}
            maxLength={128}
            required
          />
        </Field>
        <button type="submit" className="button button--primary" disabled={save.isPending}>
          {save.isPending ? 'Saving…' : 'Change password'}
        </button>
      </form>
    </section>
  )
}
