import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { AuthContext, type AuthContextValue } from './authContext'
import { LoginPage } from './LoginPage'

const defaultAuth: AuthContextValue = {
  user: null,
  isLoading: false,
  isLoginPending: false,
  error: null,
  login: vi.fn().mockResolvedValue(undefined),
  logout: vi.fn().mockResolvedValue(undefined),
}

function renderPage(auth: AuthContextValue = defaultAuth) {
  return render(
    <MemoryRouter>
      <AuthContext.Provider value={auth}>
        <LoginPage />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

describe('LoginPage', () => {
  it('ima pristupačan naslov i označena polja', () => {
    renderPage()
    expect(screen.getByRole('heading', { name: 'SZ Upravljanje', level: 1 })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'E-pošta' })).toHaveAttribute('autocomplete', 'username')
    expect(screen.getByLabelText('Lozinka')).toHaveAttribute('type', 'password')
    expect(screen.getByRole('button', { name: 'Prijavi se' })).toBeInTheDocument()
  })

  it('validira podatke pre slanja', async () => {
    const user = userEvent.setup()
    const login = vi.fn().mockResolvedValue(undefined)
    renderPage({ ...defaultAuth, login })

    await user.click(screen.getByRole('button', { name: 'Prijavi se' }))

    expect(await screen.findByText('Unesite ispravnu adresu e-pošte.')).toBeInTheDocument()
    expect(screen.getByText('Lozinka je obavezna.')).toBeInTheDocument()
    expect(login).not.toHaveBeenCalled()
  })
})
