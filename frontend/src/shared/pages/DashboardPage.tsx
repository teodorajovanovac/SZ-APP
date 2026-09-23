import AccountBalanceIcon from '@mui/icons-material/AccountBalance'
import ApartmentIcon from '@mui/icons-material/Apartment'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import GroupsIcon from '@mui/icons-material/Groups'
import NotificationsIcon from '@mui/icons-material/Notifications'
import PaymentsIcon from '@mui/icons-material/Payments'
import QueryStatsIcon from '@mui/icons-material/QueryStats'
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong'
import type { SvgIconComponent } from '@mui/icons-material'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Grid,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material'
import { alpha } from '@mui/material/styles'
import { useQueries } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink } from 'react-router-dom'
import { apiRequest } from '../../api/generated/client'
import { useAuth } from '../../features/auth/useAuth'
import { useActiveCompany } from '../../features/companies/useActiveCompany'
import type { BillingPage, InvoiceBatch } from '../../features/billing/types'
import { ledgerBankingApi } from '../../features/ledger-banking/ledgerBankingApi'
import type { BankStatementSummary, PageResponse } from '../../features/ledger-banking/types'
import { masterDataApi } from '../../features/master-data/masterDataApi'
import type { PagedResponse } from '../../features/master-data/types'

type StatusTone = 'default' | 'success' | 'warning' | 'info'

const RECENT_ROWS = 5
// Count-only queries ask for a single row: every paged endpoint returns totalCount,
// so this is the cheapest accurate count the API can give us.
const COUNT_PAGE = { page: 1, pageSize: 1 }

function formatPeriod(periodYYMM: number) {
  const month = periodYYMM % 100
  const year = 2000 + Math.floor(periodYYMM / 100)
  return `${String(month).padStart(2, '0')}/${year}`
}

export function DashboardPage() {
  const { t, i18n } = useTranslation()
  const { user } = useAuth()
  const { activeCompany } = useActiveCompany()
  const companyId = activeCompany.id
  const locale = i18n.resolvedLanguage === 'en' ? 'en' : 'sr-Latn-RS'
  const numberFormat = new Intl.NumberFormat(locale)
  const amountFormat = new Intl.NumberFormat(locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 })

  // One parallel burst, not a waterfall: four counts + two short recent lists.
  const results = useQueries({
    queries: [
      {
        queryKey: ['dashboard', companyId, 'partners-count'],
        queryFn: () => masterDataApi.partners.list(companyId, COUNT_PAGE),
      },
      {
        queryKey: ['dashboard', companyId, 'units-count'],
        queryFn: () => masterDataApi.units.list(companyId, COUNT_PAGE),
      },
      {
        queryKey: ['dashboard', companyId, 'unposted-journals-count'],
        queryFn: () =>
          apiRequest<PageResponse<unknown>>(
            `/api/v1/companies/${companyId}/journal-entries?page=1&pageSize=1&isPosted=false`,
          ),
      },
      {
        queryKey: ['dashboard', companyId, 'notices-count'],
        queryFn: () =>
          apiRequest<BillingPage<unknown>>(`/api/v1/companies/${companyId}/notices?page=1&pageSize=1`),
      },
      {
        queryKey: ['dashboard', companyId, 'recent-batches'],
        queryFn: () =>
          apiRequest<BillingPage<InvoiceBatch>>(
            `/api/v1/companies/${companyId}/invoice-batches?page=1&pageSize=${RECENT_ROWS}`,
          ),
      },
      {
        queryKey: ['dashboard', companyId, 'recent-statements'],
        queryFn: () => ledgerBankingApi.statements.list(companyId, 0, RECENT_ROWS),
      },
    ],
  })

  const [partners, units, unpostedJournals, notices, batches, statements] = results as [
    { data?: PagedResponse<unknown>; isPending: boolean },
    { data?: PagedResponse<unknown>; isPending: boolean },
    { data?: PageResponse<unknown>; isPending: boolean },
    { data?: BillingPage<unknown>; isPending: boolean },
    { data?: BillingPage<InvoiceBatch>; isPending: boolean },
    { data?: PageResponse<BankStatementSummary>; isPending: boolean },
  ]
  const hasError = results.some((result) => result.isError)

  const tiles = [
    { key: 'partners', labelKey: 'dashboard_.kpiPartners', icon: GroupsIcon, to: '/partners', value: partners.data?.totalCount, loading: partners.isPending, tone: 'default' as StatusTone },
    { key: 'units', labelKey: 'dashboard_.kpiUnits', icon: ApartmentIcon, to: '/units', value: units.data?.totalCount, loading: units.isPending, tone: 'default' as StatusTone },
    { key: 'journals', labelKey: 'dashboard_.kpiUnpostedJournals', icon: AccountBalanceIcon, to: '/ledger', value: unpostedJournals.data?.totalCount, loading: unpostedJournals.isPending, tone: 'warning' as StatusTone },
    { key: 'notices', labelKey: 'dashboard_.kpiNotices', icon: NotificationsIcon, to: '/notices', value: notices.data?.totalCount, loading: notices.isPending, tone: 'info' as StatusTone },
  ]

  const quickActions = [
    { labelKey: 'dashboard_.actionNewBatch', to: '/billing', icon: ReceiptLongIcon, primary: true },
    { labelKey: 'dashboard_.actionImportStatement', to: '/banking', icon: PaymentsIcon, primary: false },
    { labelKey: 'dashboard_.actionNotices', to: '/notices', icon: NotificationsIcon, primary: false },
    { labelKey: 'dashboard_.actionReports', to: '/reports', icon: QueryStatsIcon, primary: false },
  ]

  return (
    <Box component="section">
      <Stack spacing={0.5} sx={{ mb: 3 }}>
        <Typography component="h1" variant="h1">
          {t('welcome')}, {user?.displayName}
        </Typography>
        <Typography color="text.secondary">
          {t('dashboard_.subtitle', { company: activeCompany.name })}
        </Typography>
      </Stack>

      {hasError && <Alert severity="warning" sx={{ mb: 3 }}>{t('dashboard_.loadFailed')}</Alert>}

      <Grid container spacing={2} sx={{ mb: 3 }}>
        {tiles.map((tile) => (
          <Grid key={tile.key} size={{ xs: 6, md: 3 }}>
            <StatTile
              label={t(tile.labelKey)}
              value={tile.loading ? null : tile.value}
              format={numberFormat}
              icon={tile.icon}
              tone={tile.tone}
              to={tile.to}
            />
          </Grid>
        ))}
      </Grid>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="overline" color="text.secondary" component="h2" sx={{ display: 'block', mb: 1.5 }}>
            {t('dashboard_.quickActions')}
          </Typography>
          <Stack direction="row" spacing={1.5} useFlexGap flexWrap="wrap">
            {quickActions.map((action) => (
              <Button
                key={action.to + action.labelKey}
                component={RouterLink}
                to={action.to}
                variant={action.primary ? 'contained' : 'outlined'}
                color={action.primary ? 'primary' : 'inherit'}
                startIcon={<action.icon />}
                sx={action.primary ? undefined : { borderColor: 'divider', color: 'text.primary' }}
              >
                {t(action.labelKey)}
              </Button>
            ))}
          </Stack>
        </CardContent>
      </Card>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, lg: 6 }}>
          <PanelCard title={t('dashboard_.recentBatches')} to="/billing" viewAllLabel={t('dashboard_.viewAll')}>
            {batches.isPending ? (
              <RowSkeletons />
            ) : batches.data && batches.data.items.length > 0 ? (
              <RowList
                rows={batches.data.items.map((batch) => ({
                  id: batch.id,
                  primary: batch.caption || formatPeriod(batch.periodYYMM),
                  secondary: formatPeriod(batch.periodYYMM),
                  chipLabel: t(`dashboard_.batchStatus.${batch.status}`),
                  tone: batch.status === 'Posted' ? 'success' : batch.status === 'Generated' ? 'info' : 'default',
                }))}
              />
            ) : (
              <EmptyState text={t('dashboard_.emptyBatches')} />
            )}
          </PanelCard>
        </Grid>
        <Grid size={{ xs: 12, lg: 6 }}>
          <PanelCard title={t('dashboard_.recentStatements')} to="/banking" viewAllLabel={t('dashboard_.viewAll')}>
            {statements.isPending ? (
              <RowSkeletons />
            ) : statements.data && statements.data.items.length > 0 ? (
              <RowList
                rows={statements.data.items.map((statement) => ({
                  id: statement.id,
                  primary: `${statement.statementNumber}${statement.statementSuffix ?? ''} · ${new Date(statement.date).toLocaleDateString(locale)}`,
                  secondary: amountFormat.format(statement.newBalance),
                  chipLabel: t(`dashboard_.statementStatus.${statement.status}`),
                  tone: statement.status === 'Posted' ? 'success' : statement.status === 'Ready' ? 'info' : 'warning',
                }))}
              />
            ) : (
              <EmptyState text={t('dashboard_.emptyStatements')} />
            )}
          </PanelCard>
        </Grid>
      </Grid>
    </Box>
  )
}

interface StatTileProps {
  label: string
  value: number | null | undefined
  format: Intl.NumberFormat
  icon: SvgIconComponent
  tone: StatusTone
  to: string
}

function StatTile({ label, value, format, icon: Icon, tone, to }: StatTileProps) {
  // A headline number is a stat tile, not a one-bar chart. Tone is carried by the
  // icon chip and always paired with the written label — never colour alone.
  const toneColor = tone === 'default' ? 'primary.main' : `${tone}.main`
  return (
    <Card
      component={RouterLink}
      to={to}
      sx={{
        display: 'block',
        height: '100%',
        textDecoration: 'none',
        transition: 'border-color .15s, background-color .15s',
        '&:hover': { borderColor: 'primary.light', bgcolor: 'action.hover' },
      }}
    >
      <CardContent>
        <Stack direction="row" alignItems="flex-start" justifyContent="space-between" spacing={1}>
          <Typography variant="overline" color="text.secondary" sx={{ lineHeight: 1.4 }}>
            {label}
          </Typography>
          <Box
            sx={(theme) => ({
              display: 'grid',
              placeItems: 'center',
              width: 32,
              height: 32,
              borderRadius: 1,
              flexShrink: 0,
              color: toneColor,
              bgcolor: alpha(
                tone === 'default' ? theme.palette.primary.main : theme.palette[tone].main,
                0.1,
              ),
            })}
          >
            <Icon fontSize="small" aria-hidden="true" />
          </Box>
        </Stack>
        {value == null ? (
          <Skeleton variant="text" width={72} sx={{ fontSize: '2rem', mt: 0.5 }} />
        ) : (
          <Typography sx={{ fontSize: '2rem', fontWeight: 700, lineHeight: 1.15, mt: 0.5, color: 'text.primary' }}>
            {format.format(value)}
          </Typography>
        )}
      </CardContent>
    </Card>
  )
}

function PanelCard({ title, to, viewAllLabel, children }: { title: string; to: string; viewAllLabel: string; children: React.ReactNode }) {
  return (
    <Card sx={{ height: '100%' }}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" spacing={1} sx={{ px: 2.5, py: 1.75 }}>
        <Typography variant="h3" component="h2">{title}</Typography>
        <Button
          component={RouterLink}
          to={to}
          size="small"
          endIcon={<ArrowForwardIcon />}
          sx={{ flexShrink: 0, whiteSpace: 'nowrap' }}
        >
          {viewAllLabel}
        </Button>
      </Stack>
      <Divider />
      <Box sx={{ px: 2.5, py: 1 }}>{children}</Box>
    </Card>
  )
}

interface PanelRow {
  id: number
  primary: string
  secondary: string
  chipLabel: string
  tone: StatusTone
}

function RowList({ rows }: { rows: PanelRow[] }) {
  return (
    <Stack divider={<Divider />}>
      {rows.map((row) => (
        <Stack
          key={row.id}
          direction="row"
          alignItems="center"
          justifyContent="space-between"
          spacing={1.5}
          sx={{ py: 1.25 }}
        >
          <Box sx={{ minWidth: 0 }}>
            <Typography variant="body2" fontWeight={600} noWrap>{row.primary}</Typography>
            <Typography variant="caption" color="text.secondary" noWrap display="block">{row.secondary}</Typography>
          </Box>
          <Chip
            size="small"
            label={row.chipLabel}
            color={row.tone === 'default' ? 'default' : row.tone}
            variant={row.tone === 'default' ? 'outlined' : 'filled'}
          />
        </Stack>
      ))}
    </Stack>
  )
}

function RowSkeletons() {
  return (
    <Stack divider={<Divider />}>
      {[0, 1, 2].map((row) => (
        <Stack key={row} direction="row" alignItems="center" justifyContent="space-between" sx={{ py: 1.25 }}>
          <Box sx={{ width: '60%' }}>
            <Skeleton variant="text" width="80%" />
            <Skeleton variant="text" width="40%" />
          </Box>
          <Skeleton variant="rounded" width={72} height={22} />
        </Stack>
      ))}
    </Stack>
  )
}

function EmptyState({ text }: { text: string }) {
  return (
    <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
      {text}
    </Typography>
  )
}
