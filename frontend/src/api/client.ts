const BASE_URL: string = import.meta.env.VITE_API_URL ?? '/api'

export type FieldErrors = Record<string, string[]>

/**
 * Error thrown for any non-2xx response. The backend always answers with
 * RFC 7807 ProblemDetails, so `message` is human-readable and `fieldErrors`
 * holds validation messages keyed by camelCase field name (e.g. "isbn").
 */
export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors: FieldErrors

  constructor(status: number, message: string, fieldErrors: FieldErrors = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  body?: unknown
  params?: object
}

function buildUrl(path: string, params?: object): string {
  const query = new URLSearchParams()
  for (const [key, value] of Object.entries(params ?? {})) {
    if (value !== undefined && value !== null && value !== '') query.set(key, String(value))
  }
  const qs = query.toString()
  return `${BASE_URL}${path}${qs ? `?${qs}` : ''}`
}

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: FieldErrors
}

export async function api<T>(path: string, { method = 'GET', body, params }: RequestOptions = {}): Promise<T> {
  let response: Response
  try {
    response = await fetch(buildUrl(path, params), {
      method,
      headers: {
        Accept: 'application/json',
        ...(body !== undefined && { 'Content-Type': 'application/json' }),
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new ApiError(0, 'Cannot reach the server. Is the BookCenter API running?')
  }

  if (!response.ok) {
    let problem: ProblemDetails = {}
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Body wasn't JSON (e.g. proxy error page) — fall back to the status text.
    }
    const errors = problem.errors ?? {}
    const message =
      Object.keys(errors).length > 0
        ? 'Please fix the highlighted fields.'
        : (problem.detail ?? problem.title ?? `Request failed (${response.status} ${response.statusText})`)
    throw new ApiError(response.status, message, errors)
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

export function fieldErrorsOf(error: unknown): FieldErrors {
  return error instanceof ApiError ? error.fieldErrors : {}
}
