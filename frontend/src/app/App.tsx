import { CssBaseline } from '@mui/material'
import { ThemeProvider } from '@mui/material/styles'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthGate } from '../features/auth/AuthGate'
import { AuthProvider } from '../features/auth/AuthProvider'
import { LoginPage } from '../features/auth/LoginPage'
import { useAuth } from '../features/auth/useAuth'
import { ActiveCompanyProvider } from '../features/companies/ActiveCompanyProvider'
import { CompanyScopeProvider } from '../features/companies/CompanyScopeProvider'
import { DashboardPage } from '../shared/pages/DashboardPage'
import { NotFoundPage } from '../shared/pages/NotFoundPage'
import { RoleGuard } from '../shared/routing/RoleGuard'
import { AppShell } from './AppShell'
import { theme } from './theme'
import { useActiveCompany } from '../features/companies/useActiveCompany'
import { AddressesPage, CompanyPage, PartnersPage, StaffPage, UnitsPage } from '../features/master-data/MasterDataPages'
import { ContractsPage } from '../features/contracts/ContractsPage'
import { BillingWorkspace } from '../features/billing/BillingWorkspace'
import { SupplierInvoiceList } from '../features/suppliers/SupplierInvoiceList'
import { LedgerBankingPage } from '../features/ledger-banking/LedgerBankingPage'
import { NoticeList } from '../features/notices/NoticeList'
import { DocumentsPage } from '../features/documents'
import { EmailPage } from '../features/email'
import { PlatformAdministrationPage } from '../features/administration/PlatformAdministrationPage'
import { ReportsPage } from '../features/reports/ReportsPage'
import { EtlRunsPage } from '../features/imports/EtlRunsPage'
import { LedgerCardsPage } from '../features/ledger-cards/LedgerCardsPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
})

function AuthenticatedRoutes() {
  const { user } = useAuth()
  if (!user || user.companies.length === 0) {
    return null
  }

  return (
    <ActiveCompanyProvider key={user.id} companies={user.companies}>
      <CompanyScopeProvider>
        <Routes>
          <Route element={<AppShell />}>
            <Route index element={<DashboardPage />} />
            <Route element={<RoleGuard allowedRoles={['Root', 'Upravnik']} />}>
              <Route path="companies" element={<CompanyPage />} />
              <Route path="imports" element={<EtlRunsPage />} />
              <Route path="administration" element={<AdministrationRoute />} />
            </Route>
            <Route path="partners" element={<PartnersPage />} />
            <Route path="addresses" element={<AddressesPage />} />
            <Route element={<RoleGuard allowedRoles={['Root', 'Upravnik']} />}>
              <Route path="staff" element={<StaffPage />} />
            </Route>
            <Route path="units" element={<UnitsPage />} />
            <Route path="contracts" element={<ContractsPage />} />
            <Route path="billing" element={<BillingRoute />} />
            <Route path="suppliers" element={<SupplierRoute />} />
            <Route path="ledger" element={<LedgerRoute />} />
            <Route path="banking" element={<LedgerRoute />} />
            <Route path="kartice" element={<LedgerCardsPage />} />
            <Route path="notices" element={<NoticesRoute />} />
            <Route path="documents" element={<DocumentsRoute />} />
            <Route path="email" element={<EmailRoute />} />
            <Route path="reports" element={<ReportsPage />} />
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </CompanyScopeProvider>
    </ActiveCompanyProvider>
  )
}

function useCompanyPermissions() { const { activeCompany } = useActiveCompany(); const { user } = useAuth(); return { companyId: activeCompany.id, canWrite: Boolean(user?.roles.some(role => role === 'Root' || role === 'Upravnik' || role === 'Moderator')) } }
function AdministrationRoute() { const { companyId } = useCompanyPermissions(); return <PlatformAdministrationPage companyId={companyId} /> }
function BillingRoute() { const { companyId, canWrite } = useCompanyPermissions(); return <BillingWorkspace companyId={companyId} canPost={canWrite} /> }
function SupplierRoute() { const { companyId } = useCompanyPermissions(); return <SupplierInvoiceList companyId={companyId} /> }
function LedgerRoute() { const { canWrite } = useCompanyPermissions(); return <LedgerBankingPage canPost={canWrite} /> }
function NoticesRoute() { const { companyId, canWrite } = useCompanyPermissions(); return <NoticeList companyId={companyId} canWrite={canWrite} /> }
function DocumentsRoute() { const { companyId } = useCompanyPermissions(); return <DocumentsPage companyId={companyId} /> }
function EmailRoute() { const { companyId } = useCompanyPermissions(); return <EmailPage companyId={companyId} /> }

export function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AuthProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route element={<AuthGate />}>
                <Route path="/*" element={<AuthenticatedRoutes />} />
              </Route>
            </Routes>
          </AuthProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </ThemeProvider>
  )
}
