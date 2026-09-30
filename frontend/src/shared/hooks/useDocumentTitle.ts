import { useEffect } from 'react'
import i18n from 'i18next'

/** UX-37: browser tab / history entries show the screen, e.g. "Partneri · SZ Upravljanje". */
export function useDocumentTitle(title: string | undefined) {
  useEffect(() => {
    const app = i18n.t('appName')
    document.title = title ? `${title} · ${app}` : app
  }, [title])
}
