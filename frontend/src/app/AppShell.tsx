import LogoutIcon from '@mui/icons-material/Logout'
import MenuIcon from '@mui/icons-material/Menu'
import {
  AppBar,
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
  MenuItem,
  Select,
  Stack,
  Toolbar,
  Tooltip,
  Typography,
  useMediaQuery,
} from '@mui/material'
import { useTheme } from '@mui/material/styles'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../features/auth/useAuth'
import { useActiveCompany } from '../features/companies/useActiveCompany'
import { navigationItems } from './navigation'

const drawerWidth = 272

export function AppShell() {
  const theme = useTheme()
  const isDesktop = useMediaQuery(theme.breakpoints.up('lg'))
  const [mobileOpen, setMobileOpen] = useState(false)
  const { t, i18n } = useTranslation()
  const { user, logout } = useAuth()
  const { companies, activeCompany, selectCompany } = useActiveCompany()
  const visibleItems = navigationItems.filter(
    (item) => !item.roles || user?.roles.some((role) => item.roles?.includes(role)),
  )

  const drawer = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Toolbar>
        <Typography variant="h6" component="div" fontWeight={700}>{t('appName')}</Typography>
      </Toolbar>
      <Divider />
      <List component="nav" aria-label={t('navigation')} sx={{ flex: 1, overflowY: 'auto', py: 1 }}>
        {visibleItems.map((item) => {
          const Icon = item.icon
          return (
            <ListItemButton
              key={item.path}
              component={NavLink}
              to={item.path}
              end={item.path === '/'}
              onClick={() => setMobileOpen(false)}
              sx={{ '&.active': { bgcolor: 'action.selected', color: 'primary.main' } }}
            >
              <ListItemIcon><Icon aria-hidden="true" /></ListItemIcon>
              <ListItemText primary={t(item.labelKey)} />
            </ListItemButton>
          )
        })}
      </List>
      <Divider />
      <Box sx={{ p: 2 }}>
        <Typography variant="body2" fontWeight={600} noWrap>{user?.displayName}</Typography>
        <Typography variant="caption" color="text.secondary">{user?.roles.join(', ')}</Typography>
      </Box>
    </Box>
  )

  const changeLanguage = async (language: string) => {
    localStorage.setItem('sz.language', language)
    await i18n.changeLanguage(language)
    document.documentElement.lang = language
  }

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Box
        component="a"
        href="#main-content"
        sx={{ position: 'fixed', top: -100, left: 8, zIndex: 2000, '&:focus': { top: 8 }, bgcolor: 'background.paper', p: 1 }}
      >
        Preskoči na sadržaj
      </Box>
      <AppBar position="fixed" sx={{ zIndex: theme.zIndex.drawer + 1 }}>
        <Toolbar>
          {!isDesktop && (
            <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(true)} aria-label={t('openNavigation')}>
              <MenuIcon />
            </IconButton>
          )}
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, display: { xs: 'none', sm: 'block' } }}>
            {t('appName')}
          </Typography>
          <Stack direction="row" spacing={1} alignItems="center">
            <FormControl size="small" sx={{ minWidth: { xs: 140, sm: 210 } }}>
              <InputLabel id="company-select-label" sx={{ color: 'common.white' }}>{t('activeCompany')}</InputLabel>
              <Select
                labelId="company-select-label"
                value={activeCompany.id}
                label={t('activeCompany')}
                onChange={(event) => selectCompany(Number(event.target.value))}
                sx={{ color: 'common.white', '.MuiOutlinedInput-notchedOutline': { borderColor: 'rgba(255,255,255,.6)' }, '.MuiSvgIcon-root': { color: 'common.white' } }}
              >
                {companies.map((company) => <MenuItem key={company.id} value={company.id}>{company.name}</MenuItem>)}
              </Select>
            </FormControl>
            <FormControl size="small" sx={{ minWidth: 76 }}>
              <Select
                value={i18n.resolvedLanguage ?? 'sr-Latn'}
                onChange={(event) => void changeLanguage(event.target.value)}
                inputProps={{ 'aria-label': t('language') }}
                sx={{ color: 'common.white', '.MuiOutlinedInput-notchedOutline': { borderColor: 'rgba(255,255,255,.6)' }, '.MuiSvgIcon-root': { color: 'common.white' } }}
              >
                <MenuItem value="sr-Latn">LAT</MenuItem>
                <MenuItem value="sr-Cyrl">ЋИР</MenuItem>
                <MenuItem value="en">EN</MenuItem>
              </Select>
            </FormControl>
            <Tooltip title={t('logout')}>
              <IconButton color="inherit" onClick={() => void logout()} aria-label={t('logout')}>
                <LogoutIcon />
              </IconButton>
            </Tooltip>
          </Stack>
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
      <Box component="main" id="main-content" tabIndex={-1} sx={{ flexGrow: 1, minWidth: 0, p: { xs: 2, sm: 3 }, pt: { xs: 11, sm: 12 } }}>
        <Outlet />
      </Box>
    </Box>
  )
}
