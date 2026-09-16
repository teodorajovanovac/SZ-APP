Da li su data.mdb i eksport 898 upita iz iste produkcione verzije?
- da

Gde se nalaze Troskovi_* tabele i da li postoji još jedan backend MDB?
- Koriste se samo Troskovi_PodKonta, Troskovi_PodKonta_DefDob ostali ne.

Koji upiti su stvarno pozvani iz formi, izveštaja, makroa ili VBA-a?
- Svaka forma je drugacije implementirana od strane originalnog programera. Vecina su now queryi ili su na testerima eventi koji menjaju record source.

Da li je konačan model Partner + MyCompany + Unit, ili stari Kupac + Skustina + Objekti?
- Partner + MyCompany + Unit   

Šta tačno predstavlja MyCompany: korisnika sistema, upravnika, pravno lice ili stambenu zajednicu?
- Stamabeni zajednicu sa prosirenjem podatka ko je upravnik, i dopunskim podacima koja se nalazu u tabeli Skupstina a nemogu da se implementiraju u Partner.

Da li će aplikaciju koristiti više upravljačkih kompanija i da li podaci moraju biti tenant-izolovani?
-Da. Izolacija nije apsolutna, Korsinku se dodeljuje koje kompanije u MyComany vidi. Podaci koji nisu retriktivni sa MyCompanyId mogu biti zajednicki.

Da li partner može imati više uloga istovremeno?
- Da. Tabelom PartnerAccounting definiše da li je Kupac th Vlasnik ili Dobavljač.

Da li su kategorija pravnog lica, poslovna uloga i lokacija odvojeni šifarnici?
- Da odvojene ili može da se korsti tblShortList jer su jednostavne liste.

Da li jedna fizička jedinica uvek ostaje jedan zapis?
- Da, postoji mogućnost podele jedne jedinice na dva dela da bi se fakturisala na dva koriska.

Kako se vremenski vode vlasnik, zakupac, korisnik i primalac računa?
-To se vodi kroz Contracts gde je prav veze izmenju parnera i jedinice i definise se period trajanja. 

Kako se deli trošak između više platioca i mora li zbir udela biti tačno 100%?
- Da mora, naravno može biti tolerancija greše koje ćemo rešiti sa uvodjenjem konta zaokruživanja.

Ko je pravni dužnik kada je račun poslat zakupcu, a vlasnik odgovara za dug?
- Sve se potražuje kome je izdat račun, u slučaju spora, vlasnik odgovara za dug, računi se ne menjaju.

Koja je razlika između IDMaster, IDGrupniRacunMaster i IDKGrupniRacun?
- IDMaster je kao što smo rekli ukinut, IDGrupniRacunMaster je ID grupnog računa koji se koristi za već napisano grupisanje računa a IDKGrupniRacun je ID grupnog računa koji je generisan i upisuje se u storniranim računima.

Da li SIFRAKONTA i lnkKUPACID imaju različito značenje na nekim kontima?
- Da SIFRAKONTA se brise.

Da li su ZK, RacunIN, BenefitUpdate i OpomenaSablonEx još aktivni?
- ZK objašnjeno, RacunIN objašnjeno, BenefitUpdate se korisi i tu se upisuju Units koji ne plaćaju račune, odnosno generišu se računi i potom storniraju i preadju specifikacije iznosa pod benefitom. OpomenaSablonEx je započet proces kako bi imali fleksibilnu opomenu sa tekstovima koji migu da se menjaju.

Šta predstavljaju Konta, KontniOkvir, KontoTroska, partnerovo Account i PodKonto, i kako su povezani?
- Svaka transakcija je definiše sa Kontom - 2040, 2410, 4350..... 2040 i 4350 moraju da imaju PartnerId tj ParnerAccountingId. Time se definiše čija je transakcija. Mođutim da bi odmah imali zašta su namenjena sredstva koja je primaju ili troše definisao sam KontoTroška tj sad SubAccount da bi imali hijerarhiju šta se dešava.
2410 nema SubAccount, 2040 nema SubAccount samo kad je u pitanju pretplata, 4305 nema SubAccount samo kad je privremeno stavljeno kod partnera i treba da se u greskama lista.

Koja polja se računaju na 4 decimale, a koja se trajno knjiže na 2?
- Stavke računa ukupno i račun ukupno je 2 decimale sve ostrralo idemo sa 4.

Koji način zaokruživanja se koristi: standardni, bankarski ili nešto treće?
-do sad sam koristio standardno ali mislim da bankarski ili kombinovano je bolji jer se desavalo da se ne slaze zbir stavki sa ukupnim iznosom zbog zaokruzivanja 

Da li se ostatak zbog zaokruživanja dodeljuje jednoj stavci?
-nisam do sad imao problem sa ostatkom zbog zaokruzivanja 

Da li nalog ima draft i posted stanje, i ko sme da ga rasknjiži?
- draft da, može da se rasknjiži ali se retko koristi, uvek se sve upisuje i stornira se ako treba. Postoje korisnici koji mogu da ga knjiže.

Da li se izdati/proknjiženi dokumenti brišu ili samo storniraju?
- storniraju,  ali mogu da se i obrišu ukoliko nikad nisu ni izdati već greškom kreirani.

Kako se numerišu računi, opomene, nalozi i izvodi — globalno, po kompaniji, godini ili zgradi?
- Računi po principu SZID (sad MyCompanyId tj CompanyId), PartnerAccountingId, GodinaMesec u formatu yymm tj npr 101-1234-1121 
- Opomene slično kao račun 101-1234-P20260110 * gde je sufix PYYYYMMDD
- Naloz redno
- Izvodi po svojim brojevima redno / godina

Kako se modeluju periodi PDV-a, kamatne stope i promene kontnog plana?
- stambene zajednice nisu u PDV-u, upravnici mogu biti u pdv-u. Za sad nema potrebe za tim. 
- Kamatne stope se preuzimaju od strane NBS, imam uradjen code koji radi to i preizeću ga po generisanju projekta. 
- Kamatne stope se trutno rucno unose u tabelu stope 
- nemam plan za prmenu kontnog plana


Koje stare permisije treba prevesti u nove poslovne role?
- Ovo je ostavljeno za budućnost. Kako bi se izbegao posao prerade. Trenutno je plan da imam root, upravnik, moderator koji radi za upravnika, reviev snijem stanari, koji će verovatn imati posebnu aplikaciju sa pojednostavljenim elemenitima.

Gde se čuvaju prilozi i koliko dugo?
-Trenutno se nigde ne cuvaju prilozi, mada bi trebalo da se cuvaju na serveru. 

Koji email servis se koristi, kako se rade retry, deduplikacija i praćenje isporuke?
-trenutno app koristi pop3 mail za slanje. Takođe korisiti se kod nekih korisnika i gmail sa Auth2 protokolom. Za preuzimanje izvoda koristi se autoamtko skeniranje foldera u potrazi za xml fajlovima.

Da li je cutover paralelni rad ili kratko zamrzavanje stare aplikacije?
-Paraleleni rad kako bi se uporedo testiralo i videlo da li su podaci isti u obe aplikacije.

Kako se prenose promene nastale od februara 2026. do dana prelaska?
- Napravićemo import. Nakon definisanje baze, napravićemo excel fajlove (ili cvs) koji će biti univerzalni za import. Prebacivanje u početku ćemo radi postepeno segment po segmentu, takođe moraćemo da radimo i testiranje paralelno



uoceni problemi : 


Trigger za ravnotežu naloga će blokirati normalno knjiženje.
Predloženi trigger proverava ravnotežu posle svakog SQL iskaza: [schema-ddl-draft.sql (line 679)](/C:/Users/Tea/SZ-APP/docs/schema-ddl-draft.sql:679).
Ako EF unese duguje i potražuje kao dva odvojena INSERT iskaza u istoj transakciji, prvi će odmah pasti kao neuravnotežen. SQL Server nema odložene constraints do kraja transakcije.
Potrebno je imati bar:
status naloga Draft/Posted;
dozvolu da draft bude privremeno neuravnotežen;
proveru ravnoteže prilikom operacije Post;
jednu transakciju koja zaključava i zatvara nalog.
- NALOG prilokom unosa nesme iimati trige, kontrola ravnoteže se tek radi kada se nalog pokuša proknjižiti. U tom slučaju ne sme ni da se knjiži. Tek kada se unesu sve stavke radi se provera ravnoteže. Tek tada se i zatvara transakcija. Triger ne sme da radi prilikom unosa stavki. Nalog ako nije proknjizen ne utice na transakcije kao takve i on je tako reči u statusu DRAFT.



Model podele naplate dupliranjem jedinice nije bezbedan.
Predlog da se jedna garaža unese dva puta sa koeficijentima .3333 i .6666 može napraviti:
duplu kvadraturu;
pogrešan broj jedinica;
duplo vlasništvo;
probleme sa opomenama i pravnim postupkom;
zbir .9999, a ne 1.0000;
nemogućnost praćenja promene podele kroz vreme.
Bolji model je jedna fizička Unit plus tabela UnitBillingAllocation sa partnerom, procentom/koeficijentom i periodom važenja.
- Može



Ne postoji vremenska istorija vlasnika, zakupca i platioca.
Plan prepoznaje da je kod spora važno kome je račun izdat, ali predloženi OwnerId, TenantId i InvoicingId predstavljaju samo trenutno stanje. Potrebni su:
istorija odnosa sa ValidFrom/ValidTo;
primalac zapamćen na samoj fakturi;
snapshot imena, adrese, PIB-a i druge pravne identifikacije;
pravilo šta se dešava kod promene vlasnika usred obračunskog perioda.
Nedovršena rečenica „Po defaultu kad InvoicingId…“ nalazi se u [tehnicki-plan-faza1.md (line 65)](/C:/Users/Tea/SZ-APP/docs/tehnicki-plan-faza1.md:65).
-Napisao sam već da kreiramo novu tabelu Contract




Partner, Category, Account i MyCompany mešaju različite pojmove.
Trenutno nije razdvojeno:
pravna forma: fizičko lice, stranac, kompanija, stambena zajednica;
- Partner definise pravnu formu, fizicko, stranac, pravno lice, upravnik, SZ
poslovna uloga: kupac, dobavljač, vlasnik, zakupac, upravnik;
- Kupac dobavljac se definise u tabeli PartnerAccounting
- Vlasnik, Zakupac u tabeli Contract, mada je ranije bilo u Unit tj Objekti
konta i podkonta;
lokacijska/portfolio hijerarhija;
organizacija koja koristi aplikaciju;
stambena zajednica kojom se upravlja.
- Ovo se definise u tabeli MyComapanz koju možemo i da nazovenom samo Company
Jedan partner može istovremeno biti kupac i dobavljač. Konto nije dobar diskriminator tipa partnera, jer je knjigovodstvena konfiguracija, a ne identitet subjekta.
- Tako je, to se definise u PartnerAccounting,







Problemi u DDL-u




GK.Konto nema FK prema Konta ili KontniOkvir, iako je centralno knjigovodstveno polje.
-treba da ima prema.
RacunStavke.TipObracunaId nema FK.
- Nema potrebe imati TipObracunaId u Racunustavke dovoljna je veza prema RDOB

GK.TipStavke nema FK.
- Ima prema TipStavke.ID_TIP prema GK.TIP_STAVKE

Skustina.UplatnicaTip, SkStatus, TipSubjekta i više sličnih polja nemaju definisane šifarnike/FK.

Nalog.SkustinaId je nullable iako pravilo zahteva da nalog pripada jednoj skupštini.
- MOra da ima SkustinaId tj sad je to CompanyId

Skoro sva poslovno obavezna polja su nullable: datumi računa, iznosi, broj dokumenta, partner dobavljača itd.
- Datumi imam opciju da je nullable jer mi je bolje ostaviti privremeno prazno nego upisivati pogresan datum,

Nema poslovnih UNIQUE ograničenja za broj računa, broj izvoda, PIB, bankovni račun ili druge prirodno jedinstvene podatke.
Nema indeksa na FK i najčešća polja pretrage.
Nema CHECK pravila za duguje/potražuje, datume, procente, stope, negativne iznose i statuse.
ON DELETE CASCADE na stavkama računa i izvoda može biti neprihvatljiv za knjigovodstvene dokumente. Treba odlučiti da li se objavljeni dokumenti uopšte smeju fizički brisati.
INT IDENTITY je uveden na osnovu konvencije imena, iako eksport ne potvrđuje AutoNumber: [schema-ddl-draft.sql (line 15)](/C:/Users/Tea/SZ-APP/docs/schema-ddl-draft.sql:15).

Migracija postojećih ID vrednosti i seedova nije opisana.
- za sve korisiti resenje TrasferId kao sto sam uradio u staroj bazi.

Promena Nalog.Br_Nalog iz Double u identity istovremeno menja i identitet i poslovni broj dokumenta. Bolje je razmotriti odvojeni NalogId i NalogNumber.
- Da, bolje je imati NalogId posebno, a BrojNaloga na novou CompanyId redni broj, uvesti i sufix Godina, da mogli godisnje da se resetuju brojevi naloga.

Prebacivanje Long Integer polja u BIT nije potkrepljeno profilisanjem stvarnih vrednosti.

Skustina.PIB je INT, dok je Kupac.PIB tekst. PIB je identifikator, ne broj za računanje, pa bi trebalo uskladiti tekstualni tip i validaciju.
-PIB je text
Settings_eMail.SettingVal može sadržati lozinke/tokene. Nije definisano da se tajne ne sele u običnu tabelu.
Files čuva relativnu putanju, ali nije odlučeno gde fajlovi žive, ko im pristupa i kako se arhiviraju.
AuditLog nije definisan kao neizmenjiv zapis; nedostaje audit aktuelnog korisnika, tenant/company konteksta i correlation ID-a.
Nije definisana strategija konkurentnih izmena (rowversion) za fakture, knjiženje i uparivanje izvoda.