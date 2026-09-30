import { Alert, Button, Snackbar, type AlertColor } from '@mui/material'
import { createContext, useCallback, useContext, useState, type PropsWithChildren } from 'react'
import { Link as RouterLink } from 'react-router-dom'

export interface Notification {
  message: string
  severity?: AlertColor
  /** Optional drill-through to the result, e.g. the journal that was just posted. */
  link?: { to: string; label: string }
}

type Notify = (notification: Notification) => void
const NotifyContext = createContext<Notify>(() => undefined)

/** UX-02: `useNotify()({ message: t('…', { id }), link: { to, label } })` after a successful action. */
// eslint-disable-next-line react-refresh/only-export-components
export const useNotify = () => useContext(NotifyContext)

export function NotifyProvider({ children }: PropsWithChildren) {
  const [current, setCurrent] = useState<(Notification & { key: number }) | null>(null)
  const [open, setOpen] = useState(false)
  const notify = useCallback<Notify>((notification) => {
    setCurrent({ ...notification, key: Date.now() })
    setOpen(true)
  }, [])

  return (
    <NotifyContext.Provider value={notify}>
      {children}
      <Snackbar
        key={current?.key}
        open={open}
        autoHideDuration={current?.severity === 'error' ? 8000 : 5000}
        onClose={(_, reason) => reason !== 'clickaway' && setOpen(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          variant="filled"
          severity={current?.severity ?? 'success'}
          onClose={() => setOpen(false)}
          sx={{ alignItems: 'center', minWidth: 280 }}
          action={
            current?.link ? (
              <Button color="inherit" size="small" component={RouterLink} to={current.link.to} onClick={() => setOpen(false)}>
                {current.link.label}
              </Button>
            ) : undefined
          }
        >
          {current?.message}
        </Alert>
      </Snackbar>
    </NotifyContext.Provider>
  )
}
