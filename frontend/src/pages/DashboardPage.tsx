import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { booksApi, loansApi, readersApi } from '../api/endpoints'
import { PageHeader } from '../components/PageHeader'
import { LoanTable } from './LoansPage'

// pageSize=1 is a cheap way to ask "how many?" — we only read totalCount.
const COUNT_ONLY = { page: 1, pageSize: 1 }

function StatCard({ label, value, to, tone }: { label: string; value: number | undefined; to: string; tone?: 'danger' }) {
  return (
    <Link to={to} className={`card stat${tone === 'danger' && value ? ' stat--danger' : ''}`}>
      <span className="stat-label">{label}</span>
      <span className="stat-value">{value ?? '–'}</span>
    </Link>
  )
}

export function DashboardPage() {
  const books = useQuery({ queryKey: ['books', 'count'], queryFn: () => booksApi.list(COUNT_ONLY) })
  const readers = useQuery({ queryKey: ['readers', 'count'], queryFn: () => readersApi.list(COUNT_ONLY) })
  const active = useQuery({ queryKey: ['loans', 'count', 'active'], queryFn: () => loansApi.list({ ...COUNT_ONLY, status: 'active' }) })
  const overdue = useQuery({
    queryKey: ['loans', 'dashboard-overdue'],
    queryFn: () => loansApi.list({ page: 1, pageSize: 5, status: 'overdue' }),
  })

  return (
    <>
      <PageHeader title="Dashboard" subtitle="Today at the library." />

      <section className="stats" aria-label="Summary">
        <StatCard label="Titles" value={books.data?.totalCount} to="/books" />
        <StatCard label="Readers" value={readers.data?.totalCount} to="/readers" />
        <StatCard label="On loan" value={active.data?.totalCount} to="/loans" />
        <StatCard label="Overdue" value={overdue.data?.totalCount} to="/loans" tone="danger" />
      </section>

      <div className="section-header">
        <h2>Most overdue</h2>
        <Link to="/loans">All loans →</Link>
      </div>
      <LoanTable
        showReader
        canReturn
        loans={overdue.data?.items}
        isPending={overdue.isPending}
        error={overdue.error}
        emptyMessage="Nothing overdue. 🎉"
      />
    </>
  )
}
