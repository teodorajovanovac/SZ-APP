import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import type { CompanyContext as CompanyContextDto, CurrentUser } from '../../api/generated/client'
import { AuthContext, type AuthContextValue } from '../../features/auth/authContext'
import { CompanyContext } from '../../features/companies/companyContext'
import { RoleGuard } from './RoleGuard'

const { context } = vi.hoisted(() => ({ context: vi.fn<(id: number) => Promise<CompanyContextDto>>() }))
vi.mock('../../api/generated/client', async () => {
  const actual = await vi.importActual<typeof import('../../api/generated/client')>('../../api/generated/client')
  return { ...actual, api: { ...actual.api, companies: { ...actual.api.companies, context } } }
})

// Non-Root staff never have global Identity roles; their role comes from the active company.
const user: CurrentUser = { id: 3, displayName: 'u', email: 'u@example.test', roles: [], companies: [{ id: 11, name: 'SZ 11' }] }
const auth: AuthContextValue = { user, isLoading: false, isLoginPending: false, error: null, login: vi.fn(), logout: vi.fn() }

function renderGuard() {
  render(
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      <AuthContext.Provider value={auth}>
        <CompanyContext.Provider value={{ companies: user.companies, activeCompany: user.companies[0]!, selectCompany: vi.fn() }}>
          <MemoryRouter>
            <Routes>
              <Route element={<RoleGuard allowedRoles={['Root', 'Upravnik']} />}>
                <Route index element={<p>admin page</p>} />
              </Route>
            </Routes>
          </MemoryRouter>
        </CompanyContext.Provider>
      </AuthContext.Provider>
    </QueryClientProvider>,
  )
}

describe('RoleGuard', () => {
  it('pušta Upravnika aktivne SZ iako nema globalnu ulogu', async () => {
    context.mockResolvedValue({ id: 11, shortName: 'SZ 11', role: 'Upravnik' })
    renderGuard()
    expect(await screen.findByText('admin page')).toBeInTheDocument()
  })

  it('odbija Moderatora', async () => {
    context.mockResolvedValue({ id: 11, shortName: 'SZ 11', role: 'Moderator' })
    renderGuard()
    expect(await screen.findByRole('alert')).toBeInTheDocument()
    expect(screen.queryByText('admin page')).not.toBeInTheDocument()
  })
})
