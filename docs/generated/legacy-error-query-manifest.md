# Legacy error-like query manifest

Subset whose query name begins with `ERROR` (case-insensitive); these queries encode consistency checks or their supporting/fix queries.

Generated deterministically from `docs/queries-sql.md`. Entries: **69**.

| Name | Kind | Error-like | Target domain | Status | Source |
|---|---|---:|---|---|---|
| ERROR_001-SUMA-STAVKE-IZVODA-NIJE-SUMA-GK | SELECT | true | banking | blocked-by-missing-source | [L441](../queries-sql.md#L441) |
| ERROR_002-STAVKE U GK SA POGRESNIM SKUPSTINAMA KUPCA | SELECT | true | banking | blocked-by-missing-source | [L451](../queries-sql.md#L451) |
| ERROR_003-STAVKE IZVODA NA CEKANJU | SELECT | true | banking | blocked-by-missing-source | [L459](../queries-sql.md#L459) |
| ERROR_004-NEPOSTOJI-KUPAC-PREMA-GK | SELECT | true | banking | needs-port | [L468](../queries-sql.md#L468) |
| ERROR_005-KUPAC-BEZ-NEKRETNINE | SELECT | true | master-data | blocked-by-missing-source | [L476](../queries-sql.md#L476) |
| ERROR_006-IZVODISTAVKE-BEZ-IZVODA | SELECT | true | banking | blocked-by-missing-source | [L484](../queries-sql.md#L484) |
| ERROR_007-IZVODI-KOJI-IMAJU-R-A-NISU-R | SELECT | true | banking | needs-port | [L492](../queries-sql.md#L492) |
| ERROR_008-IZVODI-SUMASTAVKI-KONTROLA | SELECT | true | banking | needs-port | [L501](../queries-sql.md#L501) |
| ERROR_009_IZVOD-NEPOSTOJECA-SZ | SELECT | true | banking | blocked-by-missing-source | [L510](../queries-sql.md#L510) |
| ERROR_009-IZVOD-STAVKE-U-GK-POGRESNA-SZ-KORISNIKA | SELECT | true | banking | needs-port | [L518](../queries-sql.md#L518) |
| ERROR_010-STAVKE-GK-KORISNIKA-BEZ-SK | SELECT | true | ledger | blocked-by-missing-source | [L526](../queries-sql.md#L526) |
| ERROR_011-IZVOD-STAVKE-POGRESNA-SZ-U-GK | SELECT | true | banking | blocked-by-missing-source | [L534](../queries-sql.md#L534) |
| ERROR_012-IZVOD-IZVODSTAVKE-POGRESNA-SZ | SELECT | true | banking | needs-port | [L542](../queries-sql.md#L542) |
| ERROR_013-IZVODSTAVKE-GK-RAZLICIT-ID-K | SELECT | true | banking | needs-port | [L550](../queries-sql.md#L550) |
| ERROR_014-IZVOD-NALOG-SUMA-NIJE-NULA | SELECT | true | banking | blocked-by-missing-source | [L558](../queries-sql.md#L558) |
| ERROR_014-IZVOD-UPOREDO_2410 | SELECT | true | banking | needs-port | [L568](../queries-sql.md#L568) |
| ERROR_015_GK_POVEZIVANJE_SUBKONTO_RAZLIKA | SELECT | true | ledger | needs-port | [L578](../queries-sql.md#L578) |
| ERROR_016_GK_204X_DPO_DATUM_NEMA | SELECT | true | ledger | needs-port | [L589](../queries-sql.md#L589) |
| ERROR_016_GK_204X_DPO_DATUM_NEMA_GROUP | SELECT | true | master-data | needs-port | [L597](../queries-sql.md#L597) |
| ERROR_017_GK_204X_POVEZIVANJE_SUBKONTA | SELECT | true | ledger | blocked-by-missing-source | [L606](../queries-sql.md#L606) |
| ERROR_018_GK_PretplateIzvodi | SELECT | true | banking | needs-port | [L620](../queries-sql.md#L620) |
| ERROR_019-IZVOD-2410-ODOBRENJE-ZADUZENJE | SELECT | true | banking | needs-port | [L629](../queries-sql.md#L629) |
| ERROR_020_OBJEKAT-SZ-KORISNIK-SZ | SELECT | true | master-data | blocked-by-missing-source | [L637](../queries-sql.md#L637) |
| ERROR_021_RACUNI-DATUMPROMETA-DATUMVALUTE-RACUNA | SELECT | true | ledger | needs-port | [L645](../queries-sql.md#L645) |
| ERROR_021_RACUNI-DATUMPROMETA-DATUMVALUTE-RACUNA-FIX | UPDATE | true | billing | needs-port | [L653](../queries-sql.md#L653) |
| ERROR_022_MAILSEND_DONTHAVE_SENDDATE | SELECT | true | documents-email | needs-port | [L659](../queries-sql.md#L659) |
| ERROR_023_GK4350_KONTOTROSKANULL | SELECT | true | ledger | needs-port | [L668](../queries-sql.md#L668) |
| ERROR_023_RACUN_STORO_PROKNJIZEN_PLACEN | SELECT | true | ledger | needs-port | [L676](../queries-sql.md#L676) |
| ERROR_024_RACUN_STORO_PROKNJIZEN_PLACEN | SELECT | true | other | needs-port | [L684](../queries-sql.md#L684) |
| ERROR_027_GK_GRP_BY_NALOG | SELECT | true | ledger | needs-port | [L694](../queries-sql.md#L694) |
| ERROR_027_GK_GRP_BY_NALOG_SUB | SELECT | true | ledger | needs-port | [L703](../queries-sql.md#L703) |
| ERROR_030_RACUNDOB-BEZGK-NEMA-SZ-ILI-NEMA-DOB | SELECT | true | ledger | needs-port | [L711](../queries-sql.md#L711) |
| ERROR_031_RACUNDOB-BEZGK-OBRISANA-SZ | SELECT | true | ledger | needs-port | [L719](../queries-sql.md#L719) |
| ERROR_032_RACUNDOB-NIJE_KNJIZEN-NEMA-IDDOB-U-DOBAVLJACIMA | SELECT | true | ledger | needs-port | [L727](../queries-sql.md#L727) |
| ERROR_033_RDOB_GRP | SELECT | true | ledger | needs-port | [L735](../queries-sql.md#L735) |
| ERROR_033_SUB1_RDOB_GRP | SELECT | true | ledger | needs-port | [L744](../queries-sql.md#L744) |
| ERROR_041_RACUNDOB-GK-OBRISANA-SZ | SELECT | true | ledger | needs-port | [L755](../queries-sql.md#L755) |
| ERROR_042_RACUNDOB-GK-POGRRESAN-DOB | SELECT | true | ledger | needs-port | [L763](../queries-sql.md#L763) |
| ERROR_043_GK_SIFRAKONTA_LNKKUPAC | SELECT | true | ledger | needs-port | [L771](../queries-sql.md#L771) |
| ERROR_060_RACUNI-BEZ-IDK-ILI-SZID-ILI-GRID | SELECT | true | billing | needs-port | [L779](../queries-sql.md#L779) |
| ERROR_062_RACUNI_RACUNISTAVKE_GRID_RAZLICITO | SELECT | true | billing | needs-port | [L787](../queries-sql.md#L787) |
| ERROR_063_RACUNI_RACUNISTAVKE_SZID_RAZLICITO | SELECT | true | billing | needs-port | [L795](../queries-sql.md#L795) |
| ERROR_064_RACUNI_RACUNISTAVKE_IDK_RAZLICITO | SELECT | true | suppliers | needs-port | [L803](../queries-sql.md#L803) |
| ERROR_065_RACUNI_GK_SZID_RAZLICITO | SELECT | true | ledger | needs-port | [L812](../queries-sql.md#L812) |
| ERROR_066_RACUNI_GK_IDK_RAZLICITO | SELECT | true | banking | needs-port | [L820](../queries-sql.md#L820) |
| ERROR_070_GR_NEMA_PODATAKA_ADELL | SELECT | true | billing | needs-port | [L828](../queries-sql.md#L828) |
| ERROR_070_GR_NEMA_PODATAKA_ADELL_EXECUTE | DELETE | true | billing | needs-port | [L836](../queries-sql.md#L836) |
| ERROR_071_RACUNI-RACUNISTAVKE-BEZ-RACUNA | SELECT | true | billing | needs-port | [L844](../queries-sql.md#L844) |
| ERROR_071_RACUNI-RACUNISTAVKE-BEZ-RACUNA_EXECUTE | DELETE | true | billing | needs-port | [L852](../queries-sql.md#L852) |
| ERROR_075_GK-RDOB-GRPSK | SELECT | true | ledger | needs-port | [L860](../queries-sql.md#L860) |
| ERROR_081_RACUNI_SUME | SELECT | true | ledger | needs-port | [L869](../queries-sql.md#L869) |
| ERROR_082_GK_2040 | SELECT | true | ledger | needs-port | [L878](../queries-sql.md#L878) |
| ERROR_082_GK_4350 | SELECT | true | ledger | needs-port | [L887](../queries-sql.md#L887) |
| ERROR_082_GK_4350_RDOB_KN | SELECT | true | banking | needs-port | [L896](../queries-sql.md#L896) |
| ERROR_082_GK_4900 | SELECT | true | ledger | needs-port | [L905](../queries-sql.md#L905) |
| ERROR_082_GK_5590 | SELECT | true | ledger | needs-port | [L914](../queries-sql.md#L914) |
| ERROR_083_GRRAC-STAVKE-DOBRAC | SELECT | true | ledger | needs-port | [L923](../queries-sql.md#L923) |
| ERROR_084_UPOREDO-083-082 | SELECT | true | suppliers | needs-port | [L932](../queries-sql.md#L932) |
| ERROR_085_SUME_GRUPARACUNA | SELECT | true | ledger | needs-port | [L939](../queries-sql.md#L939) |
| ERROR_101_KONTOTROSKA_NEPOSTOJI | SELECT | true | ledger | needs-port | [L946](../queries-sql.md#L946) |
| ERROR_102_KONTOTROSKA_NEMAPARENT | SELECT | true | ledger | blocked-by-missing-source | [L954](../queries-sql.md#L954) |
| ERROR_103_2040_IZVOD_NEMASTAVKUIZVODA | SELECT | true | banking | needs-port | [L962](../queries-sql.md#L962) |
| ERROR_104_NALOG_RAVNOTEZA | SELECT | true | ledger | needs-port | [L970](../queries-sql.md#L970) |
| ERROR_901_A_NALOG_VISE_IZVODA | SELECT | true | banking | needs-port | [L979](../queries-sql.md#L979) |
| ERROR_901_AB_NALOG_VISE_IZVODA_GK_STAVKI | SELECT | true | ledger | needs-port | [L988](../queries-sql.md#L988) |
| ERROR_901_AC_NALOG_VISE_IZVODA_IZVODSTAVKI | SELECT | true | banking | blocked-by-missing-source | [L996](../queries-sql.md#L996) |
| ERROR_902_A_GK_STAVKA_BEZ_DATUMA | SELECT | true | banking | blocked-by-missing-source | [L1004](../queries-sql.md#L1004) |
| ERROR-IZVOD | SELECT | true | banking | needs-port | [L1012](../queries-sql.md#L1012) |
| ErrorIzvod-SumaNovStanjePrethodno-GK2410 | SELECT | true | banking | needs-port | [L1021](../queries-sql.md#L1021) |
