import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { fieldErrorsOf } from '../api/client'
import { booksApi } from '../api/endpoints'
import { useAuth } from '../auth/authContext'
import type { Book, BookListParams } from '../api/types'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { Field } from '../components/Field'
import { FormModal } from '../components/FormModal'
import { PageHeader } from '../components/PageHeader'
import { Pagination } from '../components/Pagination'
import { SearchInput } from '../components/SearchInput'
import { TableStatus } from '../components/TableStatus'
import { useDebouncedValue } from '../hooks/useDebouncedValue'
import { useAuthorOptions, useCategoryOptions, usePublisherOptions } from '../hooks/useLookups'
import { useSaveMutation } from '../hooks/useSaveMutation'

type FilterKey = 'authorId' | 'categoryId' | 'publisherId'

export function BooksPage() {
  const { isAdmin } = useAuth()
  // Filters live in the URL so links like /books?authorId=3 work and survive a refresh.
  const [searchParams, setSearchParams] = useSearchParams()
  const filterValue = (key: FilterKey) => {
    const value = Number(searchParams.get(key))
    return Number.isInteger(value) && value > 0 ? value : undefined
  }

  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const [editing, setEditing] = useState<Book | 'new' | null>(null)
  const [deleting, setDeleting] = useState<Book | null>(null)

  const authors = useAuthorOptions()
  const categories = useCategoryOptions()
  const publishers = usePublisherOptions()

  const params: BookListParams = {
    page,
    pageSize: 10,
    search: debouncedSearch,
    authorId: filterValue('authorId'),
    categoryId: filterValue('categoryId'),
    publisherId: filterValue('publisherId'),
  }
  const list = useQuery({ queryKey: ['books', params], queryFn: () => booksApi.list(params), placeholderData: keepPreviousData })

  const setFilter = (key: FilterKey, value: string) => {
    const next = new URLSearchParams(searchParams)
    if (value) next.set(key, value)
    else next.delete(key)
    setSearchParams(next)
    setPage(1)
  }

  const filters: { key: FilterKey; label: string; options: { id: number; name: string }[] }[] = [
    { key: 'authorId', label: 'All authors', options: authors.data?.items ?? [] },
    { key: 'categoryId', label: 'All categories', options: categories.data?.items ?? [] },
    { key: 'publisherId', label: 'All publishers', options: publishers.data?.items ?? [] },
  ]

  return (
    <>
      <PageHeader
        title={isAdmin ? 'Books' : 'Catalog'}
        subtitle={isAdmin ? undefined : 'Ask at the desk to borrow a book.'}
        actions={
          isAdmin && (
            <button type="button" className="button button--primary" onClick={() => setEditing('new')}>
              Add book
            </button>
          )
        }
      />

      <div className="toolbar">
        <SearchInput
          value={search}
          placeholder="Search title, ISBN or author…"
          onChange={(value) => {
            setSearch(value)
            setPage(1)
          }}
        />
        {filters.map((filter) => (
          <select
            key={filter.key}
            className="input"
            aria-label={filter.label}
            value={searchParams.get(filter.key) ?? ''}
            onChange={(e) => setFilter(filter.key, e.target.value)}
          >
            <option value="">{filter.label}</option>
            {filter.options.map((option) => (
              <option key={option.id} value={option.id}>
                {option.name}
              </option>
            ))}
          </select>
        ))}
      </div>

      <div className="card table-wrap">
        <table className="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Author</th>
              <th className="hide-sm">Category</th>
              <th className="hide-sm">Publisher</th>
              <th className="num hide-sm">Year</th>
              <th className="hide-sm">ISBN</th>
              <th className="num">Available</th>
              {isAdmin && <th className="actions" aria-label="Actions" />}
            </tr>
          </thead>
          <tbody>
            <TableStatus
              isPending={list.isPending}
              error={list.error}
              isEmpty={list.data?.items.length === 0}
              emptyMessage="No books match."
              colSpan={isAdmin ? 8 : 7}
            />
            {list.data?.items.map((book) => (
              <tr key={book.id}>
                <td className="strong">{book.name}</td>
                <td>{book.authorName}</td>
                <td className="hide-sm">{book.categoryName}</td>
                <td className="hide-sm">{book.publisherName}</td>
                <td className="num hide-sm">{book.publishYear}</td>
                <td className="mono hide-sm">{book.isbn}</td>
                <td className="num">
                  <span className={`badge ${book.copiesAvailable > 0 ? 'badge--ok' : 'badge--muted'}`}>
                    {book.copiesAvailable} / {book.totalCopies}
                  </span>
                </td>
                {isAdmin && (
                  <td className="actions">
                    <button type="button" className="button button--small" onClick={() => setEditing(book)}>
                      Edit
                    </button>
                    <button type="button" className="button button--small button--ghost-danger" onClick={() => setDeleting(book)}>
                      Delete
                    </button>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <Pagination data={list.data} onPageChange={setPage} />

      {editing && <BookForm existing={editing === 'new' ? undefined : editing} onClose={() => setEditing(null)} />}

      {deleting && (
        <ConfirmDialog
          title="Delete book"
          message={`Delete "${deleting.name}"? Loan history is kept.`}
          successMessage={`Deleted "${deleting.name}".`}
          onConfirm={() => booksApi.remove(deleting.id)}
          onClose={() => setDeleting(null)}
        />
      )}
    </>
  )
}

function BookForm({ existing, onClose }: { existing?: Book; onClose: () => void }) {
  const [form, setForm] = useState({
    name: existing?.name ?? '',
    isbn: existing?.isbn ?? '',
    publishYear: existing ? String(existing.publishYear) : '',
    totalCopies: existing ? String(existing.totalCopies) : '1',
    authorId: existing ? String(existing.authorId) : '',
    categoryId: existing ? String(existing.categoryId) : '',
    publisherId: existing ? String(existing.publisherId) : '',
  })
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm({ ...form, [key]: e.target.value })

  const authors = useAuthorOptions()
  const categories = useCategoryOptions()
  const publishers = usePublisherOptions()

  const save = useSaveMutation(
    () => {
      const input = {
        name: form.name.trim(),
        isbn: form.isbn.replace(/[\s-]/g, ''), // accept "978-9952-..." and send digits only
        publishYear: Number(form.publishYear),
        totalCopies: Number(form.totalCopies),
        authorId: Number(form.authorId),
        categoryId: Number(form.categoryId),
        publisherId: Number(form.publisherId),
      }
      return existing ? booksApi.update(existing.id, input) : booksApi.create(input)
    },
    existing ? `Saved "${form.name.trim()}".` : `Added "${form.name.trim()}".`,
    onClose,
  )
  const errors = fieldErrorsOf(save.error)
  const onLoan = existing ? existing.totalCopies - existing.copiesAvailable : 0

  const selects = [
    { key: 'authorId' as const, label: 'Author', query: authors },
    { key: 'categoryId' as const, label: 'Category', query: categories },
    { key: 'publisherId' as const, label: 'Publisher', query: publishers },
  ]

  return (
    <FormModal
      title={existing ? 'Edit book' : 'Add book'}
      submitLabel={existing ? 'Save' : 'Add book'}
      isSubmitting={save.isPending}
      error={save.error}
      onSubmit={() => save.mutate()}
      onClose={onClose}
    >
      <Field label="Title" errors={errors.name}>
        <input className="input" value={form.name} onChange={set('name')} maxLength={200} required />
      </Field>
      <div className="form-row">
        <Field label="ISBN" errors={errors.isbn} hint="13 digits">
          <input className="input mono" value={form.isbn} onChange={set('isbn')} inputMode="numeric" required />
        </Field>
        <Field label="Publish year" errors={errors.publishYear}>
          <input className="input" type="number" value={form.publishYear} onChange={set('publishYear')} required />
        </Field>
      </div>
      <Field
        label="Total copies"
        errors={errors.totalCopies}
        hint={onLoan > 0 ? `${onLoan} currently on loan — can't go below that.` : undefined}
      >
        <input className="input" type="number" min={onLoan} value={form.totalCopies} onChange={set('totalCopies')} required />
      </Field>
      {selects.map(({ key, label, query }) => (
        <Field
          key={key}
          label={label}
          errors={errors[key]}
          hint={query.data?.totalCount === 0 ? `No ${label.toLowerCase()}s yet — add one first.` : undefined}
        >
          <select className="input" value={form[key]} onChange={set(key)} required>
            <option value="">{query.isPending ? 'Loading…' : `Choose ${label.toLowerCase()}…`}</option>
            {query.data?.items.map((option) => (
              <option key={option.id} value={option.id}>
                {option.name}
              </option>
            ))}
          </select>
        </Field>
      ))}
    </FormModal>
  )
}
