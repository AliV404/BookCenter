import { Link, Navigate, Route, Routes } from 'react-router-dom'
import { authorsApi, categoriesApi, publishersApi } from './api/endpoints'
import { useAuth } from './auth/authContext'
import { RequireAdmin, RequireAuth } from './auth/guards'
import { Layout } from './components/Layout'
import { AccountPage } from './pages/AccountPage'
import { BooksPage } from './pages/BooksPage'
import { DashboardPage } from './pages/DashboardPage'
import { LoansPage } from './pages/LoansPage'
import { LoginPage } from './pages/LoginPage'
import { NamedResourcePage, type NamedResourceConfig } from './pages/NamedResourcePage'
import { ReadersPage } from './pages/ReadersPage'
import { RegisterPage } from './pages/RegisterPage'
import { UsersPage } from './pages/UsersPage'

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

function Home() {
  const { isAdmin } = useAuth()
  return isAdmin ? <DashboardPage /> : <Navigate to="/my-loans" replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="login" element={<LoginPage />} />
      <Route path="register" element={<RegisterPage />} />

      <Route element={<RequireAuth />}>
        <Route element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="books" element={<BooksPage />} />
          <Route path="my-loans" element={<LoansPage mine />} />
          <Route path="account" element={<AccountPage />} />

          <Route element={<RequireAdmin />}>
            <Route path="loans" element={<LoansPage />} />
            <Route path="readers" element={<ReadersPage />} />
            <Route path="users" element={<UsersPage />} />
            {/* `key` resets page state when switching between these three routes. */}
            <Route path="authors" element={<NamedResourcePage key="authors" config={authors} />} />
            <Route path="categories" element={<NamedResourcePage key="categories" config={categories} />} />
            <Route path="publishers" element={<NamedResourcePage key="publishers" config={publishers} />} />
          </Route>

          <Route
            path="*"
            element={
              <p>
                Page not found. <Link to="/">Back home</Link>
              </p>
            }
          />
        </Route>
      </Route>
    </Routes>
  )
}
