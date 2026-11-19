import type { PagedResult } from '../api/types'

interface PaginationProps {
  data: PagedResult<unknown> | undefined
  onPageChange: (page: number) => void
}

export function Pagination({ data, onPageChange }: PaginationProps) {
  if (!data || data.totalCount === 0) return null

  const first = (data.page - 1) * data.pageSize + 1
  const last = Math.min(data.page * data.pageSize, data.totalCount)

  return (
    <nav className="pagination" aria-label="Pagination">
      <span className="muted">
        {first}–{last} of {data.totalCount}
      </span>
      <div className="pagination-buttons">
        <button type="button" className="button" disabled={data.page <= 1} onClick={() => onPageChange(data.page - 1)}>
          Previous
        </button>
        <span className="muted">
          Page {data.page} of {data.totalPages}
        </span>
        <button
          type="button"
          className="button"
          disabled={data.page >= data.totalPages}
          onClick={() => onPageChange(data.page + 1)}
        >
          Next
        </button>
      </div>
    </nav>
  )
}
