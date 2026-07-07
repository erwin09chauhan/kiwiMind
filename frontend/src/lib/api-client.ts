import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { tokenStorage } from './token-storage'
import type { AuthResult } from '@/types/api'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'

export const apiClient = axios.create({ baseURL: API_BASE_URL })

apiClient.interceptors.request.use((config) => {
  const accessToken = tokenStorage.getAccessToken()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

let refreshPromise: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  const refreshToken = tokenStorage.getRefreshToken()
  if (!refreshToken) {
    throw new Error('No refresh token available')
  }

  const response = await axios.post<AuthResult>(`${API_BASE_URL}/api/auth/refresh`, { refreshToken })
  tokenStorage.setTokens(response.data.accessToken, response.data.refreshToken)
  return response.data.accessToken
}

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  _retried?: boolean
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined

    if (error.response?.status !== 401 || !originalRequest || originalRequest._retried) {
      throw error
    }

    originalRequest._retried = true

    try {
      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null
      })
      const accessToken = await refreshPromise

      originalRequest.headers.Authorization = `Bearer ${accessToken}`
      return apiClient(originalRequest)
    } catch (refreshError) {
      tokenStorage.clear()
      window.location.assign('/login')
      throw refreshError
    }
  },
)
