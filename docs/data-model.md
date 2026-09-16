# Šema baze — direktno izvučena iz .mdb fajlova (mdbtools, JET4)

Nema definisanih Access Relationships niti indeksa ni u jednom od tri fajla — provereno direktno na binarnom nivou (mdb-schema --relations --indexes), ne samo u eksportu. Ovo je stvarno stanje baze, ne propust u eksportu.

## data.mdb — 73 tabele (stvarni podaci)
```sql
-- ----------------------------------------------------------
-- MDB Tools - A library for reading MS Access database files
-- Copyright (C) 2000-2011 Brian Bruns and others.
-- Files in libmdb are licensed under LGPL and the utilities under
-- the GPL, see COPYING.LIB and COPYING files respectively.
-- Check out http://mdbtools.sourceforge.net
-- ----------------------------------------------------------

-- That file uses encoding UTF-8

CREATE TABLE [_Log]
 (
	[logID]			Long Integer, 
	[PC]			Text (15), 
	[WinUser]			Text (50), 
	[UserID]			Long Integer, 
	[UserTXT]			Text (50), 
	[Datum]			DateTime, 
	[Forma]			Text (60), 
	[TabCode]			Text (50), 
	[ItemID]			Long Integer, 
	[ActionType]			Text (20), 
	[msgExtra]			Text (255), 
	[msgErrNum]			Text (255), 
	[msg]			Text (255), 
	[msgPromene]			Memo/Hyperlink (255), 
	[Module]			Text (50)
);

CREATE TABLE [_Recnik]
 (
	[ID]			Long Integer, 
	[Poruka]			Text (255), 
	[Jezik]			Text (50), 
	[Index]			Long Integer, 
	[dlg]			Long Integer
);

CREATE TABLE [_RecnikJezik]
 (
	[RecLang]			Text (10) NOT NULL, 
	[Recnik]			Text (50), 
	[On]			Boolean NOT NULL, 
	[Def]			Boolean NOT NULL
);

CREATE TABLE [_RecnikN]
 (
	[ID]			Long Integer NOT NULL, 
	[CIR]			Text (255)
);

CREATE TABLE [_TableList]
 (
	[Id]			Long Integer, 
	[TableName]			Text (255), 
	[Description]			Text (255), 
	[ClearMe]			Long Integer
);

CREATE TABLE [BenefitGrupa]
 (
	[IDBenefitGrupa]			Long Integer, 
	[BenefitNaziv]			Text (255), 
	[BenefitPrint]			Text (255)
);

CREATE TABLE [Benefiti]
 (
	[IDBenefit]			Long Integer, 
	[ObjekatID]			Long Integer, 
	[KupacID]			Long Integer, 
	[MesecYYMM]			Text (255), 
	[Used]			Long Integer, 
	[DateEntry]			DateTime, 
	[DateUsed]			DateTime, 
	[RacunId]			Long Integer, 
	[BenefitGrupaId]			Long Integer, 
	[RacunStornoID]			Long Integer
);

CREATE TABLE [Dobavljac_Racuni]
 (
	[IDTRRAC]			Long Integer, 
	[SK_ID]			Long Integer, 
	[RacunNO]			Long Integer, 
	[NazivRacuna]			Text (255), 
	[Napomena]			Text (255), 
	[Dobavljac]			Text (50), 
	[DobavljacKonto]			Long Integer, 
	[TipObracuna]			Long Integer, 
	[MesecRacuna]			Text (50), 
	[IznosRacunaEUR]			Double, 
	[IznosRacunaRSD]			Currency, 
	[IznosPoKoefEUR]			Double, 
	[IznosPoKoefRSD]			Currency, 
	[PrioritetNaplate]			Long Integer, 
	[SifraKN]			Text (6), 
	[TMPprevID]			Long Integer, 
	[KontoKnjizenja]			Text (255), 
	[TipDokumenta]			Long Integer, 
	[MarkerVandrednogRacuna]			Text (255), 
	[FunkcijaNazivaRacuna]			Text (255), 
	[RBR]			Text (50), 
	[IznosRacunaKN]			Currency, 
	[DatumRacuna]			DateTime, 
	[DatumKnjizenja]			DateTime, 
	[DatumPlacanja]			DateTime, 
	[OpisRacuna]			Text (255), 
	[PozivNaBroj]			Text (255), 
	[PrethodniIDRdob]			Long Integer, 
	[NoviIDRdob]			Long Integer, 
	[NalogKnjizenja]			Long Integer, 
	[PDV]			Long Integer, 
	[ZatvaraKonto]			Text (255)
);

CREATE TABLE [Dobavljaci_Racun_TipObjekta]
 (
	[ID]			Long Integer, 
	[RacunID]			Long Integer, 
	[TipObjekta]			Long Integer
);

CREATE TABLE [Files]
 (
	[IDDokument]			Long Integer, 
	[IDDocType]			Long Integer, 
	[IDRefItem]			Long Integer, 
	[FileNameSufix]			Text (255), 
	[FileName]			Text (255), 
	[RelPathName]			Text (255), 
	[DateAdd]			DateTime, 
	[Opis]			Text (255), 
	[Ext]			Text (255), 
	[TabSource]			Text (255), 
	[Registrator]			Text (4)
);

CREATE TABLE [GK]
 (
	[STAVKAID]			Long Integer, 
	[BR_NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[DIZNOS]			Double, 
	[PIZNOS]			Double, 
	[TIP_STAVKE]			Double, 
	[DOK]			Text (50), 
	[lnkSkupstinaID]			Long Integer, 
	[lnkKUPACID]			Long Integer, 
	[lnkIzvodStavkaID]			Long Integer, 
	[NAPOMENA]			Text (255), 
	[PARAMETRI]			Text (25), 
	[OPIS]			Text (255), 
	[SIFRAKONTA]			Long Integer, 
	[DPO]			DateTime, 
	[SIFRAKN]			Text (6), 
	[RDOB]			Long Integer, 
	[RACID]			Long Integer, 
	[PRIORITET]			Long Integer, 
	[KNzaTIP]			Long Integer, 
	[RacunIN_ID]			Long Integer, 
	[KontoTroska]			Text (255), 
	[KnDokID]			Long Integer
);

CREATE TABLE [GK_TMP]
 (
	[STAVKAID]			Long Integer, 
	[BR_NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[DIZNOS]			Double, 
	[PIZNOS]			Double, 
	[TIP_STAVKE]			Double, 
	[DOK]			Text (50), 
	[lnkSkupstinaID]			Long Integer, 
	[lnkKUPACID]			Long Integer, 
	[lnkIzvodStavkaID]			Long Integer, 
	[NAPOMENA]			Text (255), 
	[PARAMETRI]			Text (25), 
	[OPIS]			Long Integer, 
	[SIFRAKONTA]			Long Integer, 
	[DPO]			DateTime, 
	[fromStavkaID]			Long Integer, 
	[SIFRAKN]			Text (6), 
	[RDOB]			Long Integer, 
	[RACID]			Long Integer, 
	[PRIORITET]			Long Integer, 
	[KNzaTIP]			Long Integer, 
	[RacunIN_ID]			Long Integer, 
	[KontoTroska]			Text (255), 
	[KnDokID]			Text (255)
);

CREATE TABLE [Godina]
 (
	[IDSZ]			Long Integer NOT NULL, 
	[Godina]			Long Integer NOT NULL, 
	[StartDatum]			DateTime, 
	[EndDatum]			DateTime, 
	[Arhivirana]			Long Integer, 
	[Prikaz]			Text (255), 
	[Folder]			Text (255), 
	[Datoteka]			Text (255), 
	[Aktuelna]			Long Integer
);

CREATE TABLE [GrupaOpomena]
 (
	[IDGrupaOpomena]			Long Integer, 
	[Naslov]			Text (50), 
	[Datum]			DateTime, 
	[MinBNR]			Long Integer, 
	[TolerancijaDuga]			Long Integer, 
	[TolerancijaDugaPoMesecu]			Long Integer, 
	[lnkSablonOpomene]			Long Integer, 
	[lnkVrstaOpomene]			Long Integer, 
	[DatumDI]			DateTime, 
	[DatumPI]			DateTime, 
	[IDSZ]			Long Integer, 
	[lnkGrupaRacuna]			Long Integer, 
	[GrupaOpomenaTXT]			Text (255), 
	[SablonTextOpomene]			Text (255), 
	[Doznaka]			Text (255)
);

CREATE TABLE [GrupaRacuna]
 (
	[IDGrupaRacuna]			Long Integer, 
	[GrupaRacunaFXN]			Text (50), 
	[GrupaRacunaFXT]			Text (50), 
	[Mesec]			Text (50), 
	[Godina]			Text (50), 
	[Mesto]			Text (50), 
	[DatumIzdavanja]			DateTime, 
	[DatumUsluge]			Text (50), 
	[DatumPrometa]			DateTime, 
	[DatumValute]			DateTime, 
	[NBS]			Currency, 
	[ID_SK]			Long Integer, 
	[NalogKN]			Long Integer, 
	[DatumSys]			DateTime, 
	[UserSys]			Long Integer, 
	[MarkerVanderdnihRacuna]			Text (255), 
	[VrstaRacuna]			Text (255), 
	[DatumStanja]			DateTime, 
	[PrethodnaValuta]			DateTime, 
	[ObracunKamate]			Long Integer
);

CREATE TABLE [Izvod]
 (
	[IzvodID]			Long Integer, 
	[BrojIzvoda]			Long Integer, 
	[SufixIzvoda]			Text (50), 
	[ID_SK]			Long Integer, 
	[Datum]			DateTime, 
	[PrethodnoStanjeIzvoda]			Currency, 
	[NovoStanje]			Currency, 
	[Duguje]			Currency, 
	[Potrazuje]			Currency, 
	[NalogaZaduzenja]			Long Integer, 
	[NalogaOdobranja]			Long Integer, 
	[Napomena]			Text (50), 
	[NalogZaKnjizenje]			Long Integer, 
	[Rasknjizen]			Boolean NOT NULL
);

CREATE TABLE [IzvodStavke]
 (
	[ID]			Long Integer, 
	[IzvodLNKID]			Long Integer, 
	[ID_SK]			Long Integer, 
	[RbStavke]			Long Integer, 
	[RbNaloga]			Long Integer, 
	[NazivPN]			Text (255), 
	[BrojRacuna]			Text (50), 
	[Poreklo]			Text (50), 
	[DatumRealizacije]			DateTime, 
	[Zaduzenje]			Currency, 
	[Odobrenje]			Currency, 
	[Doznaka]			Text (255), 
	[Sifra]			Long Integer, 
	[PozivNaBroj]			Text (50), 
	[PozivNaBrojO]			Text (50), 
	[opt_lnk_Kupac]			Long Integer, 
	[Knjizeno]			Boolean NOT NULL, 
	[Ignore]			Boolean NOT NULL, 
	[SetP]			Boolean NOT NULL
);

CREATE TABLE [KnjiznaDokumenta]
 (
	[IDDokument]			Long Integer, 
	[IDSZ]			Long Integer, 
	[IDKR]			Long Integer, 
	[IDTipDok]			Long Integer, 
	[Iznos]			Currency, 
	[NalogKN]			Long Integer, 
	[Datum]			DateTime, 
	[DokumentNaziv]			Text (255), 
	[DokumentOpis]			Text (255), 
	[KontoTroska]			Long Integer, 
	[IDTipGrupa]			Long Integer, 
	[refFromIDDok]			Long Integer, 
	[PB]			Text (30)
);

CREATE TABLE [Konta]
 (
	[Konto]			Long Integer NOT NULL, 
	[Opis]			Text (50), 
	[PrintText]			Text (255)
);

CREATE TABLE [KontniOkvir]
 (
	[Konto]			Text (255) NOT NULL, 
	[SkraceniNaziv]			Text (255), 
	[Naziv]			Text (255), 
	[Prethodni]			Text (255), 
	[Nivo]			Long Integer, 
	[Znak]			Text (255), 
	[Aktivan]			Long Integer
);

CREATE TABLE [Kupac]
 (
	[ID_K]			Long Integer NOT NULL, 
	[Naziv]			Text (100), 
	[PBroj]			Text (50), 
	[Adresa]			Text (50), 
	[MB]			Text (50), 
	[PIB]			Text (50), 
	[Adresa_Ugovor]			Text (255), 
	[Telefon]			Text (50), 
	[eMail]			Text (100), 
	[napomena]			Text (255), 
	[TR_K]			Text (50), 
	[web]			Text (50), 
	[lnk_ID_SK]			Long Integer, 
	[lnkNaselje]			Long Integer, 
	[PrintNaziv]			Text (255), 
	[PAK]			Text (50), 
	[KONTO]			Text (50), 
	[ExterniKonto]			Text (255), 
	[PDprefix]			Text (255), 
	[LK]			Text (255), 
	[Tip]			Long Integer, 
	[AutoKontoTroska]			Text (50), 
	[IDGrupniRacunMaster]			Long Integer, 
	[chkSkipPrintRacunGrupa]			Long Integer, 
	[PDVobaveznik]			Long Integer, 
	[JBJKS]			Text (255), 
	[KupacGrad]			Text (255), 
	[ZemljaKod]			Text (255), 
	[IDMaster]			Long Integer, 
	[LokacijaDostava]			Text (255), 
	[DostavaSifraPD]			Long Integer, 
	[PrintInvoiceMandatory]			Long Integer, 
	[SendToPostOffice]			Long Integer, 
	[Language]			Text (255), 
	[NapomenaExtended]			Memo/Hyperlink (255)
);

CREATE TABLE [Kurs]
 (
	[IDk]			Long Integer, 
	[Kurs]			Currency, 
	[DatumOd]			DateTime, 
	[Skolska]			Long Integer, 
	[RefCena]			Long Integer, 
	[DatumUpisa]			DateTime
);

CREATE TABLE [Mail]
 (
	[IDeMail]			Long Integer, 
	[IDPartner]			Long Integer, 
	[eMail]			Text (255), 
	[SortOrder]			Long Integer, 
	[LoginAppMaster]			Long Integer, 
	[LoginAppView]			Long Integer, 
	[SendMailRacun]			Long Integer
);

CREATE TABLE [Mail_Send]
 (
	[IDMail]			Long Integer, 
	[Subject]			Text (255), 
	[To]			Text (255), 
	[CC]			Text (255), 
	[BCC]			Text (255), 
	[Body]			Memo/Hyperlink (255), 
	[BodyHTML]			Memo/Hyperlink (255), 
	[DateCreated]			DateTime, 
	[DateSend]			DateTime, 
	[Archive]			Boolean NOT NULL, 
	[ErrorDescription]			Memo/Hyperlink (255), 
	[ErrorStatus]			Text (255)
);

CREATE TABLE [Mail_Send_Attachment]
 (
	[IDMailAttachment]			Long Integer, 
	[IDMail]			Long Integer, 
	[AttachmentFilePath]			Text (255), 
	[AttachmentFilePath2]			Text (255)
);

CREATE TABLE [Nalog]
 (
	[Br_Nalog]			Double NOT NULL, 
	[Datum]			DateTime, 
	[Saldo]			Currency, 
	[Napomena]			Text (255), 
	[Reserve]			Text (50), 
	[OpisNaloga]			Text (50), 
	[SZID]			Long Integer, 
	[DodatneNapomene]			Text (255), 
	[TipNaloga]			Long Integer
);

CREATE TABLE [Naselje]
 (
	[NaseljeID]			Long Integer NOT NULL, 
	[Naselje]			Text (50)
);

CREATE TABLE [Notes]
 (
	[ID]			Long Integer, 
	[Datum]			Text (50), 
	[Korisnik]			Text (50), 
	[Notes]			Memo/Hyperlink (255)
);

CREATE TABLE [Objekti]
 (
	[ID_O]			Long Integer, 
	[lnkSkupstinaID]			Long Integer, 
	[naziv]			Text (255), 
	[lnk_ID_K]			Long Integer NOT NULL, 
	[lnk_tip]			Long Integer, 
	[Status]			Long Integer, 
	[IO]			Double, 
	[RF_DIN]			Double, 
	[IONaslov]			Text (50), 
	[StaraKv]			Double, 
	[STatusPromene]			Text (50), 
	[Koeficijent]			Double, 
	[Ukupno]			Currency, 
	[Ulaz]			Text (50), 
	[Kategorija]			Text (5), 
	[Adresa]			Text (50), 
	[Napomena]			Text (255), 
	[Naziv_Slanja]			Text (50), 
	[Adresa_Slanja]			Text (50), 
	[PBroj_Slanja]			Text (50), 
	[PIB_Slanja]			Text (50), 
	[PLATILAC_lnk_ID_K]			Long Integer, 
	[PLATILAC_lnk_ID_K2]			Long Integer, 
	[BRGM]			Long Integer, 
	[K1]			Double, 
	[K2]			Double, 
	[K3]			Double, 
	[K4]			Double, 
	[K5]			Double, 
	[PAK_SLANJA]			Text (50), 
	[KV]			Double, 
	[BrStanara]			Long Integer, 
	[BrojPD]			Long Integer, 
	[IDVlasnik]			Long Integer, 
	[IDZakupac]			Long Integer, 
	[IDGrupnogRacuna]			Long Integer, 
	[SifraPD]			Text (255), 
	[netoKV]			Double, 
	[terasa]			Double, 
	[netoKVsaTerasom]			Double, 
	[HandOverDate]			DateTime, 
	[SpratN]			Long Integer, 
	[SpratT]			Text (255)
);

CREATE TABLE [Opomena]
 (
	[IDOpomena]			Long Integer, 
	[lnkGrupaOpomena]			Long Integer, 
	[lnkKupac]			Long Integer, 
	[BNR]			Long Integer, 
	[Dug]			Double, 
	[txtRacunOp]			Text (255), 
	[ActivnaOpomena]			Long Integer, 
	[SumaPoStavkama]			Double, 
	[PozivNaBroj]			Text (255), 
	[Troskovi]			Long Integer, 
	[Ukupno]			Long Integer
);

CREATE TABLE [OpomenaStavke]
 (
	[IDOpomenaStavka]			Long Integer, 
	[lnkOpomena]			Long Integer, 
	[lnkIDGO]			Long Integer, 
	[lnkKupacID]			Long Integer, 
	[PARAMETRI]			Text (25), 
	[DOK]			Text (255), 
	[Di]			Double, 
	[Pi]			Double, 
	[Suma]			Double, 
	[mTXT]			Text (255), 
	[DatumDospeca]			DateTime, 
	[IDRacun]			Long Integer, 
	[DatumRacuna]			DateTime, 
	[PProstor]			Text (255)
);

CREATE TABLE [OpomeneSabloni]
 (
	[IDOpomenaSablon]			Long Integer, 
	[lnkVrstaOpomene]			Long Integer, 
	[Reportname]			Text (255), 
	[Naslov]			Text (255), 
	[rptField01]			Text (255), 
	[rptField02]			Text (255), 
	[rptField03]			Memo/Hyperlink (255), 
	[rptField04]			Memo/Hyperlink (255), 
	[rptField05]			Memo/Hyperlink (255), 
	[rptField06]			Memo/Hyperlink (255), 
	[rptField07]			Text (255), 
	[rptField08]			Text (255), 
	[rptField09]			Text (255)
);

CREATE TABLE [PrinterBinLOCAL]
 (
	[IDlocalPrinterbin]			Long Integer, 
	[ID_Item]			Long Integer, 
	[TypeIndex]			Long Integer
);

CREATE TABLE [Promene]
 (
	[IDChange]			Long Integer, 
	[DateOfRequest]			DateTime, 
	[DateOfExecution]			DateTime, 
	[IDK]			Long Integer, 
	[IDO]			Long Integer, 
	[Description]			Text (255), 
	[PreviusValue]			Text (255), 
	[NewValue]			Text (255), 
	[FieldsRelated]			Text (255), 
	[RequestType]			Text (255), 
	[RequestBy]			Text (255), 
	[RequestThrou]			Text (255)
);

CREATE TABLE [Racun]
 (
	[IDRacun]			Long Integer, 
	[RBR]			Text (20), 
	[lnkGR]			Long Integer, 
	[DatumIzdavanja]			DateTime, 
	[MestoIzdavanja]			Text (50), 
	[DatumUsluge]			Text (50), 
	[DatumPrometa]			DateTime, 
	[DatumValute]			DateTime, 
	[ID_K]			Long Integer, 
	[ID_SK]			Long Integer, 
	[Kupac]			Text (255), 
	[PBroj_K]			Text (50), 
	[Adresa_K]			Text (50), 
	[PIB]			Text (50), 
	[MB]			Text (255), 
	[Suma]			Double, 
	[PDVStopa]			Double, 
	[PDVIznos]			Double, 
	[Ukupno]			Double, 
	[PrethodniDug]			Single, 
	[SvrhaUplate]			Text (255), 
	[Valuta]			Text (50), 
	[PozivNaBroj]			Text (50), 
	[PD_text]			Text (255), 
	[Napomena]			Text (255), 
	[Co]			Long Integer, 
	[Storno]			Boolean NOT NULL, 
	[extraNapomena]			Text (255), 
	[objekatNaziv]			Text (50), 
	[Upravnik]			Long Integer, 
	[SvrhaUplate2]			Text (255), 
	[ID_OX]			Long Integer, 
	[AdresaProstora]			Text (255), 
	[ZaUplatu]			Double, 
	[Naziv_Slanja]			Text (50), 
	[Adresa_Slanja]			Text (50), 
	[PBroj_Slanja]			Text (50), 
	[PIB_Slanja]			Text (50), 
	[tmpID]			Long Integer, 
	[tmpID2]			Long Integer, 
	[tmpPB]			Text (50), 
	[tmpPB2]			Text (50), 
	[SPC]			Long Integer, 
	[PD_iznos]			Double, 
	[TipPoljaZaUplatu]			Long Integer, 
	[lnkOpomenaID]			Long Integer, 
	[IDKGrupniRacun]			Long Integer, 
	[Grad_K]			Text (255), 
	[Lokacija]			Text (255), 
	[SortRacun]			Long Integer, 
	[DatumStorno]			DateTime, 
	[RacunShema]			Text (255), 
	[DatumStanja]			DateTime, 
	[KamataIznos]			Double, 
	[UkupnoRacun]			Double
);

CREATE TABLE [RacunIN]
 (
	[IDRacunIN]			Long Integer, 
	[PartnerID]			Long Integer, 
	[SkupstinaID]			Long Integer, 
	[BrojRacunaIn]			Text (255), 
	[DatumRacuna]			DateTime, 
	[DatumKnjizenja]			DateTime, 
	[DatumPlacanja]			DateTime, 
	[KontoTroska]			Text (255), 
	[IznosRacuna]			Currency
);

CREATE TABLE [RacunObjekti]
 (
	[IDRacun]			Long Integer, 
	[IDObjekat]			Long Integer
);

CREATE TABLE [RacunStavke]
 (
	[IDRacunStavke]			Long Integer, 
	[ID_R]			Long Integer, 
	[lnkGR]			Long Integer, 
	[ID_K]			Long Integer, 
	[ID_SK]			Long Integer, 
	[ID_RDOB]			Long Integer, 
	[Naziv]			Text (255), 
	[TipObracuna]			Long Integer, 
	[K1]			Double, 
	[K2]			Double, 
	[K3]			Double, 
	[K4]			Double, 
	[K5]			Double, 
	[Kolicina]			Double, 
	[CenaE]			Double, 
	[NBS]			Double, 
	[Iznos]			Double, 
	[Suma]			Double, 
	[PDVStopa]			Double, 
	[PDVIznos]			Double, 
	[UkupnoRSD]			Double, 
	[Sort]			Long Integer, 
	[DobavljacKonto]			Long Integer, 
	[IZNOSRACUNA]			Double, 
	[K1xK2]			Double, 
	[K2xK3]			Double, 
	[K2xK4]			Double, 
	[K2xK5]			Double, 
	[JM]			Text (255), 
	[ID_O]			Long Integer, 
	[M]			Double
);

CREATE TABLE [RacunStavkeBenefitArhiva]
 (
	[IDRacunStavke]			Long Integer NOT NULL, 
	[ID_R]			Long Integer, 
	[lnkGR]			Long Integer, 
	[ID_K]			Long Integer, 
	[ID_SK]			Long Integer, 
	[ID_RDOB]			Long Integer, 
	[Naziv]			Text (255), 
	[TipObracuna]			Long Integer, 
	[K1]			Double, 
	[K2]			Double, 
	[K3]			Double, 
	[K4]			Double, 
	[K5]			Double, 
	[Kolicina]			Double, 
	[CenaE]			Double, 
	[NBS]			Double, 
	[Iznos]			Double, 
	[Suma]			Double, 
	[PDVStopa]			Double, 
	[PDVIznos]			Double, 
	[UkupnoRSD]			Double, 
	[Sort]			Long Integer, 
	[DobavljacKonto]			Long Integer, 
	[IZNOSRACUNA]			Double, 
	[K1xK2]			Double, 
	[K2xK3]			Double, 
	[K2xK4]			Double, 
	[K2xK5]			Double, 
	[JM]			Text (255), 
	[ID_O]			Long Integer
);

CREATE TABLE [Settings]
 (
	[IDSettings]			Long Integer, 
	[SettingName]			Text (255), 
	[SettingVal]			Text (255), 
	[Descrition]			Text (255), 
	[Category]			Text (50), 
	[MFF]			Text (50), 
	[DefVal]			Text (255), 
	[FilterUser]			Long Integer, 
	[FilterPC]			Text (255), 
	[FilterCustom1Num]			Long Integer, 
	[FilterCustom2Num]			Long Integer, 
	[FilterCustom3Num]			Long Integer, 
	[FilterCustom1Txt]			Text (255), 
	[FilterCustom2Txt]			Text (255), 
	[FilterCustom3Txt]			Text (255), 
	[SettingVal_LT]			Memo/Hyperlink (255)
);

CREATE TABLE [Settings_eMail]
 (
	[IDSettingsEML]			Long Integer NOT NULL, 
	[SettingName]			Text (255), 
	[SettingVal]			Memo/Hyperlink (255), 
	[Category]			Text (255), 
	[Descrition]			Text (255), 
	[SZID]			Long Integer
);

CREATE TABLE [Settings_eNalog_Grupa]
 (
	[Index]			Long Integer NOT NULL, 
	[Kategorija]			Text (50), 
	[SQL_memo]			Memo/Hyperlink (255), 
	[SQL]			Text (255)
);

CREATE TABLE [Settings_FormGrid]
 (
	[IDSettGrid]			Long Integer, 
	[FormName]			Text (255), 
	[FormParent]			Text (255), 
	[CompName]			Text (255), 
	[mUserID]			Long Integer, 
	[LayoutIndex]			Long Integer, 
	[clnName]			Text (255), 
	[clnOrder]			Long Integer, 
	[clnWidth]			Long Integer, 
	[clnHidden]			Long Integer
);

CREATE TABLE [Skustina]
 (
	[IDSkupstina]			Long Integer NOT NULL, 
	[NazivSS]			Text (255), 
	[Zgrada]			Text (50), 
	[Adresa]			Text (50), 
	[Opstina]			Text (255), 
	[PBroj]			Text (50), 
	[PredstavnikSS_ID]			Long Integer, 
	[PIB]			Long Integer, 
	[TR]			Text (50), 
	[MB]			Text (50), 
	[Napomena]			Text (255), 
	[RB]			Long Integer, 
	[PrintNaziv]			Text (255), 
	[Folder]			Text (50), 
	[UplatnicaTip]			Long Integer, 
	[NaseljeLNK]			Long Integer, 
	[Konto]			Long Integer, 
	[SkStatus]			Long Integer, 
	[ExterniKonto]			Text (255), 
	[PDtext]			Text (255), 
	[Upravnik]			Long Integer, 
	[DatumUgovora]			DateTime, 
	[SZPDV]			Long Integer, 
	[TipSubjekta]			Long Integer, 
	[InvoiceIssuer]			Long Integer, 
	[Doznaka]			Text (255), 
	[RacunInfoReklamacija]			Text (255), 
	[eMail]			Text (255), 
	[eMailDisplay]			Text (255), 
	[PBrojSZ]			Text (255), 
	[GradSZ]			Text (255), 
	[Logo]			Text (255), 
	[Field1]			Text (255), 
	[QRName]			Text (255)
);

CREATE TABLE [Staff]
 (
	[IDUser]			Long Integer, 
	[User]			Text (50), 
	[StaffLogin]			Text (50), 
	[Level]			Long Integer, 
	[LastLog]			Text (50), 
	[UseLang]			Text (50), 
	[RESTRICT]			Text (255), 
	[LastPC]			Text (255)
);

CREATE TABLE [StaffPermition]
 (
	[IDPermition]			Long Integer, 
	[IDStaff]			Long Integer, 
	[KeyName]			Text (255), 
	[FormName]			Text (255), 
	[PermitionVal]			Long Integer, 
	[DisablePermition]			Long Integer
);

CREATE TABLE [StaffTMP]
 (
	[IDUser]			Long Integer, 
	[User]			Text (50), 
	[Level]			Text (50), 
	[UseLang]			Text (50), 
	[RESTRICT]			Text (255), 
	[LastPC]			Text (255)
);

CREATE TABLE [SzObjekat]
 (
	[Id]			Long Integer NOT NULL, 
	[SzId]			Long Integer, 
	[Objekat]			Text (255)
);

CREATE TABLE [SzUlaz]
 (
	[IDUlaz]			Long Integer NOT NULL, 
	[SZ]			Long Integer, 
	[Ulaz]			Text (255), 
	[Zgrada]			Text (255), 
	[Adresa]			Text (255), 
	[Oznaka]			Text (255), 
	[Opis]			Text (255), 
	[Sort]			Long Integer
);

CREATE TABLE [Table1]
 (
	[ID1]			Long Integer, 
	[ID]			Long Integer, 
	[Ph]			Text (255), 
	[Lang]			Text (255), 
	[Email]			Text (255)
);

CREATE TABLE [tblShortList]
 (
	[Index]			Long Integer NOT NULL, 
	[TableFrom]			Text (255) NOT NULL, 
	[Caption]			Text (255), 
	[ShortName]			Text (255), 
	[Description]			Text (255), 
	[IndexValue]			Long Integer, 
	[Cat1]			Long Integer
);

CREATE TABLE [tblSTATS]
 (
	[IDstt]			Long Integer, 
	[StatGrup]			Text (255), 
	[StatNaziv]			Text (255), 
	[StatQuery]			Text (255), 
	[StatSQL]			Memo/Hyperlink (255), 
	[StatReport]			Text (50), 
	[StatWHRCap]			Text (50), 
	[StatWHRComboSQL]			Memo/Hyperlink (255), 
	[StatSQLName]			Text (50)
);

CREATE TABLE [tblWhrEx]
 (
	[IDwhrex]			Long Integer, 
	[WhrNaslov]			Text (50), 
	[WhrEx]			Memo/Hyperlink (255), 
	[frmToDo]			Text (50)
);

CREATE TABLE [TekuciRacun]
 (
	[IDTR]			Long Integer, 
	[TR]			Text (255), 
	[Aktivan]			Long Integer, 
	[SortOrder]			Long Integer, 
	[IDPARTNER]			Long Integer, 
	[IDSZ]			Long Integer, 
	[IDUPRAVNIK]			Long Integer
);

CREATE TABLE [TipADDTXT]
 (
	[IDAddTXT]			Long Integer NOT NULL, 
	[AddTxT]			Text (255)
);

CREATE TABLE [TipObjekta]
 (
	[IDTipObj]			Long Integer NOT NULL, 
	[TipObj]			Text (50), 
	[Print]			Text (50), 
	[SortObj]			Long Integer, 
	[binVrednost]			Long Integer, 
	[NaslovRC]			Text (50), 
	[Disclaimer]			Memo/Hyperlink (255), 
	[Disclaimerspc]			Memo/Hyperlink (255), 
	[NaslovRC-spc]			Text (50), 
	[CB_V]			Double, 
	[IG_V]			Double, 
	[CB_KONTO]			Long Integer, 
	[IG_KONTO]			Long Integer
);

CREATE TABLE [TipObracuna]
 (
	[IDTipObr]			Long Integer NOT NULL, 
	[TipObracuna]			Text (100), 
	[IznosRacunaDobavljac]			Text (100), 
	[Napomena]			Text (255), 
	[IZNOS]			Text (255), 
	[KOLICINA]			Text (255), 
	[JM]			Text (255), 
	[JMindex]			Long Integer
);

CREATE TABLE [TipPartnera]
 (
	[IDTipPartnera]			Long Integer NOT NULL, 
	[TipNaziv]			Text (255)
);

CREATE TABLE [TipStavke]
 (
	[ID_TIP]			Long Integer NOT NULL, 
	[TipStavke]			Text (50)
);

CREATE TABLE [TipTODO]
 (
	[IDToDo]			Long Integer NOT NULL, 
	[ToDo]			Text (50)
);

CREATE TABLE [TipUplatnice]
 (
	[IDTipUplatnice]			Long Integer, 
	[TipUpl]			Text (255), 
	[Opis]			Text (50)
);

CREATE TABLE [Troskovi_PodKonta]
 (
	[PodKonto]			Text (255) NOT NULL, 
	[Naziv]			Text (255), 
	[SifraKnj_Prethodni]			Text (255), 
	[TrosakNa]			Text (255), 
	[Kamata]			Text (255)
);

CREATE TABLE [Troskovi_PodKonta_DefDob]
 (
	[IDSZ]			Long Integer NOT NULL, 
	[Konto]			Text (255) NOT NULL, 
	[DefDob]			Text (255)
);

CREATE TABLE [TRs]
 (
	[ID_TR]			Long Integer, 
	[TR_L]			Text (50), 
	[lnkKUP]			Long Integer
);

CREATE TABLE [VERSION-HISTORY]
 (
	[DAT]			DateTime, 
	[VER]			Text (50), 
	[DESC]			Text (255), 
	[FullDesc]			Memo/Hyperlink (255)
);

CREATE TABLE [Virman]
 (
	[IDVirman]			Long Integer, 
	[NaslovSablona]			Text (50), 
	[Nalogodavac]			Text (255), 
	[SvrhaPlacanja]			Text (255), 
	[Primalac]			Text (255), 
	[SifraPlacanja]			Integer, 
	[Valuta]			Text (3), 
	[Iznos]			Currency, 
	[BrojRacunaNalogodavca]			Text (50), 
	[BrojModelaNal]			Integer, 
	[PozivNaBrojNal]			Text (50), 
	[BrojRacunaPrimaoca]			Text (50), 
	[BrojModelaPrim]			Integer, 
	[PozivNaBrojPrim]			Text (50), 
	[Mesto]			Text (50), 
	[Datum]			DateTime, 
	[DatumV]			DateTime, 
	[Hitno]			Boolean NOT NULL, 
	[Tip]			Long Integer, 
	[Arhivirano]			DateTime, 
	[refSourceID]			Long Integer, 
	[refSourceTag]			Text (255), 
	[PrintMe]			Boolean NOT NULL, 
	[Favorit]			Boolean NOT NULL
);

CREATE TABLE [ZK]
 (
	[IDzk]			Long Integer, 
	[IDSK]			Long Integer, 
	[NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[PIZNOS]			Double, 
	[DIZNOS]			Double, 
	[TIPSTAVKE]			Double, 
	[PARAMETRI]			Text (25), 
	[NAPOMENA]			Text (20), 
	[PARTNERID]			Long Integer, 
	[DATUMVALUTE]			DateTime, 
	[RACUNID]			Long Integer, 
	[Dokument]			Text (255), 
	[PODKONTO]			Text (255), 
	[IDGR]			Long Integer, 
	[KNPODKONTO]			Text (255), 
	[KNDOB]			Long Integer, 
	[RDOB]			Long Integer, 
	[Prioritet]			Long Integer
);

CREATE TABLE [KamatniList]
 (
	[IDKamList]			Long Integer, 
	[IDSK]			Long Integer, 
	[Konto]			Text (50), 
	[Datum]			DateTime, 
	[DIPI]			Double, 
	[Saldo]			Double, 
	[Dana]			Long Integer, 
	[Stopa]			Double, 
	[Koeficijent]			Numeric (18, 8), 
	[Kamata]			Double, 
	[PAR]			Text (50), 
	[partnerID]			Long Integer, 
	[prostorID]			Long Integer, 
	[PodKonto]			Text (255), 
	[IDGR]			Long Integer
);

CREATE TABLE [RacunStavke_Troskovi]
 (
	[IDRacunStavke]			Long Integer, 
	[ID_R]			Long Integer, 
	[lnkGR]			Long Integer, 
	[ID_K]			Long Integer, 
	[ID_SK]			Long Integer, 
	[Ukupno]			Currency
);

CREATE TABLE [Settings_eNalog]
 (
	[IDeNalog]			Long Integer, 
	[Index]			Long Integer, 
	[BrCHR]			Long Integer, 
	[TipCHR]			Text (50), 
	[Funkcija]			Text (50), 
	[Kategorija]			Text (50), 
	[Opis]			Text (255), 
	[Defs]			Text (50), 
	[FieldName]			Text (50), 
	[Format]			Text (50), 
	[rN]			Long Integer
);

CREATE TABLE [Switchboard Items]
 (
	[SwitchboardID]			Long Integer NOT NULL, 
	[ItemNumber]			Integer NOT NULL, 
	[ItemText]			Text (255), 
	[Command]			Integer, 
	[Argument]			Text (255), 
	[fLevel]			Text (50)
);

CREATE TABLE [TemplateIzvodaKnjizenje]
 (
	[IDTemplateSub]			Long Integer, 
	[IDTemplate]			Long Integer, 
	[Template]			Text (255), 
	[FieldName]			Text (255), 
	[FieldValue]			Text (255), 
	[Function]			Text (255), 
	[SETID]			Long Integer, 
	[FFunction]			Long Integer, 
	[SETKONTO]			Text (50)
);

CREATE TABLE [ugovori]
 (
	[ID1]			Long Integer, 
	[Gradska opština Stambene zajednice]			Text (255), 
	[ID Stambene zajednice]			Double, 
	[prodavac           (Stambena zajednica)]			Text (255), 
	[Adresa stambene zajednice]			Text (255), 
	[PIB                            Stambene zajednice]			Double, 
	[Poštanski broj]			Text (255), 
	[Tekući račun  Stambene zajednice]			Text (255), 
	[Matični broj Stambene zajednice]			Double, 
	[Račun br]			Text (255), 
	[Mesto izdavanja računa:]			Text (255), 
	[Datum izdavanja računa:]			Text (255), 
	[broj objekta, posebnog dela]			Text (255), 
	[Datum pružanja usluge:]			Text (255), 
	[Objekat]			Text (255), 
	[kupac]			Text (255), 
	[Field16]			Text (255), 
	[adresa        kupca]			Text (255), 
	[maticni_broj]			Double, 
	[pib naziv]			Text (255), 
	[pib]			Text (255), 
	[kolicina u m2]			Double, 
	[jedinica]			Text (255), 
	[Jedinica za obračun - koeficijent]			Double, 
	[Mesečni fond zgrade po posebnoj jedinici od 122019-2822019]			Double, 
	[kontrolna]			Double, 
	[koeficijent]			Double, 
	[Osiguranje zajedničkih prostorija]			Double, 
	[Mesečno upravljanje]			Double, 
	[Redovan mesečni servis putničkog  lifta]			Double, 
	[Redovan mesečni servis auto lifta]			Double, 
	[Vanredan servis auto-lifta]			Double, 
	[Higijensko održavanje zgrade sa domarom]			Double, 
	[Higijensko održavanje garaže]			Double, 
	[Higijensko održavanje zgrade]			Double, 
	[tehničko održavanje]			Double, 
	[Vanrednan servis]			Double, 
	[Field37]			Double, 
	[UKUPNO]			Double, 
	[Stanje iz predhodnog perioda do]			Text (255), 
	[iznos Stanje iz predhodnog perioda do _ Maxi upravnik]			Text (255), 
	[ID]			Text (255), 
	[Stanje iz predhodnog perioda do 31122018 SZ]			Text (255), 
	[Ukupan dug za 2018]			Text (255), 
	[Rok za uplatu:]			Text (255), 
	[Model i poziv na broj:]			Text (255), 
	[Srvha uplate:]			Text (255), 
	[Valuta:]			Text (255), 
	[*Molimo Vas da izmirite dugovanje koje se odnosi na period do (M]			Text (255), 
	[Poziv na broj za dug do 31122018]			Text (255), 
	[NAPOMENA]			Text (255), 
	[Field51]			Text (255), 
	[KUPAC_IME]			Text (255)
);


```

## SZAPP.mdb — 27 lokalnih/pomoćnih tabela (front-end, privremene/GK backup/switchboard tabele)
```sql
-- ----------------------------------------------------------
-- MDB Tools - A library for reading MS Access database files
-- Copyright (C) 2000-2011 Brian Bruns and others.
-- Files in libmdb are licensed under LGPL and the utilities under
-- the GPL, see COPYING.LIB and COPYING files respectively.
-- Check out http://mdbtools.sourceforge.net
-- ----------------------------------------------------------

-- That file uses encoding UTF-8

CREATE TABLE [BenefitUpdate]
 (
	[Id]			Long Integer, 
	[Datum]			DateTime, 
	[UnitApp]			Text (255), 
	[YYMM]			Long Integer, 
	[CountMM]			Long Integer
);

CREATE TABLE [GK_PRK]
 (
	[STAVKAID]			Long Integer, 
	[BR_NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[DIZNOS]			Double, 
	[PIZNOS]			Double, 
	[TIP_STAVKE]			Double, 
	[DOK]			Text (50), 
	[lnkSkupstinaID]			Long Integer, 
	[lnkKUPACID]			Long Integer, 
	[lnkIzvodStavkaID]			Long Integer, 
	[NAPOMENA]			Text (255), 
	[PARAMETRI]			Text (25), 
	[OPIS]			Text (255), 
	[SIFRAKONTA]			Long Integer, 
	[DPO]			DateTime, 
	[SIFRAKN]			Text (6), 
	[RDOB]			Long Integer, 
	[RACID]			Long Integer, 
	[PRIORITET]			Long Integer, 
	[KNzaTIP]			Long Integer, 
	[RacunIN_ID]			Long Integer, 
	[KontoTroska]			Text (255)
);

CREATE TABLE [GK_TMP]
 (
	[STAVKAID]			Long Integer, 
	[BR_NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[PIZNOS]			Double, 
	[DIZNOS]			Double, 
	[TIP_STAVKE]			Double, 
	[DOK]			Text (50), 
	[lnkSkupstinaID]			Long Integer, 
	[lnkKUPACID]			Long Integer, 
	[lnkIzvodStavkaID]			Long Integer, 
	[NAPOMENA]			Text (255), 
	[PARAMETRI]			Text (25), 
	[OPIS]			Text (255), 
	[SIFRAKONTA]			Long Integer, 
	[DPO]			DateTime, 
	[fromStavkaID]			Long Integer, 
	[SIFRAKN]			Text (6), 
	[RDOB]			Long Integer, 
	[RACID]			Long Integer, 
	[PRIORITET]			Long Integer, 
	[KNzaTIP]			Long Integer, 
	[RacunIN_ID]			Long Integer, 
	[KontoTroska]			Text (255)
);

CREATE TABLE [IMPORT_BENEFIT]
 (
	[PDI]			Text (255) NOT NULL
);

CREATE TABLE [IMPORTKV]
 (
	[Unit]			Text (255) NOT NULL, 
	[Client]			Text (255), 
	[HD]			DateTime, 
	[P1]			Double, 
	[p2]			Double, 
	[p3]			Double, 
	[pt]			Double
);

CREATE TABLE [GK_20250531_backup_p24_bEFOREUpravljanjeSplit]
 (
	[STAVKAID]			Long Integer NOT NULL, 
	[BR_NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[DIZNOS]			Double, 
	[PIZNOS]			Double, 
	[TIP_STAVKE]			Double, 
	[DOK]			Text (50), 
	[lnkSkupstinaID]			Long Integer, 
	[lnkKUPACID]			Long Integer, 
	[lnkIzvodStavkaID]			Long Integer, 
	[NAPOMENA]			Text (255), 
	[PARAMETRI]			Text (25), 
	[OPIS]			Text (255), 
	[SIFRAKONTA]			Long Integer, 
	[DPO]			DateTime, 
	[SIFRAKN]			Text (6), 
	[RDOB]			Long Integer, 
	[RACID]			Long Integer, 
	[PRIORITET]			Long Integer, 
	[KNzaTIP]			Long Integer, 
	[RacunIN_ID]			Long Integer, 
	[KontoTroska]			Text (255), 
	[KnDokID]			Long Integer
);

CREATE TABLE [OpomenaSablonEx]
 (
	[IDOpomenaSablonEx]			Long Integer, 
	[IDOpomenaSablon]			Long Integer, 
	[KeyName]			Text (255), 
	[KeyIndex]			Long Integer, 
	[SablonText]			Text (255)
);

CREATE TABLE [Paste Errors]
 (
	[IDTRRAC]			Long Integer, 
	[PrioritetNaplate]			Long Integer, 
	[SK_ID]			Long Integer, 
	[RacunNO]			Long Integer, 
	[KontoKnjizenja]			Text (255), 
	[NazivRacuna]			Text (255), 
	[Napomena]			Text (255), 
	[Dobavljac]			Text (255), 
	[DobavljacKonto]			Long Integer, 
	[TipObracuna]			Long Integer, 
	[MesecRacuna]			Text (255), 
	[IznosRacunaEUR]			Double, 
	[IznosRacunaRSD]			Currency, 
	[IznosPoKoefEUR]			Double, 
	[IznosPoKoefRSD]			Currency, 
	[SifraKN]			Text (255), 
	[TMPprevID]			Long Integer, 
	[TipDokumenta]			Long Integer, 
	[MarkerVandrednogRacuna]			Text (255), 
	[FunkcijaNazivaRacuna]			Text (255), 
	[RBR]			Text (255), 
	[IznosRacunaKN]			Currency, 
	[DatumRacuna]			DateTime, 
	[DatumKnjizenja]			DateTime, 
	[DatumPlacanja]			DateTime, 
	[OpisRacuna]			Text (255), 
	[PozivNaBroj]			Text (255), 
	[PrethodniIDRdob]			Long Integer, 
	[NoviIDRdob]			Long Integer, 
	[NalogKnjizenja]			Long Integer, 
	[PDV]			Long Integer, 
	[ZatvaraKonto]			Text (255)
);

CREATE TABLE [PRENOS]
 (
	[ID]			Long Integer, 
	[PB]			Text (255), 
	[IDK]			Long Integer, 
	[R01]			Text (255), 
	[U1]			Double, 
	[U2]			Double, 
	[PS]			Double, 
	[txt]			Text (255), 
	[sk]			Long Integer
);

CREATE TABLE [PrinterBinLOCAL]
 (
	[IDlocalPrinterbin]			Long Integer, 
	[ID_Item]			Long Integer, 
	[TypeIndex]			Long Integer
);

CREATE TABLE [SemaKnjizenja]
 (
	[IDtplKN]			Long Integer, 
	[SemaKnjizenja]			Text (255), 
	[SourceTable]			Text (255), 
	[SourceTableWhrField]			Text (255), 
	[Stavka]			Text (255), 
	[Konto]			Text (255), 
	[DIPI]			Text (255), 
	[Znak]			Long Integer, 
	[SubAnlField]			Text (255), 
	[SubAnlFieldSource]			Text (255), 
	[Opis]			Text (255), 
	[GK_TipStavke]			Long Integer, 
	[GK_DokFn]			Text (255), 
	[GK_PNB]			Text (255), 
	[GK_IDPrtner]			Text (255), 
	[GK_RacunID]			Text (255), 
	[Sort]			Long Integer, 
	[Nalog_Opis]			Text (255), 
	[SourceSQL]			Memo/Hyperlink (255), 
	[SourceSQLValue]			Text (255)
);

CREATE TABLE [StaffTMP]
 (
	[IDUser]			Long Integer, 
	[User]			Text (50), 
	[Level]			Text (50), 
	[UseLang]			Text (50), 
	[RESTRICT]			Text (50), 
	[LastPC]			Text (255)
);

CREATE TABLE [Stope]
 (
	[ID]			Long Integer, 
	[Datum]			DateTime, 
	[Stopa]			Currency, 
	[Period]			Text (50)
);

CREATE TABLE [Switchboard Items]
 (
	[SwitchboardID]			Long Integer, 
	[ItemNumber]			Integer, 
	[ItemText]			Text (255), 
	[Command]			Integer, 
	[Argument]			Text (255), 
	[fLevel]			Text (255)
);

CREATE TABLE [tblAnaliza]
 (
	[ID]			Long Integer, 
	[GrupaPodataka]			Text (255), 
	[Naziv]			Text (255), 
	[CritcnaGrupa]			Long Integer, 
	[aQuery]			Text (255), 
	[aQuerySQL]			Memo/Hyperlink (255), 
	[ReportName]			Text (50), 
	[StatWHRCap]			Text (50), 
	[StatWHRComboSQL]			Memo/Hyperlink (255), 
	[StatSQLName]			Text (50), 
	[DatumRun]			DateTime, 
	[CountData]			Long Integer, 
	[AutoRunAll]			Long Integer, 
	[SortOrder]			Long Integer, 
	[stLinkCreatia]			Text (255), 
	[stLinkFormName]			Text (255), 
	[Active]			Long Integer, 
	[AdminAlert]			Long Integer, 
	[stOpenArgs]			Text (255), 
	[Opis]			Memo/Hyperlink (255), 
	[dellDataInQuery]			Long Integer, 
	[ExecuteQdef]			Text (255)
);

CREATE TABLE [tblIzvestaj]
 (
	[ID]			Long Integer, 
	[Naziv]			Text (255), 
	[Datasheet]			Text (255), 
	[Naslov]			Text (255), 
	[Btn1Cap]			Text (255), 
	[Btn1FF]			Text (255), 
	[Btn1Fn]			Long Integer, 
	[Btn2Cap]			Text (255), 
	[Btn2FF]			Text (255), 
	[Btn2Fn]			Long Integer, 
	[Btn3Cap]			Text (255), 
	[Btn3FF]			Text (255), 
	[Btn3Fn]			Long Integer, 
	[SortOrder]			Long Integer, 
	[cmbFilterSQL]			Memo/Hyperlink (255), 
	[cmbFilterCaption]			Text (255), 
	[cmbFilterID]			Long Integer
);

CREATE TABLE [tblIzvestajSub]
 (
	[IDsi]			Long Integer, 
	[ID_I]			Long Integer, 
	[QuerySQL]			Memo/Hyperlink (255), 
	[QueryName]			Text (255), 
	[Sort]			Long Integer, 
	[Funkcija]			Text (255)
);

CREATE TABLE [tblSifrarnik]
 (
	[ID]			Long Integer, 
	[Naziv]			Text (255), 
	[Datasheet]			Text (255), 
	[Btn1Cap]			Text (255), 
	[Btn1FF]			Text (255), 
	[Btn2Cap]			Text (255), 
	[Btn2FF]			Text (255), 
	[Btn3Cap]			Text (255), 
	[Btn3FF]			Text (255)
);

CREATE TABLE [tblSifrarnikSub]
 (
	[IDSubSfrID]			Long Integer, 
	[SifrID]			Long Integer, 
	[SQL]			Text (255), 
	[Type]			Text (255), 
	[Caption]			Text (255), 
	[Sort]			Long Integer
);

CREATE TABLE [tblSTATS_update_tblstat]
 (
	[IDstt]			Long Integer, 
	[StatGrup]			Text (255), 
	[StatNaziv]			Text (255), 
	[StatQuery]			Text (255), 
	[StatSQL]			Memo/Hyperlink (255), 
	[StatReport]			Text (50), 
	[StatWHRCap]			Text (50), 
	[StatWHRComboSQL]			Memo/Hyperlink (255), 
	[StatSQLName]			Text (50)
);

CREATE TABLE [tblWhrEx]
 (
	[IDwhrex]			Long Integer, 
	[WhrNaslov]			Text (50), 
	[WhrEx]			Memo/Hyperlink (255), 
	[frmToDo]			Text (50)
);

CREATE TABLE [TEMP_GEN]
 (
	[ID]			Long Integer, 
	[SUMA]			Currency
);

CREATE TABLE [tipStatus]
 (
	[IDStatus]			Long Integer NOT NULL, 
	[Status]			Text (255), 
	[limitTable]			Text (255), 
	[desc]			Text (255)
);

CREATE TABLE [VERSION-HISTORY]
 (
	[DAT]			DateTime, 
	[VER]			Text (50), 
	[DESC]			Text (255), 
	[FullDesc]			Memo/Hyperlink (255)
);

CREATE TABLE [GK_20250531_backup_p24_UpravljanjeSplit]
 (
	[STAVKAID]			Long Integer NOT NULL, 
	[BR_NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[DIZNOS]			Double, 
	[PIZNOS]			Double, 
	[TIP_STAVKE]			Double, 
	[DOK]			Text (50), 
	[lnkSkupstinaID]			Long Integer, 
	[lnkKUPACID]			Long Integer, 
	[lnkIzvodStavkaID]			Long Integer, 
	[NAPOMENA]			Text (255), 
	[PARAMETRI]			Text (25), 
	[OPIS]			Text (255), 
	[SIFRAKONTA]			Long Integer, 
	[DPO]			DateTime, 
	[SIFRAKN]			Text (6), 
	[RDOB]			Long Integer, 
	[RACID]			Long Integer, 
	[PRIORITET]			Long Integer, 
	[KNzaTIP]			Long Integer, 
	[RacunIN_ID]			Long Integer, 
	[KontoTroska]			Text (255), 
	[KnDokID]			Long Integer
);

CREATE TABLE [GK_PS]
 (
	[STAVKAID]			Long Integer, 
	[BR_NALOG]			Double, 
	[KONTO]			Text (50), 
	[DATUM]			DateTime, 
	[DIZNOS]			Double, 
	[PIZNOS]			Double, 
	[TIP_STAVKE]			Double, 
	[DOK]			Text (50), 
	[lnkSkupstinaID]			Long Integer, 
	[lnkKUPACID]			Long Integer, 
	[lnkIzvodStavkaID]			Long Integer, 
	[NAPOMENA]			Text (255), 
	[PARAMETRI]			Text (25), 
	[OPIS]			Text (255), 
	[SIFRAKONTA]			Long Integer, 
	[DPO]			DateTime, 
	[SIFRAKN]			Text (6), 
	[RDOB]			Long Integer, 
	[RACID]			Long Integer, 
	[PRIORITET]			Long Integer, 
	[KNzaTIP]			Long Integer, 
	[RacunIN_ID]			Long Integer, 
	[KontoTroska]			Text (255), 
	[KnDokID]			Long Integer
);

CREATE TABLE [Table1]
 (
	[stavkaid]			Long Integer NOT NULL, 
	[rdob]			Long Integer, 
	[knpodkonto]			Text (255)
);


```

## aj_fn_cmn.mdb — 23 tabele (zajednički AJ_ framework: Staff/login, Settings, rečnik za prevode)
```sql
-- ----------------------------------------------------------
-- MDB Tools - A library for reading MS Access database files
-- Copyright (C) 2000-2011 Brian Bruns and others.
-- Files in libmdb are licensed under LGPL and the utilities under
-- the GPL, see COPYING.LIB and COPYING files respectively.
-- Check out http://mdbtools.sourceforge.net
-- ----------------------------------------------------------

-- That file uses encoding UTF-8

CREATE TABLE [_Log]
 (
	[logID]			Long Integer, 
	[PC]			Text (15), 
	[WinUser]			Text (50), 
	[UserID]			Long Integer, 
	[UserTXT]			Text (50), 
	[Datum]			DateTime, 
	[Forma]			Text (60), 
	[TabCode]			Text (50), 
	[ItemID]			Long Integer, 
	[ActionType]			Text (20), 
	[msgExtra]			Text (255), 
	[msgErrNum]			Text (255), 
	[msg]			Text (255), 
	[msgPromene]			Memo/Hyperlink (255), 
	[Module]			Text (50)
);

CREATE TABLE [_Recnik]
 (
	[ID]			Long Integer, 
	[Poruka]			Text (255), 
	[Jezik]			Text (50), 
	[Index]			Long Integer, 
	[dlg]			Long Integer, 
	[Info]			Text (255)
);

CREATE TABLE [_RecnikJezik]
 (
	[RecLang]			Text (10), 
	[Recnik]			Text (50), 
	[On]			Boolean NOT NULL, 
	[Def]			Boolean NOT NULL
);

CREATE TABLE [_RecnikN]
 (
	[ID]			Long Integer, 
	[CIR]			Text (255)
);

CREATE TABLE [_TextFunction]
 (
	[ID]			Long Integer, 
	[KeyName]			Text (255), 
	[Description]			Memo/Hyperlink (255)
);

CREATE TABLE [Functions]
 (
	[ID]			Long Integer, 
	[Mod]			Text (50), 
	[Fnc]			Text (50), 
	[Primer]			Memo/Hyperlink (255), 
	[fullFunction]			Text (255), 
	[Opis]			Memo/Hyperlink (255), 
	[LastUpdate]			DateTime, 
	[TanksTo]			Text (255)
);

CREATE TABLE [References]
 (
	[ID]			Long Integer, 
	[References]			Text (50), 
	[PathDesc]			Text (50), 
	[FileName]			Text (255), 
	[Pathx86]			Text (255), 
	[Pathx64]			Text (255), 
	[Desc]			Text (255), 
	[Ver]			Text (255), 
	[Datum]			DateTime
);

CREATE TABLE [Settings]
 (
	[IDSettings]			Long Integer, 
	[SettingName]			Text (50), 
	[SettingVal]			Text (50), 
	[Descrition]			Text (255), 
	[Category]			Text (50), 
	[MFF]			Text (50), 
	[DefVal]			Text (50), 
	[FilterUser]			Long Integer, 
	[FilterPC]			Text (255), 
	[FilterCustom1Num]			Long Integer, 
	[FilterCustom2Num]			Long Integer, 
	[FilterCustom3Num]			Long Integer, 
	[FilterCustom1Txt]			Text (255), 
	[FilterCustom2Txt]			Text (255), 
	[FilterCustom3Txt]			Text (255), 
	[SettingVal_LT]			Memo/Hyperlink (255)
);

CREATE TABLE [Settings-STRUKTURA]
 (
	[IDSettings]			Long Integer, 
	[SettingName]			Text (50), 
	[SettingVal]			Text (50), 
	[Descrition]			Text (255), 
	[Category]			Text (50), 
	[MFF]			Text (50), 
	[DefVal]			Text (50), 
	[FilterUser]			Long Integer, 
	[FilterPC]			Text (255), 
	[FilterCustom1Num]			Long Integer, 
	[FilterCustom2Num]			Long Integer, 
	[FilterCustom3Num]			Long Integer, 
	[FilterCustom1Txt]			Text (255), 
	[FilterCustom2Txt]			Text (255), 
	[FilterCustom3Txt]			Text (255), 
	[SettingVal_LT]			Memo/Hyperlink (255)
);

CREATE TABLE [Staff]
 (
	[IDUser]			Long Integer, 
	[User]			Text (50), 
	[StaffLogin]			Text (50), 
	[Level]			Long Integer, 
	[LastLog]			Text (50), 
	[UseLang]			Text (50), 
	[RESTRICT]			Text (255), 
	[LastPC]			Text (255), 
	[INC]			Text (6)
);

CREATE TABLE [StaffPermition]
 (
	[IDPermition]			Long Integer, 
	[IDStaff]			Long Integer, 
	[KeyName]			Text (255), 
	[FormName]			Text (255), 
	[PermitionVal]			Long Integer, 
	[DisablePermition]			Long Integer
);

CREATE TABLE [Staff-STRUKTURA]
 (
	[IDUser]			Long Integer, 
	[User]			Text (50), 
	[StaffLogin]			Text (50), 
	[Level]			Long Integer, 
	[LastLog]			Text (50), 
	[UseLang]			Text (50), 
	[RESTRICT]			Text (255), 
	[LastPC]			Text (255)
);

CREATE TABLE [StaffTMP]
 (
	[IDUser]			Long Integer NOT NULL, 
	[User]			Text (50), 
	[StaffLogin]			Text (50), 
	[Level]			Long Integer, 
	[LastLog]			Text (50), 
	[UseLang]			Text (50), 
	[RESTRICT]			Text (255), 
	[LastPC]			Text (255)
);

CREATE TABLE [StaffTMP-STRUKTURA]
 (
	[IDUser]			Long Integer, 
	[User]			Text (50), 
	[Level]			Long Integer, 
	[UseLang]			Text (50), 
	[RESTRICT]			Text (255), 
	[LastPC]			Text (255)
);

CREATE TABLE [Switchboard Items]
 (
	[SwitchboardID]			Long Integer, 
	[ItemNumber]			Integer, 
	[ItemText]			Text (255), 
	[Command]			Integer, 
	[Argument]			Text (255), 
	[fLevel]			Text (50)
);

CREATE TABLE [Switchboard Items-STRUKTURA]
 (
	[SwitchboardID]			Long Integer, 
	[ItemNumber]			Integer, 
	[ItemText]			Text (255), 
	[Command]			Integer, 
	[Argument]			Text (255), 
	[fLevel]			Text (50)
);

CREATE TABLE [tblFixSQL-STRUKTURA-PRIMER]
 (
	[IDRef]			Long Integer, 
	[objectName]			Text (50), 
	[SQL]			Memo/Hyperlink (255), 
	[repName]			Text (50), 
	[objectType]			Text (50)
);

CREATE TABLE [tblSTATS]
 (
	[IDstt]			Long Integer, 
	[StatGrup]			Text (255), 
	[StatNaziv]			Text (255), 
	[StatQuery]			Text (255), 
	[StatSQL]			Memo/Hyperlink (255), 
	[StatReport]			Text (50), 
	[StatWHRCap]			Text (50), 
	[StatWHRComboSQL]			Memo/Hyperlink (255), 
	[StatSQLName]			Text (50), 
	[TextFilterCaption]			Text (50), 
	[StatWHRCap2]			Text (50), 
	[StatWHRComboSQL2]			Memo/Hyperlink (255), 
	[TextFilterCaption2]			Text (50), 
	[StaffLevel]			Text (255), 
	[StaffUID]			Text (255)
);

CREATE TABLE [tblSTATS-STRUKTURA]
 (
	[IDstt]			Long Integer, 
	[StatGrup]			Text (255), 
	[StatNaziv]			Text (255), 
	[StatQuery]			Text (255), 
	[StatSQL]			Memo/Hyperlink (255), 
	[StatReport]			Text (50), 
	[StatWHRCap]			Text (50), 
	[StatWHRComboSQL]			Memo/Hyperlink (255), 
	[StatSQLName]			Text (50), 
	[TextFilterCaption]			Text (50), 
	[StatWHRCap2]			Text (50), 
	[StatWHRComboSQL2]			Memo/Hyperlink (255), 
	[TextFilterCaption2]			Text (50), 
	[StaffLevel]			Text (255), 
	[StaffUID]			Text (255)
);

CREATE TABLE [tblWhrEx-STRUKTURA]
 (
	[IDwhrex]			Long Integer, 
	[WhrNaslov]			Text (50), 
	[WhrEx]			Memo/Hyperlink (255), 
	[frmToDo]			Text (50)
);

CREATE TABLE [UserLevelList]
 (
	[UserLevelID]			Long Integer NOT NULL, 
	[Caption]			Text (255)
);

CREATE TABLE [VERSION-HISTORY]
 (
	[DAT]			DateTime, 
	[VER]			Text (50), 
	[DESC]			Memo/Hyperlink (255), 
	[FullDesc]			Memo/Hyperlink (255)
);

CREATE TABLE [_Log-STRUKTURA]
 (
	[logID]			Long Integer, 
	[PC]			Text (15), 
	[WinUser]			Text (50), 
	[UserID]			Long Integer, 
	[UserTXT]			Text (50), 
	[Datum]			DateTime, 
	[Forma]			Text (60), 
	[TabCode]			Text (50), 
	[ItemID]			Long Integer, 
	[ActionType]			Text (20), 
	[msgExtra]			Text (255), 
	[msgErrNum]			Text (255), 
	[msg]			Text (255), 
	[msgPromene]			Memo/Hyperlink (255), 
	[Module]			Text (50)
);


```
