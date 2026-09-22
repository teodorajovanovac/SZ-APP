export interface AnalysisDefinition {
  id: number
  dataGroup: string
  name: string
  isCriticalGroup: boolean
  queryName: string
  reportName: string | null
  filterCaption: string | null
  autoRunAll: boolean
  sortIndex: number
  lastRunAt: string | null
  lastRowCount: number | null
  adminAlert: boolean
  description: string | null
  hasBlockedLegacyAction: boolean
}

export interface ReportDetail { id: number; queryName: string; sortIndex: number; function: string }
export interface ReportButton { id: number; caption: string; function: 'Run' | 'ExportCsv' | 'PrintHtml'; functionTypeId: number; sortIndex: number }
export interface ReportDefinition {
  id: number
  name: string
  datasheet: string | null
  title: string
  sortIndex: number
  filterCaption: string | null
  details: ReportDetail[]
  buttons: ReportButton[]
}

export type ReportParameterValue = string | number | boolean | null
export type ReportParameters = Record<string, ReportParameterValue>

export interface ReportTable {
  name: string
  function: string
  columns: string[]
  rows: unknown[][]
  rowCount: number
  isTruncated: boolean
}

export interface AnalysisRun { definitionId: number; name: string; result: ReportTable; durationMs: number }
export interface ReportRun { definitionId: number; title: string; sections: ReportTable[]; durationMs: number }
