import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '@/lib/api'
import { tokenStorage } from '@/lib/token-storage'

interface AuthContextValue {
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(() => tokenStorage.getAccessToken() !== null)

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated,
      login: async (email, password) => {
        const result = await authApi.login(email, password)
        tokenStorage.setTokens(result.accessToken, result.refreshToken)
        setIsAuthenticated(true)
      },
      register: async (email, password) => {
        const result = await authApi.register(email, password)
        tokenStorage.setTokens(result.accessToken, result.refreshToken)
        setIsAuthenticated(true)
      },
      logout: () => {
        tokenStorage.clear()
        setIsAuthenticated(false)
      },
    }),
    [isAuthenticated],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
