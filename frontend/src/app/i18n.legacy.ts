// Legacy-format exports (GAP-21) and audit history (GAP-24). Merged in i18n.ts with addResourceBundle.
export const legacyResources = {
  'sr-Latn': {
    legacyExport_: {
      title: 'Stari formati', subtitle: 'Izvozi u formatu stare aplikacije (CSV sa ; — otvara se u Excelu).',
      period: 'Period (YYMM)', download: 'Preuzmi', failed: 'Izvoz nije uspeo.', needsPeriod: 'Unesite period (npr. 2602).',
    },
    audit_: {
      title: 'Istorija izmena', subtitle: 'Ko je, kada i šta menjao u poslovnim podacima.', link: 'Istorija',
      entity: 'Entitet', itemId: 'ID zapisa', staffId: 'Korisnik (ID)', from: 'Od datuma', to: 'Do datuma', search: 'Prikaži',
      empty: 'Nema izmena za izabrane filtere.', loadFailed: 'Istorija nije dostupna.', more: 'Učitaj još',
      col: { time: 'Vreme', user: 'Korisnik', action: 'Akcija', entity: 'Entitet', item: 'Zapis', changes: 'Izmene' },
      action: { Added: 'Dodato', Modified: 'Izmenjeno', Deleted: 'Obrisano' },
    },
  },
  'sr-Cyrl': {
    legacyExport_: {
      title: 'Стари формати', subtitle: 'Извози у формату старе апликације (CSV са ; — отвара се у Екселу).',
      period: 'Период (YYMM)', download: 'Преузми', failed: 'Извоз није успео.', needsPeriod: 'Унесите период (нпр. 2602).',
    },
    audit_: {
      title: 'Историја измена', subtitle: 'Ко је, када и шта мењао у пословним подацима.', link: 'Историја',
      entity: 'Ентитет', itemId: 'ИД записа', staffId: 'Корисник (ИД)', from: 'Од датума', to: 'До датума', search: 'Прикажи',
      empty: 'Нема измена за изабране филтере.', loadFailed: 'Историја није доступна.', more: 'Учитај још',
      col: { time: 'Време', user: 'Корисник', action: 'Акција', entity: 'Ентитет', item: 'Запис', changes: 'Измене' },
      action: { Added: 'Додато', Modified: 'Измењено', Deleted: 'Обрисано' },
    },
  },
  en: {
    legacyExport_: {
      title: 'Legacy formats', subtitle: 'Exports in the old application format (;-separated CSV, opens in Excel).',
      period: 'Period (YYMM)', download: 'Download', failed: 'Export failed.', needsPeriod: 'Enter a period (e.g. 2602).',
    },
    audit_: {
      title: 'Change history', subtitle: 'Who changed what in business data, and when.', link: 'History',
      entity: 'Entity', itemId: 'Record ID', staffId: 'User (ID)', from: 'From', to: 'To', search: 'Show',
      empty: 'No changes for the selected filters.', loadFailed: 'History is unavailable.', more: 'Load more',
      col: { time: 'Time', user: 'User', action: 'Action', entity: 'Entity', item: 'Record', changes: 'Changes' },
      action: { Added: 'Added', Modified: 'Modified', Deleted: 'Deleted' },
    },
  },
}
