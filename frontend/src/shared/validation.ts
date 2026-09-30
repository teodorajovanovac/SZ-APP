import i18n from 'i18next'
import { z } from 'zod'

/**
 * UX-40: zod messages in the UI language. `zMsg('validation.x')` is resolved when the
 * field is validated, so schemas can live at module scope and still follow a language
 * switch. Anything without an explicit message falls back to the generic map below
 * instead of zod's English defaults.
 */
export const zMsg = (key: string) => ({ error: () => i18n.t(key) })

z.config({
  customError: (issue) => {
    switch (issue.code) {
      case 'too_small':
        return issue.origin === 'string' && Number(issue.minimum) <= 1
          ? i18n.t('validation.required')
          : i18n.t('validation.tooSmall', { min: String(issue.minimum) })
      case 'too_big':
        return i18n.t('validation.tooBig', { max: String(issue.maximum) })
      case 'invalid_type':
        return issue.expected === 'number' ? i18n.t('validation.number') : i18n.t('validation.required')
      case 'invalid_format':
        return issue.format === 'email' ? i18n.t('validation.email') : i18n.t('validation.format')
      default:
        return undefined
    }
  },
})
