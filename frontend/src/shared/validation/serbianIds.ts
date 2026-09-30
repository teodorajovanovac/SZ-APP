// Ports of the legacy VBA checks (ProveriJMBG, ProveriMB, ProveriPIB, TR_Provera).
// All return true only for well-formed, correctly check-digited values; callers decide
// whether an empty value is allowed.

const digitsOnly = (value: string) => /^\d+$/.test(value)

/** Mod-11 check digit: 11 - (sum mod 11), where 10 and 11 become 0. */
const mod11Digit = (sum: number) => {
  const result = 11 - (sum % 11)
  return result >= 1 && result <= 9 ? result : 0
}

export function isValidJmbg(value: string): boolean {
  if (!digitsOnly(value) || value.length !== 13) return false
  const d = [...value].map(Number)
  let sum = 0
  for (let i = 0; i < 6; i++) sum += (7 - i) * (d[i]! + d[i + 6]!)
  return mod11Digit(sum) === d[12]
}

export function isValidMb(value: string): boolean {
  if (!digitsOnly(value) || value.length !== 8) return false
  const d = [...value].map(Number)
  const weights = [2, 7, 6, 5, 4, 3, 2]
  const sum = weights.reduce((acc, weight, i) => acc + weight * d[i]!, 0)
  return mod11Digit(sum) === d[7]
}

export function isValidPib(value: string): boolean {
  if (!digitsOnly(value) || value.length !== 9) return false
  const d = [...value].map(Number)
  let carry = 10
  for (let i = 0; i < 8; i++) {
    let s = (d[i]! + carry) % 10
    if (s === 0) s = 10
    carry = (s * 2) % 11
  }
  return (11 - carry) % 10 === d[8]
}

/** Legacy KontrolniBroj(97, digits): two-digit mod-97 check, 98 - (n * 100 mod 97). */
function checkNumber97(digits: string): string {
  const result = 98n - ((BigInt(digits) * 100n) % 97n)
  return result.toString().padStart(2, '0')
}

/** Normalises to "XXX-0000000000000-KK", or null when the shape is wrong (legacy TR_Formatiraj). */
export function formatBankAccount(input: string): string | null {
  const text = input.trim()
  if (!text) return null
  const start = text.indexOf('-')
  const end = start < 0 ? -1 : text.indexOf('-', start + 1)
  let bank: string, middle: string
  if (start >= 0 && end > 0) {
    const middleLength = end - start - 1
    if (start !== 3 || middleLength < 1 || middleLength > 13 || text.length - end - 1 !== 2 ||
      text.length < 8 || text.length > 20 || !digitsOnly(text.replaceAll('-', ''))) return null
    bank = text.slice(0, 3)
    middle = text.slice(4, text.length - 3)
  } else {
    if (text.length < 6 || text.length > 18 || !digitsOnly(text)) return null
    bank = text.slice(0, 3)
    middle = text.slice(3, text.length - 2)
  }
  return `${bank}-${middle.padStart(13, '0')}-${text.slice(-2)}`
}

export function isValidBankAccount(input: string): boolean {
  const formatted = formatBankAccount(input)
  if (!formatted) return false
  const body = formatted.slice(0, -3).replaceAll('-', '')
  return checkNumber97(body) === formatted.slice(-2)
}
