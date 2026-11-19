import { api } from './client'
import type {
  AuthResponse,
  AuthUser,
  Book,
  BookInput,
  BookListParams,
  ListParams,
  LoginInput,
  Loan,
  LoanInput,
  LoanListParams,
  NamedDetail,
  NamedInput,
  NamedItem,
  PagedResult,
  Reader,
  ReaderInput,
  RegisterInput,
} from './types'

export interface CrudApi<TItem, TDetail, TInput, TParams extends ListParams = ListParams> {
  list: (params: TParams) => Promise<PagedResult<TItem>>
  get: (id: number) => Promise<TDetail>
  create: (input: TInput) => Promise<TDetail>
  update: (id: number, input: TInput) => Promise<void>
  remove: (id: number) => Promise<void>
}

function crud<TItem, TDetail, TInput, TParams extends ListParams = ListParams>(
  path: string,
): CrudApi<TItem, TDetail, TInput, TParams> {
  return {
    list: (params) => api(path, { params }),
    get: (id) => api(`${path}/${id}`),
    create: (input) => api(path, { method: 'POST', body: input }),
    update: (id, input) => api(`${path}/${id}`, { method: 'PUT', body: input }),
    remove: (id) => api(`${path}/${id}`, { method: 'DELETE' }),
  }
}

export type NamedApi = CrudApi<NamedItem, NamedDetail, NamedInput>

export const authorsApi: NamedApi = crud('/authors')
export const categoriesApi: NamedApi = crud('/categories')
export const publishersApi: NamedApi = crud('/publishers')
export const booksApi = crud<Book, Book, BookInput, BookListParams>('/books')
export const readersApi = crud<Reader, Reader, ReaderInput>('/readers')

export const loansApi = {
  list: (params: LoanListParams) => api<PagedResult<Loan>>('/borrow-records', { params }),
  borrow: (input: LoanInput) => api<Loan>('/borrow-records', { method: 'POST', body: input }),
  return: (id: number) => api<void>(`/borrow-records/${id}/return`, { method: 'POST' }),
}

export const authApi = {
  login: (input: LoginInput) => api<AuthResponse>('/auth/login', { method: 'POST', body: input }),
  register: (input: RegisterInput) => api<AuthResponse>('/auth/register', { method: 'POST', body: input }),
  me: () => api<AuthUser>('/auth/me'),
}
