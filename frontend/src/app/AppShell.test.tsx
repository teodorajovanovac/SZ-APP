import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ThemeProvider } from '@mui/material/styles'
import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import type { CurrentUser } from '../api/generated/client'
import { AuthContext, type AuthContextValue } from '../features/auth/authContext'
import { CompanyContext } from '../features/companies/companyContext'
import { CompanyScopeContext } from '../features/companies/companyScopeContext'
import type { MenuItemDto } from './useMenu'
import { theme } from './theme'
import { AppShell } from './AppShell'

// vi.mock factories are hoisted above the rest of the module, so the fixture
// data has to be declared through vi.hoisted to be visible inside it.
const { menuItems } = vi.hoisted(() => ({
  menuItems: [
    { id: 1, parentId: null, resourceKey: 'menu.dashboard', caption: 'Početna', iconName: 'Dashboard', path: '/', sortIndex: 0 },
    { id: 4, parentId: null, resourceKey: 'menu.groupCodeLists', caption: 'Šifarnici', iconName: null, path: null, sortIndex: 2 },
    { id: 5, parentId: 4, resourceKey: 'menu.partners', caption: 'Partneri', iconName: 'Groups', path: '/partners', sortIndex: 0 },
    // Administracija carries RequiredRoles "Root,Upravnik" server-side, so a Review-only
    // user simply never receives it in the response -- nothing to filter client-side.
  ] satisfies MenuItemDto[],
}))

vi.mock('../api/generated/client', async () => {
  const actual = await vi.importActual<typeof import('../api/generated/client')>('../api/generated/client')
  return { ...actual, apiRequest: vi.fn().mockResolvedValue(menuItems) }
})

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
  it('izlaže landmark-e i prikazuje stavke menija dobijene sa API-ja', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ThemeProvider theme={theme}>
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
        </ThemeProvider>
      </QueryClientProvider>,
    )

    expect(screen.getByRole('navigation', { name: 'Glavna navigacija' })).toBeInTheDocument()
    expect(screen.getByRole('main')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByRole('link', { name: 'Partneri' })).toBeInTheDocument())
    expect(screen.queryByRole('link', { name: 'Administracija' })).not.toBeInTheDocument()
    expect(screen.getByLabelText('Aktivna kompanija')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Odjavi se' })).toBeInTheDocument()
  })
})
