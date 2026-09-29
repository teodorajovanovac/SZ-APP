import { Box, Dialog, List, ListItemButton, ListItemText, TextField, Typography } from '@mui/material'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { useActiveCompany } from '../companies/useActiveCompany'
import { formatAmount } from '../../shared/format/money'
import { ledgerCardsApi, type SearchResult } from './ledgerCardsApi'

function targetOf(r: SearchResult): string {
  if (r.type === 'unit' && r.unitId != null) return `/units/${r.companyId}/${r.unitId}`
  const params = new URLSearchParams({ companyId: String(r.companyId), account: r.account ?? '2040', partner: r.title })
  if (r.partnerAccountId != null) params.set('partnerAccountId', String(r.partnerAccountId))
  if (r.type === 'payment' && r.paymentReference) params.set('paymentReference', r.paymentReference)
  return `/kartice?${params.toString()}`
}

/** DES-08: Ctrl+K / Cmd+K global search over partners, payment references and units. */
export function CommandPalette({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { activeCompany } = useActiveCompany()
  const [term, setTerm] = useState('')
  const [debounced, setDebounced] = useState('')
  const [index, setIndex] = useState(0)

  useEffect(() => {
    const id = setTimeout(() => setDebounced(term.trim()), 250)
    return () => clearTimeout(id)
  }, [term])

  const results = useQuery({
    queryKey: ['search', activeCompany.id, debounced],
    queryFn: () => ledgerCardsApi.search(activeCompany.id, debounced),
    enabled: open && debounced.length >= 2,
    placeholderData: keepPreviousData,
  })
  const items = debounced.length >= 2 ? (results.data ?? []) : []

  const close = () => { setTerm(''); setDebounced(''); setIndex(0); onClose() }
  const go = (r: SearchResult | undefined) => { if (!r) return; close(); navigate(targetOf(r)) }

  const onKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowDown') { e.preventDefault(); setIndex((i) => Math.min(i + 1, items.length - 1)) }
    else if (e.key === 'ArrowUp') { e.preventDefault(); setIndex((i) => Math.max(i - 1, 0)) }
    else if (e.key === 'Enter') { e.preventDefault(); go(items[index]) }
  }

  return (
    <Dialog open={open} onClose={close} fullWidth maxWidth="sm" slotProps={{ paper: { sx: { alignSelf: 'flex-start', mt: 10 } } }}>
      <Box sx={{ p: 2 }} onKeyDown={onKeyDown}>
        <TextField
          autoFocus
          fullWidth
          size="small"
          placeholder={t('search_.placeholder')}
          value={term}
          onChange={(e) => { setTerm(e.target.value); setIndex(0) }}
          slotProps={{ htmlInput: { 'aria-label': t('search_.placeholder') } }}
        />
        {term.trim().length < 2 ? (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>{t('search_.minChars')}</Typography>
        ) : !results.isFetching && items.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>{t('search_.noResults')}</Typography>
        ) : (
          <List dense sx={{ mt: 1, maxHeight: 420, overflowY: 'auto' }}>
            {items.map((r, i) => (
              <ListItemButton key={`${r.type}-${r.companyId}-${r.partnerAccountId ?? r.unitId ?? r.paymentReference}-${i}`} selected={i === index} onClick={() => go(r)} onMouseEnter={() => setIndex(i)}>
                <ListItemText
                  primary={`${t(`search_.type.${r.type}`)} · ${r.title}`}
                  secondary={[r.subtitle, r.balance != null ? `${t('search_.balance')}: ${formatAmount(r.balance)}` : null].filter(Boolean).join(' · ')}
                />
              </ListItemButton>
            ))}
          </List>
        )}
        <Typography variant="caption" color="text.secondary">{t('search_.hint')}</Typography>
      </Box>
    </Dialog>
  )
}
