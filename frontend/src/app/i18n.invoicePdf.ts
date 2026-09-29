import i18n from './i18n'

// Invoice PDF / bulk email strings, merged into the default namespace so the main
// i18n.ts resource object doesn't keep growing.
const bundles = {
  'sr-Latn': {
    invoicePdf: {
      download: 'Preuzmi PDF',
      downloadAll: 'Preuzmi sve (PDF)',
      sendEmail: 'Pošalji mejlom',
      confirmTitle: 'Slanje računa mejlom',
      confirmBody: 'Biće poslato {{withEmail}} od {{total}} računa. Bez e-mail adrese: {{missing}}.',
      missingList: 'Bez adrese: {{names}}',
      confirm: 'Pošalji',
      sent: 'U red za slanje stavljeno: {{enqueued}}, preskočeno: {{skipped}}.',
      failed: 'Operacija nije uspela.',
    },
  },
  'sr-Cyrl': {
    invoicePdf: {
      download: 'Преузми PDF',
      downloadAll: 'Преузми све (PDF)',
      sendEmail: 'Пошаљи мејлом',
      confirmTitle: 'Слање рачуна мејлом',
      confirmBody: 'Биће послато {{withEmail}} од {{total}} рачуна. Без е-мејл адресе: {{missing}}.',
      missingList: 'Без адресе: {{names}}',
      confirm: 'Пошаљи',
      sent: 'У ред за слање стављено: {{enqueued}}, прескочено: {{skipped}}.',
      failed: 'Операција није успела.',
    },
  },
  en: {
    invoicePdf: {
      download: 'Download PDF',
      downloadAll: 'Download all (PDF)',
      sendEmail: 'Send by email',
      confirmTitle: 'Send invoices by email',
      confirmBody: '{{withEmail}} of {{total}} invoices will be sent. Without an email address: {{missing}}.',
      missingList: 'Missing address: {{names}}',
      confirm: 'Send',
      sent: 'Queued: {{enqueued}}, skipped: {{skipped}}.',
      failed: 'Operation failed.',
    },
  },
} as const

for (const [lng, bundle] of Object.entries(bundles)) {
  i18n.addResourceBundle(lng, 'translation', bundle, true, false)
}
