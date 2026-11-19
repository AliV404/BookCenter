import { Link, Route, Routes } from 'react-router-dom'
import { authorsApi, categoriesApi, publishersApi } from './api/endpoints'
import { Layout } from './components/Layout'
import { BooksPage } from './pages/BooksPage'
import { DashboardPage } from './pages/DashboardPage'
import { LoansPage } from './pages/LoansPage'
import { NamedResourcePage, type NamedResourceConfig } from './pages/NamedResourcePage'
import { ReadersPage } from './pages/ReadersPage'

const authors: NamedResourceConfig = {
  queryKey: 'authors',
  title: 'Authors',
  singular: 'author',
  api: authorsApi,
  withBiography: true,
  bookFilter: 'authorId',
}
const categories: NamedResourceConfig = {
  queryKey: 'categories',
  title: 'Categories',
  singular: 'category',
  api: categoriesApi,
  bookFilter: 'categoryId',
}
const publishers: NamedResourceConfig = {
  queryKey: 'publishers',
  title: 'Publishers',
  singular: 'publisher',
  api: publishersApi,
  bookFilter: 'publisherId',
}

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<DashboardPage />} />
        <Route path="loans" element={<LoansPage />} />
        <Route path="books" element={<BooksPage />} />
        <Route path="readers" element={<ReadersPage />} />
        {/* `key` resets page state when switching between these three routes. */}
        <Route path="authors" element={<NamedResourcePage key="authors" config={authors} />} />
        <Route path="categories" element={<NamedResourcePage key="categories" config={categories} />} />
        <Route path="publishers" element={<NamedResourcePage key="publishers" config={publishers} />} />
        <Route
          path="*"
          element={
            <p>
              Page not found. <Link to="/">Back to the dashboard</Link>
            </p>
          }
        />
      </Route>
    </Routes>
  )
}
