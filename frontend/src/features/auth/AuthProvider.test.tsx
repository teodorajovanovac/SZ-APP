import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import { ApiProblemError } from '../../api/generated/client'
import { AuthProvider } from './AuthProvider'
import { useAuth } from './useAuth'

vi.mock('../../api/generated/client', async () => {
  const actual = await vi.importActual<typeof import('../../api/generated/client')>(
    '../../api/generated/client',
  )
  return {
    ...actual,
    api: {
      auth: {
        me: vi.fn().mockRejectedValue(new actual.ApiProblemError({ status: 401, title: 'Unauthorized' })),
        login: vi.fn(),
        logout: vi.fn(),
      },
    },
  }
})

function Probe() {
  const { error, isLoading } = useAuth()
  if (isLoading) return <p>loading</p>
  return <p>{error instanceof ApiProblemError ? 'error-exposed' : 'no-error'}</p>
}

describe('AuthProvider', () => {
  it('ne izlaže 401 sa inicijalne /auth/me provere kao korisničku grešku', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuthProvider>
          <Probe />
        </AuthProvider>
      </QueryClientProvider>,
    )

    await waitFor(() => expect(screen.getByText('no-error')).toBeInTheDocument())
  })
})
