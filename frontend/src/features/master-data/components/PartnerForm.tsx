import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Button, Stack, TextField } from '@mui/material'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { useSavePartner } from '../useMasterData'
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
})

type PartnerFormValue = z.infer<typeof partnerSchema>

interface PartnerFormProps {
  companyId: number
  partner?: Partner
  onSaved?: (partner: Partner) => void
  onCancel?: () => void
}

export function PartnerForm({ companyId, partner, onSaved, onCancel }: PartnerFormProps) {
  const save = useSavePartner(companyId, partner?.id)
  const {
    register,
    handleSubmit,
    formState: { errors },
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

  const submit = (value: PartnerFormValue) => {
    const request: SavePartner = {
      ...value,
      partnerTypeId: partner?.partnerTypeId ?? null,
    }
    save.mutate(request, { onSuccess: onSaved })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={handleSubmit(submit)} noValidate>
      {save.isError && <Alert severity="error">Partner nije sačuvan. Proverite podatke i pokušajte ponovo.</Alert>}
      <TextField label="Kratak naziv" required {...register('shortName')} error={!!errors.shortName} helperText={errors.shortName?.message} />
      <TextField label="Pun naziv" required {...register('name')} error={!!errors.name} helperText={errors.name?.message} />
      <TextField label="Matični broj" {...register('registrationNumber')} error={!!errors.registrationNumber} helperText={errors.registrationNumber?.message} />
      <TextField label="PIB" {...register('taxNumber')} error={!!errors.taxNumber} helperText={errors.taxNumber?.message} />
      <TextField label="JBKJS" {...register('jbkjs')} error={!!errors.jbkjs} helperText={errors.jbkjs?.message} />
      <TextField label="Broj lične karte" {...register('idCardNumber')} error={!!errors.idCardNumber} helperText={errors.idCardNumber?.message} />
      <TextField label="JMBG" {...register('jmbg')} error={!!errors.jmbg} helperText={errors.jmbg?.message} />
      <TextField label="Jezik" required {...register('language')} error={!!errors.language} helperText={errors.language?.message} />
      <TextField label="Napomena" multiline minRows={3} {...register('note')} />
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        {onCancel && <Button onClick={onCancel}>Odustani</Button>}
        <Button type="submit" variant="contained" disabled={save.isPending}>Sačuvaj</Button>
      </Stack>
    </Stack>
  )
}

