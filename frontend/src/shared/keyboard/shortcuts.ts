import { useEffect, useRef } from 'react'

/**
 * UX-50 / audit 9.7: one window keydown listener + a stack of page-registered combos.
 * Combos are lower-case, modifiers first: 'alt+n', 'ctrl+s', 'ctrl+enter', '?', '/'.
 * Plain keys are ignored while typing in a field; Ctrl/Alt combos always fire.
 * Keys inside an open dialog belong to the dialog (FormDialog handles Ctrl+S / Esc itself).
 */
export type ShortcutHandler = (event: KeyboardEvent) => void

/** `g` then a letter. Paths are the routes in App.tsx. */
export const NAV_SHORTCUTS: Record<string, string> = {
  d: '/',
  p: '/partners',
  u: '/units',
  c: '/contracts',
  i: '/billing',
  s: '/suppliers',
  b: '/banking',
  k: '/kartice',
  n: '/notices',
  r: '/reports',
}

const SEQUENCE_TIMEOUT_MS = 1500
const registry: { combo: string; handler: { current: ShortcutHandler } }[] = []

export function comboFromEvent(event: KeyboardEvent): string {
  const withModifier = event.ctrlKey || event.metaKey || event.altKey
  // Alt+letter yields a special character on some layouts (mac '˜'), so use the physical key.
  const key = withModifier && /^Key[A-Z]$/.test(event.code) ? event.code.slice(3).toLowerCase() : event.key.toLowerCase()
  const parts: string[] = []
  if (event.ctrlKey || event.metaKey) parts.push('ctrl')
  if (event.altKey) parts.push('alt')
  // Shift only matters for named keys; for '?' it is already part of the character.
  if (event.shiftKey && key.length > 1) parts.push('shift')
  parts.push(key)
  return parts.join('+')
}

export function isTypingTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) return false
  return target.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName)
}

/** Registers `combo` while mounted; the most recently mounted registration wins. Pass a falsy handler to disable. */
export function useShortcut(combo: string, handler: ShortcutHandler | false | undefined) {
  const ref = useRef<ShortcutHandler>(() => undefined)
  useEffect(() => {
    if (handler) ref.current = handler
  })
  const enabled = Boolean(handler)
  useEffect(() => {
    if (!enabled) return
    const entry = { combo, handler: ref }
    registry.push(entry)
    return () => {
      registry.splice(registry.indexOf(entry), 1)
    }
  }, [combo, enabled])
}

function focusPageSearch() {
  const main = document.getElementById('main-content') ?? document.body
  const input =
    main.querySelector<HTMLInputElement>('input[type="search"]') ??
    main.querySelector<HTMLInputElement>('input[type="text"]:not([disabled]):not([readonly])')
  if (!input) return false
  input.focus()
  input.select()
  return true
}

interface GlobalShortcutOptions {
  navigate: (path: string) => void
  openHelp: () => void
}

/** Installs the single window listener. Mount once, in the app shell. */
export function useGlobalShortcuts(options: GlobalShortcutOptions) {
  const optionsRef = useRef(options)
  useEffect(() => {
    optionsRef.current = options
  })

  useEffect(() => {
    let pendingG = 0
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.defaultPrevented || event.isComposing) return
      const target = event.target
      if (target instanceof Element && target.closest('[role="dialog"]')) return
      const combo = comboFromEvent(event)
      const withModifier = combo.includes('ctrl+') || combo.includes('alt+')
      if (!withModifier && isTypingTarget(target)) {
        pendingG = 0
        return
      }

      if (pendingG && Date.now() - pendingG < SEQUENCE_TIMEOUT_MS) {
        pendingG = 0
        const path = NAV_SHORTCUTS[combo]
        if (path) {
          event.preventDefault()
          optionsRef.current.navigate(path)
          return
        }
      }
      pendingG = 0

      if (combo === 'g') {
        pendingG = Date.now()
        return
      }
      if (combo === '?') {
        event.preventDefault()
        optionsRef.current.openHelp()
        return
      }
      if (combo === '/' && focusPageSearch()) {
        event.preventDefault()
        return
      }
      const entry = [...registry].reverse().find((item) => item.combo === combo)
      if (entry) {
        event.preventDefault()
        entry.handler.current(event)
      }
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [])
}
