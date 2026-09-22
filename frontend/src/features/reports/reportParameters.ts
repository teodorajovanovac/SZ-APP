import type { ReportParameters } from './types'

export function parseReportParameters(value: string): ReportParameters {
  const parsed: unknown = value.trim() ? JSON.parse(value) : {}
  if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) throw new Error('Parametri moraju biti JSON objekat.')
  const result: ReportParameters = {}
  for (const [key, item] of Object.entries(parsed)) {
    if (!/^[A-Za-z][A-Za-z0-9_]*$/.test(key)) throw new Error(`Naziv parametra '${key}' nije ispravan.`)
    if (item !== null && !['string', 'number', 'boolean'].includes(typeof item)) throw new Error(`Parametar '${key}' mora biti skalarna vrednost.`)
    result[key] = item as ReportParameters[string]
  }
  if ('CompanyId' in result || 'companyId' in result) throw new Error('CompanyId određuje server i ne unosi se ručno.')
  return result
}
