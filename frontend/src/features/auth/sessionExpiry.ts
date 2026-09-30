import { useSyncExternalStore } from 'react'

// Tiny global flag: set by the query/mutation caches on a 401, cleared after re-login.
let expired = false
const listeners = new Set<() => void>()
const emit = () => listeners.forEach((listener) => listener())

export function markSessionExpired() {
  if (expired) return
  expired = true
  emit()
}

export function clearSessionExpired() {
  expired = false
  emit()
}

export function useSessionExpired() {
  return useSyncExternalStore(
    (listener) => {
      listeners.add(listener)
      return () => listeners.delete(listener)
    },
    () => expired,
  )
}
