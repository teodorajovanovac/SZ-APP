# Potrebne ispravke

## Baza podataka

### core.Company
Id mora ne treba da bude autonumber, vec se zadaje rucno.
Kada se kreira novi Company backend ce dodeliti novi Id po principu max + 1 
Ukoliko korisnik je root mora ima opciju da može zada svoj Id i tad backend proverava da li je Id slobodan.

Nedostaju neka polja:
	[LocationCategoryId]	Long Integer,  -- FK to LoactionCategory.Id
	[Note]			        Text (255), 
	[SortIndex]			    Integer, 
	[ExternalAccount]		Text (255), 

menjam i RelativeFolderName u text(255) 

## Frontend UI


### Login
Login koji smo imali na app mnogo bolje izgleda......Videti da se izgled i sve to što smo radi sa konfiguracijama prebaci u ovaj novi frontend - ovo može i kasnije


### Header
Aktivna kompanija... Potebno je da se imamo opciju da se prikazuju podaci od svih kompanija u recimo listi Ugovori.
- Svi podaci - to su sve kompanije kojim user ima pristup i miksuju se podaci
- Podaci po LocationCategory - npr padajuci menu daje BW pa sledeci je BW / Plot 24, BW / Plot 23..... ako se izabere BW to znači svi kojitu pripadaju.
- Podaci po pojedinalnim kompanijama - kao što je sad
Padajuča lista treba da bude Select kompanenta da može i da se pretražuje unutar nje. Imam MaxiUpravnik koji ima 75 SZ tako da verujem da će im zanačiti da mogu da rade quick search unutar te liste. 


### Menu
Mislim da je pametno imati Menu u bazi podataka time rešavamo i prevodjenje na druge jezike bez promene Fronta / Translation.Id
Možemo prema tome da radimo posebno sortiranje i da eventualno definisemo po ima permisije na kojim dijalozima

    Početna

Podaci SZ
    Ugovori * ovo je mixovani prikaz: Contract > Partner + Contract > Unit  + Unit > ShortList.ShortName (TableName:UnitType) + Contract > Partner > Company (mada vidim da je isto i Contract > Unit > Company)
    i dodati jos i PartnerAccount i PartnerComms za pretragu i ... > Company > Partner > PartnerAccount

        Company.CompanyId, PartnerAccount.AccountNumber, Company.ShortName, BuildingEntity.Name, Partner.Name, Unit.Name, 
        Unit.UnitTypeId > ShortList.ShortName (TableName:UnitType), 

        * pretraga prva 2 polja posebna pretraga samo za njih, posel jedno veliko polje za sve uključujući sprevana polja: email, telefon. Posle pretrage opcija da prikazuje/pretražuje aktivne ili neaktivne ugovore.

        * Klik na kolone otvaraju podatke: 
        ** Company.CompanyId    otvara KontoKartica - za vrednost AccountNumber iz PartnerAccount ali od CompanyId
        ** PartnerAccount.AccountNumber  otvara KontoKartica Partner
        ** Company.ShortName    otvara podatke od kompanije
        ** BuildingEntity.Name  otvara podatke od ulaza
        ** Partner.Name         otvara podatke o Partneru
        ** Unit.Name            otvara podatke o jedinci
        ** Unit.UnitTypeId > ShortList.ShortName (TableName:UnitType)  - ništa

Šifarnici 
    Partneri
    Posebni delovi

Fakturisanje
    Ulazni računi * ulazak u listu računa obziram da je grupisanje po markeru YYMM
    Izlazni računi * ulazak u grupe raČuna
    Opomene * ulazak u grupe opomena

Finansije
    Izvodi
    Nalozi
    Kartice

    Izveštaji

Sistem
    Uvoz podataka
    Korisnici
    Moje kompanije
    Administracija
### Import
Neka nam napravi primere za import CSV koju mogu otvoriti u excelu da znamo sta očekuje.

