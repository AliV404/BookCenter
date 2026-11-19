import { useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { authSession } from '../api/client'
import { authApi } from '../api/endpoints'
import type { AuthResponse, AuthUser } from '../api/types'
import { useToast } from '../lib/toast'
import { AuthContext, type AuthContextValue } from './authContext'

const STORAGE_KEY = 'bookcenter.session'

interface StoredSession {
  token: string
  expiresAt: string
  user: AuthUser
}

/**
 * Note on storage: the token lives in localStorage, the simplest option for a SPA calling a
 * bearer-token API. The trade-off is that any XSS on this origin could read it; an httpOnly
 * cookie avoids that but needs CSRF protection. Fine for this project — revisit before production.
 */
function loadSession(): StoredSession | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const session = JSON.parse(raw) as StoredSession
    if (new Date(session.expiresAt).getTime() <= Date.now()) {
      localStorage.removeItem(STORAGE_KEY)
      return null
    }
    return session
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const notify = useToast()

  // Read storage synchronously so the token is attached to the very first requests.
  const [session, setSession] = useState<StoredSession | null>(() => {
    const stored = loadSession()
    authSession.token = stored?.token ?? null
    return stored
  })
  const userIdRef = useRef(session?.user.id ?? null)

  const applySession = useCallback(
    (next: StoredSession | null) => {
      authSession.token = next?.token ?? null
      try {
        if (next) localStorage.setItem(STORAGE_KEY, JSON.stringify(next))
        else localStorage.removeItem(STORAGE_KEY)
      } catch {
        // Storage unavailable (private mode): the session just won't survive a reload.
      }
      // Never show one user's cached data to the next (a refreshed token for the same user keeps it).
      const nextUserId = next?.user.id ?? null
      if (nextUserId !== userIdRef.current) queryClient.clear()
      userIdRef.current = nextUserId
      setSession(next)
    },
    [queryClient],
  )

  const logout = useCallback(() => applySession(null), [applySession])

  // Expired or revoked token (password reset, account locked, roles changed...) → sign out.
  // Route guards then redirect to /login.
  useEffect(() => {
    authSession.onUnauthorized = () => {
      if (!authSession.token) return
      applySession(null)
      notify('Your session has ended. Please sign in again.')
    }
  }, [applySession, notify])

  // Re-validate a stored token once on load (roles may have changed since it was issued).
  useEffect(() => {
    if (!authSession.token) return
    authApi
      .me()
      .then((user) => setSession((current) => (current ? { ...current, user } : current)))
      .catch(() => {}) // 401 is handled by onUnauthorized; offline just keeps the stored user
  }, [])

  const value = useMemo<AuthContextValue>(() => {
    const signIn = (response: AuthResponse) => {
      applySession({ token: response.token, expiresAt: response.expiresAt, user: response.user })
      return response.user
    }
    return {
      user: session?.user ?? null,
      isAdmin: session?.user.roles.includes('Admin') ?? false,
      login: async (input) => signIn(await authApi.login(input)),
      register: async (input) => signIn(await authApi.register(input)),
      changePassword: async (input) => {
        signIn(await authApi.changePassword(input))
      },
      logout,
    }
  }, [session, applySession, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
