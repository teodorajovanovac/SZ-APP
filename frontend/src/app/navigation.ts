import AccountBalanceIcon from '@mui/icons-material/AccountBalance'
import ApartmentIcon from '@mui/icons-material/Apartment'
import BadgeIcon from '@mui/icons-material/Badge'
import BusinessIcon from '@mui/icons-material/Business'
import DashboardIcon from '@mui/icons-material/Dashboard'
import DescriptionIcon from '@mui/icons-material/Description'
import EmailIcon from '@mui/icons-material/Email'
import GavelIcon from '@mui/icons-material/Gavel'
import GroupsIcon from '@mui/icons-material/Groups'
import ImportExportIcon from '@mui/icons-material/ImportExport'
import LocalShippingIcon from '@mui/icons-material/LocalShipping'
import MapIcon from '@mui/icons-material/Map'
import NotificationsIcon from '@mui/icons-material/Notifications'
import PaymentsIcon from '@mui/icons-material/Payments'
import QueryStatsIcon from '@mui/icons-material/QueryStats'
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong'
import SettingsIcon from '@mui/icons-material/Settings'
import type { SvgIconComponent } from '@mui/icons-material'
import type { UserRole } from '../api/generated/client'

export interface NavigationItem {
  labelKey: string
  path: string
  icon: SvgIconComponent
  roles?: UserRole[]
}

export const navigationItems: NavigationItem[] = [
  { labelKey: 'dashboard', path: '/', icon: DashboardIcon },
  { labelKey: 'companies', path: '/companies', icon: BusinessIcon, roles: ['Root', 'Upravnik'] },
  { labelKey: 'partners', path: '/partners', icon: GroupsIcon },
  { labelKey: 'addresses', path: '/addresses', icon: MapIcon },
  { labelKey: 'staff', path: '/staff', icon: BadgeIcon, roles: ['Root', 'Upravnik'] },
  { labelKey: 'units', path: '/units', icon: ApartmentIcon },
  { labelKey: 'contracts', path: '/contracts', icon: GavelIcon },
  { labelKey: 'billing', path: '/billing', icon: ReceiptLongIcon },
  { labelKey: 'suppliers', path: '/suppliers', icon: LocalShippingIcon },
  { labelKey: 'ledger', path: '/ledger', icon: AccountBalanceIcon },
  { labelKey: 'banking', path: '/banking', icon: PaymentsIcon },
  { labelKey: 'notices', path: '/notices', icon: NotificationsIcon },
  { labelKey: 'documents', path: '/documents', icon: DescriptionIcon },
  { labelKey: 'email', path: '/email', icon: EmailIcon },
  { labelKey: 'imports', path: '/imports', icon: ImportExportIcon, roles: ['Root', 'Upravnik'] },
  { labelKey: 'reports', path: '/reports', icon: QueryStatsIcon },
  { labelKey: 'administration', path: '/administration', icon: SettingsIcon, roles: ['Root', 'Upravnik'] },
]
