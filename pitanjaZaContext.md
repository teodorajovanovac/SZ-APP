Da li su data.mdb i eksport 898 upita iz iste produkcione verzije?
Gde se nalaze Troskovi_* tabele i da li postoji još jedan backend MDB?
Koji upiti su stvarno pozvani iz formi, izveštaja, makroa ili VBA-a?
Da li je konačan model Partner + MyCompany + Unit, ili stari Kupac + Skustina + Objekti?
Šta tačno predstavlja MyCompany: korisnika sistema, upravnika, pravno lice ili stambenu zajednicu?
Da li će aplikaciju koristiti više upravljačkih kompanija i da li podaci moraju biti tenant-izolovani?
Da li partner može imati više uloga istovremeno?
Da li su kategorija pravnog lica, poslovna uloga i lokacija odvojeni šifarnici?
Da li jedna fizička jedinica uvek ostaje jedan zapis?
Kako se vremenski vode vlasnik, zakupac, korisnik i primalac računa?
Kako se deli trošak između više platioca i mora li zbir udela biti tačno 100%?
Ko je pravni dužnik kada je račun poslat zakupcu, a vlasnik odgovara za dug?
Koja je razlika između IDMaster, IDGrupniRacunMaster i IDKGrupniRacun?
Da li SIFRAKONTA i lnkKUPACID imaju različito značenje na nekim kontima?
Da li su ZK, RacunIN, BenefitUpdate i OpomenaSablonEx još aktivni?
Šta predstavljaju Konta, KontniOkvir, KontoTroska, partnerovo Account i PodKonto, i kako su povezani?
Koja polja se računaju na 4 decimale, a koja se trajno knjiže na 2?
Koji način zaokruživanja se koristi: standardni, bankarski ili nešto treće?
Da li se ostatak zbog zaokruživanja dodeljuje jednoj stavci?
Da li nalog ima draft i posted stanje, i ko sme da ga rasknjiži?
Da li se izdati/proknjiženi dokumenti brišu ili samo storniraju?
Kako se numerišu računi, opomene, nalozi i izvodi — globalno, po kompaniji, godini ili zgradi?
Kako se modeluju periodi PDV-a, kamatne stope i promene kontnog plana?
Koje stare permisije treba prevesti u nove poslovne role?
Gde se čuvaju prilozi i koliko dugo?
Koji email servis se koristi, kako se rade retry, deduplikacija i praćenje isporuke?
Da li je cutover paralelni rad ili kratko zamrzavanje stare aplikacije?
Kako se prenose promene nastale od februara 2026. do dana prelaska?




uoceni problemi : 


Trigger za ravnotežu naloga će blokirati normalno knjiženje.
Predloženi trigger proverava ravnotežu posle svakog SQL iskaza: [schema-ddl-draft.sql (line 679)](/C:/Users/Tea/SZ-APP/docs/schema-ddl-draft.sql:679).
Ako EF unese duguje i potražuje kao dva odvojena INSERT iskaza u istoj transakciji, prvi će odmah pasti kao neuravnotežen. SQL Server nema odložene constraints do kraja transakcije.
Potrebno je imati bar:
status naloga Draft/Posted;
dozvolu da draft bude privremeno neuravnotežen;
proveru ravnoteže prilikom operacije Post;
jednu transakciju koja zaključava i zatvara nalog.


Model podele naplate dupliranjem jedinice nije bezbedan.
Predlog da se jedna garaža unese dva puta sa koeficijentima .3333 i .6666 može napraviti:
duplu kvadraturu;
pogrešan broj jedinica;
duplo vlasništvo;
probleme sa opomenama i pravnim postupkom;
zbir .9999, a ne 1.0000;
nemogućnost praćenja promene podele kroz vreme.
Bolji model je jedna fizička Unit plus tabela UnitBillingAllocation sa partnerom, procentom/koeficijentom i periodom važenja.




Ne postoji vremenska istorija vlasnika, zakupca i platioca.
Plan prepoznaje da je kod spora važno kome je račun izdat, ali predloženi OwnerId, TenantId i InvoicingId predstavljaju samo trenutno stanje. Potrebni su:
istorija odnosa sa ValidFrom/ValidTo;
primalac zapamćen na samoj fakturi;
snapshot imena, adrese, PIB-a i druge pravne identifikacije;
pravilo šta se dešava kod promene vlasnika usred obračunskog perioda.
Nedovršena rečenica „Po defaultu kad InvoicingId…“ nalazi se u [tehnicki-plan-faza1.md (line 65)](/C:/Users/Tea/SZ-APP/docs/tehnicki-plan-faza1.md:65).





Partner, Category, Account i MyCompany mešaju različite pojmove.
Trenutno nije razdvojeno:
pravna forma: fizičko lice, stranac, kompanija, stambena zajednica;
poslovna uloga: kupac, dobavljač, vlasnik, zakupac, upravnik;
konta i podkonta;
lokacijska/portfolio hijerarhija;
organizacija koja koristi aplikaciju;
stambena zajednica kojom se upravlja.
Jedan partner može istovremeno biti kupac i dobavljač. Konto nije dobar diskriminator tipa partnera, jer je knjigovodstvena konfiguracija, a ne identitet subjekta.








Problemi u DDL-u




GK.Konto nema FK prema Konta ili KontniOkvir, iako je centralno knjigovodstveno polje.
RacunStavke.TipObracunaId nema FK.
GK.TipStavke nema FK.
Skustina.UplatnicaTip, SkStatus, TipSubjekta i više sličnih polja nemaju definisane šifarnike/FK.
Nalog.SkustinaId je nullable iako pravilo zahteva da nalog pripada jednoj skupštini.
Skoro sva poslovno obavezna polja su nullable: datumi računa, iznosi, broj dokumenta, partner dobavljača itd.
Nema poslovnih UNIQUE ograničenja za broj računa, broj izvoda, PIB, bankovni račun ili druge prirodno jedinstvene podatke.
Nema indeksa na FK i najčešća polja pretrage.
Nema CHECK pravila za duguje/potražuje, datume, procente, stope, negativne iznose i statuse.
ON DELETE CASCADE na stavkama računa i izvoda može biti neprihvatljiv za knjigovodstvene dokumente. Treba odlučiti da li se objavljeni dokumenti uopšte smeju fizički brisati.
INT IDENTITY je uveden na osnovu konvencije imena, iako eksport ne potvrđuje AutoNumber: [schema-ddl-draft.sql (line 15)](/C:/Users/Tea/SZ-APP/docs/schema-ddl-draft.sql:15).
Migracija postojećih ID vrednosti i seedova nije opisana.
Promena Nalog.Br_Nalog iz Double u identity istovremeno menja i identitet i poslovni broj dokumenta. Bolje je razmotriti odvojeni NalogId i NalogNumber.
Prebacivanje Long Integer polja u BIT nije potkrepljeno profilisanjem stvarnih vrednosti.
Skustina.PIB je INT, dok je Kupac.PIB tekst. PIB je identifikator, ne broj za računanje, pa bi trebalo uskladiti tekstualni tip i validaciju.
Settings_eMail.SettingVal može sadržati lozinke/tokene. Nije definisano da se tajne ne sele u običnu tabelu.
Files čuva relativnu putanju, ali nije odlučeno gde fajlovi žive, ko im pristupa i kako se arhiviraju.
AuditLog nije definisan kao neizmenjiv zapis; nedostaje audit aktuelnog korisnika, tenant/company konteksta i correlation ID-a.
Nije definisana strategija konkurentnih izmena (rowversion) za fakture, knjiženje i uparivanje izvoda.