// Invoice engine extras: multi-company scope, per-building preview, interest on invoice. Merged in i18n.ts with addResourceBundle.
export const invoiceExtrasResources = {
  'sr-Latn': {
    invx_: {
      scope: 'Obuhvat', scopeSingle: 'Ova zajednica', scopeLocation: 'Lokacija', scopeAll: 'Sve zajednice', locationId: 'ID lokacije',
      building: 'Zajednica', previous: 'Prethodna serija', change: '± %', error: 'Greška',
      generatedCompanies: 'Generisano: {{ok}} zajednica, {{count}} računa. Neuspešno: {{failed}}.',
      amount: 'Osnovica', interest: 'Kamata', totalWithInterest: 'Ukupno sa kamatom', groupMember: 'Član grupnog računa (storno)',
    },
  },
  'sr-Cyrl': {
    invx_: {
      scope: 'Обухват', scopeSingle: 'Ова заједница', scopeLocation: 'Локација', scopeAll: 'Све заједнице', locationId: 'ИД локације',
      building: 'Заједница', previous: 'Претходна серија', change: '± %', error: 'Грешка',
      generatedCompanies: 'Генерисано: {{ok}} заједница, {{count}} рачуна. Неуспешно: {{failed}}.',
      amount: 'Основица', interest: 'Камата', totalWithInterest: 'Укупно са каматом', groupMember: 'Члан групног рачуна (сторно)',
    },
  },
  en: {
    invx_: {
      scope: 'Scope', scopeSingle: 'This company', scopeLocation: 'Location', scopeAll: 'All companies', locationId: 'Location ID',
      building: 'Company', previous: 'Previous batch', change: '± %', error: 'Error',
      generatedCompanies: 'Generated: {{ok}} companies, {{count}} invoices. Failed: {{failed}}.',
      amount: 'Net', interest: 'Interest', totalWithInterest: 'Total incl. interest', groupMember: 'Group invoice member (storno)',
    },
  },
}
