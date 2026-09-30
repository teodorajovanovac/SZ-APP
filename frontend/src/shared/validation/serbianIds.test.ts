import { describe, expect, it } from 'vitest'
import { formatBankAccount, isValidBankAccount, isValidJmbg, isValidMb, isValidPib } from './serbianIds'

// Check digits below were computed by hand from the legacy VBA algorithms.
describe('serbianIds', () => {
  it('validates PIB', () => {
    expect(isValidPib('101134702')).toBe(true) // Telekom Srbija
    expect(isValidPib('101134703')).toBe(false)
    expect(isValidPib('12345')).toBe(false)
  })
  it('validates JMBG', () => {
    expect(isValidJmbg('0101006500006')).toBe(true)
    expect(isValidJmbg('0101006500007')).toBe(false)
    expect(isValidJmbg('abc')).toBe(false)
  })
  it('validates MB', () => {
    expect(isValidMb('17162543')).toBe(true) // Telekom Srbija
    expect(isValidMb('17162544')).toBe(false)
  })
  it('formats and checks bank accounts', () => {
    expect(formatBankAccount('1600000000001234')).toMatch(/^160-0{3}\d+-\d\d$|^160-\d{13}-\d\d$/)
    expect(formatBankAccount('160-123-45')).toBe('160-0000000000123-45')
    expect(formatBankAccount('12')).toBeNull()
    expect(isValidBankAccount('160-0000000000123-45')).toBe(false)
    expect(isValidBankAccount('160-123-95')).toBe(true)
  })
})
