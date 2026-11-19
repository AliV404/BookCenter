import type { ReactNode } from 'react'
import { ErrorBanner } from './ErrorBanner'

interface TableStatusProps {
  isPending: boolean
  error: unknown
  isEmpty: boolean
  emptyMessage: string
  colSpan: number
}

/** Renders a single full-width table row for the loading / error / empty states, or nothing. */
export function TableStatus({ isPending, error, isEmpty, emptyMessage, colSpan }: TableStatusProps) {
  let content: ReactNode = null
  if (isPending) content = <span className="muted">Loading…</span>
  else if (error) content = <ErrorBanner error={error} />
  else if (isEmpty) content = <span className="muted">{emptyMessage}</span>

  if (!content) return null
  return (
    <tr>
      <td colSpan={colSpan} className="table-status">
        {content}
      </td>
    </tr>
  )
}
