import { CssBaseline } from '@mui/material'
import { ThemeProvider } from '@mui/material/styles'
import { QueryClientProvider } from '@tanstack/react-query'
import { lazy, Suspense, type ComponentType } from 'react'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthGate } from '../features/auth/AuthGate'
import { AuthProvider } from '../features/auth/AuthProvider'
import { LoginPage } from '../features/auth/LoginPage'
import { useAuth } from '../features/auth/useAuth'
import { ActiveCompanyProvider } from '../features/companies/ActiveCompanyProvider'
import { CompanyScopeProvider } from '../features/companies/CompanyScopeProvider'
import { useCompanyRole } from '../features/companies/useCompanyRole'
import { NotifyProvider } from '../shared/feedback/NotifyProvider'
import { PageSkeleton } from '../shared/components/PageSkeleton'
import { NotFoundPage } from '../shared/pages/NotFoundPage'
import { RoleGuard } from '../shared/routing/RoleGuard'
import { AppShell } from './AppShell'
import { queryClient } from './queryClient'
import { theme } from './theme'

// PERF-01: one chunk per screen/module. `page` picks a named export so feature
// modules keep their named exports (no default-export churn).
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function page<M extends Record<K, ComponentType<any>>, K extends keyof M>(load: () => Promise<M>, name: K) {
  return lazy(() => load().then((module) => ({ default: module[name] })))
}
const masterData = () => import('../features/master-data/MasterDataPages')
const DashboardPage = page(() => import('../shared/pages/DashboardPage'), 'DashboardPage')
const CompanyPage = page(masterData, 'CompanyPage')
const LocationCategoriesPage = page(masterData, 'LocationCategoriesPage')
const PartnersPage = page(masterData, 'PartnersPage')
const AddressesPage = page(masterData, 'AddressesPage')
const UnitsPage = page(masterData, 'UnitsPage')
const BuildingEntrancesPage = page(masterData, 'BuildingEntrancesPage')
const StaffListPage = page(() => import('../features/staff/StaffListPage'), 'StaffListPage')
const StaffDetailPage = page(() => import('../features/staff/StaffDetailPage'), 'StaffDetailPage')
const SettingsPage = page(() => import('../features/administration/SettingsPage'), 'SettingsPage')
const ShortListsPage = page(() => import('../features/administration/ShortListsPage'), 'ShortListsPage')
const AuditLogPage = page(() => import('../features/administration/AuditLogPage'), 'AuditLogPage')
const BuildingEntranceDetailPage = page(() => import('../features/master-data/pages/BuildingEntranceDetailPage'), 'BuildingEntranceDetailPage')
const PartnerDetailPage = page(() => import('../features/master-data/pages/PartnerDetailPage'), 'PartnerDetailPage')
const PartnerEditPage = page(() => import('../features/master-data/pages/PartnerEditPage'), 'PartnerEditPage')
const UnitDetailPage = page(() => import('../features/master-data/pages/UnitDetailPage'), 'UnitDetailPage')
const ContractsPage = page(() => import('../features/contracts/ContractsPage'), 'ContractsPage')
const BillingWorkspace = page(() => import('../features/billing/BillingWorkspace'), 'BillingWorkspace')
const SupplierInvoiceList = page(() => import('../features/suppliers/SupplierInvoiceList'), 'SupplierInvoiceList')
const LedgerBankingPage = page(() => import('../features/ledger-banking/LedgerBankingPage'), 'LedgerBankingPage')
const NoticeList = page(() => import('../features/notices/NoticeList'), 'NoticeList')
const DocumentsPage = page(() => import('../features/documents'), 'DocumentsPage')
const EmailPage = page(() => import('../features/email'), 'EmailPage')
const PlatformAdministrationPage = page(() => import('../features/administration/PlatformAdministrationPage'), 'PlatformAdministrationPage')
const ReportsPage = page(() => import('../features/reports/ReportsPage'), 'ReportsPage')
const EtlRunsPage = page(() => import('../features/imports/EtlRunsPage'), 'EtlRunsPage')
const ExportPage = page(() => import('../features/imports/ExportPage'), 'ExportPage')
const LedgerCardsPage = page(() => import('../features/ledger-cards/LedgerCardsPage'), 'LedgerCardsPage')

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
              <Route path="audit" element={<AuditLogPage />} />
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
          <NotifyProvider>
            <AuthProvider>
              <Suspense fallback={<PageSkeleton />}>
                <Routes>
                  <Route path="/login" element={<LoginPage />} />
                  <Route element={<AuthGate />}>
                    <Route path="/*" element={<AuthenticatedRoutes />} />
                  </Route>
                </Routes>
              </Suspense>
            </AuthProvider>
          </NotifyProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </ThemeProvider>
  )
}
