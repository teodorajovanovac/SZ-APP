/**
 * Stable adapter for the future OpenAPI-generated client.
 * Keep application code dependent on this module while generated files live in
 * `./openapi`, so regeneration never leaks transport details into features.
 */

export type UserRole = 'Root' | 'Upravnik' | 'Moderator' | 'Review'

export interface CompanySummary {
  id: number
  name: string
  registrationNumber?: string | null
}

export interface CurrentUser {
  id: number
  displayName: string
  email: string
  roles: UserRole[]
  companies: CompanySummary[]
}

export interface LoginRequest {
  email: string
  password: string
  rememberMe: boolean
}

interface AntiforgeryToken {
  token: string
  headerName: string
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errors?: Record<string, string[]>
  [extension: string]: unknown
}

export class ApiProblemError extends Error {
  readonly problem: ProblemDetails

  constructor(problem: ProblemDetails) {
    super(problem.detail || problem.title || 'Došlo je do greške pri komunikaciji sa serverom.')
    this.name = 'ApiProblemError'
    this.problem = problem
  }
}

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

async function parseProblem(response: Response): Promise<ProblemDetails> {
  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('application/json') || contentType.includes('application/problem+json')) {
    try {
      const value = (await response.json()) as ProblemDetails
      return { status: response.status, ...value }
    } catch {
      // Fall through to the status-based problem.
    }
  }

  return {
    status: response.status,
    title: response.statusText || 'HTTP greška',
  }
}

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  const method = (init.method ?? 'GET').toUpperCase()
  const unsafe = !['GET', 'HEAD', 'OPTIONS', 'TRACE'].includes(method)
  if (unsafe && path !== '/api/v1/auth/antiforgery' && !headers.has('X-CSRF-TOKEN')) {
    const csrf = await apiRequest<AntiforgeryToken>('/api/v1/auth/antiforgery')
    headers.set(csrf.headerName, csrf.token)
  }
  if (init.body && !(init.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
    credentials: 'include',
  })

  if (!response.ok) {
    throw new ApiProblemError(await parseProblem(response))
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const api = {
  auth: {
    login: async (request: LoginRequest) => {
      const csrf = await apiRequest<AntiforgeryToken>('/api/v1/auth/antiforgery')
      return apiRequest<CurrentUser>('/api/v1/auth/login', {
        method: 'POST',
        headers: { [csrf.headerName]: csrf.token },
        body: JSON.stringify(request),
      })
    },
    me: () => apiRequest<CurrentUser>('/api/v1/auth/me'),
    logout: async () => {
      const csrf = await apiRequest<AntiforgeryToken>('/api/v1/auth/antiforgery')
      return apiRequest<void>('/api/v1/auth/logout', {
        method: 'POST',
        headers: { [csrf.headerName]: csrf.token },
      })
    },
  },
  companies: {
    list: () => apiRequest<CompanySummary[]>('/api/v1/companies'),
  },
}
