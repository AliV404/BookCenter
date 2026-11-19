import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './authContext'

/** Sends anonymous visitors to /login, remembering where they were going. */
export function RequireAuth() {
  const { user } = useAuth()
  const location = useLocation()
  if (!user) return <Navigate to="/login" replace state={{ from: location }} />
  return <Outlet />
}

/** Admin-only section. Readers are sent to their own loans. (The API enforces this too.) */
export function RequireAdmin() {
  const { isAdmin } = useAuth()
  if (!isAdmin) return <Navigate to="/my-loans" replace />
  return <Outlet />
}
