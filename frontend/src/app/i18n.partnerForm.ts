// Partner form extras (MB/PIB/JMBG check digits, partner type, company, SEF flags, addresses). Merged in i18n.ts with addResourceBundle.
export const partnerFormResources = {
  'sr-Latn': {
    partnerForm_: {
      mbInvalid: 'Matični broj nije ispravan (8 cifara sa kontrolnom cifrom).',
      pibInvalid: 'PIB nije ispravan (9 cifara sa kontrolnom cifrom).',
      jmbgInvalid: 'JMBG nije ispravan (13 cifara sa kontrolnom cifrom).',
      partnerType: 'Tip partnera',
      notSelected: 'Nije izabran',
      company: 'Kompanija',
      skipValidation: 'Bez provere',
      addresses: 'Adrese',
      addressesAfterSave: 'Adrese se unose nakon što se partner prvi put sačuva.',
    },
  },
  'sr-Cyrl': {
    partnerForm_: {
      mbInvalid: 'Матични број није исправан (8 цифара са контролном цифром).',
      pibInvalid: 'ПИБ није исправан (9 цифара са контролном цифром).',
      jmbgInvalid: 'ЈМБГ није исправан (13 цифара са контролном цифром).',
      partnerType: 'Тип партнера',
      notSelected: 'Није изабран',
      company: 'Компанија',
      skipValidation: 'Без провере',
      addresses: 'Адресе',
      addressesAfterSave: 'Адресе се уносе након што се партнер први пут сачува.',
    },
  },
  en: {
    partnerForm_: {
      mbInvalid: 'Invalid registration number (8 digits with check digit).',
      pibInvalid: 'Invalid tax number (9 digits with check digit).',
      jmbgInvalid: 'Invalid personal ID number (13 digits with check digit).',
      partnerType: 'Partner type',
      notSelected: 'Not selected',
      company: 'Company',
      skipValidation: 'Skip validation',
      addresses: 'Addresses',
      addressesAfterSave: 'Addresses can be added after the partner is saved for the first time.',
    },
  },
}
