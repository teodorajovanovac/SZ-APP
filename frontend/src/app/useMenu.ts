import AccountBalanceIcon from '@mui/icons-material/AccountBalance'
import ApartmentIcon from '@mui/icons-material/Apartment'
import BadgeIcon from '@mui/icons-material/Badge'
import BusinessIcon from '@mui/icons-material/Business'
import CreditCardIcon from '@mui/icons-material/CreditCard'
import DashboardIcon from '@mui/icons-material/Dashboard'
import GavelIcon from '@mui/icons-material/Gavel'
import GroupsIcon from '@mui/icons-material/Groups'
import HelpOutlineIcon from '@mui/icons-material/HelpOutline'
import ImportExportIcon from '@mui/icons-material/ImportExport'
import LocalShippingIcon from '@mui/icons-material/LocalShipping'
import NotificationsIcon from '@mui/icons-material/Notifications'
import PaymentsIcon from '@mui/icons-material/Payments'
import QueryStatsIcon from '@mui/icons-material/QueryStats'
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong'
import SettingsIcon from '@mui/icons-material/Settings'
import type { SvgIconComponent } from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { apiRequest } from '../api/generated/client'

// MUI icons can't be resolved dynamically from a string import, so the backend
// sends the component name (IconName on MenuItem) and this maps it locally.
// Keep in sync with the icons seeded in AddMenuItems.
const iconsByName: Record<string, SvgIconComponent> = {
  AccountBalance: AccountBalanceIcon,
  Apartment: ApartmentIcon,
  Badge: BadgeIcon,
  Business: BusinessIcon,
  CreditCard: CreditCardIcon,
  Dashboard: DashboardIcon,
  Gavel: GavelIcon,
  Groups: GroupsIcon,
  ImportExport: ImportExportIcon,
  LocalShipping: LocalShippingIcon,
  Notifications: NotificationsIcon,
  Payments: PaymentsIcon,
  QueryStats: QueryStatsIcon,
  ReceiptLong: ReceiptLongIcon,
  Settings: SettingsIcon,
}

export function iconForName(name: string | null | undefined): SvgIconComponent {
  return (name && iconsByName[name]) || HelpOutlineIcon
}

export interface MenuItemDto {
  id: number
  parentId: number | null
  resourceKey: string
  caption: string
  iconName: string | null
  path: string | null
  sortIndex: number
}

export interface MenuGroup {
  item: MenuItemDto
  children: MenuItemDto[]
}

export function useMenu(companyId: number) {
  return useQuery({
    queryKey: ['platform', companyId, 'menu'] as const,
    queryFn: () => apiRequest<MenuItemDto[]>(`/api/v1/companies/${companyId}/menu`),
    enabled: companyId > 0,
  })
}

/** Groups the flat, role-filtered menu list into top-level entries with their children. */
export function buildMenuGroups(items: MenuItemDto[] | undefined): MenuGroup[] {
  if (!Array.isArray(items)) return []
  const topLevel = [...items.filter((item) => item.parentId === null)].sort((a, b) => a.sortIndex - b.sortIndex)
  return topLevel.map((item) => ({
    item,
    children: items
      .filter((child) => child.parentId === item.id)
      .sort((a, b) => a.sortIndex - b.sortIndex),
  }))
}
