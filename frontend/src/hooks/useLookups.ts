import { useQuery } from '@tanstack/react-query'
import { authorsApi, booksApi, categoriesApi, publishersApi, readersApi } from '../api/endpoints'

// Options for <select> dropdowns. The API caps pageSize at 100, which is plenty for a
// small library; a larger one would want a searchable combobox instead.
const LOOKUP = { page: 1, pageSize: 100 }

export const useAuthorOptions = () =>
  useQuery({ queryKey: ['authors', 'lookup'], queryFn: () => authorsApi.list(LOOKUP) })

export const useCategoryOptions = () =>
  useQuery({ queryKey: ['categories', 'lookup'], queryFn: () => categoriesApi.list(LOOKUP) })

export const usePublisherOptions = () =>
  useQuery({ queryKey: ['publishers', 'lookup'], queryFn: () => publishersApi.list(LOOKUP) })

export const useBookOptions = () =>
  useQuery({ queryKey: ['books', 'lookup'], queryFn: () => booksApi.list(LOOKUP) })

export const useReaderOptions = () =>
  useQuery({ queryKey: ['readers', 'lookup'], queryFn: () => readersApi.list(LOOKUP) })
