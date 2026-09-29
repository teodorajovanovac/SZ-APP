import { zodResolver } from '@hookform/resolvers/zod'
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline'
import VisibilityIcon from '@mui/icons-material/VisibilityOutlined'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOffOutlined'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  IconButton,
  InputAdornment,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { alpha } from '@mui/material/styles'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Navigate, useLocation } from 'react-router-dom'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { BrandMark } from '../../shared/components/BrandMark'
import { useAuth } from './useAuth'

interface LoginLocationState {
  from?: string
}

function BrandPanel() {
  const { t } = useTranslation()
  return (
    <Box
      sx={(theme) => ({
        display: { xs: 'none', md: 'flex' },
        flexDirection: 'column',
        justifyContent: 'space-between',
        p: { md: 6, lg: 8 },
        color: '#fff',
        position: 'relative',
        overflow: 'hidden',
        background: `linear-gradient(155deg, ${theme.palette.primary.dark} 0%, ${theme.palette.primary.main} 70%, ${theme.palette.primary.light} 130%)`,
        '&::before': {
          content: '""',
          position: 'absolute',
          width: 520,
          height: 520,
          right: -180,
          top: -160,
          borderRadius: '50%',
          background: `radial-gradient(circle, ${alpha('#fff', 0.16)} 0%, transparent 68%)`,
        },
        '&::after': {
          content: '""',
          position: 'absolute',
          width: 460,
          height: 460,
          left: -160,
          bottom: -180,
          borderRadius: '50%',
          background: `radial-gradient(circle, ${alpha('#fff', 0.1)} 0%, transparent 70%)`,
        },
      })}
    >
      <Stack direction="row" spacing={1.5} alignItems="center" sx={{ position: 'relative', zIndex: 1 }}>
        <BrandMark size={40} />
        <Typography variant="h6" component="p" sx={{ color: '#fff', fontWeight: 650 }}>
          {t('appName')}
        </Typography>
      </Stack>
      <Box sx={{ position: 'relative', zIndex: 1, maxWidth: 440 }}>
        <Typography component="p" sx={{ fontSize: { md: '1.75rem', lg: '2.125rem' }, fontWeight: 650, lineHeight: 1.2, letterSpacing: '-0.025em' }}>
          {t('login_.tagline')}
        </Typography>
        <Stack spacing={1.5} sx={{ mt: 4 }}>
          {(['point1', 'point2', 'point3'] as const).map((key) => (
            <Stack key={key} direction="row" spacing={1.25} alignItems="center">
              <CheckCircleOutlineIcon sx={{ fontSize: 20, color: alpha('#fff', 0.75) }} />
              <Typography variant="body1" sx={{ color: alpha('#fff', 0.88) }}>{t(`login_.${key}`)}</Typography>
            </Stack>
          ))}
        </Stack>
      </Box>
      <Typography variant="caption" sx={{ position: 'relative', zIndex: 1, color: alpha('#fff', 0.6) }}>
        © {new Date().getFullYear()} {t('appName')}
      </Typography>
    </Box>
  )
}

export function LoginPage() {
  const { t } = useTranslation()
  const { user, login, isLoginPending, error } = useAuth()
  const location = useLocation()
  const [showPassword, setShowPassword] = useState(false)
  const [capsLock, setCapsLock] = useState(false)
  const loginSchema = z.object({
    email: z.string().trim().email(t('login.invalidEmail')),
    password: z.string().min(1, t('login.passwordRequired')),
    rememberMe: z.boolean(),
  })
  type LoginForm = z.infer<typeof loginSchema>
  const { control, handleSubmit } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '', rememberMe: false },
  })

  const destination = (location.state as LoginLocationState | null)?.from ?? '/'
  if (user) {
    return <Navigate to={destination} replace />
  }

  return (
    <Box
      component="main"
      sx={{
        minHeight: '100vh',
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', md: 'minmax(0, 1.05fr) minmax(0, 1fr)' },
        bgcolor: 'background.paper',
      }}
    >
      <BrandPanel />
      <Box sx={{ display: 'grid', placeItems: 'center', p: { xs: 3, sm: 6 } }}>
        <Stack component="form" onSubmit={handleSubmit(login)} spacing={2.5} noValidate sx={{ width: '100%', maxWidth: 380 }}>
          <Stack direction="row" spacing={1.5} alignItems="center" sx={{ display: { xs: 'flex', md: 'none' }, mb: 1 }}>
            <BrandMark size={40} />
            <Typography variant="h6" component="p">{t('appName')}</Typography>
          </Stack>
          <Box>
            <Typography component="h1" variant="h1">{t('login_.welcome')}</Typography>
            <Typography color="text.secondary" sx={{ mt: 0.75 }}>{t('login.subtitle')}</Typography>
          </Box>
          {error ? (
            <Alert severity="error" role="alert">
              {getErrorMessage(error, t('login.failed'))}
            </Alert>
          ) : null}
          <Controller
            name="email"
            control={control}
            render={({ field, fieldState }) => (
              <TextField
                {...field}
                label={t('login.email')}
                type="email"
                size="medium"
                autoComplete="username"
                autoFocus
                error={Boolean(fieldState.error)}
                helperText={fieldState.error?.message}
              />
            )}
          />
          <Controller
            name="password"
            control={control}
            render={({ field, fieldState }) => (
              <TextField
                {...field}
                label={t('login.password')}
                type={showPassword ? 'text' : 'password'}
                size="medium"
                autoComplete="current-password"
                error={Boolean(fieldState.error)}
                helperText={fieldState.error?.message ?? (capsLock ? t('login_.capsLock') : undefined)}
                onKeyUp={(event) => setCapsLock(event.getModifierState('CapsLock'))}
                onBlur={() => { field.onBlur(); setCapsLock(false) }}
                slotProps={{
                  formHelperText: { sx: capsLock && !fieldState.error ? { color: 'warning.main' } : undefined },
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          edge="end"
                          size="small"
                          onClick={() => setShowPassword((v) => !v)}
                          aria-label={showPassword ? t('login_.hidePassword') : t('login_.showPassword')}
                        >
                          {showPassword ? <VisibilityOffIcon fontSize="small" /> : <VisibilityIcon fontSize="small" />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  },
                }}
              />
            )}
          />
          <Controller
            name="rememberMe"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={<Checkbox size="small" checked={field.value} onChange={field.onChange} />}
                label={<Typography variant="body2">{t('login.rememberMe')}</Typography>}
                sx={{ mt: '-4px !important' }}
              />
            )}
          />
          <Button type="submit" variant="contained" size="large" disabled={isLoginPending} sx={{ py: 1.4 }}>
            {isLoginPending ? t('login.submitPending') : t('login.submit')}
          </Button>
        </Stack>
      </Box>
    </Box>
  )
}
