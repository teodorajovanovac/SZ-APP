// Notice documents (GAP-09 PDF/ZIP/email), notice cost posting, "za utuženje" (GAP-20). Merged in i18n.ts with addResourceBundle.
export const noticeDocsResources = {
  'sr-Latn': {
    noticeDocs_: {
      batch: 'Serija opomena', pickBatch: 'Izaberite seriju', pdf: 'PDF', zip: 'Preuzmi sve (ZIP)', email: 'Pošalji mejlom',
      emailTitle: 'Slanje opomena mejlom', emailBody: 'Biće poslato {{withEmail}} od {{total}} opomena. Bez e-mail adrese: {{missing}}.',
      sent: 'U red za slanje stavljeno: {{enqueued}}, preskočeno: {{skipped}}.',
      postCosts: 'Proknjiži troškove', cancelCosts: 'Storniraj troškove', costsPosted: 'Troškovi proknjiženi (nalog {{id}})',
      postTitle: 'Knjiženje troškova opomena', postBody: 'Troškovi ({{amount}}) biće proknjiženi: 2040 duguje po partneru / 4900 potražuje. Podrazumevano se ne knjiže.',
      cancelTitle: 'Storno troškova opomena', cancelBody: 'Knjiženje troškova biće stornirano (crveni storno, današnji datum).',
      lawsuit: 'Za utuženje', lawyerCost: 'Trošak advokata', exportLawsuit: 'Izvoz za utuženje (CSV)', save: 'Sačuvaj', failed: 'Operacija nije uspela.',
    },
  },
  'sr-Cyrl': {
    noticeDocs_: {
      batch: 'Серија опомена', pickBatch: 'Изаберите серију', pdf: 'PDF', zip: 'Преузми све (ZIP)', email: 'Пошаљи мејлом',
      emailTitle: 'Слање опомена мејлом', emailBody: 'Биће послато {{withEmail}} од {{total}} опомена. Без е-мејл адресе: {{missing}}.',
      sent: 'У ред за слање стављено: {{enqueued}}, прескочено: {{skipped}}.',
      postCosts: 'Прокњижи трошкове', cancelCosts: 'Сторнирај трошкове', costsPosted: 'Трошкови прокњижени (налог {{id}})',
      postTitle: 'Књижење трошкова опомена', postBody: 'Трошкови ({{amount}}) биће прокњижени: 2040 дугује по партнеру / 4900 потражује. Подразумевано се не књиже.',
      cancelTitle: 'Сторно трошкова опомена', cancelBody: 'Књижење трошкова биће сторнирано (црвени сторно, данашњи датум).',
      lawsuit: 'За утужење', lawyerCost: 'Трошак адвоката', exportLawsuit: 'Извоз за утужење (CSV)', save: 'Сачувај', failed: 'Операција није успела.',
    },
  },
  en: {
    noticeDocs_: {
      batch: 'Notice batch', pickBatch: 'Select a batch', pdf: 'PDF', zip: 'Download all (ZIP)', email: 'Send by email',
      emailTitle: 'Send notices by email', emailBody: '{{withEmail}} of {{total}} notices will be sent. Without an email address: {{missing}}.',
      sent: 'Queued: {{enqueued}}, skipped: {{skipped}}.',
      postCosts: 'Post costs', cancelCosts: 'Reverse costs', costsPosted: 'Costs posted (journal {{id}})',
      postTitle: 'Post notice costs', postBody: 'Costs ({{amount}}) will be posted: 2040 debit per partner / 4900 credit. Not posted by default.',
      cancelTitle: 'Reverse notice costs', cancelBody: 'The cost posting will be reversed (red storno, today).',
      lawsuit: 'For lawsuit', lawyerCost: 'Lawyer cost', exportLawsuit: 'Lawsuit export (CSV)', save: 'Save', failed: 'Operation failed.',
    },
  },
} as const
