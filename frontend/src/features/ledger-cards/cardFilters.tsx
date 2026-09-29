import { Autocomplete, MenuItem, TextField } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useDeferredValue, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ledgerCardsApi, type SearchResult } from './ledgerCardsApi'
import { CORE_ACCOUNTS } from './useUrlFilters'

export function AccountSelect({ companyId, value, onChange }: { companyId: number; value: string; onChange: (value: string) => void }) {
  const { t } = useTranslation()
  const chart = useQuery({ queryKey: ['chart-of-accounts', companyId], queryFn: () => ledgerCardsApi.chartOfAccounts(companyId), staleTime: 5 * 60_000 })
  const names = new Map((chart.data ?? []).filter((x) => x.isActive).map((x) => [x.account, x.name]))
  const accounts = [...new Set([...CORE_ACCOUNTS, ...names.keys(), ...(value ? [value] : [])])].sort()
  return (
    <TextField select size="small" label={t('cards_.account')} value={value} onChange={(e) => onChange(e.target.value)} sx={{ minWidth: 200 }}>
      <MenuItem value="">{t('cards_.allAccounts')}</MenuItem>
      {accounts.map((account) => (
        <MenuItem key={account} value={account}>
          {account}{names.get(account) ? ` — ${names.get(account)}` : ''}
        </MenuItem>
      ))}
    </TextField>
  )
}

export interface PartnerOption {
  partnerAccountId: number
  label: string
}

/** Partner autocomplete backed by the Ctrl+K search endpoint (partner hits only). */
export function PartnerPicker({ companyId, value, onChange }: { companyId: number; value: PartnerOption | null; onChange: (value: PartnerOption | null) => void }) {
  const { t } = useTranslation()
  const [input, setInput] = useState('')
  const term = useDeferredValue(input.trim())
  const results = useQuery({
    queryKey: ['search', companyId, term],
    queryFn: () => ledgerCardsApi.search(companyId, term),
    enabled: term.length >= 2,
  })
  const options: PartnerOption[] = (results.data ?? [])
    .filter((x: SearchResult) => x.type === 'partner' && x.partnerAccountId != null)
    .map((x) => ({ partnerAccountId: x.partnerAccountId!, label: `${x.title} (${x.subtitle ?? ''})` }))

  return (
    <Autocomplete
      size="small"
      sx={{ minWidth: 280 }}
      options={options}
      value={value}
      filterOptions={(x) => x}
      loading={results.isFetching}
      isOptionEqualToValue={(a, b) => a.partnerAccountId === b.partnerAccountId}
      getOptionLabel={(x) => x.label}
      onInputChange={(_, text) => setInput(text)}
      onChange={(_, next) => onChange(next)}
      noOptionsText={term.length < 2 ? t('search_.minChars') : t('search_.noResults')}
      renderInput={(params) => <TextField {...params} label={t('cards_.partner')} />}
    />
  )
}
