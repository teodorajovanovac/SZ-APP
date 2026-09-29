// Login screen extras (password visibility, Caps Lock warning, brand panel). Merged in i18n.ts with addResourceBundle.
export const loginResources = {
  'sr-Latn': {
    login_: {
      tagline: 'Naplata, knjigovodstvo i izvodi stambenih zajednica na jednom mestu.',
      point1: 'Fakturisanje i opomene bez ručnog računanja',
      point2: 'Bankarski izvodi koji se sami upare sa uplatama',
      point3: 'Kartice i stanja stanara u realnom vremenu',
      showPassword: 'Prikaži lozinku',
      hidePassword: 'Sakrij lozinku',
      capsLock: 'Caps Lock je uključen.',
      welcome: 'Dobrodošli nazad',
    },
  },
  'sr-Cyrl': {
    login_: {
      tagline: 'Наплата, књиговодство и изводи стамбених заједница на једном месту.',
      point1: 'Фактурисање и опомене без ручног рачунања',
      point2: 'Банкарски изводи који се сами упаре са уплатама',
      point3: 'Картице и стања станара у реалном времену',
      showPassword: 'Прикажи лозинку',
      hidePassword: 'Сакриј лозинку',
      capsLock: 'Caps Lock је укључен.',
      welcome: 'Добродошли назад',
    },
  },
  en: {
    login_: {
      tagline: 'Billing, bookkeeping and bank statements for housing communities, in one place.',
      point1: 'Invoicing and reminders without manual calculation',
      point2: 'Bank statements that match payments automatically',
      point3: 'Live resident ledger cards and balances',
      showPassword: 'Show password',
      hidePassword: 'Hide password',
      capsLock: 'Caps Lock is on.',
      welcome: 'Welcome back',
    },
  },
} as const
