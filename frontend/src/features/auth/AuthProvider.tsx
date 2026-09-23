import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { PropsWithChildren } from 'react'
import { ApiProblemError, api, type CurrentUser, type LoginRequest } from '../../api/generated/client'
import { AuthContext, authQueryKey } from './authContext'

export function AuthProvider({ children }: PropsWithChildren) {
  const queryClient = useQueryClient()
  const meQuery = useQuery({
    queryKey: authQueryKey,
    queryFn: api.auth.me,
    retry: (failureCount, error) =>
      !(error instanceof ApiProblemError && error.problem.status === 401) && failureCount < 1,
    staleTime: 60_000,
  })
  const loginMutation = useMutation({
    mutationFn: api.auth.login,
    onSuccess: (user) => queryClient.setQueryData(authQueryKey, user),
  })
  const logoutMutation = useMutation({
    mutationFn: api.auth.logout,
    onSettled: () => {
      queryClient.setQueryData<CurrentUser | null>(authQueryKey, null)
      queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== 'auth' })
    },
  })

  const login = async (request: LoginRequest) => {
    await loginMutation.mutateAsync(request)
  }

  const logout = async () => {
    await logoutMutation.mutateAsync()
  }

  return (
    <AuthContext.Provider
      value={{
        user: meQuery.data ?? null,
        isLoading: meQuery.isLoading,
        isLoginPending: loginMutation.isPending,
        // Only a failed login attempt is a user-facing error. The initial /auth/me probe
        // returns 401 for every signed-out visitor — surfacing that would greet them with
        // a red "Unauthorized" alert before they typed anything.
        error: loginMutation.error,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}
