import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'

export const resources = {
  'sr-Latn': {
    translation: {
      appName: 'SZ Upravljanje',
      dashboard: 'Početna',
      companies: 'Kompanije',
      partners: 'Partneri',
      units: 'Posebni delovi',
      contracts: 'Ugovori',
      billing: 'Fakturisanje',
      suppliers: 'Dobavljači',
      ledger: 'Glavna knjiga',
      banking: 'Izvodi',
      notices: 'Opomene',
      documents: 'Dokumenti',
      email: 'E-pošta',
      imports: 'Uvozi',
      reports: 'Izveštaji',
      administration: 'Administracija',
      activeCompany: 'Aktivna kompanija',
      logout: 'Odjavi se',
      navigation: 'Glavna navigacija',
      openNavigation: 'Otvori navigaciju',
      language: 'Jezik',
      comingSoon: 'Modul je pripremljen za implementaciju.',
      welcome: 'Dobro došli',
    },
  },
  'sr-Cyrl': {
    translation: {
      appName: 'СЗ Управљање',
      dashboard: 'Почетна',
      companies: 'Компаније',
      partners: 'Партнери',
      units: 'Посебни делови',
      contracts: 'Уговори',
      billing: 'Фактурисање',
      suppliers: 'Добављачи',
      ledger: 'Главна књига',
      banking: 'Изводи',
      notices: 'Опомене',
      documents: 'Документи',
      email: 'Е-пошта',
      imports: 'Увози',
      reports: 'Извештаји',
      administration: 'Администрација',
      activeCompany: 'Активна компанија',
      logout: 'Одјави се',
      navigation: 'Главна навигација',
      openNavigation: 'Отвори навигацију',
      language: 'Језик',
      comingSoon: 'Модул је припремљен за имплементацију.',
      welcome: 'Добро дошли',
    },
  },
  en: {
    translation: {
      appName: 'SZ Management',
      dashboard: 'Home',
      companies: 'Companies',
      partners: 'Partners',
      units: 'Units',
      contracts: 'Contracts',
      billing: 'Billing',
      suppliers: 'Suppliers',
      ledger: 'General ledger',
      banking: 'Banking',
      notices: 'Notices',
      documents: 'Documents',
      email: 'Email',
      imports: 'Imports',
      reports: 'Reports',
      administration: 'Administration',
      activeCompany: 'Active company',
      logout: 'Sign out',
      navigation: 'Main navigation',
      openNavigation: 'Open navigation',
      language: 'Language',
      comingSoon: 'This module is ready for implementation.',
      welcome: 'Welcome',
    },
  },
} as const

const storedLanguage = localStorage.getItem('sz.language')
const language = storedLanguage && storedLanguage in resources ? storedLanguage : 'sr-Latn'

void i18n.use(initReactI18next).init({
  resources,
  lng: language,
  fallbackLng: 'sr-Latn',
  interpolation: { escapeValue: false },
})

export default i18n
