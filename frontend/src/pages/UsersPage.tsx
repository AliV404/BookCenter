import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { fieldErrorsOf } from '../api/client'
import { usersApi } from '../api/endpoints'
import type { Role, UserAccount } from '../api/types'
import { useAuth } from '../auth/authContext'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { Field } from '../components/Field'
import { FormModal } from '../components/FormModal'
import { PageHeader } from '../components/PageHeader'
import { Pagination } from '../components/Pagination'
import { SearchInput } from '../components/SearchInput'
import { TableStatus } from '../components/TableStatus'
import { useDebouncedValue } from '../hooks/useDebouncedValue'
import { useSaveMutation } from '../hooks/useSaveMutation'

const TABS: { role: Role | undefined; label: string }[] = [
  { role: undefined, label: 'All' },
  { role: 'Admin', label: 'Admins' },
  { role: 'Reader', label: 'Readers' },
]

type ActionKind = 'promote' | 'demote' | 'lock' | 'unlock'

interface PendingAction {
  kind: ActionKind
  user: UserAccount
}

const isAdmin = (user: UserAccount) => user.roles.includes('Admin')

/** An admin lock is stored as "locked until the end of time"; failed logins lock for minutes. */
const isLockedByAdmin = (lockedUntil: string) => new Date(lockedUntil).getFullYear() > 9000

function StatusBadge({ user }: { user: UserAccount }) {
  if (user.lockedUntil) {
    return isLockedByAdmin(user.lockedUntil) ? (
      <span className="badge badge--danger">Locked</span>
    ) : (
      <span className="badge badge--danger" title="Too many failed sign-in attempts">
        Locked until {new Date(user.lockedUntil).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
      </span>
    )
  }
  if (!isAdmin(user) && user.readerId === null) {
    return (
      <span className="badge badge--muted" title="The reader card was deleted, so this account can't sign in">
        No card
      </span>
    )
  }
  return <span className="badge badge--ok">Active</span>
}

export function UsersPage() {
  const { user: me } = useAuth()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [role, setRole] = useState<Role | undefined>(undefined)
  const debouncedSearch = useDebouncedValue(search)
  const [action, setAction] = useState<PendingAction | null>(null)
  const [resetting, setResetting] = useState<UserAccount | null>(null)

  const params = { page, pageSize: 10, search: debouncedSearch, role }
  const list = useQuery({ queryKey: ['users', params], queryFn: () => usersApi.list(params), placeholderData: keepPreviousData })

  return (
    <>
      <PageHeader
        title="Users"
        subtitle="Sign-in accounts. Readers create their own by registering; edit names and emails on the Readers page."
      />

      <div className="toolbar">
        <div className="tabs" role="tablist" aria-label="Role">
          {TABS.map((tab) => (
            <button
              key={tab.label}
              type="button"
              role="tab"
              aria-selected={role === tab.role}
              className={`tab${role === tab.role ? ' tab--active' : ''}`}
              onClick={() => {
                setRole(tab.role)
                setPage(1)
              }}
            >
              {tab.label}
            </button>
          ))}
        </div>
        <SearchInput
          value={search}
          placeholder="Search name or email…"
          onChange={(value) => {
            setSearch(value)
            setPage(1)
          }}
        />
      </div>

      <div className="card table-wrap">
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th className="hide-sm">Email</th>
              <th>Role</th>
              <th>Status</th>
              <th className="actions" aria-label="Actions" />
            </tr>
          </thead>
          <tbody>
            <TableStatus
              isPending={list.isPending}
              error={list.error}
              isEmpty={list.data?.items.length === 0}
              emptyMessage={debouncedSearch ? 'No matches.' : 'No users.'}
              colSpan={5}
            />
            {list.data?.items.map((user) => {
              const isMe = user.id === me?.id
              return (
                <tr key={user.id}>
                  <td className="strong">{user.fullName}</td>
                  <td className="hide-sm">{user.email}</td>
                  <td>{isAdmin(user) ? 'Admin' : 'Reader'}</td>
                  <td>
                    <StatusBadge user={user} />
                  </td>
                  <td className="actions">
                    {isMe ? (
                      <span className="muted">You</span>
                    ) : (
                      <>
                        <button
                          type="button"
                          className="button button--small"
                          onClick={() => setAction({ kind: isAdmin(user) ? 'demote' : 'promote', user })}
                        >
                          {isAdmin(user) ? 'Remove admin' : 'Make admin'}
                        </button>
                        <button type="button" className="button button--small" onClick={() => setResetting(user)}>
                          Reset password
                        </button>
                        {user.lockedUntil ? (
                          <button type="button" className="button button--small" onClick={() => setAction({ kind: 'unlock', user })}>
                            Unlock
                          </button>
                        ) : (
                          <button
                            type="button"
                            className="button button--small button--ghost-danger"
                            onClick={() => setAction({ kind: 'lock', user })}
                          >
                            Lock
                          </button>
                        )}
                      </>
                    )}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
      <Pagination data={list.data} onPageChange={setPage} />

      {action && <ActionDialog action={action} onClose={() => setAction(null)} />}
      {resetting && <ResetPasswordForm user={resetting} onClose={() => setResetting(null)} />}
    </>
  )
}

function ActionDialog({ action: { kind, user }, onClose }: { action: PendingAction; onClose: () => void }) {
  const name = user.fullName
  const dialogs = {
    promote: {
      title: 'Make admin',
      message: `Give ${name} full admin access? They'll be signed out and get the new access when they sign in again.`,
      confirmLabel: 'Make admin',
      tone: 'primary',
      successMessage: `${name} is now an admin.`,
      run: () => usersApi.setAdmin(user.id, true),
    },
    demote: {
      title: 'Remove admin',
      message: `Remove admin access from ${name}? They'll be signed out and keep only their reader access.`,
      confirmLabel: 'Remove admin',
      tone: 'danger',
      successMessage: `${name} is no longer an admin.`,
      run: () => usersApi.setAdmin(user.id, false),
    },
    lock: {
      title: 'Lock account',
      message: `Lock ${name}'s account? They're signed out right away and can't sign in until you unlock it.`,
      confirmLabel: 'Lock',
      tone: 'danger',
      successMessage: `Locked ${name}'s account.`,
      run: () => usersApi.lock(user.id),
    },
    unlock: {
      title: 'Unlock account',
      message: `Let ${name} sign in again?`,
      confirmLabel: 'Unlock',
      tone: 'primary',
      successMessage: `Unlocked ${name}'s account.`,
      run: () => usersApi.unlock(user.id),
    },
  } as const satisfies Record<ActionKind, unknown>

  const dialog = dialogs[kind]
  return (
    <ConfirmDialog
      title={dialog.title}
      message={dialog.message}
      confirmLabel={dialog.confirmLabel}
      tone={dialog.tone}
      successMessage={dialog.successMessage}
      onConfirm={dialog.run}
      onClose={onClose}
    />
  )
}

/** 12 characters from an unambiguous alphabet, always with letters and digits (Identity's rules). */
function generatePassword(): string {
  const letters = 'abcdefghjkmnpqrstuvwxyz'
  const alphabet = letters + '23456789'
  for (;;) {
    const bytes = crypto.getRandomValues(new Uint8Array(12))
    const password = Array.from(bytes, (b) => alphabet[b % alphabet.length]).join('')
    if (/\d/.test(password) && /[a-z]/.test(password)) return password
  }
}

function ResetPasswordForm({ user, onClose }: { user: UserAccount; onClose: () => void }) {
  const [password, setPassword] = useState('')
  const save = useSaveMutation(
    () => usersApi.resetPassword(user.id, password),
    `New password set for ${user.fullName}.`,
    onClose,
  )
  const errors = fieldErrorsOf(save.error)

  return (
    <FormModal
      title="Reset password"
      submitLabel="Set password"
      isSubmitting={save.isPending}
      error={save.error}
      onSubmit={() => save.mutate()}
      onClose={onClose}
    >
      <p>
        Choose a new password for <strong>{user.fullName}</strong> ({user.email}). They'll be signed out everywhere; give
        them the new password in person or by phone.
      </p>
      <Field label="New password" errors={errors.newPassword} hint="At least 8 characters, with a letter and a digit.">
        <input
          className="input mono"
          value={password}
          onChange={(e) => {
            setPassword(e.target.value)
            save.reset() // old errors no longer apply
          }}
          maxLength={128}
          autoComplete="new-password"
          spellCheck={false}
          required
        />
      </Field>
      <button type="button" className="button button--small" onClick={() => {
          setPassword(generatePassword())
          save.reset()
        }}>
        Generate one
      </button>
    </FormModal>
  )
}
