import { Button, Stack, Typography } from '@mui/material'
import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <Stack component="main" minHeight="100vh" alignItems="center" justifyContent="center" spacing={2}>
      <Typography component="h1" variant="h1">Stranica nije pronađena</Typography>
      <Button component={Link} to="/" variant="contained">Nazad na početnu</Button>
    </Stack>
  )
}
