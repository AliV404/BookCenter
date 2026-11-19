const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' })

/** "2026-11-19T10:00:00Z" → "Nov 19, 2026" in the viewer's locale. */
export function formatDate(iso: string): string {
  return dateFormat.format(new Date(iso))
}

/** Local calendar date as YYYY-MM-DD, for <input type="date">. */
export function toDateInputValue(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function addDays(date: Date, days: number): Date {
  const copy = new Date(date)
  copy.setDate(copy.getDate() + days)
  return copy
}

/** "2026-12-31" → the last second of that day in the viewer's timezone, as a UTC ISO string. */
export function endOfLocalDayIso(dateInputValue: string): string {
  return new Date(`${dateInputValue}T23:59:59`).toISOString()
}
