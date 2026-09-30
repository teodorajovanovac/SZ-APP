import i18n from 'i18next'
import { ApiProblemError } from './generated/client'

// ASP.NET's default ProblemDetails titles (and our statusText fallback) are English
// boilerplate; they say nothing the status code doesn't, so show a translated message instead.
const GENERIC_TITLE =
  /^(bad request|unauthorized|forbidden|not found|method not allowed|conflict|gone|payload too large|unprocessable (entity|content)|too many requests|internal server error|bad gateway|service unavailable|gateway timeout|an error occurred while processing your request\.?|one or more validation errors occurred\.?|http greška)$/i
const STATUS_KEYS: Record<number, string> = {
  401: 'errors.http401',
  403: 'errors.http403',
  404: 'errors.http404',
  409: 'errors.http409',
  413: 'errors.http413',
  429: 'errors.http429',
}

export function getErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiProblemError) {
    const validationMessages = Object.values(error.problem.errors ?? {}).flat()
    if (validationMessages.length > 0) return validationMessages.join(' ')
    const { detail, title, status = 0 } = error.problem
    if (detail) return detail
    if (title && !GENERIC_TITLE.test(title.trim())) return title
    const key = STATUS_KEYS[status] ?? (status >= 500 ? 'errors.http500' : undefined)
    return key && i18n.exists(key) ? i18n.t(key) : fallback
  }

  if (error instanceof TypeError) return i18n.exists('errors.network') ? i18n.t('errors.network') : fallback

  if (error instanceof Error && error.message) {
    return error.message
  }

  return fallback
}
