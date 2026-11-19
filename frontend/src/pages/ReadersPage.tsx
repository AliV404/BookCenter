import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { fieldErrorsOf } from '../api/client'
import { readersApi } from '../api/endpoints'
import type { Reader } from '../api/types'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { Field } from '../components/Field'
import { FormModal } from '../components/FormModal'
import { PageHeader } from '../components/PageHeader'
import { Pagination } from '../components/Pagination'
import { SearchInput } from '../components/SearchInput'
import { TableStatus } from '../components/TableStatus'
import { useDebouncedValue } from '../hooks/useDebouncedValue'
import { useSaveMutation } from '../hooks/useSaveMutation'

export function ReadersPage() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const [editing, setEditing] = useState<Reader | 'new' | null>(null)
  const [deleting, setDeleting] = useState<Reader | null>(null)

  const params = { page, pageSize: 10, search: debouncedSearch }
  const list = useQuery({ queryKey: ['readers', params], queryFn: () => readersApi.list(params), placeholderData: keepPreviousData })

  return (
    <>
      <PageHeader
        title="Readers"
        actions={
          <button type="button" className="button button--primary" onClick={() => setEditing('new')}>
            Add reader
          </button>
        }
      />

      <div className="toolbar">
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
              <th>Email</th>
              <th className="hide-sm">Phone</th>
              <th className="num">On loan</th>
              <th className="actions" aria-label="Actions" />
            </tr>
          </thead>
          <tbody>
            <TableStatus
              isPending={list.isPending}
              error={list.error}
              isEmpty={list.data?.items.length === 0}
              emptyMessage={debouncedSearch ? 'No matches.' : 'No readers yet.'}
              colSpan={5}
            />
            {list.data?.items.map((reader) => (
              <tr key={reader.id}>
                <td className="strong">
                  {reader.name} {reader.surname}
                  {reader.hasAccount && (
                    <span className="badge badge--muted reader-account" title="Signs in online with this email">
                      Online
                    </span>
                  )}
                </td>
                <td>{reader.email}</td>
                <td className="hide-sm">{reader.phone}</td>
                <td className="num">
                  {reader.activeLoans > 0 ? (
                    <Link to={`/loans?readerId=${reader.id}`}>{reader.activeLoans}</Link>
                  ) : (
                    <span className="muted">0</span>
                  )}
                </td>
                <td className="actions">
                  <button type="button" className="button button--small" onClick={() => setEditing(reader)}>
                    Edit
                  </button>
                  <button type="button" className="button button--small button--ghost-danger" onClick={() => setDeleting(reader)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <Pagination data={list.data} onPageChange={setPage} />

      {editing && <ReaderForm existing={editing === 'new' ? undefined : editing} onClose={() => setEditing(null)} />}

      {deleting && (
        <ConfirmDialog
          title="Delete reader"
          message={`Delete ${deleting.name} ${deleting.surname}? Their loan history is kept.`}
          successMessage={`Deleted ${deleting.name} ${deleting.surname}.`}
          onConfirm={() => readersApi.remove(deleting.id)}
          onClose={() => setDeleting(null)}
        />
      )}
    </>
  )
}

function ReaderForm({ existing, onClose }: { existing?: Reader; onClose: () => void }) {
  const [form, setForm] = useState({
    name: existing?.name ?? '',
    surname: existing?.surname ?? '',
    email: existing?.email ?? '',
    phone: existing?.phone ?? '',
  })
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm({ ...form, [key]: e.target.value })

  const save = useSaveMutation(
    () => {
      const input = {
        name: form.name.trim(),
        surname: form.surname.trim(),
        email: form.email.trim(),
        phone: form.phone.trim(),
      }
      return existing ? readersApi.update(existing.id, input) : readersApi.create(input)
    },
    existing ? 'Reader saved.' : 'Reader added.',
    onClose,
  )
  const errors = fieldErrorsOf(save.error)

  return (
    <FormModal
      title={existing ? 'Edit reader' : 'Add reader'}
      submitLabel={existing ? 'Save' : 'Add reader'}
      isSubmitting={save.isPending}
      error={save.error}
      onSubmit={() => save.mutate()}
      onClose={onClose}
    >
      <div className="form-row">
        <Field label="First name" errors={errors.name}>
          <input className="input" value={form.name} onChange={set('name')} maxLength={100} autoComplete="off" required />
        </Field>
        <Field label="Last name" errors={errors.surname}>
          <input className="input" value={form.surname} onChange={set('surname')} maxLength={100} autoComplete="off" required />
        </Field>
      </div>
      <Field
        label="Email"
        errors={errors.email}
        hint={existing?.hasAccount ? 'This reader signs in online: changing it also changes their sign-in email.' : undefined}
      >
        <input className="input" type="email" value={form.email} onChange={set('email')} maxLength={256} autoComplete="off" required />
      </Field>
      <Field label="Phone" errors={errors.phone}>
        <input className="input" type="tel" value={form.phone} onChange={set('phone')} maxLength={30} autoComplete="off" required />
      </Field>
    </FormModal>
  )
}
