import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Divider, Grid, Stack, TextField, Typography } from '@mui/material'
import type { BaseSyntheticEvent } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../../api/problemDetails'
import { FormActions, isSaveNewSubmit } from '../../../shared/components/FormActions'
import { useSavePartner } from '../useMasterData'
import type { Partner, SavePartner } from '../types'

// zod messages are i18n keys (translated at render) so the schema can stay at module scope.
const partnerSchema = z.object({
  shortName: z.string().trim().min(1, 'validation.shortNameRequired').max(100),
  name: z.string().trim().min(1, 'validation.fullNameRequired').max(255),
  registrationNumber: z.string().trim().max(10).optional(),
  taxNumber: z.string().trim().max(10).optional(),
  jbkjs: z.string().trim().max(10).optional(),
  idCardNumber: z.string().trim().max(20).optional(),
  jmbg: z.string().trim().max(15).optional(),
  language: z.string().trim().min(1, 'validation.required').max(10),
  note: z.string().optional(),
})

type PartnerFormValue = z.infer<typeof partnerSchema>

interface PartnerFormProps {
  companyId: number
  partner?: Partner
  onSaved?: (partner: Partner) => void
  /** Offers "Sačuvaj i novi"; called instead of onSaved when that button (Ctrl+Enter) was used. */
  onSavedNew?: (partner: Partner) => void
  onCancel?: () => void
}

export function PartnerForm({ companyId, partner, onSaved, onSavedNew, onCancel }: PartnerFormProps) {
  const { t } = useTranslation()
  const save = useSavePartner(companyId, partner?.id)
  const {
    register,
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
    },
  })
  const message = (error?: { message?: string }) => (error?.message ? t(error.message) : undefined)

  const submit = (value: PartnerFormValue, event?: BaseSyntheticEvent) => {
    const andNew = Boolean(onSavedNew) && isSaveNewSubmit(event)
    // idCardNumber/jmbg come back from the API masked (e.g. "***1234"), so this form can
    // never preload the real value — it always starts blank. Sending that blank back as ''
    // would make the backend erase the stored value on every unrelated edit. Only send
    // these two fields when the user actually typed into them (dirtyFields), so an
    // untouched field is omitted from the request and the backend leaves it alone.
    const request: SavePartner = {
      ...value,
      idCardNumber: dirtyFields.idCardNumber ? value.idCardNumber : undefined,
      jmbg: dirtyFields.jmbg ? value.jmbg : undefined,
      partnerTypeId: partner?.partnerTypeId ?? null,
    }
    save.mutate(request, { onSuccess: andNew ? onSavedNew : onSaved })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={handleSubmit(submit)} noValidate>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, t('partners_.saveError'))}</Alert>}
      <Grid container spacing={2} columnSpacing={3}>
        <Section>{t('sections.identification')}</Section>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label={t('fields.shortName')} required {...register('shortName')} error={!!errors.shortName} helperText={message(errors.shortName)} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label={t('fields.fullName')} required {...register('name')} error={!!errors.name} helperText={message(errors.name)} />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField fullWidth size="small" label={t('fields.registrationNumber')} {...register('registrationNumber')} error={!!errors.registrationNumber} helperText={message(errors.registrationNumber)} />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField fullWidth size="small" label={t('fields.taxNumber')} {...register('taxNumber')} error={!!errors.taxNumber} helperText={message(errors.taxNumber)} />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField fullWidth size="small" label={t('fields.jbkjs')} {...register('jbkjs')} error={!!errors.jbkjs} helperText={message(errors.jbkjs)} />
        </Grid>

        <Section>{t('sections.personal')}</Section>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField
            fullWidth
            size="small"
            label={t('fields.idCardNumber')}
            placeholder={partner?.maskedIdCardNumber ?? undefined}
            {...register('idCardNumber')}
            error={!!errors.idCardNumber}
            helperText={message(errors.idCardNumber) ?? (partner?.maskedIdCardNumber ? t('partners_.maskedFieldHint', { value: partner.maskedIdCardNumber }) : undefined)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField
            fullWidth
            size="small"
            label={t('fields.jmbg')}
            placeholder={partner?.maskedJmbg ?? undefined}
            {...register('jmbg')}
            error={!!errors.jmbg}
            helperText={message(errors.jmbg) ?? (partner?.maskedJmbg ? t('partners_.maskedFieldHint', { value: partner.maskedJmbg }) : undefined)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField fullWidth size="small" label={t('fields.language')} required {...register('language')} error={!!errors.language} helperText={message(errors.language)} />
        </Grid>

        <Section>{t('sections.note')}</Section>
        <Grid size={12}>
          <TextField fullWidth size="small" label={t('fields.note')} multiline minRows={3} {...register('note')} />
        </Grid>
      </Grid>
      <FormActions onCancel={onCancel} pending={save.isPending} saveNew={Boolean(onSavedNew)} />
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
