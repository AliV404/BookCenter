// TypeScript mirrors of the backend DTOs (BookCenter.Application/DTOs).
// JSON uses camelCase, so `ISBN` in C# is `isbn` here.

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ListParams {
  page?: number
  pageSize?: number
  search?: string
}

/** Short reference to a book, used inside author / category / publisher details. */
export interface BookRef {
  id: number
  name: string
}

// ---------- Authors, categories, publishers ("named" resources)

export interface NamedItem {
  id: number
  name: string
  bookCount: number
}

export interface NamedDetail {
  id: number
  name: string
  biography?: string | null // authors only
  books: BookRef[]
}

export interface NamedInput {
  name: string
  biography?: string | null // authors only
}

// ---------- Books

export interface Book {
  id: number
  name: string
  isbn: string
  publishYear: number
  totalCopies: number
  copiesAvailable: number
  authorId: number
  authorName: string
  categoryId: number
  categoryName: string
  publisherId: number
  publisherName: string
}

export interface BookInput {
  name: string
  isbn: string
  publishYear: number
  totalCopies: number
  authorId: number
  categoryId: number
  publisherId: number
}

export interface BookListParams extends ListParams {
  authorId?: number
  categoryId?: number
  publisherId?: number
}

// ---------- Readers

export interface Reader {
  id: number
  name: string
  surname: string
  email: string
  phone: string
  activeLoans: number
  /** Signs in online; their email is also their login. */
  hasAccount: boolean
}

export interface ReaderInput {
  name: string
  surname: string
  email: string
  phone: string
}

// ---------- Loans (BorrowRecords)

export type LoanStatus = 'all' | 'active' | 'overdue' | 'returned'

export interface Loan {
  id: number
  bookId: number
  bookName: string
  readerId: number
  readerFullName: string
  borrowedAt: string
  dueDate: string
  returnedAt: string | null
  isOverdue: boolean
}

export interface LoanInput {
  bookId: number
  readerId: number
  /** ISO 8601 date-time, e.g. 2026-12-31T19:59:59.000Z */
  dueDate: string
}

export interface LoanListParams extends ListParams {
  status?: LoanStatus
  readerId?: number
  bookId?: number
}

// ---------- Auth

export type Role = 'Admin' | 'Reader'

export interface AuthUser {
  id: string
  email: string
  fullName: string
  roles: Role[]
  readerId: number | null
}

export interface AuthResponse {
  token: string
  expiresAt: string
  user: AuthUser
}

export interface LoginInput {
  email: string
  password: string
}

export interface RegisterInput extends ReaderInput {
  password: string
}

export interface ChangePasswordInput {
  currentPassword: string
  newPassword: string
}

/** A login account as seen by an admin. */
export interface UserAccount {
  id: string
  email: string
  fullName: string
  roles: Role[]
  /** Linked reader card; null for staff-only accounts or when the card was deleted. */
  readerId: number | null
  /** Set while locked, by an admin (far future) or after too many failed logins. */
  lockedUntil: string | null
}

export interface UserListParams extends ListParams {
  role?: Role
}
