import type { PaginationState, SortingState } from '@tanstack/react-table'
import { useSearchParams } from 'react-router-dom'

type UrlValue = string | number | boolean
type Patch<T> = { [K in keyof T]?: T[K] | null }

/**
 * UX-21: list filters/page/sort live in the query string, so "Back" after a drill-down
 * (and a copied link) restores the same view. Values are typed by their defaults;
 * defaults are omitted from the URL. Any change that doesn't touch `page` resets it.
 */
export function useUrlState<T extends Record<string, UrlValue>>(defaults: T): [T, (patch: Patch<T>) => void] {
  const [params, setParams] = useSearchParams()
  const values = { ...defaults }
  for (const key of Object.keys(defaults) as (keyof T & string)[]) {
    const raw = params.get(key)
    if (raw === null) continue
    const fallback = defaults[key]
    const parsed =
      typeof fallback === 'number'
        ? Number.isFinite(Number(raw)) ? Number(raw) : fallback
        : typeof fallback === 'boolean' ? raw === '1' || raw === 'true' : raw
    values[key] = parsed as T[typeof key]
  }

  const set = (patch: Patch<T>) =>
    setParams(
      (current) => {
        const next = new URLSearchParams(current)
        for (const [key, value] of Object.entries(patch)) {
          if (value === null || value === undefined || value === '' || value === defaults[key]) next.delete(key)
          else next.set(key, typeof value === 'boolean' ? (value ? '1' : '0') : String(value))
        }
        if (!('page' in patch)) next.delete('page')
        return next
      },
      { replace: true },
    )

  return [values, set]
}

interface TableUrlState {
  page: number
  size: number
  sort?: string
  desc?: boolean
}

/** Adapts `useUrlState` values (page, size, sort, desc) to ServerDataTable's controlled props. */
export function urlTableProps<T extends TableUrlState>(state: T, set: (patch: Patch<T>) => void) {
  const pagination: PaginationState = { pageIndex: state.page, pageSize: state.size }
  const sorting: SortingState = state.sort ? [{ id: state.sort, desc: Boolean(state.desc) }] : []
  return {
    pagination,
    sorting,
    onPaginationChange: (value: PaginationState) =>
      set({ page: value.pageIndex, size: value.pageSize } as Patch<T>),
    onSortingChange: (value: SortingState) =>
      set({ sort: value[0]?.id ?? '', desc: value[0]?.desc ?? false } as Patch<T>),
  }
}
