import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { fieldErrorsOf } from '../api/client'
import type { NamedApi } from '../api/endpoints'
import type { NamedDetail, NamedItem } from '../api/types'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { ErrorBanner } from '../components/ErrorBanner'
import { Field } from '../components/Field'
import { FormModal } from '../components/FormModal'
import { Modal } from '../components/Modal'
import { PageHeader } from '../components/PageHeader'
import { Pagination } from '../components/Pagination'
import { SearchInput } from '../components/SearchInput'
import { TableStatus } from '../components/TableStatus'
import { useDebouncedValue } from '../hooks/useDebouncedValue'
import { useSaveMutation } from '../hooks/useSaveMutation'

/** Authors, categories and publishers share one page: a name (+ biography for authors) and a book count. */
export interface NamedResourceConfig {
  queryKey: string
  title: string
  singular: string
  api: NamedApi
  withBiography?: boolean
  /** Query-string parameter that filters the Books page by this resource. */
  bookFilter: 'authorId' | 'categoryId' | 'publisherId'
}

type Editing = { mode: 'create' } | { mode: 'edit'; id: number } | null

export function NamedResourcePage({ config }: { config: NamedResourceConfig }) {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const [editing, setEditing] = useState<Editing>(null)
  const [deleting, setDeleting] = useState<NamedItem | null>(null)

  const params = { page, pageSize: 10, search: debouncedSearch }
  const list = useQuery({
    queryKey: [config.queryKey, params],
    queryFn: () => config.api.list(params),
    placeholderData: keepPreviousData,
  })

  return (
    <>
      <PageHeader
        title={config.title}
        actions={
          <button type="button" className="button button--primary" onClick={() => setEditing({ mode: 'create' })}>
            Add {config.singular}
          </button>
        }
      />

      <div className="toolbar">
        <SearchInput
          value={search}
          placeholder={`Search ${config.title.toLowerCase()}…`}
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
              <th className="num">Books</th>
              <th className="actions" aria-label="Actions" />
            </tr>
          </thead>
          <tbody>
            <TableStatus
              isPending={list.isPending}
              error={list.error}
              isEmpty={list.data?.items.length === 0}
              emptyMessage={debouncedSearch ? 'No matches.' : `No ${config.title.toLowerCase()} yet.`}
              colSpan={3}
            />
            {list.data?.items.map((item) => (
              <tr key={item.id}>
                <td className="strong">{item.name}</td>
                <td className="num">
                  {item.bookCount > 0 ? (
                    <Link to={`/books?${config.bookFilter}=${item.id}`}>{item.bookCount}</Link>
                  ) : (
                    <span className="muted">0</span>
                  )}
                </td>
                <td className="actions">
                  <button type="button" className="button button--small" onClick={() => setEditing({ mode: 'edit', id: item.id })}>
                    Edit
                  </button>
                  <button type="button" className="button button--small button--ghost-danger" onClick={() => setDeleting(item)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <Pagination data={list.data} onPageChange={setPage} />

      {editing?.mode === 'create' && <NamedForm config={config} onClose={() => setEditing(null)} />}
      {editing?.mode === 'edit' && <EditNamed config={config} id={editing.id} onClose={() => setEditing(null)} />}

      {deleting && (
        <ConfirmDialog
          title={`Delete ${config.singular}`}
          message={`Delete "${deleting.name}"? This cannot be undone from the app.`}
          successMessage={`Deleted "${deleting.name}".`}
          onConfirm={() => config.api.remove(deleting.id)}
          onClose={() => setDeleting(null)}
        />
      )}
    </>
  )
}

/** Loads the full record (list rows don't include the biography) before showing the form. */
function EditNamed({ config, id, onClose }: { config: NamedResourceConfig; id: number; onClose: () => void }) {
  const detail = useQuery({ queryKey: [config.queryKey, 'detail', id], queryFn: () => config.api.get(id) })

  if (detail.data) return <NamedForm config={config} existing={detail.data} onClose={onClose} />

  return (
    <Modal title={`Edit ${config.singular}`} onClose={onClose}>
      <div className="modal-body">{detail.error ? <ErrorBanner error={detail.error} /> : <p className="muted">Loading…</p>}</div>
    </Modal>
  )
}

interface NamedFormProps {
  config: NamedResourceConfig
  existing?: NamedDetail
  onClose: () => void
}

function NamedForm({ config, existing, onClose }: NamedFormProps) {
  const [name, setName] = useState(existing?.name ?? '')
  const [biography, setBiography] = useState(existing?.biography ?? '')

  const save = useSaveMutation(
    () => {
      const input = {
        name: name.trim(),
        ...(config.withBiography && { biography: biography.trim() || null }),
      }
      return existing ? config.api.update(existing.id, input) : config.api.create(input)
    },
    existing ? `Saved "${name.trim()}".` : `Added "${name.trim()}".`,
    onClose,
  )
  const errors = fieldErrorsOf(save.error)

  return (
    <FormModal
      title={existing ? `Edit ${config.singular}` : `Add ${config.singular}`}
      submitLabel={existing ? 'Save' : 'Add'}
      isSubmitting={save.isPending}
      error={save.error}
      onSubmit={() => save.mutate()}
      onClose={onClose}
    >
      <Field label="Name" errors={errors.name}>
        <input className="input" value={name} onChange={(e) => setName(e.target.value)} maxLength={150} required />
      </Field>
      {config.withBiography && (
        <Field label="Biography" errors={errors.biography} hint="Optional">
          <textarea className="input" rows={4} value={biography} onChange={(e) => setBiography(e.target.value)} maxLength={2000} />
        </Field>
      )}
    </FormModal>
  )
}
