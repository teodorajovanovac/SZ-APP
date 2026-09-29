// Ledger cards (GAP-07), partner balances (GAP-08), Ctrl+K search (DES-08). Merged in i18n.ts with addResourceBundle.
export const cardsResources = {
  'sr-Latn': {
    cards_: {
      title: 'Kartice', tabCard: 'Kartica', account: 'Konto', allAccounts: 'Sva konta', partner: 'Partner',
      from: 'Od datuma', to: 'Do datuma', paymentReference: 'Poziv na broj', document: 'Dokument', groupBy: 'Grupisanje',
      grouping: { none: 'Bez grupisanja', paymentReference: 'Po pozivu na broj', document: 'Po dokumentu', openItems: 'Samo otvorene stavke' },
      print: 'Štampaj', pickFilter: 'Izaberite konto ili partnera da biste videli karticu.', loadFailed: 'Kartica nije dostupna.',
      opening: 'Početno stanje', carry: 'Prenos', totals: 'Ukupno', empty: 'Nema stavki za izabrane filtere.',
      dblClickHint: 'Dvoklik na red otvara nalog.', journalTitle: 'Nalog {{id}}',
      col: { date: 'Datum', journal: 'Nalog', count: 'Broj stavki', document: 'Dokument', description: 'Opis', lineType: 'Tip', dueDate: 'Valuta', paymentReference: 'Poziv na broj', debit: 'Duguje', credit: 'Potražuje', balance: 'Saldo', account: 'Konto', partnerAccount: 'Partner' },
      lineTypes: { 1: 'Izvod', 3: 'Izdat račun', 4: 'Ulazni račun', 7: 'Storno', 8: 'Preknjiženje', 11: 'Provizija', 91: 'Kompenzacija', 94: 'Preuzeto stanje', 97: 'Pozajmica', 98: 'Preuzeto dugovanje', 99: 'Početno stanje' },
    },
    balances_: {
      title: 'Stanja partnera', asOf: 'Stanje na dan', onlyDebtors: 'Samo dužnici', minDebt: 'Minimalni dug', empty: 'Nema partnera za izabrane filtere.',
      overdueHint: 'Dospeli dug: stavke sa valutom pre izabranog datuma koje nisu zatvorene (uplate zatvaraju najstarije stavke prve).',
      col: { accountNumber: 'Šifra', partner: 'Partner', overdue: 'Dospelo', lastPayment: 'Poslednja uplata' },
    },
    search_: {
      open: 'Pretraga (Ctrl+K)', placeholder: 'Ime, PIB, šifra kupca, poziv na broj, stan…', minChars: 'Unesite bar 2 znaka.',
      noResults: 'Nema rezultata.', hint: '↑/↓ izbor · Enter otvara · Esc zatvara', balance: 'Saldo',
      type: { partner: 'Partner', unit: 'Posebni deo', payment: 'Poziv na broj' },
    },
  },
  'sr-Cyrl': {
    cards_: {
      title: 'Картице', tabCard: 'Картица', account: 'Конто', allAccounts: 'Сва конта', partner: 'Партнер',
      from: 'Од датума', to: 'До датума', paymentReference: 'Позив на број', document: 'Документ', groupBy: 'Груписање',
      grouping: { none: 'Без груписања', paymentReference: 'По позиву на број', document: 'По документу', openItems: 'Само отворене ставке' },
      print: 'Штампај', pickFilter: 'Изаберите конто или партнера да бисте видели картицу.', loadFailed: 'Картица није доступна.',
      opening: 'Почетно стање', carry: 'Пренос', totals: 'Укупно', empty: 'Нема ставки за изабране филтере.',
      dblClickHint: 'Двоклик на ред отвара налог.', journalTitle: 'Налог {{id}}',
      col: { date: 'Датум', journal: 'Налог', count: 'Број ставки', document: 'Документ', description: 'Опис', lineType: 'Тип', dueDate: 'Валута', paymentReference: 'Позив на број', debit: 'Дугује', credit: 'Потражује', balance: 'Салдо', account: 'Конто', partnerAccount: 'Партнер' },
      lineTypes: { 1: 'Извод', 3: 'Издат рачун', 4: 'Улазни рачун', 7: 'Сторно', 8: 'Прекњижење', 11: 'Провизија', 91: 'Компензација', 94: 'Преузето стање', 97: 'Позајмица', 98: 'Преузето дуговање', 99: 'Почетно стање' },
    },
    balances_: {
      title: 'Стања партнера', asOf: 'Стање на дан', onlyDebtors: 'Само дужници', minDebt: 'Минимални дуг', empty: 'Нема партнера за изабране филтере.',
      overdueHint: 'Доспели дуг: ставке са валутом пре изабраног датума које нису затворене (уплате затварају најстарије ставке прве).',
      col: { accountNumber: 'Шифра', partner: 'Партнер', overdue: 'Доспело', lastPayment: 'Последња уплата' },
    },
    search_: {
      open: 'Претрага (Ctrl+K)', placeholder: 'Име, ПИБ, шифра купца, позив на број, стан…', minChars: 'Унесите бар 2 знака.',
      noResults: 'Нема резултата.', hint: '↑/↓ избор · Enter отвара · Esc затвара', balance: 'Салдо',
      type: { partner: 'Партнер', unit: 'Посебни део', payment: 'Позив на број' },
    },
  },
  en: {
    cards_: {
      title: 'Ledger cards', tabCard: 'Card', account: 'Account', allAccounts: 'All accounts', partner: 'Partner',
      from: 'From', to: 'To', paymentReference: 'Payment reference', document: 'Document', groupBy: 'Grouping',
      grouping: { none: 'No grouping', paymentReference: 'By payment reference', document: 'By document', openItems: 'Open items only' },
      print: 'Print', pickFilter: 'Choose an account or partner to see the card.', loadFailed: 'Card is unavailable.',
      opening: 'Opening balance', carry: 'Carried forward', totals: 'Totals', empty: 'No entries for the selected filters.',
      dblClickHint: 'Double-click a row to open the journal.', journalTitle: 'Journal {{id}}',
      col: { date: 'Date', journal: 'Journal', count: 'Lines', document: 'Document', description: 'Description', lineType: 'Type', dueDate: 'Due date', paymentReference: 'Payment reference', debit: 'Debit', credit: 'Credit', balance: 'Balance', account: 'Account', partnerAccount: 'Partner' },
      lineTypes: { 1: 'Bank statement', 3: 'Invoice issued', 4: 'Supplier invoice', 7: 'Reversal', 8: 'Reclassification', 11: 'Bank fee', 91: 'Compensation', 94: 'Balance taken over', 97: 'Loan', 98: 'Debt taken over', 99: 'Opening balance' },
    },
    balances_: {
      title: 'Partner balances', asOf: 'Balance as of', onlyDebtors: 'Debtors only', minDebt: 'Minimum debt', empty: 'No partners for the selected filters.',
      overdueHint: 'Overdue: open items with a due date before the selected date (payments settle the oldest items first).',
      col: { accountNumber: 'Account no.', partner: 'Partner', overdue: 'Overdue', lastPayment: 'Last payment' },
    },
    search_: {
      open: 'Search (Ctrl+K)', placeholder: 'Name, tax ID, customer number, payment reference, unit…', minChars: 'Type at least 2 characters.',
      noResults: 'No results.', hint: '↑/↓ select · Enter opens · Esc closes', balance: 'Balance',
      type: { partner: 'Partner', unit: 'Unit', payment: 'Payment reference' },
    },
  },
} as const
