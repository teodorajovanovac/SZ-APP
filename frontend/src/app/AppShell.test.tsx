import { ThemeProvider } from '@mui/material/styles'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import type { CurrentUser } from '../api/generated/client'
import { AuthContext, type AuthContextValue } from '../features/auth/authContext'
import { CompanyContext } from '../features/companies/companyContext'
import { CompanyScopeContext } from '../features/companies/companyScopeContext'
import { theme } from './theme'
import { AppShell } from './AppShell'

const reviewUser: CurrentUser = {
  id: 7,
  displayName: 'Revizor',
  email: 'review@example.test',
  roles: ['Review'],
  companies: [{ id: 11, name: 'Stambena zajednica 11' }],
}

const auth: AuthContextValue = {
  user: reviewUser,
  isLoading: false,
  isLoginPending: false,
  error: null,
  login: vi.fn().mockResolvedValue(undefined),
  logout: vi.fn().mockResolvedValue(undefined),
}

describe('AppShell', () => {
  it('izlaže landmark-e i skriva nedozvoljene stavke', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <ThemeProvider theme={theme}>
        <QueryClientProvider client={queryClient}>
          <MemoryRouter>
            <AuthContext.Provider value={auth}>
              <CompanyContext.Provider
                value={{ companies: reviewUser.companies, activeCompany: reviewUser.companies[0]!, selectCompany: vi.fn() }}
              >
                <CompanyScopeContext.Provider
                  value={{ scope: { mode: 'single', companyId: reviewUser.companies[0]!.id }, setScope: vi.fn() }}
                >
                  <Routes>
                    <Route element={<AppShell />}>
                      <Route index element={<h1>Kontrolna tabla</h1>} />
                    </Route>
                  </Routes>
                </CompanyScopeContext.Provider>
              </CompanyContext.Provider>
            </AuthContext.Provider>
          </MemoryRouter>
        </QueryClientProvider>
      </ThemeProvider>,
    )

    expect(screen.getByRole('navigation', { name: 'Glavna navigacija' })).toBeInTheDocument()
    expect(screen.getByRole('main')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Partneri' })).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Administracija' })).not.toBeInTheDocument()
    expect(screen.getByLabelText('Aktivna kompanija')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Odjavi se' })).toBeInTheDocument()
  })
})
