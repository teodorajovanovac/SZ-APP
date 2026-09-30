import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query'
import { ApiProblemError } from '../api/generated/client'
import { markSessionExpired } from '../features/auth/sessionExpiry'

const isUnauthorized = (error: unknown) => error instanceof ApiProblemError && error.problem.status === 401

// UX-04: any 401 outside the auth probe means the cookie expired mid-work. Flag it so the
// re-login dialog opens over the current page instead of every screen showing a raw error.
const onError = (error: unknown, key?: readonly unknown[]) => {
  if (isUnauthorized(error) && key?.[0] !== 'auth') markSessionExpired()
}

export const queryClient = new QueryClient({
  queryCache: new QueryCache({ onError: (error, query) => onError(error, query.queryKey) }),
  mutationCache: new MutationCache({ onError: (error) => onError(error) }),
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      // UX-06: 4xx answers won't change on retry; only retry network/5xx once.
      retry: (failureCount, error) =>
        !(error instanceof ApiProblemError && (error.problem.status ?? 0) < 500) && failureCount < 1,
    },
  },
})
