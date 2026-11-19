import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/authContext'

interface NavItem {
  to: string
  label: string
  end?: boolean
}

const adminLinks: NavItem[] = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/loans', label: 'Loans' },
  { to: '/books', label: 'Books' },
  { to: '/readers', label: 'Readers' },
  { to: '/authors', label: 'Authors' },
  { to: '/categories', label: 'Categories' },
  { to: '/publishers', label: 'Publishers' },
]

const readerLinks: NavItem[] = [
  { to: '/my-loans', label: 'My loans' },
  { to: '/books', label: 'Catalog' },
]

export function Layout() {
  const { user, isAdmin, logout } = useAuth()
  const links = isAdmin ? adminLinks : readerLinks

  return (
    <div className="app">
      <aside className="sidebar">
        <div className="brand">
          <img src="/favicon.svg" alt="" width={28} height={28} />
          <span>BookCenter</span>
        </div>
        <nav className="nav" aria-label="Main">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={link.end}
              className={({ isActive }) => `nav-link${isActive ? ' nav-link--active' : ''}`}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
        {user && (
          <div className="user-box">
            <div className="user-name">{user.fullName}</div>
            <div className="muted user-meta">
              {user.email} · {isAdmin ? 'Admin' : 'Reader'}
            </div>
            <button type="button" className="button button--small" onClick={logout}>
              Sign out
            </button>
          </div>
        )}
      </aside>
      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}
