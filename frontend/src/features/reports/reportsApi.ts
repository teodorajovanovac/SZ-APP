import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiProblemError, apiRequest, type ProblemDetails } from '../../api/generated/client'
import type { AnalysisDefinition, AnalysisRun, ReportDefinition, ReportParameters, ReportRun } from './types'

// apiRequest attaches the antiforgery header automatically for unsafe methods.
async function run<T>(path: string, parameters: ReportParameters) {
  return apiRequest<T>(path, { method: 'POST', body: JSON.stringify({ parameters }) })
}

// download() can't go through apiRequest (it needs the raw Response for Blob, not
// parsed JSON), so it's the one caller that still has to fetch its own CSRF token.
async function csrfHeaders() {
  const token = await apiRequest<{ token: string; headerName: string }>('/api/v1/auth/antiforgery')
  return { [token.headerName]: token.token, 'Content-Type': 'application/json', Accept: 'application/json' }
}

async function download(path: string, parameters: ReportParameters): Promise<Blob> {
  const response = await fetch(path, {
    method: 'POST', credentials: 'include', headers: await csrfHeaders(), body: JSON.stringify({ parameters }),
  })
  if (!response.ok) {
    let problem: ProblemDetails = { status: response.status, title: response.statusText }
    try { problem = { ...problem, ...await response.json() as ProblemDetails } } catch { /* status problem is sufficient */ }
    throw new ApiProblemError(problem)
  }
  return response.blob()
}

export function useAnalysisDefinitions(companyId: number) {
  return useQuery({ queryKey: ['companies', companyId, 'analyses'], queryFn: () => apiRequest<AnalysisDefinition[]>(`/api/v1/companies/${companyId}/analyses`) })
}

export function useReportDefinitions(companyId: number) {
  return useQuery({ queryKey: ['companies', companyId, 'reports'], queryFn: () => apiRequest<ReportDefinition[]>(`/api/v1/companies/${companyId}/reports`) })
}

export function useRunAnalysis(companyId: number) {
  return useMutation({ mutationFn: ({ id, parameters }: { id: number; parameters: ReportParameters }) => run<AnalysisRun>(`/api/v1/companies/${companyId}/analyses/${id}/run`, parameters) })
}

export function useRunReport(companyId: number) {
  return useMutation({ mutationFn: ({ id, parameters }: { id: number; parameters: ReportParameters }) => run<ReportRun>(`/api/v1/companies/${companyId}/reports/${id}/run`, parameters) })
}

export function useExportReport(companyId: number) {
  return useMutation({ mutationFn: ({ kind, id, format, parameters }: { kind: 'analyses' | 'reports'; id: number; format: 'export.csv' | 'print'; parameters: ReportParameters }) => download(`/api/v1/companies/${companyId}/${kind}/${id}/${format}`, parameters) })
}

export function saveBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url; anchor.download = fileName; anchor.click()
  setTimeout(() => URL.revokeObjectURL(url), 0)
}

export function openPrintableHtml(blob: Blob) {
  const url = URL.createObjectURL(blob)
  window.open(url, '_blank', 'noopener,noreferrer')
  setTimeout(() => URL.revokeObjectURL(url), 60_000)
}
