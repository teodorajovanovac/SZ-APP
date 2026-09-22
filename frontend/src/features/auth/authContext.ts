import { createContext } from 'react'
import type { CurrentUser, LoginRequest } from '../../api/generated/client'

export interface AuthContextValue {
  user: CurrentUser | null
  isLoading: boolean
  isLoginPending: boolean
  error: unknown
  login: (request: LoginRequest) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export const authQueryKey = ['auth', 'me'] as const
