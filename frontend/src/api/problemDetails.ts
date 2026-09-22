import { ApiProblemError } from './generated/client'

export function getErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiProblemError) {
    const validationMessages = Object.values(error.problem.errors ?? {}).flat()
    return validationMessages.length > 0 ? validationMessages.join(' ') : error.message
  }

  if (error instanceof Error && error.message) {
    return error.message
  }

  return fallback
}
