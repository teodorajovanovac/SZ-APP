import LogoutIcon from '@mui/icons-material/Logout'
import MenuIcon from '@mui/icons-material/Menu'
import {
  AppBar,
  Avatar,
  Box,
  Divider,
  Drawer,
  FormControl,
  IconButton,
  InputLabel,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  ListSubheader,
  MenuItem,
  Select,
  Stack,
  Toolbar,
  Tooltip,
  Typography,
  useMediaQuery,
} from '@mui/material'
import { alpha, useTheme } from '@mui/material/styles'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../features/auth/useAuth'
import { useActiveCompany } from '../features/companies/useActiveCompany'
import { buildMenuGroups, iconForName, useMenu } from './useMenu'

const drawerWidth = 264

export function AppShell() {
  const theme = useTheme()
  const isDesktop = useMediaQuery(theme.breakpoints.up('lg'))
  const [mobileOpen, setMobileOpen] = useState(false)
  const { t, i18n } = useTranslation()
  const { user, logout } = useAuth()
  const { companies, activeCompany, selectCompany } = useActiveCompany()
  // Menu structure, captions and role filtering all come from the API now
  // (core.MenuItem + Translation) so they can be edited without a frontend deploy.
  const menu = useMenu(activeCompany.id)
  const groups = buildMenuGroups(menu.data)

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
  }

  const companySelect = (
    <FormControl size="small" fullWidth={!isDesktop} sx={{ minWidth: isDesktop ? 220 : undefined }}>
      <InputLabel id="company-select-label" sx={isDesktop ? { color: 'common.white' } : undefined}>
        {t('activeCompany')}
      </InputLabel>
      <Select
        labelId="company-select-label"
        value={activeCompany.id}
        label={t('activeCompany')}
        onChange={(event) => selectCompany(Number(event.target.value))}
        sx={isDesktop ? onDarkSurface : undefined}
      >
        {companies.map((company) => <MenuItem key={company.id} value={company.id}>{company.name}</MenuItem>)}
      </Select>
    </FormControl>
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
                    <ListItemText primary={leaf.caption} slotProps={{ primary: { variant: 'body2' } }} />
                  </ListItemButton>
                )
              })}
            </List>
          )
        })}
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
            <Stack direction="row" spacing={1} alignItems="center">
              {companySelect}
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
