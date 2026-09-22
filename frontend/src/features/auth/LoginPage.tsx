import { zodResolver } from '@hookform/resolvers/zod'
import LockOutlinedIcon from '@mui/icons-material/LockOutlined'
import {
  Alert,
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  FormControlLabel,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { Navigate, useLocation } from 'react-router-dom'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { useAuth } from './useAuth'

const loginSchema = z.object({
  email: z.string().trim().email('Unesite ispravnu adresu e-pošte.'),
  password: z.string().min(1, 'Lozinka je obavezna.'),
  rememberMe: z.boolean(),
})

type LoginForm = z.infer<typeof loginSchema>

interface LoginLocationState {
  from?: string
}

export function LoginPage() {
  const { user, login, isLoginPending, error } = useAuth()
  const location = useLocation()
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
        placeItems: 'center',
        p: 2,
        background: 'linear-gradient(145deg, #17324d 0%, #315b7d 52%, #edf1f4 52%)',
      }}
    >
      <Card sx={{ width: '100%', maxWidth: 420 }}>
        <CardContent sx={{ p: { xs: 3, sm: 5 } }}>
          <Stack component="form" onSubmit={handleSubmit(login)} spacing={2.5} noValidate>
            <Avatar sx={{ bgcolor: 'secondary.main', alignSelf: 'center' }}>
              <LockOutlinedIcon />
            </Avatar>
            <Box textAlign="center">
              <Typography component="h1" variant="h1">SZ Upravljanje</Typography>
              <Typography color="text.secondary">Prijava zaposlenih</Typography>
            </Box>
            {error ? (
              <Alert severity="error" role="alert">
                {getErrorMessage(error, 'Prijava nije uspela.')}
              </Alert>
            ) : null}
            <Controller
              name="email"
              control={control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="E-pošta"
                  type="email"
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
                  label="Lozinka"
                  type="password"
                  autoComplete="current-password"
                  error={Boolean(fieldState.error)}
                  helperText={fieldState.error?.message}
                />
              )}
            />
            <Controller
              name="rememberMe"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={<Checkbox checked={field.value} onChange={field.onChange} />}
                  label="Ostani prijavljen/a"
                />
              )}
            />
            <Button type="submit" variant="contained" size="large" disabled={isLoginPending}>
              {isLoginPending ? 'Prijavljivanje…' : 'Prijavi se'}
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  )
}
