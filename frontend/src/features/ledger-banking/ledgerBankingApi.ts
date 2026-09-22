import { apiRequest } from '../../api/generated/client'
import type {
  BankStatementImport,
  BankStatementSummary,
  JournalEntrySummary,
  PageResponse,
  PostingResult,
} from './types'

interface AntiforgeryToken {
  token: string
  headerName: string
}

async function mutationRequest<T>(path: string, method: 'POST' | 'PUT', body: unknown): Promise<T> {
  const csrf = await apiRequest<AntiforgeryToken>('/api/v1/auth/antiforgery')
  return apiRequest<T>(path, {
    method,
    headers: {
      [csrf.headerName]: csrf.token,
      'Idempotency-Key': crypto.randomUUID(),
    },
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
    import: (companyId: number, request: BankStatementImport) =>
      mutationRequest(companyPath(companyId, '/bank-statements/import'), 'POST', request),
    post: (companyId: number, statement: BankStatementSummary) =>
      mutationRequest<PostingResult>(companyPath(companyId, `/bank-statements/${statement.id}/post`), 'POST', {
        rowVersion: statement.rowVersion,
      }),
  },
}
