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
import { useCompanyRole } from '../features/companies/useCompanyRole'
import { AddressesPage, BuildingEntrancesPage, CompanyPage, LocationCategoriesPage, PartnersPage, UnitsPage } from '../features/master-data/MasterDataPages'
import { StaffListPage } from '../features/staff/StaffListPage'
import { StaffDetailPage } from '../features/staff/StaffDetailPage'
import { SettingsPage } from '../features/administration/SettingsPage'
import { ShortListsPage } from '../features/administration/ShortListsPage'
import { BuildingEntranceDetailPage } from '../features/master-data/pages/BuildingEntranceDetailPage'
import { PartnerDetailPage } from '../features/master-data/pages/PartnerDetailPage'
import { PartnerEditPage } from '../features/master-data/pages/PartnerEditPage'
import { UnitDetailPage } from '../features/master-data/pages/UnitDetailPage'
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
import { ExportPage } from '../features/imports/ExportPage'
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
              <Route path="location-categories" element={<LocationCategoriesPage />} />
              <Route path="imports" element={<EtlRunsPage />} />
              <Route path="export" element={<ExportPage />} />
              <Route path="administration" element={<AdministrationRoute />} />
            </Route>
            <Route path="partners" element={<PartnersPage />} />
            <Route path="partners/new" element={<PartnerEditPage />} />
            <Route path="partners/:companyId/:partnerId/edit" element={<PartnerEditPage />} />
            <Route path="addresses" element={<AddressesPage />} />
            <Route element={<RoleGuard allowedRoles={['Root', 'Upravnik']} />}>
              <Route path="staff" element={<StaffListPage />} />
              <Route path="staff/:staffId" element={<StaffDetailPage />} />
              <Route path="settings" element={<SettingsPage />} />
            </Route>
            <Route element={<RoleGuard allowedRoles={['Root']} />}>
              <Route path="short-lists" element={<ShortListsPage />} />
            </Route>
            <Route path="units" element={<UnitsPage />} />
            <Route path="contracts" element={<ContractsPage />} />
            <Route path="partners/:companyId/:partnerId" element={<PartnerDetailPage />} />
            <Route path="units/:companyId/:unitId" element={<UnitDetailPage />} />
            <Route path="building-entrances" element={<BuildingEntrancesPage />} />
            <Route path="building-entrances/:companyId/:entranceId" element={<BuildingEntranceDetailPage />} />
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

const useCompanyPermissions = useCompanyRole
function AdministrationRoute() { const { companyId } = useCompanyPermissions(); return <PlatformAdministrationPage companyId={companyId} /> }
function BillingRoute() { const { companyId, canWrite } = useCompanyPermissions(); return <BillingWorkspace companyId={companyId} canPost={canWrite} /> }
function SupplierRoute() { const { companyId, canWrite } = useCompanyPermissions(); return <SupplierInvoiceList companyId={companyId} canPost={canWrite} /> }
function LedgerRoute() { const { canWrite, canAdmin } = useCompanyPermissions(); return <LedgerBankingPage canPost={canWrite} canUnlock={canAdmin} /> }
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
