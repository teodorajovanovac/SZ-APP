import LogoutIcon from '@mui/icons-material/Logout'
import MenuIcon from '@mui/icons-material/Menu'
import {
  AppBar,
  Autocomplete,
  Avatar,
  Box,
  Divider,
  Drawer,
  FormControl,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  ListSubheader,
  MenuItem,
  Select,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Toolbar,
  Tooltip,
  Typography,
  useMediaQuery,
} from '@mui/material'
import { alpha, useTheme } from '@mui/material/styles'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../features/auth/useAuth'
import type { CompanyScope } from '../features/companies/companyScope'
import { useActiveCompany } from '../features/companies/useActiveCompany'
import { useCompanyScope } from '../features/companies/useCompanyScope'
import { useLocationCategories } from '../features/master-data/useMasterData'
import type { LocationCategory } from '../features/master-data/types'
import { navigationItems, type NavigationItem } from './navigation'

const drawerWidth = 264

interface LocationCategoryOption {
  id: number
  label: string
}

// Flattens the parentId-linked category tree into a searchable list of full paths
// (e.g. "BW", "BW / Plot 24"), depth-first so children follow their parent — the
// backend already orders siblings by SortIndex/Name, we just preserve that order.
function flattenLocationCategories(categories: LocationCategory[]): LocationCategoryOption[] {
  const byParent = new Map<number | null, LocationCategory[]>()
  for (const category of categories) {
    const siblings = byParent.get(category.parentId) ?? []
    siblings.push(category)
    byParent.set(category.parentId, siblings)
  }
  const options: LocationCategoryOption[] = []
  const visit = (parentId: number | null, prefix: string, ancestors: Set<number>) => {
    for (const node of byParent.get(parentId) ?? []) {
      if (ancestors.has(node.id)) continue // defensive: ignore a cyclic parentId
      const label = prefix ? `${prefix} / ${node.name}` : node.name
      options.push({ id: node.id, label })
      visit(node.id, label, new Set(ancestors).add(node.id))
    }
  }
  visit(null, '', new Set())
  return options
}

// Seventeen flat links read as a wall. Grouping them by what the user is doing
// (records / invoicing / books / system) makes the list scannable; the dashboard
// stays ungrouped at the top. Any nav item not listed here falls into "system",
// so adding a route to navigation.ts can never make it disappear from the menu.
const navGroups: Array<{ titleKey: string | null; paths: string[] }> = [
  { titleKey: null, paths: ['/'] },
  { titleKey: 'shell.groupMasterData', paths: ['/companies', '/partners', '/addresses', '/staff', '/units', '/contracts'] },
  { titleKey: 'shell.groupBilling', paths: ['/billing', '/suppliers', '/notices'] },
  { titleKey: 'shell.groupAccounting', paths: ['/ledger', '/banking', '/reports'] },
  { titleKey: 'shell.groupSystem', paths: ['/documents', '/email', '/imports', '/administration'] },
]

function groupItems(items: NavigationItem[]) {
  const assigned = new Set(navGroups.flatMap((group) => group.paths))
  return navGroups.map((group, index) => ({
    titleKey: group.titleKey,
    items: items.filter((item) =>
      index === navGroups.length - 1 && !assigned.has(item.path)
        ? true
        : group.paths.includes(item.path),
    ),
  })).filter((group) => group.items.length > 0)
}

export function AppShell() {
  const theme = useTheme()
  const isDesktop = useMediaQuery(theme.breakpoints.up('lg'))
  const [mobileOpen, setMobileOpen] = useState(false)
  const { t, i18n } = useTranslation()
  const { user, logout } = useAuth()
  const { companies, activeCompany } = useActiveCompany()
  const { scope, setScope } = useCompanyScope()
  const [scopeMode, setScopeMode] = useState<CompanyScope['mode']>(scope.mode)
  // Lazily fetched: only once the user actually switches to "Po lokaciji" — a
  // fresh page load in single-company mode never fires this request.
  const { data: locationCategories = [] } = useLocationCategories(scopeMode === 'location' ? activeCompany.id : 0)
  const locationOptions = useMemo(() => flattenLocationCategories(locationCategories), [locationCategories])
  const selectedLocationOption =
    scope.mode === 'location' ? locationOptions.find((option) => option.id === scope.locationCategoryId) ?? null : null

  const handleSelectCompany = (companyId: number) => setScope({ mode: 'single', companyId })

  const handleScopeModeChange = (_: unknown, nextMode: CompanyScope['mode'] | null) => {
    if (!nextMode) return
    setScopeMode(nextMode)
    if (nextMode === 'single') setScope({ mode: 'single', companyId: activeCompany.id })
    if (nextMode === 'all') setScope({ mode: 'all' })
    // 'location' commits once a category is actually picked below.
  }

  const handleSelectLocationCategory = (option: LocationCategoryOption | null) => {
    if (option) setScope({ mode: 'location', locationCategoryId: option.id })
  }

  const visibleItems = navigationItems.filter(
    (item) => !item.roles || user?.roles.some((role) => item.roles?.includes(role)),
  )
  const groups = groupItems(visibleItems)

  const changeLanguage = async (language: string) => {
    localStorage.setItem('sz.language', language)
    await i18n.changeLanguage(language)
    document.documentElement.lang = language
  }

  const onDarkSurface = {
    color: 'common.white',
    '.MuiOutlinedInput-notchedOutline': { borderColor: alpha('#fff', 0.45) },
    '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: alpha('#fff', 0.75) },
    '.MuiSvgIcon-root': { color: 'common.white' },
    '.MuiInputLabel-root': { color: alpha('#fff', 0.85) },
    '.MuiInputLabel-root.Mui-focused': { color: 'common.white' },
  }

  const companySelect = (
    <Autocomplete
      size="small"
      disableClearable
      fullWidth
      sx={{ width: isDesktop ? 240 : '100%' }}
      options={companies}
      value={activeCompany}
      isOptionEqualToValue={(option, value) => option.id === value.id}
      getOptionLabel={(option) => option.name}
      onChange={(_, value) => value && handleSelectCompany(value.id)}
      renderInput={(params) => (
        <TextField {...params} label={t('activeCompany')} sx={isDesktop ? onDarkSurface : undefined} />
      )}
    />
  )

  const scopeToggle = (
    <ToggleButtonGroup
      size="small"
      exclusive
      fullWidth={!isDesktop}
      value={scopeMode}
      onChange={handleScopeModeChange}
      aria-label={t('companyScope.groupLabel')}
      sx={
        isDesktop
          ? {
              '.MuiToggleButton-root': { color: alpha('#fff', 0.85), borderColor: alpha('#fff', 0.45) },
              '.MuiToggleButton-root.Mui-selected': { color: 'common.white', bgcolor: alpha('#fff', 0.18) },
            }
          : undefined
      }
    >
      <ToggleButton value="single">{t('companyScope.single')}</ToggleButton>
      <ToggleButton value="all">{t('companyScope.all')}</ToggleButton>
      <ToggleButton value="location">{t('companyScope.location')}</ToggleButton>
    </ToggleButtonGroup>
  )

  const locationCategorySelect = scopeMode === 'location' && (
    <Autocomplete
      size="small"
      fullWidth
      sx={{ width: isDesktop ? 220 : '100%' }}
      options={locationOptions}
      value={selectedLocationOption}
      isOptionEqualToValue={(option, value) => option.id === value.id}
      getOptionLabel={(option) => option.label}
      noOptionsText={t('companyScope.noLocationCategories')}
      onChange={(_, value) => handleSelectLocationCategory(value)}
      renderInput={(params) => (
        <TextField {...params} label={t('companyScope.locationCategory')} sx={isDesktop ? onDarkSurface : undefined} />
      )}
    />
  )

  const languageSelect = (
    <FormControl size="small" sx={{ minWidth: isDesktop ? 84 : undefined }} fullWidth={!isDesktop}>
      <Select
        value={i18n.resolvedLanguage ?? 'sr-Latn'}
        onChange={(event) => void changeLanguage(event.target.value)}
        inputProps={{ 'aria-label': t('language') }}
        sx={isDesktop ? onDarkSurface : undefined}
      >
        <MenuItem value="sr-Latn">LAT</MenuItem>
        <MenuItem value="sr-Cyrl">ЋИР</MenuItem>
        <MenuItem value="en">EN</MenuItem>
      </Select>
    </FormControl>
  )

  const drawer = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column', minHeight: 0 }}>
      {/* The fixed AppBar sits above the drawer at every width, so this strip is a
          pure spacer — the app name already lives in the header. */}
      <Toolbar sx={{ flexShrink: 0 }} />
      {!isDesktop && (
        <>
          <Stack spacing={1.5} sx={{ px: 2, pt: 2, pb: 1 }}>
            {companySelect}
            {scopeToggle}
            {locationCategorySelect}
            {languageSelect}
          </Stack>
          <Divider sx={{ mt: 1 }} />
        </>
      )}
      {/* minHeight:0 lets this flex child actually shrink, so the list scrolls
          instead of pushing the user footer off the bottom of the viewport. */}
      <Box
        component="nav"
        aria-label={t('navigation')}
        sx={{ flex: 1, minHeight: 0, overflowY: 'auto', overscrollBehavior: 'contain', px: 1.25, py: 1 }}
      >
        {groups.map((group, index) => (
          <List
            key={group.titleKey ?? 'primary'}
            dense
            disablePadding
            sx={{ mb: index === groups.length - 1 ? 0 : 0.5 }}
            subheader={
              group.titleKey ? (
                <ListSubheader disableSticky sx={{ px: 1.5, pt: 1 }}>{t(group.titleKey)}</ListSubheader>
              ) : undefined
            }
          >
            {group.items.map((item) => {
              const Icon = item.icon
              return (
                <ListItemButton
                  key={item.path}
                  component={NavLink}
                  to={item.path}
                  end={item.path === '/'}
                  onClick={() => setMobileOpen(false)}
                  sx={{
                    minHeight: 40,
                    mb: 0.25,
                    color: 'text.primary',
                    '&.active': {
                      bgcolor: 'action.selected',
                      color: 'primary.main',
                      fontWeight: 700,
                      '& .MuiListItemIcon-root': { color: 'primary.main' },
                      '& .MuiListItemText-primary': { fontWeight: 700 },
                    },
                  }}
                >
                  <ListItemIcon><Icon fontSize="small" aria-hidden="true" /></ListItemIcon>
                  <ListItemText primary={t(item.labelKey)} slotProps={{ primary: { variant: 'body2' } }} />
                </ListItemButton>
              )
            })}
          </List>
        ))}
      </Box>
      <Divider />
      <Stack direction="row" spacing={1.25} alignItems="center" sx={{ p: 2, flexShrink: 0 }}>
        <Avatar sx={{ width: 34, height: 34, bgcolor: 'primary.main', fontSize: '0.8125rem', fontWeight: 700 }}>
          {(user?.displayName ?? '?').slice(0, 2).toUpperCase()}
        </Avatar>
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="body2" fontWeight={600} noWrap>{user?.displayName}</Typography>
          <Typography variant="caption" color="text.secondary" noWrap display="block">
            {user?.roles.join(', ')}
          </Typography>
        </Box>
      </Stack>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      <Box
        component="a"
        href="#main-content"
        sx={{
          position: 'fixed', top: -100, left: 8, zIndex: 2000, '&:focus': { top: 8 },
          bgcolor: 'background.paper', color: 'text.primary', px: 2, py: 1, borderRadius: 1, boxShadow: 3,
        }}
      >
        {t('skipToContent')}
      </Box>
      <AppBar position="fixed" sx={{ zIndex: theme.zIndex.drawer + 1 }}>
        <Toolbar sx={{ gap: 1 }}>
          {!isDesktop && (
            <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(true)} aria-label={t('openNavigation')}>
              <MenuIcon />
            </IconButton>
          )}
          <Typography variant="h6" component="p" sx={{ flexGrow: 1, minWidth: 0 }} noWrap>
            {t('appName')}
          </Typography>
          {/* On phones the company and language pickers live in the drawer — the
              header keeps the product name and the one destructive action. */}
          {isDesktop && (
            <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" sx={{ rowGap: 1 }}>
              {companySelect}
              {scopeToggle}
              {locationCategorySelect}
              {languageSelect}
            </Stack>
          )}
          <Tooltip title={t('logout')}>
            <IconButton color="inherit" onClick={() => void logout()} aria-label={t('logout')}>
              <LogoutIcon />
            </IconButton>
          </Tooltip>
        </Toolbar>
      </AppBar>
      <Box component="aside" sx={{ width: { lg: drawerWidth }, flexShrink: { lg: 0 } }}>
        <Drawer
          variant={isDesktop ? 'permanent' : 'temporary'}
          open={isDesktop || mobileOpen}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{ '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box' } }}
        >
          {drawer}
        </Drawer>
      </Box>
      <Box
        component="main"
        id="main-content"
        tabIndex={-1}
        sx={{
          flexGrow: 1,
          minWidth: 0,
          px: { xs: 2, sm: 3, lg: 4 },
          pb: { xs: 4, sm: 6 },
          pt: { xs: 2.5, sm: 3 },
          mt: { xs: 7, sm: 8 },
        }}
      >
        <Outlet />
      </Box>
    </Box>
  )
}
