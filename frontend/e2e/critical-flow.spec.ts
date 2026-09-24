import { expect, test } from '@playwright/test'

test('prijava, aktivna kompanija i pristup izveštajima', async ({ page }) => {
  const user = { id: 1, displayName: 'Root korisnik', email: 'root@example.test', preferredLanguage: 'sr-Latn', roles: ['Root'], companies: [{ id: 10, name: 'SZ Test', registrationNumber: '12345678' }] }
  let authenticated = false
  await page.route('**/api/v1/**', async route => {
    const url = new URL(route.request().url())
    if (url.pathname === '/api/v1/auth/antiforgery') return route.fulfill({ json: { token: 'test-token', headerName: 'X-CSRF-TOKEN' } })
    if (url.pathname === '/api/v1/auth/login') { authenticated = true; return route.fulfill({ json: user }) }
    if (url.pathname === '/api/v1/auth/me') return authenticated ? route.fulfill({ json: user }) : route.fulfill({ status: 401, json: { status: 401, title: 'Unauthorized' } })
    if (url.pathname.endsWith('/analyses') || url.pathname.endsWith('/reports')) return route.fulfill({ json: [] })
    return route.fulfill({ json: { items: [], page: 1, pageSize: 25, totalCount: 0 } })
  })
  await page.goto('/login')
  await page.getByLabel(/E-po/).fill('root@example.test')
  await page.getByLabel('Lozinka').fill('Strong-Test-123!')
  await page.getByRole('button', { name: 'Prijavi se' }).click()
  await expect(page.getByRole('combobox', { name: 'Aktivna kompanija' })).toHaveValue('SZ Test')
  await page.locator('a[href="/reports"]').click()
  await expect(page.getByRole('heading', { name: 'Analize i izveštaji' })).toBeVisible()
})
