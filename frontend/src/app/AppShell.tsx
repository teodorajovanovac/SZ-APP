import KeyboardArrowUpDownIcon from '@mui/icons-material/UnfoldMore'
import LockResetIcon from '@mui/icons-material/LockReset'
import LogoutIcon from '@mui/icons-material/Logout'
import MenuIcon from '@mui/icons-material/Menu'
import SearchIcon from '@mui/icons-material/Search'
import {
  AppBar,
  Autocomplete,
  Avatar,
  Box,
  ButtonBase,
  Divider,
  Drawer,
  FormControl,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  ListSubheader,
  Menu,
  MenuItem,
  Select,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Toolbar,
  Typography,
  useMediaQuery,
} from '@mui/material'
import { alpha, useTheme } from '@mui/material/styles'
import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../features/auth/useAuth'
import { ChangePasswordForm } from '../features/auth/ChangePasswordForm'
import { BrandMark } from '../shared/components/BrandMark'
import { FormDialog } from '../shared/components/FormDialog'
import { CommandPalette } from '../features/ledger-cards/CommandPalette'
import type { CompanyScope } from '../features/companies/companyScope'
import { useActiveCompany } from '../features/companies/useActiveCompany'
import { useCompanyScope } from '../features/companies/useCompanyScope'
import { useLocationCategories } from '../features/master-data/useMasterData'
import type { LocationCategory } from '../features/master-data/types'
import { buildMenuGroups, iconForName, useMenu } from './useMenu'

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
export function AppShell() {
  const theme = useTheme()
  const isDesktop = useMediaQuery(theme.breakpoints.up('lg'))
  const [mobileOpen, setMobileOpen] = useState(false)
  const [changingPassword, setChangingPassword] = useState(false)
  const [searchOpen, setSearchOpen] = useState(false)
  const [accountAnchor, setAccountAnchor] = useState<HTMLElement | null>(null)
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') { e.preventDefault(); setSearchOpen(true) }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [])
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

  // Menu structure, captions and role filtering all come from the API now
  // (core.MenuItem + Translation) so they can be edited without a frontend deploy.
  const menu = useMenu(activeCompany.id)
  const groups = buildMenuGroups(menu.data)

  const changeLanguage = async (language: string) => {
    localStorage.setItem('sz.language', language)
    await i18n.changeLanguage(language)
    document.documentElement.lang = language
  }

  const companySelect = (
    <Autocomplete
      size="small"
      disableClearable
      fullWidth
      sx={{ width: isDesktop ? 220 : '100%' }}
      options={companies}
      value={activeCompany}
      isOptionEqualToValue={(option, value) => option.id === value.id}
      getOptionLabel={(option) => option.name}
      onChange={(_, value) => value && handleSelectCompany(value.id)}
      renderInput={(params) => (
        <TextField {...params} label={t('activeCompany')} />
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
        <TextField {...params} label={t('companyScope.locationCategory')} />
      )}
    />
  )

  const languageSelect = (
    <FormControl size="small" sx={{ minWidth: isDesktop ? 84 : undefined }} fullWidth={!isDesktop}>
      <Select
        value={i18n.resolvedLanguage ?? 'sr-Latn'}
        onChange={(event) => void changeLanguage(event.target.value)}
        inputProps={{ 'aria-label': t('language') }}
      >
        <MenuItem value="sr-Latn">LAT</MenuItem>
        <MenuItem value="sr-Cyrl">ЋИР</MenuItem>
        <MenuItem value="en">EN</MenuItem>
      </Select>
    </FormControl>
  )

  const drawer = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column', minHeight: 0 }}>
      <Stack direction="row" spacing={1.25} alignItems="center" sx={{ px: 2.5, height: 64, flexShrink: 0 }}>
        <BrandMark size={32} />
        <Typography component="p" variant="subtitle1" noWrap sx={{ fontWeight: 650, letterSpacing: '-0.01em' }}>
          {t('appName')}
        </Typography>
      </Stack>
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
        {groups.map((group, index) => {
          // A top-level item with a Path (e.g. the dashboard) is a leaf, not a
          // group header -- render it alone, ungrouped, like before.
          const leaves = group.item.path ? [group.item] : group.children
          return (
            <List
              key={group.item.id}
              dense
              disablePadding
              sx={{ mb: index === groups.length - 1 ? 0 : 0.5 }}
              subheader={
                group.item.path ? undefined : (
                  <ListSubheader disableSticky sx={{ px: 1.5, pt: 1 }}>{group.item.caption}</ListSubheader>
                )
              }
            >
              {leaves.map((leaf) => {
                const Icon = iconForName(leaf.iconName)
                return (
                  <ListItemButton
                    key={leaf.id}
                    component={NavLink}
                    to={leaf.path ?? '/'}
                    end={leaf.path === '/'}
                    onClick={() => setMobileOpen(false)}
                    sx={{
                      minHeight: 38,
                      mb: 0.25,
                      position: 'relative',
                      color: 'text.primary',
                      '& .MuiListItemText-primary': { fontWeight: 500 },
                      '&.active': {
                        bgcolor: 'action.selected',
                        color: 'primary.dark',
                        '&::before': {
                          content: '""',
                          position: 'absolute',
                          left: -10,
                          top: 9,
                          bottom: 9,
                          width: 3,
                          borderRadius: '0 3px 3px 0',
                          bgcolor: 'primary.main',
                        },
                        '& .MuiListItemIcon-root': { color: 'primary.main' },
                        '& .MuiListItemText-primary': { fontWeight: 650 },
                      },
                    }}
                  >
                    <ListItemIcon><Icon fontSize="small" aria-hidden="true" /></ListItemIcon>
                    <ListItemText primary={leaf.caption} slotProps={{ primary: { variant: 'body2' } }} />
                  </ListItemButton>
                )
              })}
            </List>
          )
        })}
      </Box>
      <Box sx={{ p: 1.25, flexShrink: 0, borderTop: 1, borderColor: 'divider' }}>
        <ListItemButton onClick={(event) => setAccountAnchor(event.currentTarget)} sx={{ gap: 1.25, py: 1, borderRadius: 2 }}>
          <Avatar sx={{ width: 34, height: 34, bgcolor: 'primary.main', fontSize: '0.8125rem', fontWeight: 700 }}>
            {(user?.displayName ?? '?').slice(0, 2).toUpperCase()}
          </Avatar>
          <Box sx={{ minWidth: 0, flex: 1 }}>
            <Typography variant="body2" fontWeight={600} noWrap>{user?.displayName}</Typography>
            <Typography variant="caption" color="text.secondary" noWrap display="block">
              {user?.roles.join(', ')}
            </Typography>
          </Box>
          <KeyboardArrowUpDownIcon fontSize="small" sx={{ color: 'text.secondary' }} />
        </ListItemButton>
      </Box>
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
      <AppBar
        position="fixed"
        sx={{ zIndex: theme.zIndex.drawer + 1, width: { lg: `calc(100% - ${drawerWidth}px)` }, ml: { lg: `${drawerWidth}px` } }}
      >
        <Toolbar sx={{ gap: 1.5, minHeight: { xs: 56, sm: 64 } }}>
          {!isDesktop && (
            <IconButton edge="start" onClick={() => setMobileOpen(true)} aria-label={t('openNavigation')}>
              <MenuIcon />
            </IconButton>
          )}
          {!isDesktop && <BrandMark size={28} />}
          <ButtonBase
            onClick={() => setSearchOpen(true)}
            aria-label={t('search_.open')}
            sx={{
              display: 'flex',
              justifyContent: 'flex-start',
              gap: 1,
              px: 1.5,
              height: 38,
              width: { xs: 'auto', md: 300 },
              borderRadius: 2.5,
              border: 1,
              borderColor: 'divider',
              bgcolor: alpha(theme.palette.text.primary, 0.03),
              color: 'text.secondary',
              typography: 'body2',
              '&:hover': { borderColor: 'text.secondary', bgcolor: alpha(theme.palette.text.primary, 0.05) },
            }}
          >
            <SearchIcon fontSize="small" />
            <Box component="span" sx={{ display: { xs: 'none', md: 'inline' }, flex: 1, textAlign: 'left' }}>{t('search_.open')}</Box>
          </ButtonBase>
          <CommandPalette open={searchOpen} onClose={() => setSearchOpen(false)} />
          <Box sx={{ flexGrow: 1 }} />
          {/* On phones the company and language pickers live in the drawer. */}
          {isDesktop && (
            <Stack direction="row" spacing={1.25} alignItems="center">
              {companySelect}
              {scopeToggle}
              {locationCategorySelect}
              {languageSelect}
            </Stack>
          )}
        </Toolbar>
      </AppBar>
      <Menu
        anchorEl={accountAnchor}
        open={Boolean(accountAnchor)}
        onClose={() => setAccountAnchor(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'left' }}
        transformOrigin={{ vertical: 'bottom', horizontal: 'left' }}
        slotProps={{ paper: { sx: { minWidth: 220, mt: -1 } } }}
      >
        <MenuItem onClick={() => { setAccountAnchor(null); setChangingPassword(true) }}>
          <ListItemIcon><LockResetIcon fontSize="small" /></ListItemIcon>
          {t('staffAdmin_.changePassword')}
        </MenuItem>
        <MenuItem onClick={() => { setAccountAnchor(null); void logout() }}>
          <ListItemIcon><LogoutIcon fontSize="small" /></ListItemIcon>
          {t('logout')}
        </MenuItem>
      </Menu>
      <FormDialog open={changingPassword} title={t('staffAdmin_.changePassword')} onClose={() => setChangingPassword(false)} maxWidth="xs">
        {changingPassword && <ChangePasswordForm onDone={() => setChangingPassword(false)} onCancel={() => setChangingPassword(false)} />}
      </FormDialog>
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
