import type { LocationCategory } from './types'

export interface LocationCategoryOption {
  id: number
  label: string
}

// Flattens the parentId-linked tree into full paths ("BW", "BW / Plot 24"), depth-first so
// children follow their parent; the backend already orders siblings by SortIndex/Name.
export function flattenLocationCategories(categories: LocationCategory[]): LocationCategoryOption[] {
  const byParent = new Map<number | null, LocationCategory[]>()
  const ids = new Set(categories.map((c) => c.id))
  for (const category of categories) {
    // An orphan (parent missing from the list) is treated as a root so it is never dropped.
    const parent = category.parentId != null && ids.has(category.parentId) ? category.parentId : null
    const siblings = byParent.get(parent) ?? []
    siblings.push(category)
    byParent.set(parent, siblings)
  }
  const options: LocationCategoryOption[] = []
  const visit = (parentId: number | null, prefix: string, ancestors: Set<number>) => {
    for (const node of byParent.get(parentId) ?? []) {
      if (ancestors.has(node.id)) continue
      const name = node.name.trim()
      const label = prefix ? `${prefix} / ${name}` : name
      options.push({ id: node.id, label })
      visit(node.id, label, new Set(ancestors).add(node.id))
    }
  }
  visit(null, '', new Set())
  return options
}
