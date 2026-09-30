import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { Alert, Button, Checkbox, Divider, FormControlLabel, FormGroup, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../../api/problemDetails'
import { useActiveCompany } from '../../companies/useActiveCompany'
import { useSavePartner } from '../useMasterData'
import { useShortList } from '../useShortList'
import { PartnerAddressesSection } from './PartnerAddressesSection'
import type { Partner, SavePartner } from '../types'

const partnerSchema = z.object({
  shortName: z.string().trim().min(1, 'Kratak naziv je obavezan.').max(100),
  name: z.string().trim().min(1, 'Pun naziv je obavezan.').max(255),
  registrationNumber: z.string().trim().max(10).optional(),
  taxNumber: z.string().trim().max(10).optional(),
  jbkjs: z.string().trim().max(10).optional(),
  idCardNumber: z.string().trim().max(20).optional(),
  jmbg: z.string().trim().max(15).optional(),
  language: z.string().trim().min(1).max(10),
  note: z.string().optional(),
  partnerTypeId: z.number().nullable(),
  isSefUser: z.boolean(),
  isCrfUser: z.boolean(),
  skipAutoCheckSef: z.boolean(),
})

type PartnerFormValue = z.infer<typeof partnerSchema>

interface PartnerFormProps {
  companyId: number
  partner?: Partner
  onSaved?: (partner: Partner) => void
  onCancel?: () => void
}

export function PartnerForm({ companyId, partner, onSaved, onCancel }: PartnerFormProps) {
  const { t } = useTranslation()
  // The company list is already limited to what the signed-in staff member may access.
  const { companies } = useActiveCompany()
  const [selectedCompanyId, setSelectedCompanyId] = useState(partner?.companyId ?? companyId)
  const save = useSavePartner(selectedCompanyId, partner?.id)
  const partnerTypes = useShortList(selectedCompanyId, 'PartnerType')
  const {
    register,
    control,
    handleSubmit,
    formState: { errors, dirtyFields },
  } = useForm<PartnerFormValue>({
    resolver: zodResolver(partnerSchema),
    defaultValues: {
      shortName: partner?.shortName ?? '',
      name: partner?.name ?? '',
      registrationNumber: partner?.registrationNumber ?? '',
      taxNumber: partner?.taxNumber ?? '',
      jbkjs: partner?.jbkjs ?? '',
      idCardNumber: '',
      jmbg: '',
      language: partner?.language ?? 'sr-Latn',
      note: partner?.note ?? '',
      partnerTypeId: partner?.partnerTypeId ?? null,
      isSefUser: partner?.isSefUser ?? false,
      isCrfUser: partner?.isCrfUser ?? false,
      skipAutoCheckSef: partner?.skipAutoCheckSef ?? false,
    },
  })

  const submit = (value: PartnerFormValue) => {
    // idCardNumber/jmbg come back from the API masked (e.g. "***1234"), so this form can
    // never preload the real value — it always starts blank. Sending that blank back as ''
    // would make the backend erase the stored value on every unrelated edit. Only send
    // these two fields when the user actually typed into them (dirtyFields), so an
    // untouched field is omitted from the request and the backend leaves it alone.
    const request: SavePartner = {
      ...value,
      idCardNumber: dirtyFields.idCardNumber ? value.idCardNumber : undefined,
      jmbg: dirtyFields.jmbg ? value.jmbg : undefined,
    }
    save.mutate(request, { onSuccess: onSaved })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={handleSubmit(submit)} noValidate>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, 'Partner nije sačuvan. Proverite podatke i pokušajte ponovo.')}</Alert>}
      <Grid container spacing={2} columnSpacing={3}>
        <Section>Identifikacija</Section>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label="Kratak naziv" required {...register('shortName')} error={!!errors.shortName} helperText={errors.shortName?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label="Pun naziv" required {...register('name')} error={!!errors.name} helperText={errors.name?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            control={control}
            name="partnerTypeId"
            render={({ field }) => (
              <TextField
                select
                fullWidth
                size="small"
                label="Tip partnera"
                value={field.value ?? ''}
                onChange={(event) => field.onChange(event.target.value === '' ? null : Number(event.target.value))}
              >
                <MenuItem value=""><em>Nije izabran</em></MenuItem>
                {partnerTypes.data?.map((type) => <MenuItem key={type.id} value={type.id}>{type.caption}</MenuItem>)}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            select
            fullWidth
            size="small"
            label="Kompanija"
            value={selectedCompanyId}
            onChange={(event) => setSelectedCompanyId(Number(event.target.value))}
            // A saved partner belongs to its company; only a new partner can pick one.
            disabled={Boolean(partner)}
          >
            {companies.map((company) => <MenuItem key={company.id} value={company.id}>{company.name}</MenuItem>)}
          </TextField>
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <TextField fullWidth size="small" label="Matični broj" {...register('registrationNumber')} error={!!errors.registrationNumber} helperText={errors.registrationNumber?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <TextField fullWidth size="small" label="PIB" {...register('taxNumber')} error={!!errors.taxNumber} helperText={errors.taxNumber?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <TextField fullWidth size="small" label="JBKJS" {...register('jbkjs')} error={!!errors.jbkjs} helperText={errors.jbkjs?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <FormGroup row sx={{ flexWrap: 'nowrap' }}>
            <Controller control={control} name="isSefUser" render={({ field }) => (
              <FormControlLabel control={<Checkbox size="small" checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />} label="SEF" />
            )} />
            <Controller control={control} name="isCrfUser" render={({ field }) => (
              <FormControlLabel control={<Checkbox size="small" checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />} label="CRF" />
            )} />
            <Controller control={control} name="skipAutoCheckSef" render={({ field }) => (
              <FormControlLabel control={<Checkbox size="small" checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />} label="Bez provere" />
            )} />
          </FormGroup>
        </Grid>

        <Section>Lični podaci</Section>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField
            fullWidth
            size="small"
            label="Broj lične karte"
            placeholder={partner?.maskedIdCardNumber ?? undefined}
            {...register('idCardNumber')}
            error={!!errors.idCardNumber}
            helperText={errors.idCardNumber?.message ?? (partner?.maskedIdCardNumber ? t('partners_.maskedFieldHint', { value: partner.maskedIdCardNumber }) : undefined)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField
            fullWidth
            size="small"
            label="JMBG"
            placeholder={partner?.maskedJmbg ?? undefined}
            {...register('jmbg')}
            error={!!errors.jmbg}
            helperText={errors.jmbg?.message ?? (partner?.maskedJmbg ? t('partners_.maskedFieldHint', { value: partner.maskedJmbg }) : undefined)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField fullWidth size="small" label="Jezik" required {...register('language')} error={!!errors.language} helperText={errors.language?.message} />
        </Grid>

        <Section>Adrese</Section>
        <Grid size={12}>
          {partner
            ? <PartnerAddressesSection companyId={selectedCompanyId} partnerId={partner.id} />
            : <Typography color="text.secondary" variant="body2">Adrese se unose nakon što se partner prvi put sačuva.</Typography>}
        </Grid>

        <Section>Napomena</Section>
        <Grid size={12}>
          <TextField fullWidth size="small" label="Napomena" multiline minRows={3} {...register('note')} />
        </Grid>
      </Grid>
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        {onCancel && <Button onClick={onCancel}>Odustani</Button>}
        <Button type="submit" variant="contained" disabled={save.isPending}>Sačuvaj</Button>
      </Stack>
    </Stack>
  )
}

function Section({ children }: { children: string }) {
  return (
    <Grid size={12}>
      <Typography variant="overline" color="text.secondary" component="h3">{children}</Typography>
      <Divider />
    </Grid>
  )
}

