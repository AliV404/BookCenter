import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { fieldErrorsOf } from '../api/client'
import { loansApi } from '../api/endpoints'
import type { Loan, LoanListParams, LoanStatus } from '../api/types'
import { Field } from '../components/Field'
import { FormModal } from '../components/FormModal'
import { PageHeader } from '../components/PageHeader'
import { Pagination } from '../components/Pagination'
import { SearchInput } from '../components/SearchInput'
import { TableStatus } from '../components/TableStatus'
import { useDebouncedValue } from '../hooks/useDebouncedValue'
import { useBookOptions, useReaderOptions } from '../hooks/useLookups'
import { useSaveMutation } from '../hooks/useSaveMutation'
import { addDays, endOfLocalDayIso, formatDate, toDateInputValue } from '../lib/dates'

const TABS: { status: LoanStatus; label: string }[] = [
  { status: 'active', label: 'On loan' },
  { status: 'overdue', label: 'Overdue' },
  { status: 'returned', label: 'Returned' },
  { status: 'all', label: 'All' },
]

const DEFAULT_LOAN_DAYS = 14
const MAX_LOAN_DAYS = 90 // must match CreateBorrowRecordValidator.MaxLoanDays

/** `mine`: a reader's own loans — read-only. The API also enforces this scoping. */
export function LoansPage({ mine = false }: { mine?: boolean }) {
  const [searchParams, setSearchParams] = useSearchParams()
  const readerId = mine ? undefined : Number(searchParams.get('readerId')) || undefined

  const [status, setStatus] = useState<LoanStatus>('active')
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const [borrowing, setBorrowing] = useState(false)

  const params: LoanListParams = { page, pageSize: 10, search: debouncedSearch, status, readerId }
  const list = useQuery({ queryKey: ['loans', params], queryFn: () => loansApi.list(params), placeholderData: keepPreviousData })

  return (
    <>
      <PageHeader
        title={mine ? 'My loans' : 'Loans'}
        actions={
          !mine && (
            <button type="button" className="button button--primary" onClick={() => setBorrowing(true)}>
              New loan
            </button>
          )
        }
      />

      <div className="toolbar">
        <div className="tabs" role="tablist" aria-label="Loan status">
          {TABS.map((tab) => (
            <button
              key={tab.status}
              type="button"
              role="tab"
              aria-selected={status === tab.status}
              className={`tab${status === tab.status ? ' tab--active' : ''}`}
              onClick={() => {
                setStatus(tab.status)
                setPage(1)
              }}
            >
              {tab.label}
            </button>
          ))}
        </div>
        <SearchInput
          value={search}
          placeholder={mine ? 'Search books…' : 'Search book or reader…'}
          onChange={(value) => {
            setSearch(value)
            setPage(1)
          }}
        />
        {readerId && (
          <button type="button" className="chip" onClick={() => setSearchParams({})}>
            Reader #{readerId} ×
          </button>
        )}
      </div>

      <LoanTable
        showReader={!mine}
        canReturn={!mine}
        loans={list.data?.items}
        isPending={list.isPending}
        error={list.error}
        emptyMessage={status === 'overdue' ? 'Nothing overdue. 🎉' : mine ? "You haven't borrowed anything here yet." : 'No loans here.'}
      />
      <Pagination data={list.data} onPageChange={setPage} />

      {borrowing && <BorrowForm onClose={() => setBorrowing(false)} />}
    </>
  )
}

interface LoanTableProps {
  showReader: boolean
  canReturn: boolean
  loans: Loan[] | undefined
  isPending: boolean
  error: unknown
  emptyMessage: string
}

export function LoanTable({ showReader, canReturn, loans, isPending, error, emptyMessage }: LoanTableProps) {
  const columns = 4 + (showReader ? 1 : 0) + (canReturn ? 1 : 0)
  return (
    <div className="card table-wrap">
      <table className="table">
        <thead>
          <tr>
            <th>Book</th>
            {showReader && <th>Reader</th>}
            <th className="hide-sm">Borrowed</th>
            <th>Due</th>
            <th>Status</th>
            {canReturn && <th className="actions" aria-label="Actions" />}
          </tr>
        </thead>
        <tbody>
          <TableStatus isPending={isPending} error={error} isEmpty={loans?.length === 0} emptyMessage={emptyMessage} colSpan={columns} />
          {loans?.map((loan) => (
            <tr key={loan.id}>
              <td className="strong">{loan.bookName}</td>
              {showReader && <td>{loan.readerFullName}</td>}
              <td className="hide-sm">{formatDate(loan.borrowedAt)}</td>
              <td>{formatDate(loan.dueDate)}</td>
              <td>
                <LoanStatusBadge loan={loan} />
              </td>
              {canReturn && <td className="actions">{loan.returnedAt === null && <ReturnButton loan={loan} />}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function LoanStatusBadge({ loan }: { loan: Loan }) {
  if (loan.returnedAt) return <span className="badge badge--muted">Returned {formatDate(loan.returnedAt)}</span>
  if (loan.isOverdue) return <span className="badge badge--danger">Overdue</span>
  return <span className="badge badge--ok">On loan</span>
}

function ReturnButton({ loan }: { loan: Loan }) {
  const mutation = useSaveMutation(() => loansApi.return(loan.id), `"${loan.bookName}" returned.`)
  return (
    <>
      <button type="button" className="button button--small" disabled={mutation.isPending} onClick={() => mutation.mutate()}>
        {mutation.isPending ? 'Returning…' : 'Return'}
      </button>
      {mutation.error && <span className="field-error">{mutation.error.message}</span>}
    </>
  )
}

function BorrowForm({ onClose }: { onClose: () => void }) {
  const [today] = useState(() => new Date())
  const [readerId, setReaderId] = useState('')
  const [bookId, setBookId] = useState('')
  const [dueDate, setDueDate] = useState(toDateInputValue(addDays(today, DEFAULT_LOAN_DAYS)))

  const readers = useReaderOptions()
  const books = useBookOptions()

  const save = useSaveMutation(
    () => loansApi.borrow({ readerId: Number(readerId), bookId: Number(bookId), dueDate: endOfLocalDayIso(dueDate) }),
    'Loan created.',
    onClose,
  )
  const errors = fieldErrorsOf(save.error)

  return (
    <FormModal title="New loan" submitLabel="Lend book" isSubmitting={save.isPending} error={save.error} onSubmit={() => save.mutate()} onClose={onClose}>
      <Field label="Reader" errors={errors.readerId} hint={readers.data?.totalCount === 0 ? 'No readers yet — add one first.' : undefined}>
        <select className="input" value={readerId} onChange={(e) => setReaderId(e.target.value)} required>
          <option value="">{readers.isPending ? 'Loading…' : 'Choose reader…'}</option>
          {readers.data?.items.map((r) => (
            <option key={r.id} value={r.id}>
              {r.name} {r.surname} — {r.email}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Book" errors={errors.bookId}>
        <select className="input" value={bookId} onChange={(e) => setBookId(e.target.value)} required>
          <option value="">{books.isPending ? 'Loading…' : 'Choose book…'}</option>
          {books.data?.items.map((b) => (
            <option key={b.id} value={b.id} disabled={b.copiesAvailable === 0}>
              {b.name} — {b.copiesAvailable === 0 ? 'none available' : `${b.copiesAvailable} available`}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Due date" errors={errors.dueDate} hint={`Up to ${MAX_LOAN_DAYS} days.`}>
        <input
          className="input"
          type="date"
          value={dueDate}
          min={toDateInputValue(today)}
          max={toDateInputValue(addDays(today, MAX_LOAN_DAYS - 1))}
          onChange={(e) => setDueDate(e.target.value)}
          required
        />
      </Field>
    </FormModal>
  )
}
