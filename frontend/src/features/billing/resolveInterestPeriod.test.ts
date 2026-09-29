import { describe, expect, it } from 'vitest'
import { resolveInterestPeriod } from './billingApi'

const presets = [
  { key: 'fromPreviousDueDate' as const, start: '2026-08-25', end: '2026-08-31' },
  { key: 'wholeMonth' as const, start: '2026-08-01', end: '2026-08-31' },
]

describe('resolveInterestPeriod', () => {
  it('uses the company default preset until the user picks one', () => {
    expect(resolveInterestPeriod({ interestPreset: '', interestStart: '', interestEnd: '' }, presets, 'wholeMonth'))
      .toEqual({ key: 'wholeMonth', start: '2026-08-01', end: '2026-08-31' })
  })

  it('uses the edited dates for a custom range', () => {
    expect(resolveInterestPeriod({ interestPreset: 'custom', interestStart: '2026-08-10', interestEnd: '2026-08-20' }, presets, 'wholeMonth'))
      .toEqual({ key: 'custom', start: '2026-08-10', end: '2026-08-20' })
  })
})
