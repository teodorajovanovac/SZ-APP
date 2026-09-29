import { useRef, useState } from 'react'
import { Alert, Box, Button, List, ListItem, ListItemText, Stack, Typography } from '@mui/material'
import UploadFileIcon from '@mui/icons-material/UploadFile'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { ApiProblemError } from '../../api/generated/client'
import { ledgerBankingApi } from './ledgerBankingApi'

/** GAP-04: drag-and-drop (or click-to-choose) upload for a bank statement file. Native HTML5 drag
 * events -- no drag-and-drop library needed for a single drop target. */
export function BankStatementUpload({ companyId, onImported }: { companyId: number; onImported: (statementId: number) => void }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const inputRef = useRef<HTMLInputElement>(null)
  const [dragOver, setDragOver] = useState(false)

  const upload = useMutation({
    mutationFn: (file: File) => ledgerBankingApi.statements.importFile(companyId, file),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['bank-statements', companyId] })
      onImported(result.statement.header.id)
    },
  })

  const handleFiles = (files: FileList | null) => {
    const file = files?.[0]
    if (file) upload.mutate(file)
  }

  return (
    <Stack spacing={1.5}>
      <Box
        onClick={() => inputRef.current?.click()}
        onDragOver={(event) => { event.preventDefault(); setDragOver(true) }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(event) => {
          event.preventDefault()
          setDragOver(false)
          handleFiles(event.dataTransfer.files)
        }}
        sx={{
          border: '2px dashed',
          borderColor: dragOver ? 'primary.main' : 'divider',
          borderRadius: 1,
          p: 3,
          textAlign: 'center',
          cursor: 'pointer',
          bgcolor: dragOver ? 'action.hover' : 'transparent',
        }}
        role="button"
        tabIndex={0}
        aria-label={t('bankImport_.dropHere')}
      >
        <UploadFileIcon color={dragOver ? 'primary' : 'action'} fontSize="large" />
        <Typography sx={{ mt: 1 }}>{t('bankImport_.dropHere')}</Typography>
        <Button size="small" sx={{ mt: 1 }} onClick={(e) => { e.stopPropagation(); inputRef.current?.click() }}>
          {t('bankImport_.chooseFile')}
        </Button>
        <input
          ref={inputRef}
          type="file"
          hidden
          accept=".xml,.txt"
          onChange={(event) => { handleFiles(event.target.files); event.target.value = '' }}
        />
      </Box>
      {upload.isPending ? <Alert severity="info">{t('bankImport_.uploading')}</Alert> : null}
      {upload.isError ? (
        <Alert severity="error">
          {upload.error instanceof ApiProblemError ? upload.error.problem.detail ?? upload.error.problem.title : t('bankImport_.importFailed')}
        </Alert>
      ) : null}
      {upload.data ? (
        <Alert severity={upload.data.alreadyImported ? 'warning' : 'success'}>
          <Typography fontWeight={600}>
            {upload.data.alreadyImported ? t('bankImport_.alreadyImported') : t('bankImport_.importedTitle')}
          </Typography>
          <Typography variant="body2">{t('bankImport_.formatDetected', { name: upload.data.formatName })}</Typography>
          {upload.data.warnings.length > 0 ? (
            <>
              <Typography variant="body2" fontWeight={600} sx={{ mt: 1 }}>{t('bankImport_.warningsTitle')}</Typography>
              <List dense disablePadding>
                {upload.data.warnings.map((warning, index) => (
                  <ListItem key={index} disableGutters>
                    <ListItemText primary={warning} />
                  </ListItem>
                ))}
              </List>
            </>
          ) : null}
        </Alert>
      ) : null}
    </Stack>
  )
}
