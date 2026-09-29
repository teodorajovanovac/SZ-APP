import { apiRequest } from '../../api/generated/client'
import type {
  BankStatementAllocationInput,
  BankStatementDetail,
  BankStatementFormat,
  BankStatementImport,
  BankStatementImportResult,
  BankStatementLine,
  BankStatementSummary,
  BankTemplate,
  JournalEntrySummary,
  MatchPartnerOption,
  PageResponse,
  PostingPeriodLock,
  PostingResult,
} from './types'

// apiRequest attaches the antiforgery header automatically for unsafe methods; only
// the idempotency key needs adding here.
async function mutationRequest<T>(path: string, method: 'POST' | 'PUT', body: unknown): Promise<T> {
  return apiRequest<T>(path, {
    method,
    headers: { 'Idempotency-Key': crypto.randomUUID() },
    body: JSON.stringify(body),
  })
}

function companyPath(companyId: number, path: string) {
  return `/api/v1/companies/${companyId}${path}`
}

export const ledgerBankingApi = {
  journals: {
    list: (companyId: number, page: number, pageSize: number) =>
      apiRequest<PageResponse<JournalEntrySummary>>(
        companyPath(companyId, `/journal-entries?page=${page + 1}&pageSize=${pageSize}`),
      ),
    post: (companyId: number, journal: JournalEntrySummary) =>
      mutationRequest<PostingResult>(companyPath(companyId, `/journal-entries/${journal.id}/post`), 'POST', {
        rowVersion: journal.rowVersion,
      }),
    reverse: (companyId: number, journal: JournalEntrySummary) =>
      mutationRequest<PostingResult>(companyPath(companyId, `/journal-entries/${journal.id}/reverse`), 'POST', {
        rowVersion: journal.rowVersion,
      }),
  },
  statements: {
    list: (companyId: number, page: number, pageSize: number) =>
      apiRequest<PageResponse<BankStatementSummary>>(
        companyPath(companyId, `/bank-statements?page=${page + 1}&pageSize=${pageSize}`),
      ),
    get: (companyId: number, statementId: number) =>
      apiRequest<BankStatementDetail>(companyPath(companyId, `/bank-statements/${statementId}`)),
    formats: (companyId: number) =>
      apiRequest<BankStatementFormat[]>(companyPath(companyId, '/bank-statements/formats')),
    // GAP-04: multipart upload; bank format auto-detected from the file name unless chosen.
    importFile: async (companyId: number, file: File, bankCode?: number, bankAccountId?: number) => {
      const form = new FormData()
      form.append('file', file)
      if (bankCode != null) form.append('bankCode', String(bankCode))
      if (bankAccountId != null) form.append('bankAccountId', String(bankAccountId))
      return apiRequest<BankStatementImportResult>(companyPath(companyId, '/bank-statements/import'), {
        method: 'POST',
        headers: { 'Idempotency-Key': crypto.randomUUID() },
        body: form,
      })
    },
    importJson: (companyId: number, request: BankStatementImport) =>
      mutationRequest(companyPath(companyId, '/bank-statements/import-json'), 'POST', request),
    rematch: (companyId: number, statementId: number) =>
      mutationRequest<BankStatementDetail>(companyPath(companyId, `/bank-statements/${statementId}/rematch`), 'POST', {}),
    acceptConfident: (companyId: number, statementId: number) =>
      mutationRequest<BankStatementDetail>(companyPath(companyId, `/bank-statements/${statementId}/accept-confident`), 'POST', {}),
    post: (companyId: number, statement: BankStatementSummary) =>
      mutationRequest<PostingResult>(companyPath(companyId, `/bank-statements/${statement.id}/post`), 'POST', {
        rowVersion: statement.rowVersion,
      }),
    unpost: (companyId: number, statement: BankStatementSummary) =>
      mutationRequest<PostingResult>(companyPath(companyId, `/bank-statements/${statement.id}/unpost`), 'POST', {
        rowVersion: statement.rowVersion,
      }),
  },
  lines: {
    accept: (companyId: number, lineId: number, rowVersion: string, allocations?: BankStatementAllocationInput[]) =>
      mutationRequest<BankStatementLine>(companyPath(companyId, `/bank-statement-lines/${lineId}/accept`), 'POST', {
        allocations: allocations ?? null,
        rowVersion,
      }),
    reopen: (companyId: number, lineId: number, rowVersion: string) =>
      mutationRequest<BankStatementLine>(companyPath(companyId, `/bank-statement-lines/${lineId}/reopen`), 'POST', {
        rowVersion,
      }),
    assignPartner: (companyId: number, lineId: number, partnerAccountId: number, rowVersion: string) =>
      mutationRequest<BankStatementLine>(companyPath(companyId, `/bank-statement-lines/${lineId}/assign-partner`), 'POST', {
        partnerAccountId,
        rowVersion,
      }),
    savePayerAccount: (companyId: number, lineId: number, partnerAccountId: number) =>
      mutationRequest<void>(companyPath(companyId, `/bank-statement-lines/${lineId}/save-payer-account`), 'POST', {
        partnerAccountId,
      }),
    createTemplate: (companyId: number, lineId: number, partnerAccountId: number, name?: string, subAccountId?: string) =>
      mutationRequest<BankTemplate>(companyPath(companyId, `/bank-statement-lines/${lineId}/create-template`), 'POST', {
        partnerAccountId,
        name: name ?? null,
        subAccountId: subAccountId ?? null,
      }),
  },
  partners: {
    search: (companyId: number, search: string) =>
      apiRequest<MatchPartnerOption[]>(
        companyPath(companyId, `/bank-statement-partners?search=${encodeURIComponent(search)}`),
      ),
  },
  templates: {
    list: (companyId: number) => apiRequest<BankTemplate[]>(companyPath(companyId, '/bank-statement-templates')),
  },
  postingPeriods: {
    list: (companyId: number) => apiRequest<PostingPeriodLock[]>(companyPath(companyId, '/posting-periods')),
    lock: (companyId: number, periodYYMM: number) =>
      apiRequest<PostingPeriodLock>(companyPath(companyId, `/posting-periods/${periodYYMM}/lock`), { method: 'POST' }),
    unlock: (companyId: number, periodYYMM: number) =>
      apiRequest<PostingPeriodLock>(companyPath(companyId, `/posting-periods/${periodYYMM}/unlock`), { method: 'POST' }),
  },
}
