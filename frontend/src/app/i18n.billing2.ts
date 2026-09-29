// Server-side invoice generation wizard (audit 9.1, flow A 5.9). Merged in i18n.ts with addResourceBundle.
export const billing2Resources = {
  'sr-Latn': {
    billing2_: {
      title: 'Obračun računa', previewButton: 'Pregled obračuna', generateButton: 'Generiši račune',
      previewFailed: 'Obračun nije uspeo.', customers: 'Broj kupaca: {{count}}',
      net: 'Osnovica', vat: 'PDV', interest: 'Kamata', total: 'Ukupno', customer: 'Kupac',
      generated: 'Generisano računa: {{count}}.', alreadyGenerated: 'Serija za ovaj period je već generisana ({{count}} računa).',
      interestNote: 'Kamata se dodaje posebnim obračunom kamate.',
      created: 'Serija je kreirana. Proverite obračun pa generišite račune.',
    },
  },
  'sr-Cyrl': {
    billing2_: {
      title: 'Обрачун рачуна', previewButton: 'Преглед обрачуна', generateButton: 'Генериши рачуне',
      previewFailed: 'Обрачун није успео.', customers: 'Број купаца: {{count}}',
      net: 'Основица', vat: 'ПДВ', interest: 'Камата', total: 'Укупно', customer: 'Купац',
      generated: 'Генерисано рачуна: {{count}}.', alreadyGenerated: 'Серија за овај период је већ генерисана ({{count}} рачуна).',
      interestNote: 'Камата се додаје посебним обрачуном камате.',
      created: 'Серија је креирана. Проверите обрачун па генеришите рачуне.',
    },
  },
  en: {
    billing2_: {
      title: 'Invoice calculation', previewButton: 'Preview calculation', generateButton: 'Generate invoices',
      previewFailed: 'Calculation failed.', customers: 'Customers: {{count}}',
      net: 'Net', vat: 'VAT', interest: 'Interest', total: 'Total', customer: 'Customer',
      generated: 'Invoices generated: {{count}}.', alreadyGenerated: 'This period was already generated ({{count}} invoices).',
      interestNote: 'Interest is added by the separate interest run.',
      created: 'Batch created. Review the calculation, then generate the invoices.',
    },
  },
}
