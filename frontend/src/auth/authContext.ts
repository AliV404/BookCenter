import { createContext, useContext } from 'react'
import type { AuthUser, LoginInput, RegisterInput } from '../api/types'

export interface AuthContextValue {
  user: AuthUser | null
  isAdmin: boolean
  login: (input: LoginInput) => Promise<AuthUser>
  register: (input: RegisterInput) => Promise<AuthUser>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth must be used inside <AuthProvider>')
  return value
}

/** Where to land after signing in. */
export const homePathFor = (user: AuthUser) => (user.roles.includes('Admin') ? '/' : '/my-loans')
