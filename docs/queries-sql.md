# Tačan SQL tekst svih upita — izvučeno direktno iz Accessa (QueryDef.SQL)

Generisano VBA makroom koji je korisnica pokrenula u `SZAPP.mdb` (petlja kroz `CurrentDb.QueryDefs`, štampa `.Name` i `.SQL`). Ovo je merodavan izvor za logiku upita — sadrži kompletne JOIN/GROUP BY/HAVING klauzule, za razliku od `queries/*.txt` design-format eksporta ili automatskog izvlačenja preko mdbtools (oba gube deo informacija). Ukupno 898 upita (isključeni su automatski generisani skriveni upiti sa prefiksom `~sq_`, koji su vezani za combo/list box-ove i podforme — ti se prirodno reprodukuju kroz odgovarajuće React komponente, ne treba ih ručno portovati).

```
=====QUERY=====
_DUPLIRAJPODATKE
-----SQL-----
INSERT INTO Troskovi_Racuni ( RacunNO, NazivRacuna, Napomena, Dobavljac, DobavljacKonto, IznosE, IznosD, TipObracuna, MesecRacuna, S, L, G )
SELECT Troskovi_Racuni.RacunNO, Troskovi_Racuni.NazivRacuna, Troskovi_Racuni.Napomena, Troskovi_Racuni.Dobavljac, Troskovi_Racuni.DobavljacKonto, Troskovi_Racuni.IznosE, Troskovi_Racuni.IznosD, Troskovi_Racuni.TipObracuna, [nov mesec] AS MR, Troskovi_Racuni.S, Troskovi_Racuni.L, Troskovi_Racuni.G
FROM Troskovi_Racuni;


=====QUERY=====
AAAAA
-----SQL-----
INSERT INTO RacunStavke ( ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5 )
SELECT 9999 AS Expr1, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, -[Kolicina] AS Expr2, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5
FROM RacunStavke
WHERE (((RacunStavke.ID_R)=34));


=====QUERY=====
AAAAAB
-----SQL-----
INSERT INTO RacunStavke ( ID_R )
SELECT 9999 AS Expr1, RacunStavke.*
FROM RacunStavke
WHERE (((RacunStavke.ID_R)=34));


=====QUERY=====
AdresaDostave
-----SQL-----
SELECT Kupac.ID_K, SzUlaz.Adresa, SzUlaz.Zgrada, TipObjekta.Print, Objekti.SifraPD, Objekti.BrojPD, Skustina.PBrojSZ, Skustina.GradSZ, SzUlaz.Ulaz
FROM Skustina INNER JOIN (((Kupac INNER JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O) INNER JOIN SzUlaz ON Objekti.Ulaz = SzUlaz.Ulaz) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) ON Skustina.IDSkupstina = SzUlaz.SZ
WHERE (((TipObjekta.IDTipObj)=1 Or (TipObjekta.IDTipObj)=2));


=====QUERY=====
Analiza_ClearData
-----SQL-----
UPDATE tblAnaliza SET tblAnaliza.CountData = 0, tblAnaliza.DatumRun = Null;


=====QUERY=====
ArchiveMails
-----SQL-----
UPDATE Mail_Send SET Mail_Send.Archive = -1;


=====QUERY=====
aZURIRANJEpARAMETARUgK
-----SQL-----
SELECT GK.DOK, OcistiPozivNaBroj(Right([DOK],Len([DOK])-2)) AS Expr1, GK.BR_NALOG, GK.NAPOMENA, GK.PARAMETRI
FROM GK
WHERE (((GK.BR_NALOG)=2) AND ((GK.NAPOMENA) Is Not Null));


=====QUERY=====
AzuriranjeSkupstineUGK
-----SQL-----
UPDATE GK INNER JOIN KupciSkupstine ON GK.lnkKUPACID=KupciSkupstine.ID_K SET GK.lnkSkupstinaID = KupciSkupstine.lnkSkupstinaID;


=====QUERY=====
AzuriranjeSkupstineUGK?67
-----SQL-----
UPDATE Skustina INNER JOIN GK ON Skustina.PIB=GK.OPIS SET GK.lnkSkupstinaID = Skustina.IDSkupstina, GK.BR_NALOG = 3, GK.lnkKUPACID = 1, GK.DATUM = #6/30/2012#, GK.DPO = #7/15/2012#;


=====QUERY=====
BenefitCount
-----SQL-----
SELECT BenefitKupacYYMM.KupacID, Count(BenefitKupacYYMM.Used) AS CountOfUsed, Sum(BenefitKupacYYMM.Used) AS SumOfUsed
FROM BenefitKupacYYMM
GROUP BY BenefitKupacYYMM.KupacID;


=====QUERY=====
BENEFIT-GRP
-----SQL-----
SELECT Benefiti.DateEntry, Benefiti.KupacID, Benefiti.MesecYYMM
FROM Benefiti
GROUP BY Benefiti.DateEntry, Benefiti.KupacID, Benefiti.MesecYYMM;


=====QUERY=====
BENEFIT-GRP_Crosstab
-----SQL-----
TRANSFORM Count([BENEFIT-GRP].KupacID) AS CountOfKupacID
SELECT Kupac.lnk_ID_SK, [BENEFIT-GRP].[KupacID], Kupac.Naziv, Count([BENEFIT-GRP].[KupacID]) AS [Total Of DateEntry]
FROM [BENEFIT-GRP] INNER JOIN Kupac ON [BENEFIT-GRP].KupacID = Kupac.ID_K
GROUP BY Kupac.lnk_ID_SK, [BENEFIT-GRP].[KupacID], Kupac.Naziv
PIVOT [BENEFIT-GRP].[MesecYYMM];


=====QUERY=====
BenefitHave
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, Benefiti.KupacID, Benefiti.MesecYYMM, Benefiti.Used
FROM Benefiti INNER JOIN GrupaRacuna ON Benefiti.MesecYYMM = GrupaRacuna.GrupaRacunaFXN
GROUP BY GrupaRacuna.IDGrupaRacuna, Benefiti.KupacID, Benefiti.MesecYYMM, Benefiti.Used
HAVING (((Benefiti.Used)=0));


=====QUERY=====
BenefitiExport
-----SQL-----
SELECT Skustina.IDSkupstina AS ID_SZ, Skustina.Zgrada, TipObjekta.TipObj, Benefiti.MesecYYMM, Objekti.SifraPD AS Unit, Objekti.lnk_ID_K, Objekti.K1, Objekti.K2, Objekti.K3, Kupac.Naziv, Benefiti.DateEntry
FROM Kupac INNER JOIN (((Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Benefiti ON Objekti.ID_O = Benefiti.ObjekatID) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) ON Kupac.ID_K = Objekti.lnk_ID_K
WHERE (((Benefiti.DateEntry) Is Not Null) AND ((TipObjekta.IDTipObj)=1))
ORDER BY Skustina.IDSkupstina, Objekti.BrojPD;


=====QUERY=====
BenefitiExport_All
-----SQL-----
SELECT Benefiti.MesecYYMM, Skustina.IDSkupstina AS ID_SZ, Skustina.Zgrada, TipObjekta.TipObj, Objekti.SifraPD AS Unit, Objekti.lnk_ID_K, Objekti.K1, Objekti.K2, Objekti.K3, Kupac.Naziv, Benefiti.Used
FROM Kupac INNER JOIN (((Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Benefiti ON Objekti.ID_O = Benefiti.ObjekatID) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) ON Kupac.ID_K = Objekti.lnk_ID_K
ORDER BY Benefiti.MesecYYMM, Skustina.IDSkupstina, Objekti.BrojPD;


=====QUERY=====
BenefitiExport_GPMima
-----SQL-----
SELECT Objekti.lnk_tip, Objekti.SifraPD, Objekti.ID_O, BenefitiExport.lnk_ID_K, BenefitiExport.lnkSkupstinaID AS Expr1
FROM BenefitiExport INNER JOIN Objekti ON BenefitiExport.lnk_ID_K = Objekti.lnk_ID_K
WHERE (((Objekti.lnk_tip)=4));


=====QUERY=====
BENEFITI-LISTING-2304
-----SQL-----
SELECT [BENEFIT-LISTING].*
FROM [BENEFIT-LISTING]
WHERE ((([BENEFIT-LISTING].MesecYYMM)="2304"));


=====QUERY=====
BENEFITI-LISTING-2305
-----SQL-----
SELECT [BENEFIT-LISTING].*
FROM [BENEFIT-LISTING]
WHERE ((([BENEFIT-LISTING].MesecYYMM)="2305"));


=====QUERY=====
BENEFITI-LISTING-2306
-----SQL-----
SELECT [BENEFIT-LISTING].*
FROM [BENEFIT-LISTING]
WHERE ((([BENEFIT-LISTING].MesecYYMM)="2306"));


=====QUERY=====
BENEFITI-LISTING-2307
-----SQL-----
SELECT [BENEFIT-LISTING].*
FROM [BENEFIT-LISTING]
WHERE ((([BENEFIT-LISTING].MesecYYMM)="2307"));


=====QUERY=====
BENEFITI-LISTING-2308
-----SQL-----
SELECT [BENEFIT-LISTING].*
FROM [BENEFIT-LISTING]
WHERE ((([BENEFIT-LISTING].MesecYYMM)="2308"));


=====QUERY=====
BenefitKupacYYMM
-----SQL-----
SELECT Benefiti.KupacID, Benefiti.MesecYYMM, Benefiti.Used
FROM Benefiti
GROUP BY Benefiti.KupacID, Benefiti.MesecYYMM, Benefiti.Used
ORDER BY Benefiti.MesecYYMM;


=====QUERY=====
BENEFIT-LISTING
-----SQL-----
SELECT Objekti.ID_O, Objekti.Ulaz, TipObjekta.TipObj, Objekti.SifraPD, Objekti.naziv, Objekti.BrojPD, Kupac.ID_K, Kupac.Naziv, Benefiti.MesecYYMM
FROM Kupac AS Kupac_1 INNER JOIN (((Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Benefiti ON Objekti.ID_O = Benefiti.ObjekatID) ON Kupac_1.IDMaster = Kupac.ID_K
ORDER BY TipObjekta.TipObj, Objekti.BrojPD, Kupac_1.ID_K;


=====QUERY=====
BenefitRacun_2402
-----SQL-----
SELECT Racun.IDRacun, Racun.lnkGR, BenefitKupacYYMM.KupacID, BenefitKupacYYMM.MesecYYMM
FROM Racun RIGHT JOIN BenefitKupacYYMM ON Racun.ID_K = BenefitKupacYYMM.KupacID
WHERE (((Racun.lnkGR)=37 Or (Racun.lnkGR)=38 Or (Racun.lnkGR)=39 Or (Racun.lnkGR) Is Null) AND ((BenefitKupacYYMM.MesecYYMM)="2402"));


=====QUERY=====
BenefitRacun_2403
-----SQL-----
SELECT Racun.IDRacun, Racun.lnkGR, BenefitKupacYYMM.KupacID, BenefitKupacYYMM.MesecYYMM
FROM Racun RIGHT JOIN BenefitKupacYYMM ON Racun.ID_K = BenefitKupacYYMM.KupacID
WHERE (((Racun.lnkGR)=48 Or (Racun.lnkGR)=49 Or (Racun.lnkGR)=50 Or (Racun.lnkGR) Is Null) AND ((BenefitKupacYYMM.MesecYYMM)="2403"));


=====QUERY=====
BenefitRacun_2404
-----SQL-----
SELECT Racun.IDRacun, Racun.lnkGR, BenefitKupacYYMM.KupacID, BenefitKupacYYMM.MesecYYMM
FROM Racun RIGHT JOIN BenefitKupacYYMM ON Racun.ID_K = BenefitKupacYYMM.KupacID
WHERE (((Racun.lnkGR)=52 Or (Racun.lnkGR)=53 Or (Racun.lnkGR)=54 Or (Racun.lnkGR) Is Null) AND ((BenefitKupacYYMM.MesecYYMM)="2404"));


=====QUERY=====
BenefitRacun_2405
-----SQL-----
SELECT Racun.IDRacun, Racun.lnkGR, BenefitKupacYYMM.KupacID, BenefitKupacYYMM.MesecYYMM
FROM Racun RIGHT JOIN BenefitKupacYYMM ON Racun.ID_K = BenefitKupacYYMM.KupacID
WHERE (((Racun.lnkGR)=55 Or (Racun.lnkGR)=56 Or (Racun.lnkGR)=57 Or (Racun.lnkGR) Is Null) AND ((BenefitKupacYYMM.MesecYYMM)="2405"));


=====QUERY=====
BenefitRacun_2406
-----SQL-----
SELECT Racun.IDRacun, Racun.lnkGR, BenefitKupacYYMM.KupacID, BenefitKupacYYMM.MesecYYMM
FROM Racun RIGHT JOIN BenefitKupacYYMM ON Racun.ID_K = BenefitKupacYYMM.KupacID
WHERE (((Racun.lnkGR)=58 Or (Racun.lnkGR)=59 Or (Racun.lnkGR)=60 Or (Racun.lnkGR) Is Null) AND ((BenefitKupacYYMM.MesecYYMM)="2406"));


=====QUERY=====
BenefitRacun_2407
-----SQL-----
SELECT Racun.IDRacun, Racun.lnkGR, BenefitKupacYYMM.KupacID, BenefitKupacYYMM.MesecYYMM
FROM Racun RIGHT JOIN BenefitKupacYYMM ON Racun.ID_K = BenefitKupacYYMM.KupacID
WHERE (((Racun.lnkGR)=65 Or (Racun.lnkGR)=66 Or (Racun.lnkGR)=67 Or (Racun.lnkGR) Is Null) AND ((BenefitKupacYYMM.MesecYYMM)="2407"));


=====QUERY=====
BenefitRacun_2408
-----SQL-----
SELECT Racun.IDRacun, Racun.lnkGR, BenefitKupacYYMM.KupacID, BenefitKupacYYMM.MesecYYMM
FROM Racun RIGHT JOIN BenefitKupacYYMM ON Racun.ID_K = BenefitKupacYYMM.KupacID
WHERE (((Racun.lnkGR)=73 Or (Racun.lnkGR)=74 Or (Racun.lnkGR)=75 Or (Racun.lnkGR) Is Null) AND ((BenefitKupacYYMM.MesecYYMM)="2404"));


=====QUERY=====
BenefitRacunStavkaArhive_2402_Append
-----SQL-----
INSERT INTO RacunStavkeBenefitArhiva ( IDRacunStavke, ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, Suma, PDVStopa, PDVIznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5, JM, ID_O )
SELECT RacunStavke.IDRacunStavke, RacunStavke.ID_R, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5, RacunStavke.JM, RacunStavke.ID_O
FROM RacunStavke INNER JOIN BenefitRacunStavkeArhive_2402 ON RacunStavke.IDRacunStavke = BenefitRacunStavkeArhive_2402.IDRacunStavke;


=====QUERY=====
BenefitRacunStavkaArhive_2403_Append
-----SQL-----
INSERT INTO RacunStavkeBenefitArhiva ( IDRacunStavke, ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, Suma, PDVStopa, PDVIznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5, JM, ID_O )
SELECT RacunStavke.IDRacunStavke, RacunStavke.ID_R, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5, RacunStavke.JM, RacunStavke.ID_O
FROM RacunStavke INNER JOIN BenefitRacunStavkeArhive_2403 ON RacunStavke.IDRacunStavke = BenefitRacunStavkeArhive_2403.IDRacunStavke;


=====QUERY=====
BenefitRacunStavkaArhive_2404_Append
-----SQL-----
INSERT INTO RacunStavkeBenefitArhiva ( IDRacunStavke, ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, Suma, PDVStopa, PDVIznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5, JM, ID_O )
SELECT RacunStavke.IDRacunStavke, RacunStavke.ID_R, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5, RacunStavke.JM, RacunStavke.ID_O
FROM RacunStavke INNER JOIN BenefitRacunStavkeArhive_2404 ON RacunStavke.IDRacunStavke = BenefitRacunStavkeArhive_2404.IDRacunStavke;


=====QUERY=====
BenefitRacunStavkaArhive_2405_Append
-----SQL-----
INSERT INTO RacunStavkeBenefitArhiva ( IDRacunStavke, ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, Suma, PDVStopa, PDVIznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5, JM, ID_O )
SELECT RacunStavke.IDRacunStavke, RacunStavke.ID_R, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5, RacunStavke.JM, RacunStavke.ID_O
FROM RacunStavke INNER JOIN BenefitRacunStavkeArhive_2405 ON RacunStavke.IDRacunStavke = BenefitRacunStavkeArhive_2405.IDRacunStavke;


=====QUERY=====
BenefitRacunStavkaArhive_2406_Append
-----SQL-----
INSERT INTO RacunStavkeBenefitArhiva ( IDRacunStavke, ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, Suma, PDVStopa, PDVIznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5, JM, ID_O )
SELECT RacunStavke.IDRacunStavke, RacunStavke.ID_R, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5, RacunStavke.JM, RacunStavke.ID_O
FROM RacunStavke INNER JOIN BenefitRacunStavkeArhive_2406 ON RacunStavke.IDRacunStavke = BenefitRacunStavkeArhive_2406.IDRacunStavke;


=====QUERY=====
BenefitRacunStavkaArhive_2407_Append
-----SQL-----
INSERT INTO RacunStavkeBenefitArhiva ( IDRacunStavke, ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, Suma, PDVStopa, PDVIznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5, JM, ID_O )
SELECT RacunStavke.IDRacunStavke, RacunStavke.ID_R, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5, RacunStavke.JM, RacunStavke.ID_O
FROM RacunStavke INNER JOIN BenefitRacunStavkeArhive_2407 ON RacunStavke.IDRacunStavke = BenefitRacunStavkeArhive_2407.IDRacunStavke;


=====QUERY=====
BenefitRacunStavkaArhive_2408_Append
-----SQL-----
INSERT INTO RacunStavkeBenefitArhiva ( IDRacunStavke, ID_R, lnkGR, ID_K, ID_SK, ID_RDOB, Naziv, TipObracuna, K1, K2, K3, K4, K5, Kolicina, CenaE, NBS, Iznos, Suma, PDVStopa, PDVIznos, UkupnoRSD, Sort, DobavljacKonto, IZNOSRACUNA, K1xK2, K2xK3, K2xK4, K2xK5, JM, ID_O )
SELECT RacunStavke.IDRacunStavke, RacunStavke.ID_R, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, RacunStavke.ID_RDOB, RacunStavke.Naziv, RacunStavke.TipObracuna, RacunStavke.K1, RacunStavke.K2, RacunStavke.K3, RacunStavke.K4, RacunStavke.K5, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.Sort, RacunStavke.DobavljacKonto, RacunStavke.IZNOSRACUNA, RacunStavke.K1xK2, RacunStavke.K2xK3, RacunStavke.K2xK4, RacunStavke.K2xK5, RacunStavke.JM, RacunStavke.ID_O
FROM RacunStavke INNER JOIN BenefitRacunStavkeArhive_2408 ON RacunStavke.IDRacunStavke = BenefitRacunStavkeArhive_2408.IDRacunStavke;


=====QUERY=====
BenefitRacunStavkeArhive_2402
-----SQL-----
SELECT RacunStavke.IDRacunStavke, BenefitRacun_2402.IDRacun, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.DobavljacKonto, RacunStavke.Kolicina, RacunStavke.ID_RDOB
FROM RacunStavke RIGHT JOIN BenefitRacun_2402 ON RacunStavke.ID_R = BenefitRacun_2402.IDRacun
WHERE (((RacunStavke.DobavljacKonto)=9001));


=====QUERY=====
BenefitRacunStavkeArhive_2403
-----SQL-----
SELECT;


=====QUERY=====
BenefitRacunStavkeArhive_2404
-----SQL-----
SELECT;


=====QUERY=====
BenefitRacunStavkeArhive_2405
-----SQL-----
SELECT;


=====QUERY=====
BenefitRacunStavkeArhive_2406
-----SQL-----
SELECT RacunStavke.IDRacunStavke, BenefitRacun_2406.IDRacun, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.DobavljacKonto, RacunStavke.Kolicina, RacunStavke.ID_RDOB
FROM RacunStavke RIGHT JOIN BenefitRacun_2406 ON RacunStavke.ID_R = BenefitRacun_2406.IDRacun
WHERE (((RacunStavke.DobavljacKonto)=9001));


=====QUERY=====
BenefitRacunStavkeArhive_2407
-----SQL-----
SELECT RacunStavke.IDRacunStavke, BenefitRacun_2407.IDRacun, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.DobavljacKonto, RacunStavke.Kolicina, RacunStavke.ID_RDOB
FROM RacunStavke RIGHT JOIN BenefitRacun_2407 ON RacunStavke.ID_R = BenefitRacun_2407.IDRacun
WHERE (((RacunStavke.DobavljacKonto)=9001));


=====QUERY=====
BenefitRacunStavkeArhive_2408
-----SQL-----
SELECT RacunStavke.IDRacunStavke, BenefitRacun_2408.IDRacun, RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.DobavljacKonto, RacunStavke.Kolicina, RacunStavke.ID_RDOB
FROM RacunStavke RIGHT JOIN BenefitRacun_2408 ON RacunStavke.ID_R = BenefitRacun_2408.IDRacun
WHERE (((RacunStavke.DobavljacKonto)=9001) AND ((RacunStavke.Naziv)="Korekcija cene održavanja rata 2-6 / Maintenance price correction installments 2-6"));


=====QUERY=====
BenefitRacunStavkeArhive_2408_update
-----SQL-----
UPDATE RacunStavke RIGHT JOIN BenefitRacun_2408 ON RacunStavke.ID_R = BenefitRacun_2408.IDRacun SET RacunStavke.UkupnoRSD = 0
WHERE (((RacunStavke.DobavljacKonto)=9001) AND ((RacunStavke.Naziv)="Korekcija cene održavanja rata 2-6 / Maintenance price correction installments 2-6"));


=====QUERY=====
BenefitUpdateRacun
-----SQL-----
SELECT Racun.IDRacun, GrupaRacuna.IDGrupaRacuna, BenefitKupacYYMM.KupacID, "Benefit FM " & First([SumOfUsed])+1 & "/" & First([CountOfUsed]) & " - " & GetBenefitText([BenefitKupacYYMM].[KupacID]) & Chr(13) & Chr(10) & "Na racunu je primenjen popust na uslugu održavanja stanova i garažnih mesta." & Chr(13) & Chr(10) & "A discount has been applied to the service of maintenance of apartments and garage spaces." AS TXT
FROM Racun INNER JOIN (BenefitCount INNER JOIN (BenefitKupacYYMM INNER JOIN GrupaRacuna ON BenefitKupacYYMM.MesecYYMM = GrupaRacuna.GrupaRacunaFXN) ON BenefitCount.KupacID = BenefitKupacYYMM.KupacID) ON (Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) AND (Racun.ID_K = BenefitKupacYYMM.KupacID)
GROUP BY Racun.IDRacun, GrupaRacuna.IDGrupaRacuna, BenefitKupacYYMM.KupacID;


=====QUERY=====
Copy Of Query308
-----SQL-----
SELECT GrupaRacuna.Mesec, GrupaRacuna.Godina, GrupaRacuna.ID_SK, Objekti.lnk_tip
FROM ((Racun INNER JOIN RacunObjekti ON Racun.IDRacun = RacunObjekti.IDRacun) INNER JOIN Objekti ON RacunObjekti.IDObjekat = Objekti.ID_O) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GrupaRacuna.Mesec)="02") AND ((GrupaRacuna.Godina)="2026") AND ((GrupaRacuna.ID_SK)=231) AND ((Objekti.lnk_tip)=4));


=====QUERY=====
Copy Of Query85
-----SQL-----
SELECT Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.KontoKnjizenja, Sum(GK_FILTER_DOBAVLJACI.SumOfDIZNOS) AS SumOfSumOfDIZNOS, Sum(GK_FILTER_DOBAVLJACI.SumOfPIZNOS) AS SumOfSumOfPIZNOS, GK_FILTER_DOBAVLJACI.KontoTroska
FROM Dobavljac_Racuni INNER JOIN GK_FILTER_DOBAVLJACI ON (Dobavljac_Racuni.IDTRRAC = GK_FILTER_DOBAVLJACI.RDOB) AND (Dobavljac_Racuni.SK_ID = GK_FILTER_DOBAVLJACI.lnkSkupstinaID)
GROUP BY Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.KontoKnjizenja, GK_FILTER_DOBAVLJACI.KontoTroska
HAVING (((Dobavljac_Racuni.SK_ID)=101));


=====QUERY=====
dell_KILLSPCSTAVKE
-----SQL-----
DELETE RacunStavke.*, RacunStavke.lnkGR, Racun.SPC, RacunStavke.lnkTVP
FROM RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun
WHERE (((RacunStavke.lnkGR)=107) AND ((Racun.SPC)=1) AND ((RacunStavke.lnkTVP)=17 Or (RacunStavke.lnkTVP)=18 Or (RacunStavke.lnkTVP)=19 Or (RacunStavke.lnkTVP)=20 Or (RacunStavke.lnkTVP)=52 Or (RacunStavke.lnkTVP)=24 Or (RacunStavke.lnkTVP)=25 Or (RacunStavke.lnkTVP)=26 Or (RacunStavke.lnkTVP)=27 Or (RacunStavke.lnkTVP)=53 Or (RacunStavke.lnkTVP)=38 Or (RacunStavke.lnkTVP)=39 Or (RacunStavke.lnkTVP)=40 Or (RacunStavke.lnkTVP)=41 Or (RacunStavke.lnkTVP)=54));


=====QUERY=====
Dobavljaci_Racun_List
-----SQL-----
SELECT Skustina.NazivSS, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, Kupac.ID_K, Kupac.Naziv, Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.IznosPoKoefRSD, Dobavljac_Racuni.MarkerVandrednogRacuna AS MVR, Dobavljac_Racuni.PrioritetNaplate, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.KontoKnjizenja, Dobavljac_Racuni.TipDokumenta, Troskovi_PodKonta.Naziv, TipRacunaDobavljac.Caption
FROM (((Dobavljac_Racuni INNER JOIN Skustina ON Dobavljac_Racuni.SK_ID = Skustina.IDSkupstina) INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN Troskovi_PodKonta ON Dobavljac_Racuni.KontoKnjizenja = Troskovi_PodKonta.PodKonto) INNER JOIN TipRacunaDobavljac ON Dobavljac_Racuni.TipDokumenta = TipRacunaDobavljac.Index
ORDER BY Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.PrioritetNaplate;


=====QUERY=====
DOBAVLJACI_RACUNI_SORT
-----SQL-----
SELECT Dobavljac_Racuni.*
FROM Dobavljac_Racuni
ORDER BY Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.RacunNO;


=====QUERY=====
DPO_FIX_TS1_K2040
-----SQL-----
UPDATE GK SET GK.DPO = [DATUM]
WHERE (((GK.DPO) Is Null) AND ((GK.TIP_STAVKE)=1));


=====QUERY=====
eMail_Partner
-----SQL-----
SELECT Mail.IDPartner
FROM Mail
GROUP BY Mail.IDPartner;


=====QUERY=====
eMail_Partner_list
-----SQL-----
SELECT Mail.IDPartner, Kupac.lnk_ID_SK, Kupac.ID_K
FROM Kupac INNER JOIN Mail ON Kupac.ID_K = Mail.IDPartner
GROUP BY Mail.IDPartner, Kupac.lnk_ID_SK, Kupac.ID_K;


=====QUERY=====
EmailoviPlot24
-----SQL-----
SELECT PLOT24EMAL.Unit, PLOT24EMAL.Company, Kupac.ID_K, PLOT24EMAL.PersonalEmail, PLOT24EMAL.FullName, Kupac.Naziv, Kupac.PIB, Kupac.MB, Kupac.PIB, Kupac.MB, Kupac.Adresa, Kupac.IDMaster, Objekti.ID_O
FROM (Objekti RIGHT JOIN PLOT24EMAL ON Objekti.SifraPD = PLOT24EMAL.Unit) LEFT JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
WHERE (((Objekti.ID_O) Is Null));


=====QUERY=====
ERROR_001-SUMA-STAVKE-IZVODA-NIJE-SUMA-GK
-----SQL-----
SELECT IzvodStavke.ID, [DatumRealizacije] & " / " & [ID_SK] & " / " & [NazivSS] & " / " & [NazivPN] AS Expr1, IzvodStavke.ID_SK, IzvodStavke.DatumRealizacije, Skustina.NazivSS, IzvodStavke.NazivPN, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, ([Odobrenje]-[Zaduzenje])-(Sum([PIZNOS]-[DIZNOS]))=0 AS Kontrola
FROM (GK RIGHT JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) INNER JOIN Skustina ON IzvodStavke.ID_SK = Skustina.IDSkupstina
GROUP BY IzvodStavke.ID, [DatumRealizacije] & " / " & [ID_SK] & " / " & [NazivSS] & " / " & [NazivPN], IzvodStavke.ID_SK, IzvodStavke.DatumRealizacije, Skustina.NazivSS, IzvodStavke.NazivPN, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, IzvodStavke.RbStavke, GK.lnkIzvodStavkaID, IzvodStavke.IzvodLNKID
HAVING (((([Odobrenje]-[Zaduzenje])-(Sum([PIZNOS]-[DIZNOS]))=0)=0))
ORDER BY IzvodStavke.RbStavke DESC;


=====QUERY=====
ERROR_002-STAVKE U GK SA POGRESNIM SKUPSTINAMA KUPCA
-----SQL-----
SELECT Kupac.ID_K, "ID-K: " & [ID_K] & " / " & [Skustina_1].[NazivSS] & " / GK SZ:" & [lnkSkupstinaID] & " / " & [Skustina].[NazivSS] AS Expr1, Skustina_1.NazivSS AS [Skupstina kupca], GK.DATUM, GK.PIZNOS, GK.DIZNOS, Kupac.Naziv, Skustina.NazivSS AS [Skupstina izvoda], GK.lnkIzvodStavkaID
FROM ((GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Skustina AS Skustina_1 ON Kupac.lnk_ID_SK = Skustina_1.IDSkupstina
WHERE ((([lnkSkupstinaID]=[lnk_ID_SK])=0) AND ((Kupac.lnk_ID_SK)<>0));


=====QUERY=====
ERROR_003-STAVKE IZVODA NA CEKANJU
-----SQL-----
SELECT IzvodStavke.ID, [DatumRealizacije] & " /  RB: " & [RbStavke] & ". /  SZ: " & [ID_SK] AS Expr1, IzvodStavke.IzvodLNKID, IzvodStavke.ID_SK, IzvodStavke.RbStavke, IzvodStavke.DatumRealizacije, IzvodStavke.RbNaloga, IzvodStavke.NazivPN, IzvodStavke.BrojRacuna, IzvodStavke.Poreklo, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, IzvodStavke.Doznaka, IzvodStavke.Sifra, IzvodStavke.PozivNaBroj, IzvodStavke.PozivNaBrojO, IzvodStavke.opt_lnk_Kupac, IzvodStavke.Knjizeno, IzvodStavke.Ignore, IzvodStavke.SetP
FROM IzvodStavke LEFT JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID
WHERE (((GK.lnkIzvodStavkaID) Is Null))
ORDER BY IzvodStavke.RbStavke, IzvodStavke.DatumRealizacije DESC;


=====QUERY=====
ERROR_004-NEPOSTOJI-KUPAC-PREMA-GK
-----SQL-----
SELECT GK.*, Skustina.NazivSS, Skustina.IDSkupstina
FROM (((GK LEFT JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID) INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina
WHERE (((Kupac.ID_K) Is Null));


=====QUERY=====
ERROR_005-KUPAC-BEZ-NEKRETNINE
-----SQL-----
SELECT Kupac.ID_K, [ID_K] & " / " & [Kupac].[Naziv] & " /  SZ: " & [lnk_ID_SK] & " / " & [NazivSS] AS Expr1, Kupac.Naziv, Skustina.NazivSS, Kupac.lnk_ID_SK, Skustina.NazivSS
FROM (Kupac LEFT JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina
WHERE (((Kupac.ID_K)<8000) AND ((Objekti.lnk_ID_K) Is Null));


=====QUERY=====
ERROR_006-IZVODISTAVKE-BEZ-IZVODA
-----SQL-----
SELECT IzvodStavke.ID, [DatumRealizacije] & " /  RB: " & [RbStavke] & ". /  SZ: " & [IzvodStavke].[ID_SK] AS Expr1, IzvodStavke.IzvodLNKID, IzvodStavke.ID_SK, IzvodStavke.RbStavke, IzvodStavke.RbNaloga, IzvodStavke.NazivPN, IzvodStavke.BrojRacuna, IzvodStavke.Poreklo, IzvodStavke.DatumRealizacije, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, IzvodStavke.Doznaka, IzvodStavke.Sifra, IzvodStavke.PozivNaBroj, IzvodStavke.PozivNaBrojO, IzvodStavke.opt_lnk_Kupac, IzvodStavke.Knjizeno, IzvodStavke.Ignore, IzvodStavke.SetP
FROM IzvodStavke LEFT JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID
WHERE (((Izvod.IzvodID) Is Null));


=====QUERY=====
ERROR_007-IZVODI-KOJI-IMAJU-R-A-NISU-R
-----SQL-----
SELECT IzvodStavke.ID, [Izvod].[Datum] & " / " & [Izvod].[ID_SK] & " / " & [NazivSS] AS Expr1, Izvod.IzvodID, Izvod.BrojIzvoda, Izvod.SufixIzvoda, Izvod.Datum, Izvod.ID_SK, Skustina.NazivSS, Izvod.Rasknjizen, Izvod.NalogZaKnjizenje
FROM ((IzvodStavke LEFT JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID) INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID) INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina
WHERE (((Izvod.Rasknjizen)=True) AND ((GK.lnkIzvodStavkaID) Is Null))
ORDER BY Izvod.Datum DESC;


=====QUERY=====
ERROR_008-IZVODI-SUMASTAVKI-KONTROLA
-----SQL-----
SELECT Izvod.BrojIzvoda, Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.Duguje, Izvod.Potrazuje, Sum(IzvodStavke.Zaduzenje) AS SumOfZaduzenje, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje, (Sum(IzvodStavke.Zaduzenje)=Izvod.Duguje) And (Sum(IzvodStavke.Odobrenje)=Izvod.Potrazuje) And (Count(IzvodStavke.ID)=(Izvod.NalogaZaduzenja+Izvod.NalogaOdobranja)) AS Ispravno, Count(IzvodStavke.ID) AS BrojStavki, (Izvod.NalogaZaduzenja+Izvod.NalogaOdobranja) AS KontrolniBrojStavki
FROM (Izvod LEFT JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID) INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina
GROUP BY Izvod.BrojIzvoda, Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.Duguje, Izvod.Potrazuje, (Izvod.NalogaZaduzenja+Izvod.NalogaOdobranja)
HAVING ((((Sum([IzvodStavke].[Zaduzenje])=[Izvod].[Duguje]) And (Sum([IzvodStavke].[Odobrenje])=[Izvod].[Potrazuje]) And (Count([IzvodStavke].[ID])=([Izvod].[NalogaZaduzenja]+[Izvod].[NalogaOdobranja])))=0));


=====QUERY=====
ERROR_009_IZVOD-NEPOSTOJECA-SZ
-----SQL-----
SELECT Izvod.IzvodID, "IZVOD ID: " & [IzvodID] & " / SZ : " & [ID_SK] & " / DATUM : " & [DATUM] AS Expr1
FROM Izvod LEFT JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina
WHERE (((Skustina.IDSkupstina) Is Null));


=====QUERY=====
ERROR_009-IZVOD-STAVKE-U-GK-POGRESNA-SZ-KORISNIKA
-----SQL-----
SELECT Skustina_1.NazivSS AS [Skupstina kupca], GK.DATUM, GK.PIZNOS, GK.DIZNOS, Kupac.ID_K, Kupac.Naziv, Skustina.NazivSS AS [Skupstina izvoda], GK.lnkIzvodStavkaID
FROM ((GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Skustina AS Skustina_1 ON Kupac.lnk_ID_SK = Skustina_1.IDSkupstina
WHERE ((([lnkSkupstinaID]=[lnk_ID_SK])=0) AND ((Kupac.lnk_ID_SK)<>0));


=====QUERY=====
ERROR_010-STAVKE-GK-KORISNIKA-BEZ-SK
-----SQL-----
SELECT Kupac.ID_K, [ID_K] & " / " & [Naziv] & " / " & "GK: " & [Datum] & " / " & [BR_NALOG] AS Expr1, Kupac.Naziv, GK.PIZNOS, GK.DIZNOS, GK.DATUM, GK.BR_NALOG, GK.BR_NALOG
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.lnkSkupstinaID)=0));


=====QUERY=====
ERROR_011-IZVOD-STAVKE-POGRESNA-SZ-U-GK
-----SQL-----
SELECT IzvodStavke.ID, [DatumRealizacije] & " / " & [ID_SK] & " / " & [NazivSS] AS Expr2
FROM Skustina INNER JOIN (GK INNER JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) ON Skustina.IDSkupstina = IzvodStavke.ID_SK
WHERE ((([lnkSkupstinaID]=[ID_SK])=0));


=====QUERY=====
ERROR_012-IZVOD-IZVODSTAVKE-POGRESNA-SZ
-----SQL-----
SELECT Izvod.IzvodID, Izvod.Datum, IzvodStavke.DatumRealizacije, IzvodStavke.ID_SK, Izvod.ID_SK, IzvodStavke.opt_lnk_Kupac
FROM IzvodStavke INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID
WHERE ((([Izvod].[ID_SK]=[IzvodStavke].[ID_SK])=0));


=====QUERY=====
ERROR_013-IZVODSTAVKE-GK-RAZLICIT-ID-K
-----SQL-----
SELECT IzvodStavke.ID, IzvodStavke.NazivPN, IzvodStavke.opt_lnk_Kupac, GK.lnkKUPACID, Kupac.Naziv, GK.lnkSkupstinaID, IzvodStavke.ID_SK, Kupac.lnk_ID_SK
FROM Kupac INNER JOIN (GK INNER JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) ON Kupac.ID_K = GK.lnkKUPACID
WHERE ((([lnkKUPACID]=[opt_lnk_Kupac])=0));


=====QUERY=====
ERROR_014-IZVOD-NALOG-SUMA-NIJE-NULA
-----SQL-----
SELECT Izvod.IzvodID, [ID_SK] & " - " & [BrojIzvoda] & " - " & [Izvod].[Datum] AS Expr1, Izvod.ID_SK, Izvod.BrojIzvoda, Izvod.Datum, Round(Sum(Round([DIZNOS],4)-Round([PIZNOS],4)),2) AS Suma
FROM (Izvod INNER JOIN Nalog ON Izvod.NalogZaKnjizenje = Nalog.Br_Nalog) INNER JOIN GK ON Nalog.Br_Nalog = GK.BR_NALOG
WHERE (((Izvod.Rasknjizen)=True))
GROUP BY Izvod.IzvodID, [ID_SK] & " - " & [BrojIzvoda] & " - " & [Izvod].[Datum], Izvod.ID_SK, Izvod.BrojIzvoda, Izvod.Datum
HAVING (((Round(Sum(Round([DIZNOS],4)-Round([PIZNOS],4)),2))<>0));


=====QUERY=====
ERROR_014-IZVOD-UPOREDO_2410
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkSkupstinaID, GK.BR_NALOG, GK.DATUM, Sum([DIZNOS]-[PIZNOS]) AS DIPI, Izvod.Duguje, Izvod.Potrazuje, Izvod.BrojIzvoda, Izvod.SufixIzvoda, Izvod.PrethodnoStanjeIzvoda, Izvod.NovoStanje
FROM GK LEFT JOIN Izvod ON GK.BR_NALOG = Izvod.NalogZaKnjizenje
WHERE (((Izvod.Napomena) Is Null Or (Izvod.Napomena)=""))
GROUP BY GK.KONTO, GK.lnkSkupstinaID, GK.BR_NALOG, GK.DATUM, Izvod.Datum, Izvod.Duguje, Izvod.Potrazuje, Izvod.BrojIzvoda, Izvod.SufixIzvoda, Izvod.PrethodnoStanjeIzvoda, Izvod.NovoStanje
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
ERROR_015_GK_POVEZIVANJE_SUBKONTO_RAZLIKA
-----SQL-----
SELECT GK.lnkKUPACID, [lnkKUPACID] & " - " & [DOK] AS info, Sum(GK.PIZNOS) AS PI, Sum(GK.DIZNOS) AS DI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, GK.DOK, GK.RDOB, GK.lnkSkupstinaID, GK.KontoTroska
FROM GK
WHERE (((GK.KONTO)='2040'))
GROUP BY GK.lnkKUPACID, [lnkKUPACID] & " - " & [DOK], GK.DOK, GK.RDOB, GK.lnkSkupstinaID, GK.KontoTroska
HAVING (((Sum(GK.PIZNOS))<>0) AND ((Sum(GK.DIZNOS))<>0) AND ((Round(Sum([DIZNOS]-[PIZNOS]),2))<0))
ORDER BY GK.lnkKUPACID, GK.DOK;


=====QUERY=====
ERROR_016_GK_204X_DPO_DATUM_NEMA
-----SQL-----
SELECT GK.lnkKUPACID, GK.lnkKUPACID AS info, GK.STAVKAID, GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.DPO, GK.DIZNOS, GK.PIZNOS, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID
FROM GK
WHERE (((GK.KONTO) Like "204*") AND ((GK.DATUM) Is Null)) OR (((GK.KONTO) Like "204*") AND ((GK.DPO) Is Null));


=====QUERY=====
ERROR_016_GK_204X_DPO_DATUM_NEMA_GROUP
-----SQL-----
SELECT ERROR_016_GK_204X_DPO_DATUM_NEMA.lnkKUPACID, [ERROR_016_GK_204X_DPO_DATUM_NEMA].[lnkKUPACID] & " - " & [DIPI] AS INFO, ERROR_016_GK_204X_DPO_DATUM_NEMA.lnkSkupstinaID, Count(ERROR_016_GK_204X_DPO_DATUM_NEMA.lnkKUPACID) AS CountOflnkKUPACID
FROM ERROR_016_GK_204X_DPO_DATUM_NEMA INNER JOIN GK_PARTNERI ON ERROR_016_GK_204X_DPO_DATUM_NEMA.lnkKUPACID = GK_PARTNERI.lnkKUPACID
WHERE ((([DIPI]>0)=-1))
GROUP BY [ERROR_016_GK_204X_DPO_DATUM_NEMA].[lnkKUPACID] & " - " & [DIPI], ERROR_016_GK_204X_DPO_DATUM_NEMA.lnkSkupstinaID, ERROR_016_GK_204X_DPO_DATUM_NEMA.lnkKUPACID;


=====QUERY=====
ERROR_017_GK_204X_POVEZIVANJE_SUBKONTA
-----SQL-----
SELECT Q.lnkKUPACID, [lnk_ID_SK] & "-" &  [ID_K] & " - " & [Naziv] AS IDK
FROM (SELECT GK.lnkKUPACID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, Round(Sum([PIZNOS]),2) AS SP, Round(Sum([DIZNOS]),2) AS SD, Count(GK.STAVKAID) AS BrojStavki
FROM GK
WHERE (((GK.KONTO) Like '204*'))
GROUP BY GK.lnkKUPACID, GK.DOK, GK.RDOB, GK.lnkSkupstinaID
HAVING (((Round(Sum([DIZNOS]-[PIZNOS]),2))<>0) AND ((Round(Sum([DIZNOS]),2))>-0.01))
)  AS Q INNER JOIN Kupac ON Q.lnkKUPACID = Kupac.ID_K
GROUP BY Q.lnkKUPACID, [lnk_ID_SK] & "-" & [ID_K] & " - " & [Naziv]
HAVING (((Sum(IIf([Q].[SPD]>0,1,0)))>0) AND ((Sum(IIf([Q].[SPD]<0,1,0)))>0));


=====QUERY=====
ERROR_018_GK_PretplateIzvodi
-----SQL-----
SELECT GK.BR_NALOG, GK.BR_NALOG & '-' & GK.lnkKUPACID AS Info, GK.STAVKAID, GK.lnkSkupstinaID, GK.KONTO, GK.DATUM, GK.DIZNOS, GK.PIZNOS, GK.TIP_STAVKE, GK.lnkKUPACID, Izvod.BrojIzvoda, Izvod.SufixIzvoda, GK.lnkIzvodStavkaID, Izvod.NalogZaKnjizenje, [BR_NALOG]=[NalogZaKnjizenje] AS Expr1, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.IDTRRAC
FROM ((GK INNER JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.PIZNOS)<0) AND ((GK.TIP_STAVKE)=1) AND ((GK.lnkIzvodStavkaID)>0) AND ((Dobavljac_Racuni.IznosRacunaKN)>0 Or (Dobavljac_Racuni.IznosRacunaKN) Is Null))
ORDER BY GK.DATUM DESC;


=====QUERY=====
ERROR_019-IZVOD-2410-ODOBRENJE-ZADUZENJE
-----SQL-----
SELECT IzvodSumaZO.RbNaloga, [RbNaloga] & " - " & [ID_SK] AS Info, IzvodSumaZO.ID_SK, IzvodSumaZO.SumOfOdobrenje, IzvodSumaZO.SumOfZaduzenje, GK_2410_Sum.SumOfDIZNOS, GK_2410_Sum.SumOfPIZNOS, [SumOfOdobrenje]-[SumOfDIZNOS] AS DI, [SumOfZaduzenje]-[SumOfPIZNOS] AS PI
FROM IzvodSumaZO LEFT JOIN GK_2410_Sum ON IzvodSumaZO.RbNaloga = GK_2410_Sum.BR_NALOG
WHERE ((([SumOfOdobrenje]-[SumOfDIZNOS])<>0)) OR ((([SumOfZaduzenje]-[SumOfPIZNOS])<>0));


=====QUERY=====
ERROR_020_OBJEKAT-SZ-KORISNIK-SZ
-----SQL-----
SELECT Kupac.ID_K, [ID_K] & " / " & [Kupac].[Naziv] & "  -  " & [lnk_ID_SK] & " / " & [Skustina_1].[NazivSS] & "  -  " & [Objekti].[naziv] & " / " & [lnkSkupstinaID] & " / " & [Skustina].[NazivSS] AS Expr1, Kupac.Naziv, Skustina_1.NazivSS, Objekti.naziv, Objekti.lnkSkupstinaID, Skustina.NazivSS
FROM ((Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Skustina AS Skustina_1 ON Kupac.lnk_ID_SK = Skustina_1.IDSkupstina
WHERE ((([lnkSkupstinaID]=[lnk_ID_SK])=0));


=====QUERY=====
ERROR_021_RACUNI-DATUMPROMETA-DATUMVALUTE-RACUNA
-----SQL-----
SELECT GK.KONTO, GK.DATUM, GK.DPO, GrupaRacuna.DatumValute, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumPrometa, GK.lnkSkupstinaID, [DATUM]<>[DATUMPROMETA] AS DP
FROM GrupaRacuna INNER JOIN GK ON GrupaRacuna.NalogKN = GK.BR_NALOG
WHERE ((([DATUM]<>[DATUMPROMETA])=-1));


=====QUERY=====
ERROR_021_RACUNI-DATUMPROMETA-DATUMVALUTE-RACUNA-FIX
-----SQL-----
UPDATE [ERROR_021_RACUNI-DATUMPROMETA-DATUMVALUTE-RACUNA] SET [ERROR_021_RACUNI-DATUMPROMETA-DATUMVALUTE-RACUNA].DATUM = [DatumPrometa];


=====QUERY=====
ERROR_022_MAILSEND_DONTHAVE_SENDDATE
-----SQL-----
SELECT Mail_Send.*
FROM Mail_Send
WHERE (((Mail_Send.DateSend) Is Null) AND ((Mail_Send.ErrorStatus) Is Null))
ORDER BY Mail_Send.IDMail DESC;


=====QUERY=====
ERROR_023_GK4350_KONTOTROSKANULL
-----SQL-----
SELECT GK.BR_NALOG, GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.DIZNOS, GK.PIZNOS, GK.lnkKUPACID
FROM GK
WHERE (((GK.KONTO)="4350") AND ((GK.KontoTroska) Is Null));


=====QUERY=====
ERROR_023_RACUN_STORO_PROKNJIZEN_PLACEN
-----SQL-----
SELECT Racun.IDRacun, GK.*, Racun.Storno
FROM GK LEFT JOIN Racun ON GK.RACID = Racun.IDRacun
WHERE (((GK.KONTO)="2040"));


=====QUERY=====
ERROR_024_RACUN_STORO_PROKNJIZEN_PLACEN
-----SQL-----
SELECT ERROR_023_RACUN_STORO_PROKNJIZEN_PLACEN.IDRacun, Round(Sum([DIZNOS]),2) AS DI, Round(Sum([PIZNOS]),2) AS PI
FROM ERROR_023_RACUN_STORO_PROKNJIZEN_PLACEN
WHERE (((ERROR_023_RACUN_STORO_PROKNJIZEN_PLACEN.Storno)=True)) OR (((ERROR_023_RACUN_STORO_PROKNJIZEN_PLACEN.Storno)=True))
GROUP BY ERROR_023_RACUN_STORO_PROKNJIZEN_PLACEN.IDRacun
HAVING (((Round(Sum([DIZNOS]),2))<>0)) OR (((Round(Sum([PIZNOS]),2))<>0));


=====QUERY=====
ERROR_027_GK_GRP_BY_NALOG
-----SQL-----
SELECT ERROR_027_GK_GRP_BY_NALOG_SUB.BR_NALOG, [ERROR_027_GK_GRP_BY_NALOG_SUB].[BR_NALOG] & " / " & [Napomena] AS Expr1, Count(ERROR_027_GK_GRP_BY_NALOG_SUB.lnkSkupstinaID) AS CountOflnkSkupstinaID, Sum(ERROR_027_GK_GRP_BY_NALOG_SUB.CountOfSTAVKAID) AS SumOfCountOfSTAVKAID, Nalog.Napomena
FROM ERROR_027_GK_GRP_BY_NALOG_SUB LEFT JOIN Nalog ON ERROR_027_GK_GRP_BY_NALOG_SUB.BR_NALOG = Nalog.Br_Nalog
GROUP BY ERROR_027_GK_GRP_BY_NALOG_SUB.BR_NALOG, [ERROR_027_GK_GRP_BY_NALOG_SUB].[BR_NALOG] & " / " & [Napomena], Nalog.Napomena
HAVING (((Count(ERROR_027_GK_GRP_BY_NALOG_SUB.lnkSkupstinaID))>1));


=====QUERY=====
ERROR_027_GK_GRP_BY_NALOG_SUB
-----SQL-----
SELECT GK.BR_NALOG, GK.lnkSkupstinaID, Count(GK.STAVKAID) AS CountOfSTAVKAID
FROM GK
GROUP BY GK.BR_NALOG, GK.lnkSkupstinaID;


=====QUERY=====
ERROR_030_RACUNDOB-BEZGK-NEMA-SZ-ILI-NEMA-DOB
-----SQL-----
SELECT Dobavljac_Racuni.*, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.DobavljacKonto
FROM Dobavljac_Racuni LEFT JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB
WHERE (((Dobavljac_Racuni.SK_ID) Is Null) AND ((GK.STAVKAID) Is Null)) OR (((Dobavljac_Racuni.DobavljacKonto) Is Null) AND ((GK.STAVKAID) Is Null));


=====QUERY=====
ERROR_031_RACUNDOB-BEZGK-OBRISANA-SZ
-----SQL-----
SELECT Dobavljac_Racuni.*, GK.STAVKAID, Skustina.IDSkupstina
FROM (Dobavljac_Racuni LEFT JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB) LEFT JOIN Skustina ON Dobavljac_Racuni.SK_ID = Skustina.IDSkupstina
WHERE (((GK.STAVKAID) Is Null) AND ((Skustina.IDSkupstina) Is Null));


=====QUERY=====
ERROR_032_RACUNDOB-NIJE_KNJIZEN-NEMA-IDDOB-U-DOBAVLJACIMA
-----SQL-----
SELECT Dobavljac_Racuni.*, GK.STAVKAID, Kupac.ID_K
FROM (Dobavljac_Racuni LEFT JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB) LEFT JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K
WHERE (((GK.STAVKAID) Is Null) AND ((Kupac.ID_K) Is Null));


=====QUERY=====
ERROR_033_RDOB_GRP
-----SQL-----
SELECT ERROR_033_SUB1_RDOB_GRP.IDTRRAC, ERROR_033_SUB1_RDOB_GRP.GrupaRacunaFXN, ERROR_033_SUB1_RDOB_GRP.ID_SK, ERROR_033_SUB1_RDOB_GRP.BR_NALOG, ERROR_033_SUB1_RDOB_GRP.DATUM, ERROR_033_SUB1_RDOB_GRP.lnkSkupstinaID, ERROR_033_SUB1_RDOB_GRP.SumOfDIZNOS, ERROR_033_SUB1_RDOB_GRP.SumOfPIZNOS, ERROR_033_SUB1_RDOB_GRP.KontoTroska, ERROR_033_SUB1_RDOB_GRP.DOK, ERROR_033_SUB1_RDOB_GRP.MesecRacuna, ERROR_033_SUB1_RDOB_GRP.Troskovi_PodKonta.Naziv, ERROR_033_SUB1_RDOB_GRP.NazivSS, ERROR_033_SUB1_RDOB_GRP.KontrolaSK, ERROR_033_SUB1_RDOB_GRP.BrojKnjizenja, ERROR_033_SUB1_RDOB_GRP.DobavljacKonto, ERROR_033_SUB1_RDOB_GRP.Kupac.Naziv, ERROR_033_SUB1_RDOB_GRP.IznosRacunaRSD, ERROR_033_SUB1_RDOB_GRP.NazivRacuna, ERROR_033_SUB1_RDOB_GRP.IznosRacunaKN
FROM ERROR_033_SUB1_RDOB_GRP
WHERE (((ERROR_033_SUB1_RDOB_GRP.IDTRRAC) In (SELECT [IDTRRAC] FROM [ERROR_033_SUB1_RDOB_GRP] As Tmp GROUP BY [IDTRRAC] HAVING Count(*)>1 )))
ORDER BY ERROR_033_SUB1_RDOB_GRP.IDTRRAC;


=====QUERY=====
ERROR_033_SUB1_RDOB_GRP
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.DATUM, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID] AS KontrolaSK, Count(GK.STAVKAID) AS BrojKnjizenja, Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.IDTRRAC
FROM ((((GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) LEFT JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN GrupaRacuna ON GK.BR_NALOG = GrupaRacuna.NalogKN
WHERE (((GK.KONTO)='2040' Or (GK.KONTO)='4350'))
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.DATUM, GK.lnkSkupstinaID, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID], Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.IDTRRAC
HAVING (((GrupaRacuna.ID_SK)=[IDSZ]))
ORDER BY GrupaRacuna.ID_SK, Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.IDTRRAC;


=====QUERY=====
ERROR_041_RACUNDOB-GK-OBRISANA-SZ
-----SQL-----
SELECT Dobavljac_Racuni.*, Skustina.IDSkupstina
FROM (Dobavljac_Racuni INNER JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB) LEFT JOIN Skustina ON Dobavljac_Racuni.SK_ID = Skustina.IDSkupstina
WHERE (((Skustina.IDSkupstina) Is Null));


=====QUERY=====
ERROR_042_RACUNDOB-GK-POGRRESAN-DOB
-----SQL-----
SELECT Dobavljac_Racuni.*, GK.STAVKAID, Kupac.ID_K
FROM (Dobavljac_Racuni INNER JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB) LEFT JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K
WHERE (((Kupac.ID_K) Is Null));


=====QUERY=====
ERROR_043_GK_SIFRAKONTA_LNKKUPAC
-----SQL-----
SELECT GK.*
FROM GK
WHERE ((([lnkKUPACID]=[SIFRAKONTA])=0));


=====QUERY=====
ERROR_060_RACUNI-BEZ-IDK-ILI-SZID-ILI-GRID
-----SQL-----
SELECT Racun.*, Racun.ID_K, Racun.ID_SK, Racun.lnkGR
FROM Racun
WHERE (((Racun.ID_K)=0)) OR (((Racun.ID_SK)=0)) OR (((Racun.lnkGR)=0));


=====QUERY=====
ERROR_062_RACUNI_RACUNISTAVKE_GRID_RAZLICITO
-----SQL-----
SELECT Racun.lnkGR, RacunStavke.lnkGR, [Racun].[lnkGR]=[RacunStavke].[lnkGR] AS Expr1
FROM RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun
WHERE ((([Racun].[lnkGR]=[RacunStavke].[lnkGR])=0));


=====QUERY=====
ERROR_063_RACUNI_RACUNISTAVKE_SZID_RAZLICITO
-----SQL-----
SELECT Racun.lnkGR, RacunStavke.lnkGR, [Racun].[ID_SK]=[RacunStavke].[ID_SK] AS Expr1, Racun.ID_SK, RacunStavke.ID_SK
FROM RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun
WHERE ((([Racun].[ID_SK]=[RacunStavke].[ID_SK])=0));


=====QUERY=====
ERROR_064_RACUNI_RACUNISTAVKE_IDK_RAZLICITO
-----SQL-----
SELECT Racun.ID_K, RacunStavke.ID_K, Racun.IDRacun, Racun.DatumIzdavanja, RacunStavke.ID_SK, Racun.ID_SK, RacunStavke.ID_RDOB
FROM RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun
GROUP BY Racun.ID_K, RacunStavke.ID_K, Racun.IDRacun, Racun.DatumIzdavanja, RacunStavke.ID_SK, Racun.ID_SK, RacunStavke.ID_RDOB, [Racun].[ID_K]=[RacunStavke].[ID_K]
HAVING ((([Racun].[ID_K]=[RacunStavke].[ID_K])=0));


=====QUERY=====
ERROR_065_RACUNI_GK_SZID_RAZLICITO
-----SQL-----
SELECT [lnkSkupstinaID]=[ID_SK] AS Expr1, Racun.IDRacun, Racun.RBR, Racun.DatumIzdavanja, Racun.ID_SK, GK.STAVKAID
FROM GK INNER JOIN Racun ON GK.RACID = Racun.IDRacun
WHERE ((([lnkSkupstinaID]=[ID_SK])=0));


=====QUERY=====
ERROR_066_RACUNI_GK_IDK_RAZLICITO
-----SQL-----
SELECT [lnkKUPACID]=[ID_K] AS Expr2, Racun.IDRacun, Racun.RBR, Racun.DatumIzdavanja, Racun.ID_SK, Racun.ID_K, GK.lnkKUPACID, GK.DIZNOS, GK.PIZNOS, GK.lnkIzvodStavkaID
FROM GK INNER JOIN Racun ON GK.RACID = Racun.IDRacun
WHERE ((([lnkKUPACID]=[ID_K])=0));


=====QUERY=====
ERROR_070_GR_NEMA_PODATAKA_ADELL
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna
FROM Racun RIGHT JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((Racun.IDRacun) Is Null));


=====QUERY=====
ERROR_070_GR_NEMA_PODATAKA_ADELL_EXECUTE
-----SQL-----
DELETE Racun.IDRacun, GrupaRacuna.*
FROM GrupaRacuna LEFT JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((Racun.IDRacun) Is Null));


=====QUERY=====
ERROR_071_RACUNI-RACUNISTAVKE-BEZ-RACUNA
-----SQL-----
SELECT RacunStavke.*
FROM Racun RIGHT JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R
WHERE (((Racun.IDRacun) Is Null));


=====QUERY=====
ERROR_071_RACUNI-RACUNISTAVKE-BEZ-RACUNA_EXECUTE
-----SQL-----
DELETE RacunStavke.*, Racun.IDRacun
FROM Racun RIGHT JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R
WHERE (((Racun.IDRacun) Is Null));


=====QUERY=====
ERROR_075_GK-RDOB-GRPSK
-----SQL-----
SELECT GK.RDOB, GK.lnkSkupstinaID
FROM GK
GROUP BY GK.RDOB, GK.lnkSkupstinaID
HAVING (((GK.RDOB)>0));


=====QUERY=====
ERROR_081_RACUNI_SUME
-----SQL-----
SELECT GrupaRacuna.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.MarkerVanderdnihRacuna, GrupaRacuna.NalogKN, Count(Racun.IDRacun) AS CountOfIDRacun, Sum(Racun.Ukupno) AS SumOfUkupno
FROM GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
GROUP BY GrupaRacuna.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.MarkerVanderdnihRacuna, GrupaRacuna.NalogKN
HAVING (((GrupaRacuna.ID_SK)=101));


=====QUERY=====
ERROR_082_GK_2040
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.RDOB, GK.lnkKUPACID
FROM GK
GROUP BY GK.KONTO, GK.RDOB, GK.lnkKUPACID
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
ERROR_082_GK_4350
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.RDOB, GK.SIFRAKONTA
FROM GK
GROUP BY GK.KONTO, GK.RDOB, GK.SIFRAKONTA
HAVING (((GK.KONTO)="4350"));


=====QUERY=====
ERROR_082_GK_4350_RDOB_KN
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.RDOB, GK.SIFRAKONTA, GK.lnkIzvodStavkaID, GK.lnkKUPACID
FROM GK
GROUP BY GK.KONTO, GK.RDOB, GK.SIFRAKONTA, GK.lnkIzvodStavkaID, GK.lnkKUPACID
HAVING (((GK.KONTO)="4350") AND ((GK.lnkIzvodStavkaID)=0));


=====QUERY=====
ERROR_082_GK_4900
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.RDOB
FROM GK
GROUP BY GK.KONTO, GK.RDOB
HAVING (((GK.KONTO)="4900"));


=====QUERY=====
ERROR_082_GK_5590
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.RDOB
FROM GK
GROUP BY GK.KONTO, GK.RDOB
HAVING (((GK.KONTO)="5590"));


=====QUERY=====
ERROR_083_GRRAC-STAVKE-DOBRAC
-----SQL-----
SELECT GrupaRacuna.DatumIzdavanja, GrupaRacuna.ID_SK, GrupaRacuna.NalogKN, RacunStavke.ID_SK, RacunStavke.Naziv, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.KontoKnjizenja, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.TipDokumenta, RacunStavke.DobavljacKonto, Dobavljac_Racuni.DobavljacKonto, Dobavljac_Racuni.SK_ID
FROM (GrupaRacuna INNER JOIN RacunStavke ON GrupaRacuna.IDGrupaRacuna = RacunStavke.lnkGR) LEFT JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC
GROUP BY GrupaRacuna.DatumIzdavanja, GrupaRacuna.ID_SK, GrupaRacuna.NalogKN, RacunStavke.ID_SK, RacunStavke.Naziv, Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.KontoKnjizenja, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.TipDokumenta, RacunStavke.DobavljacKonto, Dobavljac_Racuni.DobavljacKonto, Dobavljac_Racuni.SK_ID
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
ERROR_084_UPOREDO-083-082
-----SQL-----
SELECT [ERROR_083_GRRAC-STAVKE-DOBRAC].*, ERROR_082_GK_4350.*, ERROR_082_GK_4900.*, ERROR_082_GK_5590.*, ERROR_082_GK_4350.SIFRAKONTA
FROM (([ERROR_083_GRRAC-STAVKE-DOBRAC] LEFT JOIN ERROR_082_GK_4350 ON [ERROR_083_GRRAC-STAVKE-DOBRAC].IDTRRAC = ERROR_082_GK_4350.RDOB) LEFT JOIN ERROR_082_GK_4900 ON [ERROR_083_GRRAC-STAVKE-DOBRAC].IDTRRAC = ERROR_082_GK_4900.RDOB) LEFT JOIN ERROR_082_GK_5590 ON [ERROR_083_GRRAC-STAVKE-DOBRAC].IDTRRAC = ERROR_082_GK_5590.RDOB;


=====QUERY=====
ERROR_085_SUME_GRUPARACUNA
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, GrupaRacuna.NalogKN, GK_2040.SumOfDIZNOS, GK_4350.SumOfPIZNOS, GK_4900.SumOfPIZNOS, GK_5590.SumOfDIZNOS
FROM (((GK_2040 RIGHT JOIN GrupaRacuna ON GK_2040.BR_NALOG = GrupaRacuna.NalogKN) LEFT JOIN GK_4350 ON GrupaRacuna.NalogKN = GK_4350.BR_NALOG) LEFT JOIN GK_5590 ON GrupaRacuna.NalogKN = GK_5590.BR_NALOG) LEFT JOIN GK_4900 ON GrupaRacuna.NalogKN = GK_4900.BR_NALOG;


=====QUERY=====
ERROR_101_KONTOTROSKA_NEPOSTOJI
-----SQL-----
SELECT GK.STAVKAID, GK.KontoTroska, GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.DIZNOS, GK.PIZNOS, GK.TIP_STAVKE, GK.DOK, TKONTO.PodKonto
FROM GK LEFT JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto
WHERE ((Not (GK.KontoTroska) Is Null And (GK.KontoTroska)<>"") AND ((TKONTO.PodKonto) Is Null));


=====QUERY=====
ERROR_102_KONTOTROSKA_NEMAPARENT
-----SQL-----
SELECT TKONTO.TKONTO, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta_1.PodKonto, Troskovi_PodKonta_4.PodKonto
FROM ((TKONTO LEFT JOIN Troskovi_PodKonta ON TKONTO.TKONTO = Troskovi_PodKonta.PodKonto) LEFT JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.GRUPA1 = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Troskovi_PodKonta AS Troskovi_PodKonta_4 ON TKONTO.GRUPA4 = Troskovi_PodKonta_4.PodKonto
WHERE (((Troskovi_PodKonta.PodKonto) Is Null)) OR (((Troskovi_PodKonta_1.PodKonto) Is Null)) OR (((Troskovi_PodKonta_4.PodKonto) Is Null));


=====QUERY=====
ERROR_103_2040_IZVOD_NEMASTAVKUIZVODA
-----SQL-----
SELECT GK.lnkKUPACID, GK.lnkKUPACID, GK.lnkIzvodStavkaID, GK.TIP_STAVKE, GK.STAVKAID, GK.BR_NALOG
FROM GK
WHERE (((GK.lnkIzvodStavkaID)=0) AND ((GK.TIP_STAVKE)=1) AND ((GK.KONTO)="2040"));


=====QUERY=====
ERROR_104_NALOG_RAVNOTEZA
-----SQL-----
SELECT GK.BR_NALOG, GK.BR_NALOG, Round(Sum([DIZNOS])-Sum([PIZNOS]),2) AS DIPI
FROM GK
GROUP BY GK.BR_NALOG, GK.BR_NALOG
HAVING (((Round(Sum([DIZNOS])-Sum([PIZNOS]),2))<>0));


=====QUERY=====
ERROR_901_A_NALOG_VISE_IZVODA
-----SQL-----
SELECT Count(Izvod.IzvodID) AS CountOfIzvodID, Izvod.NalogZaKnjizenje
FROM Izvod
GROUP BY Izvod.NalogZaKnjizenje
HAVING (((Count(Izvod.IzvodID))>1));


=====QUERY=====
ERROR_901_AB_NALOG_VISE_IZVODA_GK_STAVKI
-----SQL-----
SELECT [ERROR_901_A_NALOG_VISE_IZVODA].NalogZaKnjizenje, Count(GK.STAVKAID) AS GK_STAVKI
FROM GK RIGHT JOIN ERROR_901_A_NALOG_VISE_IZVODA ON GK.BR_NALOG=[ERROR_901_A_NALOG_VISE_IZVODA].NalogZaKnjizenje
GROUP BY [ERROR_901_A_NALOG_VISE_IZVODA].NalogZaKnjizenje;


=====QUERY=====
ERROR_901_AC_NALOG_VISE_IZVODA_IZVODSTAVKI
-----SQL-----
SELECT ERROR_901_A_NALOG_VISE_IZVODA.NalogZaKnjizenje, Izvod.IzvodID, Izvod.ID_SK, Count(IzvodStavke.ID) AS CountOfID, "IZVOD " & [BrojIzvoda] & "/" & [SUFIXIZVODA] AS Expr1
FROM IzvodStavke RIGHT JOIN (Izvod RIGHT JOIN ERROR_901_A_NALOG_VISE_IZVODA ON Izvod.NalogZaKnjizenje = ERROR_901_A_NALOG_VISE_IZVODA.NalogZaKnjizenje) ON IzvodStavke.IzvodLNKID = Izvod.IzvodID
GROUP BY ERROR_901_A_NALOG_VISE_IZVODA.NalogZaKnjizenje, Izvod.IzvodID, Izvod.ID_SK, "IZVOD " & [BrojIzvoda] & "/" & [SUFIXIZVODA];


=====QUERY=====
ERROR_902_A_GK_STAVKA_BEZ_DATUMA
-----SQL-----
SELECT GK.STAVKAID, "IzvodStavkaID :  / " & [lnkIzvodStavkaID] & " / RacunTroskaID :  " & [RacunIN_ID] & " / PartnerID : " & [lnkKUPACID] AS Expr1, GK.*
FROM GK
WHERE (((GK.DATUM) Is Null));


=====QUERY=====
ERROR-IZVOD
-----SQL-----
SELECT GK.BR_NALOG, Sum(IzvodStavke.Zaduzenje) AS SumOfZaduzenje, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje, Sum(GK.DIZNOS) AS SumOfDIZNOS, Nalog.OpisNaloga, Nalog.SZID, Izvod.ID_SK
FROM ((Izvod LEFT JOIN GK ON Izvod.NalogZaKnjizenje = GK.BR_NALOG) INNER JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID) LEFT JOIN Nalog ON GK.BR_NALOG = Nalog.Br_Nalog
WHERE (((GK.KONTO)="2410"))
GROUP BY GK.BR_NALOG, Nalog.OpisNaloga, Nalog.SZID, Izvod.ID_SK;


=====QUERY=====
ErrorIzvod-SumaNovStanjePrethodno-GK2410
-----SQL-----
SELECT Izvod.ID_SK, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, [NovoStanje]-[PrethodnoStanjeIzvoda] AS SumaPoIzvoduStanje, GK_2410_grpNalog.SUMA, GK_2410_grpNalog.BR_NALOG, Izvod.NalogZaKnjizenje, Round(([NovoStanje]-[PrethodnoStanjeIzvoda])-([SUMA]),2) AS RAZLIKA
FROM GK_2410_grpNalog LEFT JOIN Izvod ON GK_2410_grpNalog.BR_NALOG = Izvod.NalogZaKnjizenje
WHERE (((Round(([NovoStanje]-[PrethodnoStanjeIzvoda])-([SUMA]),2))<>0));


=====QUERY=====
EXPORT_001_LISTA_OBJEKATA_SA_KORISNICIMA
-----SQL-----
SELECT Skustina.NazivSS, Skustina.Zgrada, SzUlaz.Ulaz, SzUlaz.Adresa AS [Adresa ulaza], Objekti.naziv AS [Poseban deo], Kupac.Naziv, Objekti.K1 AS Kvadratura, TipObjekta.Print AS [Tip dela], Kupac.ID_K AS PODKONTO, Kupac.Naziv AS Korisnik, Kupac.PIB
FROM Skustina INNER JOIN ((SzUlaz INNER JOIN (Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) ON SzUlaz.Ulaz = Objekti.Ulaz) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) ON Skustina.IDSkupstina = SzUlaz.SZ
WHERE (((Objekti.Status)=1))
ORDER BY Skustina.IDSkupstina, Kupac.ID_K, Objekti.BrojPD;


=====QUERY=====
EXPORT_002_LISTA_KORISNIKA_EMAIL
-----SQL-----
SELECT Skustina.NazivSS, Skustina.Zgrada, Kupac.ID_K AS PODKONTO, Mail.eMail, Kupac.Naziv AS Korisnik
FROM ((Mail INNER JOIN Kupac ON Mail.IDPartner = Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) INNER JOIN Objekti_By_KupacID ON Kupac.ID_K = Objekti_By_KupacID.lnk_ID_K
WHERE (((Mail.SendMailRacun)=-1))
ORDER BY Skustina.IDSkupstina, Kupac.ID_K, Mail.SortOrder;


=====QUERY=====
EXPORT_003_PREBROJAVANJE
-----SQL-----
SELECT Skustina.NazivSS, Skustina.Zgrada, TipObjekta.Print, Count(Objekti.ID_O) AS BrojPosebnihDelova, Sum(Objekti.K1) AS Kvadratura
FROM (Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Objekti.Status)=1) AND ((Objekti.lnk_ID_K) Is Not Null))
GROUP BY Skustina.IDSkupstina, Skustina.NazivSS, Skustina.Zgrada, TipObjekta.Print, TipObjekta.SortObj
ORDER BY Skustina.IDSkupstina, TipObjekta.SortObj;


=====QUERY=====
EXPORT001 - Listing - SZ-PD-VL
-----SQL-----
SELECT Skustina.IDSkupstina AS IDSZ, Skustina.NazivSS AS [Stambena zajednica], Skustina.Zgrada, Objekti.Ulaz, TipObjekta.Print AS [Vrsta posebnog dela], Objekti.SifraPD, Kupac.Naziv AS Vlasnik, Objekti.K1 AS [K1 - m2], Objekti.K2 AS [K2 - keoficijent], Objekti.K3 AS [K3 - broj], Objekti.K4 AS [K4 - broj]
FROM ((Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Objekti.Status)=1))
ORDER BY Skustina.IDSkupstina, TipObjekta.SortObj, Objekti.BrojPD;


=====QUERY=====
EXPORT002-ListingRacuna
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Racun.ID_SK, Skustina.NazivSS, Skustina.Zgrada, Racun.ID_K, Racun.Kupac, Racun.RBR, Racun.UkupnoRacun, Racun.KamataIznos, Racun.Ukupno, Racun.PozivNaBroj
FROM (Racun INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GrupaRacuna.GrupaRacunaFXN)=[YYMM]) AND ((Racun.Storno)=False))
ORDER BY Racun.ID_SK, Racun.ID_K;


=====QUERY=====
EXPORT002-ListingRacuna-KONTROLA-003
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Racun.lnkGR, First(Racun.Suma) AS FirstOfSuma, Sum(RacunStavke.Suma) AS SumOfSuma, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, Racun.RBR, Round(First([Racun].[Suma])-Sum([RacunStavke].[Suma]),2) AS Expr1
FROM GrupaRacuna INNER JOIN (Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R) ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
GROUP BY GrupaRacuna.GrupaRacunaFXN, Racun.lnkGR, Racun.RBR
HAVING (((GrupaRacuna.GrupaRacunaFXN)=[YYMM]) AND ((Round(First([Racun].[Suma])-Sum([RacunStavke].[Suma]),2))<>0));


=====QUERY=====
EXPORT003A-SumeStavkeRacunaKamata
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, Skustina.Zgrada, Troskovi_PodKonta.Naziv, Sum(ZK.DIZNOS) AS SumOfDIZNOS
FROM ((ZK INNER JOIN (GrupaRacuna INNER JOIN Skustina ON GrupaRacuna.ID_SK = Skustina.IDSkupstina) ON ZK.IDGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Troskovi_PodKonta ON ZK.PODKONTO = Troskovi_PodKonta.PodKonto) INNER JOIN Racun ON (Racun.ID_K = ZK.PARTNERID) AND (GrupaRacuna.IDGrupaRacuna = Racun.lnkGR)
WHERE (((Racun.Storno)=False))
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, Skustina.Zgrada, Troskovi_PodKonta.Naziv, GrupaRacuna.GrupaRacunaFXN
HAVING (((GrupaRacuna.GrupaRacunaFXN)=[YYMM]));


=====QUERY=====
EXPORT003-SumeStavkeRacuna
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, Skustina.Zgrada, RacunStavke.Naziv, Sum(RacunStavke.Kolicina) AS SumOfKolicina, RacunStavke.JM, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, RacunStavke.Sort, Kupac.Naziv
FROM (((Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R) INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Skustina ON GrupaRacuna.ID_SK = Skustina.IDSkupstina) INNER JOIN Kupac ON RacunStavke.DobavljacKonto = Kupac.ID_K
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, Skustina.Zgrada, RacunStavke.Naziv, RacunStavke.JM, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Sort, Kupac.Naziv, RacunStavke.ID_SK, GrupaRacuna.GrupaRacunaFXN
HAVING (((GrupaRacuna.GrupaRacunaFXN)=[YYMM]))
ORDER BY RacunStavke.ID_SK, RacunStavke.Sort;


=====QUERY=====
Export004-SumeStavkeRacunaBenefiti
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, RacunStavkeBenefitArhiva.ID_SK, GrupaRacuna.GrupaRacunaFXT, Skustina.Zgrada, RacunStavkeBenefitArhiva.Naziv, Sum(RacunStavkeBenefitArhiva.Kolicina) AS SumOfKolicina, RacunStavkeBenefitArhiva.JM, RacunStavkeBenefitArhiva.CenaE, RacunStavkeBenefitArhiva.NBS, RacunStavkeBenefitArhiva.Iznos, Sum(RacunStavkeBenefitArhiva.UkupnoRSD) AS SumOfUkupnoRSD, RacunStavkeBenefitArhiva.Sort, Kupac.Naziv
FROM (((Racun INNER JOIN RacunStavkeBenefitArhiva ON Racun.IDRacun = RacunStavkeBenefitArhiva.ID_R) INNER JOIN GrupaRacuna ON RacunStavkeBenefitArhiva.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Skustina ON GrupaRacuna.ID_SK = Skustina.IDSkupstina) INNER JOIN Kupac ON RacunStavkeBenefitArhiva.DobavljacKonto = Kupac.ID_K
GROUP BY GrupaRacuna.GrupaRacunaFXN, RacunStavkeBenefitArhiva.ID_SK, GrupaRacuna.GrupaRacunaFXT, Skustina.Zgrada, RacunStavkeBenefitArhiva.Naziv, RacunStavkeBenefitArhiva.JM, RacunStavkeBenefitArhiva.CenaE, RacunStavkeBenefitArhiva.NBS, RacunStavkeBenefitArhiva.Iznos, RacunStavkeBenefitArhiva.Sort, Kupac.Naziv, GrupaRacuna.GrupaRacunaFXN
ORDER BY GrupaRacuna.GrupaRacunaFXN, RacunStavkeBenefitArhiva.ID_SK, RacunStavkeBenefitArhiva.Sort;


=====QUERY=====
EXPORT005_Benefiti_All
-----SQL-----
SELECT Benefiti.MesecYYMM, Skustina.IDSkupstina AS ID_SZ, Skustina.Zgrada, TipObjekta.TipObj, Objekti.SifraPD AS Unit, Objekti.lnk_ID_K, Objekti.K1, Objekti.K2, Objekti.K3, Kupac.Naziv, Benefiti.Used
FROM Kupac INNER JOIN (((Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Benefiti ON Objekti.ID_O = Benefiti.ObjekatID) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) ON Kupac.ID_K = Objekti.lnk_ID_K
ORDER BY Benefiti.MesecYYMM, Skustina.IDSkupstina, Objekti.BrojPD;


=====QUERY=====
EXPORT006_SUMA-RACUNA
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Skustina.Zgrada, Count(Racun.IDRacun) AS CountOfIDRacun, Sum(Kupac.chkSkipPrintRacunGrupa) AS SumOfchkSkipPrintRacunGrupa
FROM ((GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina) INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K
WHERE (((Racun.Storno)=False))
GROUP BY GrupaRacuna.GrupaRacunaFXN, Skustina.Zgrada;


=====QUERY=====
EXPORT007 - Listing - SZ-PD-VL - NEAKTIVNO
-----SQL-----
SELECT Skustina.IDSkupstina AS IDSZ, Skustina.NazivSS AS [Stambena zajednica], Skustina.Zgrada, Objekti.Ulaz, TipObjekta.Print AS [Vrsta posebnog dela], Objekti.SifraPD, Kupac.Naziv AS Vlasnik, Objekti.K1 AS [K1 - m2], Objekti.K2 AS [K2 - keoficijent], Objekti.K3 AS [K3 - broj]
FROM ((Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Objekti.Status)<>1))
ORDER BY Skustina.IDSkupstina, TipObjekta.SortObj, Objekti.BrojPD;


=====QUERY=====
EXPORT008-KontaktLista
-----SQL-----
SELECT Kupac.ID_K, Skustina.Zgrada, Kupac.Naziv, Kupac.Telefon, Objekti.naziv, Mail.eMail, Mail.SendMailRacun, TipObjekta.TipObj
FROM ((Mail RIGHT JOIN (Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) ON Mail.IDPartner = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) LEFT JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina
WHERE (((Mail.SendMailRacun)=-1 Or (Mail.SendMailRacun) Is Null))
ORDER BY Objekti.BrojPD;


=====QUERY=====
Fin_Izvestaj_SZ_00
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SUMA, Max(GK.DATUM) AS MaxOfDATUM, Min(GK.DATUM) AS MinOfDATUM
FROM GK
WHERE (((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.KONTO, GK.lnkSkupstinaID
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
Fin_Izvestaj_SZ_00_PS_Y
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SUMA, Max(GK.DATUM) AS MaxOfDATUM, Min(GK.DATUM) AS MinOfDATUM, Year([DATUM]) & ". /PROMET ZA PERIOD : " AS YYYYNASLOV
FROM GK
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.KONTO, GK.lnkSkupstinaID, Year([DATUM]) & ". /PROMET ZA PERIOD : "
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
Fin_Izvestaj_SZ_00_PSTS_Y
-----SQL-----
SELECT KONTO, lnkSkupstinaID, SumOfDIZNOS, SumOfPIZNOS, SUMA, MaxOfDATUM, MinOfDATUM, YYYYNASLOV
FROM Fin_Izvestaj_SZ_00_PS_Y
UNION SELECT KONTO, lnkSkupstinaID, SumOfDIZNOS, SumOfPIZNOS, SUMA, MaxOfDATUM, MinOfDATUM, YYYYNASLOV
FROM Fin_Izvestaj_SZ_00_TS
ORDER BY MinOfDATUM;


=====QUERY=====
Fin_Izvestaj_SZ_00_TS
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SUMA, Max(GK.DATUM) AS MaxOfDATUM, Min(GK.DATUM) AS MinOfDATUM, "          . /PROMET ZA PERIOD : " AS YYYYNASLOV
FROM GK
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.KONTO, GK.lnkSkupstinaID, "          . /PROMET ZA PERIOD : "
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
Fin_Izvestaj_SZ_01
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SUMA, Max(GK.DATUM) AS MaxOfDATUM, Min(GK.DATUM) AS MinOfDATUM
FROM GK
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.KONTO, GK.lnkSkupstinaID
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
Fin_Izvestaj_SZ_02
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub0.GRUPA1, Fin_Izvestaj_SZ_02_sub0.TKontoNazivGrupa, Fin_Izvestaj_SZ_02_sub0.TKONTO, Fin_Izvestaj_SZ_02_sub0.TKontoNaziv, Fin_Izvestaj_SZ_02_sub1.Fakturisano4900, Fin_Izvestaj_SZ_02_sub2.Fakturisano2040, Fin_Izvestaj_SZ_02_sub2.Uplaceno2040, Fin_Izvestaj_SZ_02_sub4.PlacenoDob, Fin_Izvestaj_SZ_02_sub4.PotrazujuDob, Sum([Uplaceno2040]-[PlacenoDob]) AS Nenaplaceno2040
FROM ((Fin_Izvestaj_SZ_02_sub0 INNER JOIN Fin_Izvestaj_SZ_02_sub1 ON (Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub1.lnkSkupstinaID) AND (Fin_Izvestaj_SZ_02_sub0.TKONTO = Fin_Izvestaj_SZ_02_sub1.KontoTroska)) INNER JOIN Fin_Izvestaj_SZ_02_sub2 ON (Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub2.lnkSkupstinaID) AND (Fin_Izvestaj_SZ_02_sub0.TKONTO = Fin_Izvestaj_SZ_02_sub2.KontoTroska)) INNER JOIN Fin_Izvestaj_SZ_02_sub4 ON (Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub4.lnkSkupstinaID) AND (Fin_Izvestaj_SZ_02_sub0.TKONTO = Fin_Izvestaj_SZ_02_sub4.TKONTO)
WHERE (((Fin_Izvestaj_SZ_02_sub0.GRUPA1)<>"10"));


=====QUERY=====
Fin_Izvestaj_SZ_02_0
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub0.GRUPA1, Fin_Izvestaj_SZ_02_sub0.TKontoNazivGrupa, Fin_Izvestaj_SZ_02_sub0.TKONTO, Fin_Izvestaj_SZ_02_sub0.TKontoNaziv, Fin_Izvestaj_SZ_02_sub1.Fakturisano4900, Fin_Izvestaj_SZ_02_sub2.Fakturisano2040, Fin_Izvestaj_SZ_02_sub2.Uplaceno2040, Fin_Izvestaj_SZ_02_sub4.PlacenoDob, Fin_Izvestaj_SZ_02_sub4.PotrazujuDob, [Uplaceno2040]-[PlacenoDob] AS Stanje, [Uplaceno2040]-[PlacenoDob] AS Nenaplaceno2040
FROM ((Fin_Izvestaj_SZ_02_sub0 INNER JOIN Fin_Izvestaj_SZ_02_sub1 ON (Fin_Izvestaj_SZ_02_sub0.TKONTO = Fin_Izvestaj_SZ_02_sub1.KontoTroska) AND (Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub1.lnkSkupstinaID)) INNER JOIN Fin_Izvestaj_SZ_02_sub2 ON (Fin_Izvestaj_SZ_02_sub0.TKONTO = Fin_Izvestaj_SZ_02_sub2.KontoTroska) AND (Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub2.lnkSkupstinaID)) INNER JOIN Fin_Izvestaj_SZ_02_sub4 ON (Fin_Izvestaj_SZ_02_sub0.TKONTO = Fin_Izvestaj_SZ_02_sub4.TKONTO) AND (Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub4.lnkSkupstinaID)
WHERE (((Fin_Izvestaj_SZ_02_sub0.GRUPA1) Like "10*"));


=====QUERY=====
Fin_Izvestaj_SZ_02_2
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub0.GRUPA4, Fin_Izvestaj_SZ_02_sub0.TKontoNazivGrupa4, Sum(Fin_Izvestaj_SZ_02_sub2.Fakturisano2040) AS SumOfFakturisano2040, Sum(Fin_Izvestaj_SZ_02_sub2.Uplaceno2040) AS SumOfUplaceno2040, Sum(Fin_Izvestaj_SZ_02_sub4.PlacenoDob) AS SumOfPlacenoDob, Sum(Fin_Izvestaj_SZ_02_sub4.PotrazujuDob) AS SumOfPotrazujuDob, Sum([Uplaceno2040]-[PlacenoDob]) AS Stanje, Fin_Izvestaj_SZ_02_sub0.GRUPA1, Fin_Izvestaj_SZ_02_sub0.TKontoNazivGrupa, Fin_Izvestaj_SZ_02_sub0.YYYY, Sum([Fakturisano2040]-[Uplaceno2040]) AS Nenaplaceno2040, IIf([grupa1]="10",Sum([Uplaceno2040]-[PlacenoDob]),IIf(Sum([Uplaceno2040]-[PlacenoDob])<0,Sum([Uplaceno2040]-[PlacenoDob]),0)) AS FondStanje
FROM (Fin_Izvestaj_SZ_02_sub0 LEFT JOIN Fin_Izvestaj_SZ_02_sub2 ON Fin_Izvestaj_SZ_02_sub0.JNEXP = Fin_Izvestaj_SZ_02_sub2.JNEXP) LEFT JOIN Fin_Izvestaj_SZ_02_sub4 ON Fin_Izvestaj_SZ_02_sub0.JNEXP = Fin_Izvestaj_SZ_02_sub4.JNEXP
GROUP BY Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub0.GRUPA4, Fin_Izvestaj_SZ_02_sub0.TKontoNazivGrupa4, Fin_Izvestaj_SZ_02_sub0.GRUPA1, Fin_Izvestaj_SZ_02_sub0.TKontoNazivGrupa, Fin_Izvestaj_SZ_02_sub0.YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_02_2_PS_YY
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub0_PS_YY.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub0_PS_YY.GRUPA4, Fin_Izvestaj_SZ_02_sub0_PS_YY.TKontoNazivGrupa4, Sum(Fin_Izvestaj_SZ_02_sub2_PS_YY.Fakturisano2040) AS SumOfFakturisano2040, Sum(Fin_Izvestaj_SZ_02_sub2_PS_YY.Uplaceno2040) AS SumOfUplaceno2040, Sum(Fin_Izvestaj_SZ_02_sub4_PS_YY.PlacenoDob) AS SumOfPlacenoDob, Sum(Fin_Izvestaj_SZ_02_sub4_PS_YY.PotrazujuDob) AS SumOfPotrazujuDob, Sum([Uplaceno2040]-[PlacenoDob]) AS Stanje, Fin_Izvestaj_SZ_02_sub0_PS_YY.GRUPA1, Fin_Izvestaj_SZ_02_sub0_PS_YY.TKontoNazivGrupa, Fin_Izvestaj_SZ_02_sub0_PS_YY.YYYY, Sum([Fakturisano2040]-[Uplaceno2040]) AS Nenaplaceno2040, Fin_Izvestaj_SZ_02_sub0_PS_YY.YYYY AS ShowYY, IIf([grupa1]="10",Sum([Uplaceno2040]-[PlacenoDob]),IIf(Sum([Uplaceno2040]-[PlacenoDob])<0,Sum([Uplaceno2040]-[PlacenoDob]),0)) AS FondStanje
FROM (Fin_Izvestaj_SZ_02_sub0_PS_YY INNER JOIN Fin_Izvestaj_SZ_02_sub2_PS_YY ON (Fin_Izvestaj_SZ_02_sub0_PS_YY.YYYY = Fin_Izvestaj_SZ_02_sub2_PS_YY.YYYY) AND (Fin_Izvestaj_SZ_02_sub0_PS_YY.TKONTO = Fin_Izvestaj_SZ_02_sub2_PS_YY.KontoTroska) AND (Fin_Izvestaj_SZ_02_sub0_PS_YY.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub2_PS_YY.lnkSkupstinaID)) INNER JOIN Fin_Izvestaj_SZ_02_sub4_PS_YY ON (Fin_Izvestaj_SZ_02_sub0_PS_YY.YYYY = Fin_Izvestaj_SZ_02_sub4_PS_YY.YYYY) AND (Fin_Izvestaj_SZ_02_sub0_PS_YY.TKONTO = Fin_Izvestaj_SZ_02_sub4_PS_YY.TKONTO) AND (Fin_Izvestaj_SZ_02_sub0_PS_YY.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub4_PS_YY.lnkSkupstinaID)
GROUP BY Fin_Izvestaj_SZ_02_sub0_PS_YY.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub0_PS_YY.GRUPA4, Fin_Izvestaj_SZ_02_sub0_PS_YY.TKontoNazivGrupa4, Fin_Izvestaj_SZ_02_sub0_PS_YY.GRUPA1, Fin_Izvestaj_SZ_02_sub0_PS_YY.TKontoNazivGrupa, Fin_Izvestaj_SZ_02_sub0_PS_YY.YYYY, Fin_Izvestaj_SZ_02_sub0_PS_YY.YYYY
ORDER BY Fin_Izvestaj_SZ_02_sub0_PS_YY.YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_02_2_PSTS_YY
-----SQL-----
SELECT lnkSkupstinaID, GRUPA4, TKontoNazivGrupa4, SumOfFakturisano2040, SumOfUplaceno2040, SumOfPlacenoDob, SumOfPotrazujuDob, Stanje, GRUPA1, TKontoNazivGrupa, Nenaplaceno2040 , YYYY, YYYY AS SHOWYY, FondStanje
FROM Fin_Izvestaj_SZ_02_2_PS_YY
UNION SELECT lnkSkupstinaID, GRUPA4, TKontoNazivGrupa4, SumOfFakturisano2040, SumOfUplaceno2040, SumOfPlacenoDob, SumOfPotrazujuDob, Stanje, GRUPA1, TKontoNazivGrupa, Nenaplaceno2040, YYYY, "" AS SHOWYY, FondStanje
FROM Fin_Izvestaj_SZ_02_2
ORDER BY YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_02_sub0
-----SQL-----
SELECT frmIzvestajFilterGK.lnkSkupstinaID, TKONTO.TKONTO, Troskovi_PodKonta.Naziv AS TKontoNaziv, TKONTO.GRUPA1, Troskovi_PodKonta_1.Naziv AS TKontoNazivGrupa, TKONTO.GRUPA4, Troskovi_PodKonta_4.Naziv AS TKontoNazivGrupa4, Year(Max([DATUM])) AS YYYY, [lnkSkupstinaID] & "-" & [KontoTroska] AS JNEXP
FROM ((frmIzvestajFilterGK INNER JOIN (TKONTO INNER JOIN Troskovi_PodKonta ON TKONTO.TKONTO = Troskovi_PodKonta.PodKonto) ON frmIzvestajFilterGK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.GRUPA1 = Troskovi_PodKonta_1.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_4 ON TKONTO.GRUPA4 = Troskovi_PodKonta_4.PodKonto
WHERE (((frmIzvestajFilterGK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]) AND ((frmIzvestajFilterGK.KontoTroska) Like "1*"))
GROUP BY frmIzvestajFilterGK.lnkSkupstinaID, TKONTO.TKONTO, Troskovi_PodKonta.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_1.Naziv, TKONTO.GRUPA4, Troskovi_PodKonta_4.Naziv, [lnkSkupstinaID] & "-" & [KontoTroska];


=====QUERY=====
Fin_Izvestaj_SZ_02_sub0_PS
-----SQL-----
SELECT GK.lnkSkupstinaID, TKONTO.TKONTO, Troskovi_PodKonta.Naziv AS TKontoNaziv, TKONTO.GRUPA1, Troskovi_PodKonta_1.Naziv AS TKontoNazivGrupa, TKONTO.GRUPA4, Troskovi_PodKonta_4.Naziv AS TKontoNazivGrupa4
FROM ((GK INNER JOIN (TKONTO INNER JOIN Troskovi_PodKonta ON TKONTO.TKONTO = Troskovi_PodKonta.PodKonto) ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.GRUPA1 = Troskovi_PodKonta_1.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_4 ON TKONTO.GRUPA4 = Troskovi_PodKonta_4.PodKonto
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]) AND ((GK.KontoTroska) Like "1*"))
GROUP BY GK.lnkSkupstinaID, TKONTO.TKONTO, Troskovi_PodKonta.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_1.Naziv, TKONTO.GRUPA4, Troskovi_PodKonta_4.Naziv;


=====QUERY=====
Fin_Izvestaj_SZ_02_sub0_PS_YY
-----SQL-----
SELECT GK.lnkSkupstinaID, TKONTO.TKONTO, Troskovi_PodKonta.Naziv AS TKontoNaziv, TKONTO.GRUPA1, Troskovi_PodKonta_1.Naziv AS TKontoNazivGrupa, TKONTO.GRUPA4, Troskovi_PodKonta_4.Naziv AS TKontoNazivGrupa4, Year([DATUM]) AS YYYY
FROM ((GK INNER JOIN (TKONTO INNER JOIN Troskovi_PodKonta ON TKONTO.TKONTO = Troskovi_PodKonta.PodKonto) ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.GRUPA1 = Troskovi_PodKonta_1.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_4 ON TKONTO.GRUPA4 = Troskovi_PodKonta_4.PodKonto
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]) AND ((GK.KontoTroska) Like "1*"))
GROUP BY GK.lnkSkupstinaID, TKONTO.TKONTO, Troskovi_PodKonta.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_1.Naziv, TKONTO.GRUPA4, Troskovi_PodKonta_4.Naziv, Year([DATUM])
HAVING (((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]))
ORDER BY Year([DATUM]);


=====QUERY=====
Fin_Izvestaj_SZ_02_sub1
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Sum(GK.PIZNOS) AS Fakturisano4900
FROM GK
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, GK.KONTO
HAVING (((GK.KONTO)="4900"));


=====QUERY=====
Fin_Izvestaj_SZ_02_sub1_PS
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Sum(GK.PIZNOS) AS Fakturisano4900
FROM GK
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, GK.KONTO
HAVING (((GK.KONTO)="4900"));


=====QUERY=====
Fin_Izvestaj_SZ_02_sub1_PS_YY
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Sum(GK.PIZNOS) AS Fakturisano4900, Year([DATUM]) AS YYYY
FROM GK
WHERE (((GK.KONTO)="4900") AND ((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, Year([DATUM]);


=====QUERY=====
Fin_Izvestaj_SZ_02_sub2
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Sum(GK.DIZNOS) AS Fakturisano2040, Sum(GK.PIZNOS) AS Uplaceno2040, [lnkSkupstinaID] & "-" & [KontoTroska] AS JNEXP
FROM GK
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, GK.KONTO, [lnkSkupstinaID] & "-" & [KontoTroska]
HAVING (((GK.KontoTroska) Like "1*") AND ((GK.KONTO)="2040"));


=====QUERY=====
Fin_Izvestaj_SZ_02_sub2_PS
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Sum(GK.DIZNOS) AS Fakturisano2040, Sum(GK.PIZNOS) AS Uplaceno2040
FROM GK
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, GK.KONTO
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
Fin_Izvestaj_SZ_02_sub2_PS_YY
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Sum(GK.DIZNOS) AS Fakturisano2040, Sum(GK.PIZNOS) AS Uplaceno2040, Year([DATUM]) AS YYYY
FROM GK
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, GK.KONTO, Year([DATUM])
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
Fin_Izvestaj_SZ_02_sub3
-----SQL-----
SELECT GK.lnkSkupstinaID, TKONTO.TKONTO, Sum(GK.PIZNOS) AS Fakturisano4900
FROM GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, TKONTO.TKONTO, GK.KONTO
HAVING (((GK.KONTO)="4900"));


=====QUERY=====
Fin_Izvestaj_SZ_02_sub4
-----SQL-----
SELECT GK.lnkSkupstinaID, TKONTO.TKONTO, Sum(GK.DIZNOS) AS PlacenoDob, Sum(GK.PIZNOS) AS PotrazujuDob, [lnkSkupstinaID] & "-" & [TKONTO] AS JNEXP
FROM GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto
WHERE (((GK.KONTO)="4350" Or (GK.KONTO)="5532") AND ((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo] Or (GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, TKONTO.TKONTO, [lnkSkupstinaID] & "-" & [TKONTO];


=====QUERY=====
Fin_Izvestaj_SZ_02_sub4_PS
-----SQL-----
SELECT GK.lnkSkupstinaID, TKONTO.TKONTO, Sum(GK.DIZNOS) AS PlacenoDob, Sum(GK.PIZNOS) AS PotrazujuDob
FROM GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto
WHERE (((GK.KONTO)="4350" Or (GK.KONTO)="5532") AND ((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, TKONTO.TKONTO;


=====QUERY=====
Fin_Izvestaj_SZ_02_sub4_PS_YY
-----SQL-----
SELECT GK.lnkSkupstinaID, TKONTO.TKONTO, Sum(GK.DIZNOS) AS PlacenoDob, Sum(GK.PIZNOS) AS PotrazujuDob, Year([DATUM]) AS YYYY
FROM GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto
WHERE (((GK.KONTO)="4350" Or (GK.KONTO)="5532") AND ((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, TKONTO.TKONTO, Year([DATUM]);


=====QUERY=====
Fin_Izvestaj_SZ_02_sub5
-----SQL-----
SELECT GK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, TKONTO.TKONTO, Sum(GK.DIZNOS) AS Placeno, Sum(GK.PIZNOS) AS Potrazuju, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.MesecRacuna, Kupac.Naziv, Dobavljac_Racuni.TipDokumenta, Kupac.ID_K, "" AS YYYY
FROM (((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.KONTO)="4350" Or (GK.KONTO)="5532") AND ((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, TKONTO.TKONTO, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.MesecRacuna, Kupac.Naziv, Dobavljac_Racuni.TipDokumenta, Kupac.ID_K;


=====QUERY=====
Fin_Izvestaj_SZ_02_sub5_PS
-----SQL-----
SELECT GK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, TKONTO.TKONTO, Sum(GK.DIZNOS) AS Placeno, Sum(GK.PIZNOS) AS Potrazuju, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.MesecRacuna, Kupac.Naziv, Dobavljac_Racuni.TipDokumenta, Kupac.ID_K, Year([Datum]) AS YYYY
FROM (((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.KONTO)="4350" Or (GK.KONTO)="5532") AND ((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, TKONTO.TKONTO, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.MesecRacuna, Kupac.Naziv, Dobavljac_Racuni.TipDokumenta, Kupac.ID_K, Year([Datum]);


=====QUERY=====
Fin_Izvestaj_SZ_02A
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub0.TKONTO, Fin_Izvestaj_SZ_02_sub0.TKontoNaziv, Fin_Izvestaj_SZ_02_sub0.TKontoNazivGrupa
FROM Fin_Izvestaj_SZ_02_sub0 INNER JOIN Fin_Izvestaj_SZ_02_sub1 ON (Fin_Izvestaj_SZ_02_sub0.TKONTO = Fin_Izvestaj_SZ_02_sub1.PodKonto) AND (Fin_Izvestaj_SZ_02_sub0.lnkSkupstinaID = Fin_Izvestaj_SZ_02_sub1.lnkSkupstinaID);


=====QUERY=====
Fin_Izvestaj_SZ_02A_sub1
-----SQL-----
SELECT GK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, Sum(GK.PIZNOS) AS Fakturisano4900, Troskovi_PodKonta_1.Naziv
FROM ((TKONTO INNER JOIN GK ON TKONTO.PodKonto = GK.KontoTroska) INNER JOIN Troskovi_PodKonta ON TKONTO.TKONTO = Troskovi_PodKonta.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.GRUPA1 = Troskovi_PodKonta_1.PodKonto
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, Troskovi_PodKonta_1.Naziv, GK.KONTO
HAVING (((GK.KONTO)="4900"));


=====QUERY=====
Fin_Izvestaj_SZ_03
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0) AS [DUG-TMP], IIf(Sum([PIZNOS])=0,Sum([DIZNOS]),Sum([PIZNOS])) AS [OBRACUNATO-TMP], IIf([GK].[KONTO]='5532',0,IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0)) AS DUG, IIf([GK].[KONTO]='5532' Or [GK].[KONTO]='5241',Sum([DIZNOS]-[PIZNOS]),Sum([PIZNOS])) AS OBRACUNATO
FROM (((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]) AND ((TKONTO.TrosakNa) Like "10*")) OR (((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]) AND ((TKONTO.TrosakNa) Like "10*"))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna
HAVING (((GK.KONTO)="4350" Or (GK.KONTO)="5532") AND ((Dobavljac_Racuni.TipDokumenta) Is Null)) OR (((GK.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta)=2))
ORDER BY Dobavljac_Racuni.DatumRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_03_1
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0) AS [DUG-TMP], IIf(Sum([PIZNOS])=0,Sum([DIZNOS]),Sum([PIZNOS])) AS [OBRACUNATO-TMP], IIf([GK].[KONTO]='5532',0,IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0)) AS DUG, IIf([GK].[KONTO]='5532' Or [GK].[KONTO]='5241',Sum([DIZNOS]-[PIZNOS]),Sum([PIZNOS])) AS OBRACUNATO, GrupaRacuna.MarkerVanderdnihRacuna
FROM (((((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna, GrupaRacuna.MarkerVanderdnihRacuna
HAVING (((GK.KONTO)="2040") AND (Not (GrupaRacuna.MarkerVanderdnihRacuna) Is Null And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like "V*" And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like ""))
ORDER BY Dobavljac_Racuni.DatumRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_03_1_PS
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, "" AS RDOB, Year([DATUM]) AS NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Year([Dobavljac_Racuni].[DATUMRACUNA]) AS DatumRacuna, IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0) AS [DUG-TMP], IIf(Sum([PIZNOS])=0,Sum([DIZNOS]),Sum([PIZNOS])) AS [OBRACUNATO-TMP], IIf([GK].[KONTO]='5532',0,IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0)) AS DUG, IIf([GK].[KONTO]='5532' Or [GK].[KONTO]='5241',Sum([DIZNOS]-[PIZNOS]),Sum([PIZNOS])) AS OBRACUNATO, Year([DATUM]) AS YYYY, "" AS MarkerVanderdnihRacuna
FROM (((((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE ((Not (GrupaRacuna.MarkerVanderdnihRacuna) Is Null And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like "V*" And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like "") AND ((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Year([Dobavljac_Racuni].[DATUMRACUNA]), Year([DATUM]), "", "", ""
HAVING (((GK.KONTO)="2040"))
ORDER BY Year([DATUM]);


=====QUERY=====
Fin_Izvestaj_SZ_03_1_PSTS_YY
-----SQL-----
SELECT lnkSkupstinaID, KONTO, RDOB, NazivRacuna, NazivDobavljaca, SumOfDIZNOS, SumOfPIZNOS, SALDO, GRUPA1, TKontoNazivGrupa, TKONTO, TKontoNaziv, KontoTroska, NazivKontoTroska, DatumRacuna, [DUG-TMP], [OBRACUNATO-TMP], DUG, OBRACUNATO, MarkerVanderdnihRacuna
FROM Fin_Izvestaj_SZ_03_1_PS
UNION SELECT lnkSkupstinaID, KONTO, RDOB, NazivRacuna, NazivDobavljaca, SumOfDIZNOS, SumOfPIZNOS, SALDO, GRUPA1, TKontoNazivGrupa, TKONTO, TKontoNaziv, KontoTroska, NazivKontoTroska, DatumRacuna, [DUG-TMP], [OBRACUNATO-TMP], DUG, OBRACUNATO, MarkerVanderdnihRacuna
FROM Fin_Izvestaj_SZ_03_1_TS;


=====QUERY=====
Fin_Izvestaj_SZ_03_1_TS
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0) AS [DUG-TMP], IIf(Sum([PIZNOS])=0,Sum([DIZNOS]),Sum([PIZNOS])) AS [OBRACUNATO-TMP], IIf([GK].[KONTO]='5532',0,IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0)) AS DUG, IIf([GK].[KONTO]='5532' Or [GK].[KONTO]='5241',Sum([DIZNOS]-[PIZNOS]),Sum([PIZNOS])) AS OBRACUNATO, GrupaRacuna.MarkerVanderdnihRacuna
FROM (((((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna, GrupaRacuna.MarkerVanderdnihRacuna
HAVING (((GK.KONTO)="2040") AND (Not (GrupaRacuna.MarkerVanderdnihRacuna) Is Null And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like "V*" And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like ""))
ORDER BY Dobavljac_Racuni.DatumRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_03_1_TSx
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, "" AS RDOB, "IZDATO RACUNA " & Count([IDTRRAC]) & " \U PERIODU : " & Min([Dobavljac_Racuni].[DATUMRACUNA]) & " - " & Max([Dobavljac_Racuni].[DATUMRACUNA]) AS NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Year([Dobavljac_Racuni].[DATUMRACUNA]) AS DatumRacuna, IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0) AS [DUG-TMP], IIf(Sum([PIZNOS])=0,Sum([DIZNOS]),Sum([PIZNOS])) AS [OBRACUNATO-TMP], IIf([GK].[KONTO]='5532',0,IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0)) AS DUG, IIf([GK].[KONTO]='5532' Or [GK].[KONTO]='5241',Sum([DIZNOS]-[PIZNOS]),Sum([PIZNOS])) AS OBRACUNATO, Year([Dobavljac_Racuni].[DATUMRACUNA]) AS YYYY, "" AS MarkerVanderdnihRacuna
FROM (((((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE ((Not (GrupaRacuna.MarkerVanderdnihRacuna) Is Null And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like "V*" And (GrupaRacuna.MarkerVanderdnihRacuna) Not Like "") AND ((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Year([Dobavljac_Racuni].[DATUMRACUNA]), "", "", Year([Dobavljac_Racuni].[DATUMRACUNA]), ""
HAVING (((GK.KONTO)="2040"))
ORDER BY Year([Dobavljac_Racuni].[DATUMRACUNA]);


=====QUERY=====
Fin_Izvestaj_SZ_03_DobavljacRacunZatvoren
-----SQL-----
SELECT Dobavljac_Racuni.IDTRRAC, Round(Sum([DIZNOS]-[PIZNOS]),2)=0 AS Zatvoreno
FROM frmIzvestajFilterGK_TS LEFT JOIN Dobavljac_Racuni ON frmIzvestajFilterGK_TS.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((frmIzvestajFilterGK_TS.KONTO)="5532") AND ((Dobavljac_Racuni.TipDokumenta) Is Null)) OR (((frmIzvestajFilterGK_TS.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta)=2)) OR (((frmIzvestajFilterGK_TS.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta) Is Null)) OR (((frmIzvestajFilterGK_TS.KONTO)="5241") AND ((Dobavljac_Racuni.TipDokumenta) Is Null))
GROUP BY Dobavljac_Racuni.IDTRRAC
HAVING (((Round(Sum([DIZNOS]-[PIZNOS]),2)=0)=-1)) OR (((Round(Sum([DIZNOS]-[PIZNOS]),2)=0)=-1)) OR (((Round(Sum([DIZNOS]-[PIZNOS]),2)=0)=-1)) OR (((Round(Sum([DIZNOS]-[PIZNOS]),2)=0)=-1));


=====QUERY=====
Fin_Izvestaj_SZ_03_PS
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,Max([GK].[datum]),[Dobavljac_Racuni].[DatumRacuna]) AS DatumRacunaMix, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],"") AS Grupisanje, Round(Sum([DIZNOS]-[PIZNOS]),2)=0 AS Zatvoreno
FROM (((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]) AND ((TKONTO.TrosakNa) Like "10*")) OR (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]) AND ((TKONTO.TrosakNa) Like "10*"))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],"")
HAVING (((GK.KONTO)="5532" Or (GK.KONTO)="4350" Or (GK.KONTO)="5241") AND ((Dobavljac_Racuni.TipDokumenta) Is Null)) OR (((GK.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta)=2))
ORDER BY Dobavljac_Racuni.DatumRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_03_PS_GRP
-----SQL-----
SELECT Fin_Izvestaj_SZ_03_PS.lnkSkupstinaID, Fin_Izvestaj_SZ_03_PS.NazivDobavljaca, "PERIOD " & Year([DatumRacunaMix]) & ". GODINE" AS NazivRacuna, Sum(Fin_Izvestaj_SZ_03_PS.SumOfDIZNOS) AS SumOfSumOfDIZNOS, Sum(Fin_Izvestaj_SZ_03_PS.SumOfPIZNOS) AS SumOfSumOfPIZNOS, Sum(Fin_Izvestaj_SZ_03_PS.SALDO) AS SumOfSALDO, Fin_Izvestaj_SZ_03_PS.GRUPA1, Fin_Izvestaj_SZ_03_PS.TKontoNazivGrupa, Fin_Izvestaj_SZ_03_PS.TKONTO, Fin_Izvestaj_SZ_03_PS.TKontoNaziv, Fin_Izvestaj_SZ_03_PS.KontoTroska, Fin_Izvestaj_SZ_03_PS.NazivKontoTroska, Year([DatumRacunaMix]) AS YYYY, DateSerial(Year([DatumRacunaMix]),12,31) AS NoviDatum, Fin_Izvestaj_SZ_03_PS.KONTO
FROM Fin_Izvestaj_SZ_03_PS
GROUP BY Fin_Izvestaj_SZ_03_PS.lnkSkupstinaID, Fin_Izvestaj_SZ_03_PS.NazivDobavljaca, "PERIOD " & Year([DatumRacunaMix]) & ". GODINE", Fin_Izvestaj_SZ_03_PS.GRUPA1, Fin_Izvestaj_SZ_03_PS.TKontoNazivGrupa, Fin_Izvestaj_SZ_03_PS.TKONTO, Fin_Izvestaj_SZ_03_PS.TKontoNaziv, Fin_Izvestaj_SZ_03_PS.KontoTroska, Fin_Izvestaj_SZ_03_PS.NazivKontoTroska, Year([DatumRacunaMix]), DateSerial(Year([DatumRacunaMix]),12,31), Fin_Izvestaj_SZ_03_PS.KONTO;


=====QUERY=====
Fin_Izvestaj_SZ_03_PSTSDOD
-----SQL-----
SELECT lnkSkupstinaID, konto, NazivRacuna, NazivDobavljaca, SumOfSumOfDIZNOS as SumOfDIZNOS, SumOfSumOfPIZNOS as SumOfPIZNOS, SumOfSALDO as SALDO, GRUPA1, TKontoNazivGrupa, TKONTO, TKontoNaziv, KontoTroska, NazivKontoTroska, NoviDatum as DatumRacunaMix, 1 as ForceSort, 0 as Grupisanje
FROM Fin_Izvestaj_SZ_03_PS_GRP
UNION SELECT lnkSkupstinaID, konto, NazivRacuna, NazivDobavljaca, SumOfDIZNOS, SumOfPIZNOS, SALDO, GRUPA1, TKontoNazivGrupa, TKONTO, TKontoNaziv, KontoTroska, NazivKontoTroska, DatumRacunaMix, 2 as ForceSort, Grupisanje
FROM Fin_Izvestaj_SZ_03_TS
ORDER BY ForceSort, DatumRacunaMix;


=====QUERY=====
Fin_Izvestaj_SZ_03_SVE
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,Max([GK].[datum]),[Dobavljac_Racuni].[DatumRacuna]) AS DatumRacunaMix, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],"") AS Grupisanje, Round(Sum([DIZNOS]-[PIZNOS]),2)=0 AS Zatvoreno, GK.DATUM
FROM (((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],""), GK.DATUM
HAVING (((GK.KONTO)="5532" Or (GK.KONTO)="4350" Or (GK.KONTO)="5241") AND ((Dobavljac_Racuni.TipDokumenta) Is Null) AND ((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo])) OR (((GK.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta)=2) AND ((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]))
ORDER BY Dobavljac_Racuni.DatumRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_03_TS
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,Max([GK].[datum]),[Dobavljac_Racuni].[DatumRacuna]) AS DatumRacunaMix, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],"") AS Grupisanje, Round(Sum([DIZNOS]-[PIZNOS]),2)=0 AS Zatvoreno
FROM (((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((TKONTO.TrosakNa) Like "10*") AND ((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],"")
HAVING (((GK.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta) Is Null Or (Dobavljac_Racuni.TipDokumenta)=2))
ORDER BY IIf(Nz([RDOB],0)=0,Max([GK].[datum]),[Dobavljac_Racuni].[DatumRacuna]);


=====QUERY=====
Fin_Izvestaj_SZ_03_TS_DOD
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv AS TKontoNazivGrupa, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv AS TKontoNaziv, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,Max([GK].[datum]),[Dobavljac_Racuni].[DatumRacuna]) AS DatumRacunaMix, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],"") AS Grupisanje, Round(Sum([DIZNOS]-[PIZNOS]),2)=0 AS Zatvoreno
FROM (((Troskovi_PodKonta AS Troskovi_PodKonta_2 INNER JOIN ((GK INNER JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) ON Troskovi_PodKonta_2.PodKonto = TKONTO.GRUPA1) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.TKONTO = Troskovi_PodKonta_1.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd])) OR (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, TKONTO.GRUPA1, Troskovi_PodKonta_2.Naziv, TKONTO.TKONTO, Troskovi_PodKonta_1.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna, IIf(Nz([RDOB],0)=0,[GK].[STAVKAID],"")
HAVING (((GK.KONTO)="5532" Or (GK.KONTO)="4350" Or (GK.KONTO)="5241") AND ((Dobavljac_Racuni.TipDokumenta) Is Null) AND ((Round(Sum([DIZNOS]-[PIZNOS]),2)=0)=0)) OR (((GK.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta)=2) AND ((Round(Sum([DIZNOS]-[PIZNOS]),2)=0)=0))
ORDER BY Dobavljac_Racuni.DatumRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_04
-----SQL-----
SELECT Fin_Izvestaj_SZ_04_sub1.IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1.ID_SK, Fin_Izvestaj_SZ_04_sub1.Dokument, Fin_Izvestaj_SZ_04_sub1.GrupaRacunaFXT, Fin_Izvestaj_SZ_04_sub1.CountOfIDRacun, Fin_Izvestaj_SZ_04_sub1.DatumIzdavanja, Fin_Izvestaj_SZ_04_sub1.DatumValute, Fin_Izvestaj_SZ_04_sub1.Racun.DatumPrometa AS DatumPrometa, Fin_Izvestaj_SZ_04_sub1.SumOfUkupno, Fin_Izvestaj_SZ_04_sub2.SumOfPIZNOS, Fin_Izvestaj_SZ_04_sub2.SumOfDIZNOS, [SumOfDIZNOS]-[SumOfPIZNOS] AS DugF
FROM Fin_Izvestaj_SZ_04_sub1 INNER JOIN Fin_Izvestaj_SZ_04_sub2 ON Fin_Izvestaj_SZ_04_sub1.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2.IDGrupaRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_04_RR
-----SQL-----
SELECT Fin_Izvestaj_SZ_04_sub1.IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1.ID_SK, Fin_Izvestaj_SZ_04_sub1.Dokument, Fin_Izvestaj_SZ_04_sub1.GrupaRacunaFXT, Fin_Izvestaj_SZ_04_sub1.CountOfIDRacun, Fin_Izvestaj_SZ_04_sub1.DatumIzdavanja, Fin_Izvestaj_SZ_04_sub1.DatumValute, Fin_Izvestaj_SZ_04_sub1.Racun.DatumPrometa AS DatumPrometa, Fin_Izvestaj_SZ_04_sub1.SumOfUkupno, Fin_Izvestaj_SZ_04_sub2.SumOfPIZNOS, Fin_Izvestaj_SZ_04_sub2.SumOfDIZNOS, [SumOfDIZNOS]-[SumOfPIZNOS] AS DugF, Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna
FROM Fin_Izvestaj_SZ_04_sub1 INNER JOIN Fin_Izvestaj_SZ_04_sub2 ON Fin_Izvestaj_SZ_04_sub1.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2.IDGrupaRacuna
WHERE (((Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna) Is Null Or (Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna) Like "V*" Or (Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna) Like ""));


=====QUERY=====
Fin_Izvestaj_SZ_04_RR_PS
-----SQL-----
SELECT Fin_Izvestaj_SZ_04_sub1_PS.IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1_PS.ID_SK, Fin_Izvestaj_SZ_04_sub1_PS.Dokument, Fin_Izvestaj_SZ_04_sub1_PS.GrupaRacunaFXT, Fin_Izvestaj_SZ_04_sub1_PS.CountOfIDRacun, Fin_Izvestaj_SZ_04_sub1_PS.DatumIzdavanja, Fin_Izvestaj_SZ_04_sub1_PS.DatumValute, Fin_Izvestaj_SZ_04_sub1_PS.Racun.DatumPrometa AS DatumPrometa, Fin_Izvestaj_SZ_04_sub1_PS.SumOfUkupno, Fin_Izvestaj_SZ_04_sub2_TS.SumOfPIZNOS, Fin_Izvestaj_SZ_04_sub2_TS.SumOfDIZNOS, [SumOfDIZNOS]-[SumOfPIZNOS] AS DugF, Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna, Fin_Izvestaj_SZ_04_sub1_PS.YYYY
FROM Fin_Izvestaj_SZ_04_sub1_PS INNER JOIN Fin_Izvestaj_SZ_04_sub2_TS ON Fin_Izvestaj_SZ_04_sub1_PS.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2_TS.IDGrupaRacuna
WHERE (((Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Is Null Or (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Like "V*" Or (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Like ""));


=====QUERY=====
Fin_Izvestaj_SZ_04_RR_PS_GRP
-----SQL-----
SELECT "" AS IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1_PS.ID_SK, Fin_Izvestaj_SZ_04_sub1_PS.YYYY AS Dokument, "Broj mesecni racuna " & Count([IDGrupaRacuna]) AS GrupaRacunaFXT, Sum(Fin_Izvestaj_SZ_04_sub1_PS.CountOfIDRacun) AS SumOfCountOfIDRacun, "" AS DatumIzdavanja, "" AS DatumValute, Max(Fin_Izvestaj_SZ_04_sub1_PS.MinOfDatumPrometa) AS DatumPrometa, Sum(Fin_Izvestaj_SZ_04_sub1_PS.SumOfUkupno) AS SumOfSumOfUkupno, Sum(Fin_Izvestaj_SZ_04_sub2_TS.SumOfPIZNOS) AS SumOfSumOfPIZNOS, Sum(Fin_Izvestaj_SZ_04_sub2_TS.SumOfDIZNOS) AS SumOfSumOfDIZNOS, Sum([SumOfDIZNOS]-[SumOfPIZNOS]) AS DugF, Fin_Izvestaj_SZ_04_sub1_PS.YYYY, Min(Fin_Izvestaj_SZ_04_sub1_PS.MinOfDatumPrometa) AS MinOfMinOfDatumPrometa, Max(Fin_Izvestaj_SZ_04_sub1_PS.MinOfDatumPrometa) AS MaxOfMinOfDatumPrometa
FROM Fin_Izvestaj_SZ_04_sub1_PS INNER JOIN Fin_Izvestaj_SZ_04_sub2_TS ON Fin_Izvestaj_SZ_04_sub1_PS.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2_TS.IDGrupaRacuna
WHERE (((Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Is Null Or (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Like "V*" Or (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Like ""))
GROUP BY Fin_Izvestaj_SZ_04_sub1_PS.ID_SK, Fin_Izvestaj_SZ_04_sub1_PS.YYYY, Fin_Izvestaj_SZ_04_sub1_PS.YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_04_RR_PSTS_YY
-----SQL-----
SELECT IDGrupaRacuna, ID_SK, Dokument, GrupaRacunaFXT, SumOfCountOfIDRacun, DatumIzdavanja, DatumValute, DatumPrometa, SumOfSumOfUkupno, SumOfSumOfPIZNOS, SumOfSumOfDIZNOS, DugF, YYYY
FROM Fin_Izvestaj_SZ_04_RR_PS_GRP
UNION SELECT IDGrupaRacuna, ID_SK, Dokument, GrupaRacunaFXT, CountOfIDRacun, DatumIzdavanja, DatumValute, DatumPrometa, SumOfUkupno, SumOfPIZNOS, SumOfDIZNOS, DugF, YYYY
FROM Fin_Izvestaj_SZ_04_RR_TS;


=====QUERY=====
Fin_Izvestaj_SZ_04_RR_TS
-----SQL-----
SELECT Fin_Izvestaj_SZ_04_sub1_TS.IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1_TS.ID_SK, Fin_Izvestaj_SZ_04_sub1_TS.Dokument, Fin_Izvestaj_SZ_04_sub1_TS.GrupaRacunaFXT, Fin_Izvestaj_SZ_04_sub1_TS.CountOfIDRacun, Fin_Izvestaj_SZ_04_sub1_TS.DatumIzdavanja, Fin_Izvestaj_SZ_04_sub1_TS.DatumValute, Fin_Izvestaj_SZ_04_sub1_TS.Racun.DatumPrometa AS DatumPrometa, Fin_Izvestaj_SZ_04_sub1_TS.SumOfUkupno, Fin_Izvestaj_SZ_04_sub2_TS.SumOfPIZNOS, Fin_Izvestaj_SZ_04_sub2_TS.SumOfDIZNOS, [SumOfDIZNOS]-[SumOfPIZNOS] AS DugF, Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna, Fin_Izvestaj_SZ_04_sub1_TS.YYYY
FROM Fin_Izvestaj_SZ_04_sub1_TS INNER JOIN Fin_Izvestaj_SZ_04_sub2_TS ON Fin_Izvestaj_SZ_04_sub1_TS.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2_TS.IDGrupaRacuna
WHERE (((Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna) Is Null Or (Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna) Like "V*" Or (Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna) Like ""));


=====QUERY=====
Fin_Izvestaj_SZ_04_sub1
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN] AS Dokument, GrupaRacuna.GrupaRacunaFXT, Count(Racun.IDRacun) AS CountOfIDRacun, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, Sum(Racun.Ukupno) AS SumOfUkupno, Racun.DatumPrometa, GrupaRacuna.MarkerVanderdnihRacuna
FROM GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((Racun.Storno)=False))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN], GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, Racun.DatumPrometa, GrupaRacuna.MarkerVanderdnihRacuna
HAVING (((Racun.DatumPrometa) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
Fin_Izvestaj_SZ_04_sub1_PS
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN] AS Dokument, GrupaRacuna.GrupaRacunaFXT, Count(Racun.IDRacun) AS CountOfIDRacun, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, Min(GrupaRacuna.DatumPrometa) AS MinOfDatumPrometa, Sum(Racun.Ukupno) AS SumOfUkupno, Racun.DatumPrometa, GrupaRacuna.MarkerVanderdnihRacuna, Max(GrupaRacuna.DatumPrometa) AS MaxOfDatumPrometa, Year([GrupaRacuna].[DatumPrometa]) AS YYYY
FROM GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((Racun.Storno)=False))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN], GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, Racun.DatumPrometa, GrupaRacuna.MarkerVanderdnihRacuna, Year([GrupaRacuna].[DatumPrometa])
HAVING (((Racun.DatumPrometa)<[Forms]![Izvestaji]![txtDatumoD]))
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
Fin_Izvestaj_SZ_04_sub1_TS
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN] AS Dokument, GrupaRacuna.GrupaRacunaFXT, Count(Racun.IDRacun) AS CountOfIDRacun, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, Min(GrupaRacuna.DatumPrometa) AS MinOfDatumPrometa, Sum(Racun.Ukupno) AS SumOfUkupno, Racun.DatumPrometa, GrupaRacuna.MarkerVanderdnihRacuna, Max(GrupaRacuna.DatumPrometa) AS MaxOfDatumPrometa, Year([GrupaRacuna].[DatumPrometa]) AS YYYY
FROM GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((Racun.Storno)=False))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN], GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, Racun.DatumPrometa, GrupaRacuna.MarkerVanderdnihRacuna, Year([GrupaRacuna].[DatumPrometa])
HAVING (((Racun.DatumPrometa) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
Fin_Izvestaj_SZ_04_sub2
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN] AS Dokument, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS
FROM GK INNER JOIN (GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) ON GK.RACID = Racun.IDRacun
WHERE (((Racun.Storno)=False) AND ((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN], GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa
HAVING (((GrupaRacuna.DatumPrometa)<=[Forms]![Izvestaji]![txtDatumDo]))
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
Fin_Izvestaj_SZ_04_sub2_PS
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN] AS Dokument, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS
FROM GK INNER JOIN (GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) ON GK.RACID = Racun.IDRacun
WHERE (((Racun.Storno)=False) AND ((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN], GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
Fin_Izvestaj_SZ_04_sub2_TS
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN] AS Dokument, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, Min(GrupaRacuna.DatumPrometa) AS MinOfDatumPrometa, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS
FROM GK INNER JOIN (GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) ON GK.RACID = Racun.IDRacun
WHERE (((Racun.Storno)=False) AND ((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN], GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
Fin_Izvestaj_SZ_04_VR
-----SQL-----
SELECT Fin_Izvestaj_SZ_04_sub1.IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1.ID_SK, Fin_Izvestaj_SZ_04_sub1.Dokument, Fin_Izvestaj_SZ_04_sub1.GrupaRacunaFXT, Fin_Izvestaj_SZ_04_sub1.CountOfIDRacun, Fin_Izvestaj_SZ_04_sub1.DatumIzdavanja, Fin_Izvestaj_SZ_04_sub1.DatumValute, Fin_Izvestaj_SZ_04_sub1.Racun.DatumPrometa AS DatumPrometa, Fin_Izvestaj_SZ_04_sub1.SumOfUkupno, Fin_Izvestaj_SZ_04_sub2.SumOfPIZNOS, Fin_Izvestaj_SZ_04_sub2.SumOfDIZNOS, [SumOfDIZNOS]-[SumOfPIZNOS] AS DugF, Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna
FROM Fin_Izvestaj_SZ_04_sub1 INNER JOIN Fin_Izvestaj_SZ_04_sub2 ON Fin_Izvestaj_SZ_04_sub1.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2.IDGrupaRacuna
WHERE ((Not (Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna) Is Null And (Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna) Not Like "V*" And (Fin_Izvestaj_SZ_04_sub1.MarkerVanderdnihRacuna) Not Like ""));


=====QUERY=====
Fin_Izvestaj_SZ_04_VR_PS
-----SQL-----
SELECT Fin_Izvestaj_SZ_04_sub1_PS.IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1_PS.ID_SK, Fin_Izvestaj_SZ_04_sub1_PS.Dokument, Fin_Izvestaj_SZ_04_sub1_PS.GrupaRacunaFXT, Fin_Izvestaj_SZ_04_sub1_PS.CountOfIDRacun, Fin_Izvestaj_SZ_04_sub1_PS.DatumIzdavanja, Fin_Izvestaj_SZ_04_sub1_PS.DatumValute, Fin_Izvestaj_SZ_04_sub1_PS.Racun.DatumPrometa AS DatumPrometa, Fin_Izvestaj_SZ_04_sub1_PS.SumOfUkupno, Fin_Izvestaj_SZ_04_sub2_TS.SumOfPIZNOS, Fin_Izvestaj_SZ_04_sub2_TS.SumOfDIZNOS, [SumOfDIZNOS]-[SumOfPIZNOS] AS DugF, Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna, Fin_Izvestaj_SZ_04_sub1_PS.YYYY
FROM Fin_Izvestaj_SZ_04_sub1_PS INNER JOIN Fin_Izvestaj_SZ_04_sub2_TS ON Fin_Izvestaj_SZ_04_sub1_PS.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2_TS.IDGrupaRacuna
WHERE ((Not (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Is Null And (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Not Like "V*" And (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Not Like ""));


=====QUERY=====
Fin_Izvestaj_SZ_04_VR_PS_GRP
-----SQL-----
SELECT "" AS IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1_PS.ID_SK, Fin_Izvestaj_SZ_04_sub1_PS.YYYY AS Dokument, "Broj mesecni racuna " & Count([IDGrupaRacuna]) AS GrupaRacunaFXT, Sum(Fin_Izvestaj_SZ_04_sub1_PS.CountOfIDRacun) AS SumOfCountOfIDRacun, "" AS DatumIzdavanja, "" AS DatumValute, "" AS DatumPrometa, Sum(Fin_Izvestaj_SZ_04_sub1_PS.SumOfUkupno) AS SumOfSumOfUkupno, Sum(Fin_Izvestaj_SZ_04_sub2_TS.SumOfPIZNOS) AS SumOfSumOfPIZNOS, Sum(Fin_Izvestaj_SZ_04_sub2_TS.SumOfDIZNOS) AS SumOfSumOfDIZNOS, Sum([SumOfDIZNOS]-[SumOfPIZNOS]) AS DugF, Fin_Izvestaj_SZ_04_sub1_PS.YYYY, Min(Fin_Izvestaj_SZ_04_sub1_PS.MinOfDatumPrometa) AS MinOfMinOfDatumPrometa, Max(Fin_Izvestaj_SZ_04_sub1_PS.MinOfDatumPrometa) AS MaxOfMinOfDatumPrometa
FROM Fin_Izvestaj_SZ_04_sub1_PS INNER JOIN Fin_Izvestaj_SZ_04_sub2_TS ON Fin_Izvestaj_SZ_04_sub1_PS.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2_TS.IDGrupaRacuna
WHERE ((Not (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Is Null And (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Not Like "V*" And (Fin_Izvestaj_SZ_04_sub1_PS.MarkerVanderdnihRacuna) Not Like ""))
GROUP BY Fin_Izvestaj_SZ_04_sub1_PS.ID_SK, "", Fin_Izvestaj_SZ_04_sub1_PS.YYYY, Fin_Izvestaj_SZ_04_sub1_PS.YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_04_VR_PSTS_YY
-----SQL-----
SELECT IDGrupaRacuna, ID_SK, Dokument, GrupaRacunaFXT, SumOfCountOfIDRacun, DatumIzdavanja, DatumValute, DatumPrometa, SumOfSumOfUkupno, SumOfSumOfPIZNOS, SumOfSumOfDIZNOS, DugF, YYYY
FROM Fin_Izvestaj_SZ_04_VR_PS_GRP
UNION SELECT IDGrupaRacuna, ID_SK, Dokument, GrupaRacunaFXT, CountOfIDRacun, DatumIzdavanja, DatumValute, DatumPrometa, SumOfUkupno, SumOfPIZNOS, SumOfDIZNOS, DugF, YYYY
FROM Fin_Izvestaj_SZ_04_VR_TS;


=====QUERY=====
Fin_Izvestaj_SZ_04_VR_TS
-----SQL-----
SELECT Fin_Izvestaj_SZ_04_sub1_TS.IDGrupaRacuna, Fin_Izvestaj_SZ_04_sub1_TS.ID_SK, Fin_Izvestaj_SZ_04_sub1_TS.Dokument, Fin_Izvestaj_SZ_04_sub1_TS.GrupaRacunaFXT, Fin_Izvestaj_SZ_04_sub1_TS.CountOfIDRacun, Fin_Izvestaj_SZ_04_sub1_TS.DatumIzdavanja, Fin_Izvestaj_SZ_04_sub1_TS.DatumValute, Fin_Izvestaj_SZ_04_sub1_TS.Racun.DatumPrometa AS DatumPrometa, Fin_Izvestaj_SZ_04_sub1_TS.SumOfUkupno, Fin_Izvestaj_SZ_04_sub2_TS.SumOfPIZNOS, Fin_Izvestaj_SZ_04_sub2_TS.SumOfDIZNOS, [SumOfDIZNOS]-[SumOfPIZNOS] AS DugF, Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna, Fin_Izvestaj_SZ_04_sub1_TS.YYYY
FROM Fin_Izvestaj_SZ_04_sub1_TS INNER JOIN Fin_Izvestaj_SZ_04_sub2_TS ON Fin_Izvestaj_SZ_04_sub1_TS.IDGrupaRacuna = Fin_Izvestaj_SZ_04_sub2_TS.IDGrupaRacuna
WHERE ((Not (Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna) Is Null And (Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna) Not Like "V*" And (Fin_Izvestaj_SZ_04_sub1_TS.MarkerVanderdnihRacuna) Not Like ""));


=====QUERY=====
Fin_Izvestaj_SZ_05
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Sum(GK.DIZNOS) AS Placeno, Sum(GK.PIZNOS) AS Uplaceno, Kupac.Naziv
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, Kupac.Naziv, GK.KONTO
HAVING (((GK.KONTO)="5532"));


=====QUERY=====
Fin_Izvestaj_SZ_06
-----SQL-----
SELECT frmIzvestajFilterGK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, Sum(frmIzvestajFilterGK.DIZNOS) AS SumOfDIZNOS, Sum(frmIzvestajFilterGK.PIZNOS) AS SumOfPIZNOS, frmIzvestajFilterGK.KONTO
FROM Troskovi_PodKonta INNER JOIN frmIzvestajFilterGK ON Troskovi_PodKonta.PodKonto = frmIzvestajFilterGK.KontoTroska
GROUP BY frmIzvestajFilterGK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, frmIzvestajFilterGK.KONTO
HAVING (((Troskovi_PodKonta.PodKonto) Like "3*"));


=====QUERY=====
Fin_Izvestaj_SZ_06_2_SUB1
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS NazivDobavljaca, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO, GK.KontoTroska, Troskovi_PodKonta.naziv AS NazivKontoTroska, Dobavljac_Racuni.DatumRacuna, IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0) AS [DUG-TMP], IIf(Sum([PIZNOS])=0,Sum([DIZNOS]),Sum([PIZNOS])) AS [OBRACUNATO-TMP], IIf([GK].[KONTO]='5532',0,IIf(Sum([PIZNOS])>0,Sum([PIZNOS]-[DIZNOS]),0)) AS DUG, IIf([GK].[KONTO]='5532' Or [GK].[KONTO]='5241',Sum([DIZNOS]-[PIZNOS]),Sum([PIZNOS])) AS OBRACUNATO, Troskovi_PodKonta.PodKonto, GK.DATUM
FROM ((GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Dobavljac_Racuni.TipDokumenta, GK.RDOB, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv, GK.KontoTroska, Troskovi_PodKonta.naziv, Dobavljac_Racuni.DatumRacuna, Troskovi_PodKonta.PodKonto, GK.DATUM
HAVING (((GK.KONTO)="4350") AND ((Dobavljac_Racuni.TipDokumenta)=3))
ORDER BY Dobavljac_Racuni.DatumRacuna;


=====QUERY=====
Fin_Izvestaj_SZ_06_3
-----SQL-----
SELECT frmIzvestajFilterGK_TS.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, frmIzvestajFilterGK_TS.DATUM, Kupac.ID_K, Kupac.Naziv, frmIzvestajFilterGK_TS.DOK, frmIzvestajFilterGK_TS.DIZNOS, frmIzvestajFilterGK_TS.PIZNOS, frmIzvestajFilterGK_TS.NAPOMENA, frmIzvestajFilterGK_TS.OPIS
FROM Kupac INNER JOIN (Troskovi_PodKonta INNER JOIN frmIzvestajFilterGK_TS ON Troskovi_PodKonta.PodKonto = frmIzvestajFilterGK_TS.KontoTroska) ON Kupac.ID_K = frmIzvestajFilterGK_TS.lnkKUPACID
WHERE (((Troskovi_PodKonta.PodKonto) Not Like "31912" And (Troskovi_PodKonta.PodKonto) Like "3*"));


=====QUERY=====
Fin_Izvestaj_SZ_06_3_PS
-----SQL-----
SELECT GK.lnkSkupstinaID AS ID_SZ, Troskovi_PodKonta.PodKonto, "" AS DATUM, Troskovi_PodKonta.Naziv AS TPKNAZIV, "" AS ID_K, "" AS Partner, "" AS DOK, Sum(GK.DIZNOS) AS DI, Sum(GK.PIZNOS) AS PI, Year([GK].[DATUM]) AS YYYY, Year([GK].[Datum]) & " / " & [Troskovi_PodKonta].[Naziv] AS TPKNAZIVGODINA
FROM (GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]))
GROUP BY GK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, "", Year([GK].[DATUM]), Year([GK].[Datum]) & " / " & [Troskovi_PodKonta].[Naziv], "", "", ""
HAVING (((Troskovi_PodKonta.PodKonto) Not Like "31912" And (Troskovi_PodKonta.PodKonto) Like "3*"));


=====QUERY=====
Fin_Izvestaj_SZ_06_3_TS
-----SQL-----
SELECT GK.lnkSkupstinaID AS ID_SZ, Troskovi_PodKonta.PodKonto, GK.DATUM, Troskovi_PodKonta.Naziv AS TPKNAZIV, Kupac.ID_K, Kupac.Naziv AS Partner, GK.DOK, GK.DIZNOS AS DI, GK.PIZNOS AS PI, Year([DATUM]) AS YYYY, Year([Datum]) & " / " & [Troskovi_PodKonta].[Naziv] AS TPKNAZIVGODINA
FROM (GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((Troskovi_PodKonta.PodKonto) Not Like "31912" And (Troskovi_PodKonta.PodKonto) Like "3*") AND ((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]));


=====QUERY=====
Fin_Izvestaj_SZ_06_3_TSPS_YYYY
-----SQL-----
SELECT ID_SZ, PodKonto, DATUM, TPKNAZIV, ID_K, Partner, DOK, DI, PI, YYYY, TPKNAZIVGODINA
FROM Fin_Izvestaj_SZ_06_3_PS
UNION SELECT ID_SZ, PodKonto, DATUM, TPKNAZIV, ID_K, Partner, DOK, DI, PI, YYYY, TPKNAZIVGODINA
FROM Fin_Izvestaj_SZ_06_3_TS;


=====QUERY=====
Fin_Izvestaj_SZ_07_Stanari_NeRasporedjeno
-----SQL-----
SELECT frmIzvestajFilterGK_TS.lnkSkupstinaID, Sum(frmIzvestajFilterGK_TS.DIZNOS) AS SumOfDIZNOS, Sum(frmIzvestajFilterGK_TS.PIZNOS) AS SumOfPIZNOS
FROM frmIzvestajFilterGK_TS LEFT JOIN Troskovi_PodKonta ON frmIzvestajFilterGK_TS.KontoTroska = Troskovi_PodKonta.PodKonto
WHERE (((frmIzvestajFilterGK_TS.KONTO) Like "204*"))
GROUP BY frmIzvestajFilterGK_TS.lnkSkupstinaID, Troskovi_PodKonta.PodKonto
HAVING (((Troskovi_PodKonta.PodKonto) Is Null));


=====QUERY=====
Fin_Izvestaj_SZ_07_Stanari_NeRasporedjeno_PS
-----SQL-----
SELECT frmIzvestajFilterGK.lnkSkupstinaID, Sum(frmIzvestajFilterGK.DIZNOS) AS SumOfDIZNOS, Sum(frmIzvestajFilterGK.PIZNOS) AS SumOfPIZNOS
FROM frmIzvestajFilterGK LEFT JOIN Troskovi_PodKonta ON frmIzvestajFilterGK.KontoTroska = Troskovi_PodKonta.PodKonto
WHERE (((frmIzvestajFilterGK.KONTO)="2040"))
GROUP BY frmIzvestajFilterGK.lnkSkupstinaID, Troskovi_PodKonta.PodKonto
HAVING (((Troskovi_PodKonta.PodKonto) Is Null));


=====QUERY=====
Fin_Izvestaj_SZ_08
-----SQL-----
SELECT Fin_Izvestaj_SZ_08_sub2_procenat_naplate.lnkSkupstinaID, Fin_Izvestaj_SZ_08_sub2_procenat_naplate.ID_K, Fin_Izvestaj_SZ_08_sub2_procenat_naplate.Naziv, Fin_Izvestaj_SZ_08_sub2_procenat_naplate.SumOfPotrazuju, Fin_Izvestaj_SZ_08_sub2_procenat_naplate.SumOfPlaceno, Fin_Izvestaj_SZ_08_sub2_procenat_naplate.NaplacenoP, Fin_Izvestaj_SZ_08_sub1_procenat_naplate.Naplata, [Naplata]-[NaplacenoP] AS DoplatitiP, ([Naplata]-[NaplacenoP])*[SumOfPotrazuju] AS DoplatitiIznos
FROM Fin_Izvestaj_SZ_08_sub1_procenat_naplate INNER JOIN Fin_Izvestaj_SZ_08_sub2_procenat_naplate ON Fin_Izvestaj_SZ_08_sub1_procenat_naplate.ID_SK = Fin_Izvestaj_SZ_08_sub2_procenat_naplate.lnkSkupstinaID;


=====QUERY=====
Fin_Izvestaj_SZ_08_sub1_procenat_naplate
-----SQL-----
SELECT Fin_Izvestaj_SZ_04.ID_SK, Sum([SumOfPIZNOS])/Sum([SumOfUkupno]) AS Naplata
FROM Fin_Izvestaj_SZ_04
GROUP BY Fin_Izvestaj_SZ_04.ID_SK;


=====QUERY=====
Fin_Izvestaj_SZ_08_sub2_procenat_naplate
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub5.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub5.ID_K, Fin_Izvestaj_SZ_02_sub5.Kupac.Naziv, Sum(Fin_Izvestaj_SZ_02_sub5.Potrazuju) AS SumOfPotrazuju, Sum(Fin_Izvestaj_SZ_02_sub5.Placeno) AS SumOfPlaceno, Sum([Placeno])/Sum([Potrazuju]) AS NaplacenoP
FROM Fin_Izvestaj_SZ_02_sub5
WHERE (((Fin_Izvestaj_SZ_02_sub5.PodKonto) Like "11*"))
GROUP BY Fin_Izvestaj_SZ_02_sub5.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub5.ID_K, Fin_Izvestaj_SZ_02_sub5.Kupac.Naziv;


=====QUERY=====
Fin_Izvestaj_SZ_09
-----SQL-----
SELECT Fin_Izvestaj_SZ_09_sub1_4350.lnkSkupstinaID, Fin_Izvestaj_SZ_09_sub1_4350.ID_K, Fin_Izvestaj_SZ_09_sub1_4350.Naziv, Fin_Izvestaj_SZ_09_sub1_4350.SumOfPotrazuju, Fin_Izvestaj_SZ_09_sub1_4350.SumOfPlaceno, Fin_Izvestaj_SZ_09_sub2_2040.Naplaceno, ([Naplaceno]-[SumOfPlaceno]) AS DoplatitiIznos, "" AS YYYY, Fin_Izvestaj_SZ_09_sub1_4350.YYYY AS SortYYYY
FROM Fin_Izvestaj_SZ_09_sub2_2040 INNER JOIN Fin_Izvestaj_SZ_09_sub1_4350 ON (Fin_Izvestaj_SZ_09_sub1_4350.ID_K = Fin_Izvestaj_SZ_09_sub2_2040.DobavljacKonto) AND (Fin_Izvestaj_SZ_09_sub2_2040.ID_SK = Fin_Izvestaj_SZ_09_sub1_4350.lnkSkupstinaID);


=====QUERY=====
Fin_Izvestaj_SZ_09_PS
-----SQL-----
SELECT Fin_Izvestaj_SZ_09_sub1_4350_PS.lnkSkupstinaID, Fin_Izvestaj_SZ_09_sub1_4350_PS.ID_K, Fin_Izvestaj_SZ_09_sub1_4350_PS.Naziv, Fin_Izvestaj_SZ_09_sub1_4350_PS.SumOfPotrazuju, Fin_Izvestaj_SZ_09_sub1_4350_PS.SumOfPlaceno, Fin_Izvestaj_SZ_09_sub2_2040_PS.Naplaceno, ([Naplaceno]-[SumOfPlaceno]) AS DoplatitiIznos, Fin_Izvestaj_SZ_09_sub2_2040_PS.YYYY, Fin_Izvestaj_SZ_09_sub2_2040_PS.YYYY AS SortYYYY
FROM Fin_Izvestaj_SZ_09_sub2_2040_PS INNER JOIN Fin_Izvestaj_SZ_09_sub1_4350_PS ON (Fin_Izvestaj_SZ_09_sub2_2040_PS.YYYY = Fin_Izvestaj_SZ_09_sub1_4350_PS.YYYY) AND (Fin_Izvestaj_SZ_09_sub2_2040_PS.ID_SK = Fin_Izvestaj_SZ_09_sub1_4350_PS.lnkSkupstinaID) AND (Fin_Izvestaj_SZ_09_sub2_2040_PS.DobavljacKonto = Fin_Izvestaj_SZ_09_sub1_4350_PS.ID_K)
ORDER BY Fin_Izvestaj_SZ_09_sub2_2040_PS.YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_09_PSTS_YY
-----SQL-----
SELECT lnkSkupstinaID, ID_K, Naziv, SumOfPotrazuju, SumOfPlaceno, Naplaceno, DoplatitiIznos, YYYY, SortYYYY, 2 as ForceSort
FROM Fin_Izvestaj_SZ_09_PS ORDER BY SortYYYY;
UNION SELECT lnkSkupstinaID, ID_K, Naziv, SumOfPotrazuju, SumOfPlaceno, Naplaceno, DoplatitiIznos, YYYY, SortYYYY, 1 as ForceSort
FROM Fin_Izvestaj_SZ_09
ORDER BY ForceSort DESC , SortYYYY;


=====QUERY=====
Fin_Izvestaj_SZ_09_sub1_4350
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub5.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub5.ID_K, Fin_Izvestaj_SZ_02_sub5.Kupac.Naziv, Sum(Fin_Izvestaj_SZ_02_sub5.Potrazuju) AS SumOfPotrazuju, Sum(Fin_Izvestaj_SZ_02_sub5.Placeno) AS SumOfPlaceno, Sum([Placeno])/Sum([Potrazuju]) AS NaplacenoP, Fin_Izvestaj_SZ_02_sub5.YYYY
FROM Fin_Izvestaj_SZ_02_sub5
WHERE (((Fin_Izvestaj_SZ_02_sub5.PodKonto) Like "11*"))
GROUP BY Fin_Izvestaj_SZ_02_sub5.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub5.ID_K, Fin_Izvestaj_SZ_02_sub5.Kupac.Naziv, Fin_Izvestaj_SZ_02_sub5.YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_09_sub1_4350_PS
-----SQL-----
SELECT Fin_Izvestaj_SZ_02_sub5_PS.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub5_PS.ID_K, Fin_Izvestaj_SZ_02_sub5_PS.Kupac.Naziv, Sum(Fin_Izvestaj_SZ_02_sub5_PS.Potrazuju) AS SumOfPotrazuju, Sum(Fin_Izvestaj_SZ_02_sub5_PS.Placeno) AS SumOfPlaceno, Sum([Placeno])/Sum([Potrazuju]) AS NaplacenoP, Fin_Izvestaj_SZ_02_sub5_PS.YYYY, Fin_Izvestaj_SZ_02_sub5_PS.YYYY
FROM Fin_Izvestaj_SZ_02_sub5_PS
WHERE (((Fin_Izvestaj_SZ_02_sub5_PS.PodKonto) Like "11*"))
GROUP BY Fin_Izvestaj_SZ_02_sub5_PS.lnkSkupstinaID, Fin_Izvestaj_SZ_02_sub5_PS.ID_K, Fin_Izvestaj_SZ_02_sub5_PS.Kupac.Naziv, Fin_Izvestaj_SZ_02_sub5_PS.YYYY;


=====QUERY=====
Fin_Izvestaj_SZ_09_sub2_2040
-----SQL-----
SELECT GK.lnkSkupstinaID AS ID_SK, Sum(GK.PIZNOS) AS Naplaceno, Dobavljac_Racuni.DobavljacKonto
FROM Dobavljac_Racuni INNER JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB
WHERE (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]) AND ((GK.KontoTroska) Like "11*"))
GROUP BY GK.lnkSkupstinaID, Dobavljac_Racuni.DobavljacKonto, GK.KONTO
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
Fin_Izvestaj_SZ_09_sub2_2040_PS
-----SQL-----
SELECT GK.lnkSkupstinaID AS ID_SK, Sum(GK.PIZNOS) AS Naplaceno, Dobavljac_Racuni.DobavljacKonto, Year([DATUM]) AS YYYY
FROM Dobavljac_Racuni INNER JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]) AND ((GK.KontoTroska) Like "11*"))
GROUP BY GK.lnkSkupstinaID, Dobavljac_Racuni.DobavljacKonto, Year([DATUM]), GK.KONTO
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
Fin_Izvestaj_SZ_09_sub2_2040_TS
-----SQL-----
SELECT GK.lnkSkupstinaID AS ID_SK, Sum(GK.PIZNOS) AS Naplaceno, Dobavljac_Racuni.DobavljacKonto, GK.DATUM, Year([DATUM]) AS YYYY
FROM Dobavljac_Racuni INNER JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RDOB
WHERE (((GK.KontoTroska) Like "11*"))
GROUP BY GK.lnkSkupstinaID, Dobavljac_Racuni.DobavljacKonto, GK.DATUM, GK.KONTO, Year([DATUM])
HAVING (((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]) AND ((GK.KONTO)="2040"));


=====QUERY=====
Fin_Izvestaj_SZ_11
-----SQL-----
SELECT GK.KontoTroska, GK.DATUM, GK.KONTO, Kupac.ID_K, Kupac.Naziv, GK.DOK, GK.DIZNOS, GK.PIZNOS, Year([Datum]) AS yy, Month([Datum]) AS mm
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.KontoTroska) Like "3*" And Not (GK.KontoTroska)="31912"))
ORDER BY GK.DOK, GK.DATUM;


=====QUERY=====
Find duplicates for Izvod
-----SQL-----
SELECT Izvod.[NalogZaKnjizenje], Izvod.[IzvodID], Izvod.[BrojIzvoda], Izvod.[SufixIzvoda], Izvod.[ID_SK], Izvod.[Datum]
FROM Izvod
WHERE (((Izvod.[NalogZaKnjizenje]) In (SELECT [NalogZaKnjizenje] FROM [Izvod] As Tmp GROUP BY [NalogZaKnjizenje] HAVING Count(*)>1 )))
ORDER BY Izvod.[NalogZaKnjizenje];


=====QUERY=====
Find duplicates for Objekti
-----SQL-----
SELECT Objekti.SifraPD, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.naziv, Objekti.lnk_ID_K, Objekti.lnk_tip, Objekti.Status, Objekti.IO, Objekti.RF_DIN, Objekti.IONaslov, Objekti.StaraKv, Objekti.STatusPromene, Objekti.Koeficijent, Objekti.Ukupno, Objekti.Ulaz, Objekti.Kategorija, Objekti.Adresa, Objekti.Napomena, Objekti.Naziv_Slanja, Objekti.Adresa_Slanja, Objekti.PBroj_Slanja, Objekti.PIB_Slanja, Objekti.PLATILAC_lnk_ID_K, Objekti.PLATILAC_lnk_ID_K2, Objekti.BRGM, Objekti.K1, Objekti.K2, Objekti.K3, Objekti.K4, Objekti.K5, Objekti.PAK_SLANJA, Objekti.KV, Objekti.BrStanara, Objekti.BrojPD, Objekti.IDVlasnik, Objekti.IDZakupac, Objekti.IDGrupnogRacuna, Objekti.netoKV, Objekti.terasa, Objekti.netoKVsaTerasom, Objekti.HandOverDate
FROM Objekti
WHERE (((Objekti.SifraPD) In (SELECT [SifraPD] FROM [Objekti] As Tmp GROUP BY [SifraPD] HAVING Count(*)>1 )))
ORDER BY Objekti.SifraPD;


=====QUERY=====
Find duplicates for Racun
-----SQL-----
SELECT Racun.[ID_K], Racun.[IDRacun], Racun.[RBR], Racun.[lnkGR], Racun.[DatumIzdavanja], Racun.[MestoIzdavanja], Racun.[DatumUsluge], Racun.[DatumPrometa], Racun.[DatumValute], Racun.[ID_SK], Racun.[Kupac], Racun.[PBroj_K], Racun.[Adresa_K], Racun.[PIB], Racun.[MB], Racun.[Suma], Racun.[PDVStopa], Racun.[PDVIznos], Racun.[Ukupno], Racun.[PrethodniDug], Racun.[SvrhaUplate], Racun.[Valuta], Racun.[PozivNaBroj], Racun.[PD_text], Racun.[Napomena], Racun.[Co], Racun.[Storno], Racun.[extraNapomena], Racun.[objekatNaziv], Racun.[Upravnik], Racun.[SvrhaUplate2], Racun.[ID_OX], Racun.[AdresaProstora], Racun.[ZaUplatu], Racun.[Naziv_Slanja], Racun.[Adresa_Slanja], Racun.[PBroj_Slanja], Racun.[PIB_Slanja], Racun.[tmpID], Racun.[tmpID2], Racun.[tmpPB], Racun.[tmpPB2], Racun.[SPC], Racun.[PD_iznos], Racun.[TipPoljaZaUplatu], Racun.[lnkOpomenaID], Racun.[IDKGrupniRacun], Racun.[Grad_K], Racun.[Lokacija], Racun.[SortRacun], Racun.[DatumStorno], Racun.[RacunShema]
FROM Racun
WHERE (((Racun.[ID_K]) In (SELECT [ID_K] FROM [Racun] As Tmp GROUP BY [ID_K] HAVING Count(*)>1 )))
ORDER BY Racun.[ID_K];


=====QUERY=====
Find duplicates for RacunObjekti
-----SQL-----
SELECT First(RacunObjekti.[IDRacun]) AS [IDRacun Field], First(RacunObjekti.[IDObjekat]) AS [IDObjekat Field], Count(RacunObjekti.[IDRacun]) AS NumberOfDups
FROM RacunObjekti
GROUP BY RacunObjekti.[IDRacun], RacunObjekti.[IDObjekat]
HAVING (((Count(RacunObjekti.[IDRacun]))>1) AND ((Count(RacunObjekti.[IDObjekat]))>1));


=====QUERY=====
Find duplicates for RacunStavke
-----SQL-----
SELECT RacunStavke.[ID_RDOB], RacunStavke.[IDRacunStavke], RacunStavke.[ID_R], RacunStavke.[lnkGR], RacunStavke.[ID_K], RacunStavke.[ID_SK]
FROM RacunStavke
WHERE (((RacunStavke.[ID_RDOB]) In (SELECT [ID_RDOB] FROM [RacunStavke] As Tmp GROUP BY [ID_RDOB] HAVING Count(*)>1 )))
ORDER BY RacunStavke.[ID_RDOB];


=====QUERY=====
Find duplicates for Table2
-----SQL-----
SELECT Table2.[Field1], Table2.[ID]
FROM Table2
WHERE (((Table2.[Field1]) In (SELECT [Field1] FROM [Table2] As Tmp GROUP BY [Field1] HAVING Count(*)>1 )))
ORDER BY Table2.[Field1];


=====QUERY=====
FIX_NULLS
-----SQL-----
UPDATE Racun SET Racun.PrethodniDug = 0
WHERE (((Racun.PrethodniDug) Is Null));


=====QUERY=====
Fix_Rdob_Table5
-----SQL-----
UPDATE GK INNER JOIN Table5 ON GK.STAVKAID = Table5.StavkaID SET GK.RDOB = [NewRdob];


=====QUERY=====
Fix_RFob
-----SQL-----
SELECT [IDTRRAC]=[RDOB] AS Expr1, GK.STAVKAID, Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.SK_ID, RacunStavke.UkupnoRSD, GK.DATUM, GK.DIZNOS, GK.PIZNOS, GK.lnkIzvodStavkaID, GK.RDOB, GK.RACID, GK.RacunIN_ID, RacunStavke.ID_K
FROM (RacunStavke INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN GK ON (RacunStavke.ID_K = GK.lnkKUPACID) AND (RacunStavke.ID_R = GK.RACID) AND (Dobavljac_Racuni.KontoKnjizenja = GK.KontoTroska) AND (GK.PIZNOS = RacunStavke.UkupnoRSD)
WHERE ((([IDTRRAC]=[RDOB])=0) AND ((GK.lnkSkupstinaID)=129));


=====QUERY=====
Fix_SortRacun
-----SQL-----
UPDATE (RacunStavke INNER JOIN Objekti ON RacunStavke.ID_O = Objekti.ID_O) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj SET RacunStavke.Sort = TipObjekta.SortObj;


=====QUERY=====
FIX_TEMP_KUPAC_PDBD
-----SQL-----
UPDATE Kupac SET Kupac.PBPD_F = Replace(kupac.PBPD,"-","")
WHERE (((Kupac.PBPD) Is Not Null));


=====QUERY=====
FIX001_Izvod_Rasknjizenje
-----SQL-----
UPDATE (IzvodStavke LEFT JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID) INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID SET Izvod.Napomena = Null
WHERE (((Izvod.Napomena)="R") AND ((GK.lnkIzvodStavkaID) Is Null));


=====QUERY=====
FIX002_RACUN_CUSTOM-NAPOMENA
-----SQL-----
UPDATE Racun SET Racun.extraNapomena = [UNESITE TEKST NAPOMENE]
WHERE (((Racun.lnkGR)=[UNESITE ID GRUPE RACUNA]) AND ((Racun.ID_SK)=[UNESITE ID SKUPSTINE]));


=====QUERY=====
FIX003_ERROR_010-STAVKE-GK-DOBAVLJACI-BEZ-SK
-----SQL-----
UPDATE (Skustina INNER JOIN (GK INNER JOIN (IzvodStavke INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID) ON GK.lnkIzvodStavkaID = IzvodStavke.ID) ON Skustina.IDSkupstina = Izvod.ID_SK) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K SET GK.lnkSkupstinaID = Izvod.ID_SK
WHERE (((GK.lnkSkupstinaID)=0));


=====QUERY=====
fltFormDobavljaci
-----SQL-----
SELECT Dobavljac_Racuni.SK_ID, Count(Dobavljac_Racuni.IDTRRAC) AS BRP
FROM Dobavljac_Racuni
WHERE (((Dobavljac_Racuni.MesecRacuna)=Forms!GrupaRacuna_Add!txtYYMM Or (Dobavljac_Racuni.MesecRacuna) Is Null) And ((Dobavljac_Racuni.TipDokumenta)=1))
GROUP BY Dobavljac_Racuni.SK_ID;


=====QUERY=====
fltFormRacuni
-----SQL-----
SELECT Count(Racun.IDRacun) AS BRR, Racun.ID_SK
FROM Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GrupaRacuna.GrupaRacunaFXN)=Forms!GrupaRacuna_Add!txtYYMM Or (GrupaRacuna.GrupaRacunaFXN) Is Null) And ((GrupaRacuna.MarkerVanderdnihRacuna) Is Null))
GROUP BY Racun.ID_SK;


=====QUERY=====
fltFormRacuniVR
-----SQL-----
SELECT GrupaRacuna.ID_SK, Count(GrupaRacuna.MarkerVanderdnihRacuna) AS CountOfMarkerVanderdnihRacuna
FROM GrupaRacuna
WHERE (((GrupaRacuna.GrupaRacunaFXN)=Forms!GrupaRacuna_Add!txtYYMM Or (GrupaRacuna.GrupaRacunaFXN) Is Null) And ((GrupaRacuna.MarkerVanderdnihRacuna) Is Not Null))
GROUP BY GrupaRacuna.ID_SK;


=====QUERY=====
fltMarkerList
-----SQL-----
SELECT tblShortList.Index, tblShortList.ShortName
FROM tblShortList
WHERE (((tblShortList.TableFrom)="MarkerRacuna"));


=====QUERY=====
frmIzvestaj_Dobavljaci
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Sum(frmIzvestajFilterGK.DIZNOS) AS Duguje, Sum(frmIzvestajFilterGK.PIZNOS) AS Potražuje, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Stanje, First(frmIzvestajFilterGK.lnkSkupstinaID) AS FirstOflnkSkupstinaID
FROM frmIzvestajFilterGK RIGHT JOIN Kupac ON frmIzvestajFilterGK.lnkKUPACID = Kupac.ID_K
WHERE (((Kupac.KONTO)="4350" Or (Kupac.KONTO)="5532"))
GROUP BY Kupac.ID_K, Kupac.Naziv, frmIzvestajFilterGK.lnkKUPACID
HAVING (((First(frmIzvestajFilterGK.lnkSkupstinaID))=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
frmIzvestaj_GK_Filter_001
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.DATUM)>=#1/1/2019# And (GK.DATUM)<=#3/16/2019#) AND ((GK.KONTO) Like '2040*') AND ((GK.lnkSkupstinaID)=101));


=====QUERY=====
frmIzvestaj_GK_Filter_001_Crosstab
-----SQL-----
TRANSFORM Sum(frmIzvestaj_GK_Filter_001.[PIZNOS]) AS Dug
SELECT frmIzvestaj_GK_Filter_001.[lnkKUPACID], Round(Sum([DIZNOS]-[PIZNOS]),2) AS Stanje
FROM frmIzvestaj_GK_Filter_001
GROUP BY frmIzvestaj_GK_Filter_001.[lnkKUPACID]
PIVOT frmIzvestaj_GK_Filter_001.[SIFRAKN];


=====QUERY=====
frmIzvestaj_Izvod
-----SQL-----
SELECT Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda AS Godina, GK.lnkKUPACID AS KONTOPARTNERA, Kupac.Naziv AS NazivPartnera, Count(GK.STAVKAID) AS BrojKnjizenja, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS, GK.KontoTroska, Troskovi_PodKonta.Naziv AS NazivTroska, GK.RacunIN_ID
FROM ((((Izvod INNER JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID) INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina) LEFT JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID) LEFT JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) LEFT JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
GROUP BY Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, GK.lnkKUPACID, Kupac.Naziv, GK.KontoTroska, Troskovi_PodKonta.Naziv, GK.RacunIN_ID, IzvodStavke.RbStavke
HAVING (((Izvod.ID_SK)=Forms!Izvestaji!cmbSZ) And ((Izvod.Datum)>=Forms!Izvestaji!txtDatumOd And (Izvod.Datum)<=Forms!Izvestaji!txtDatumDo))
ORDER BY Izvod.Datum, IzvodStavke.RbStavke;


=====QUERY=====
frmIzvestaj_Izvod_grp
-----SQL-----
SELECT Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda AS Godina, GK.lnkKUPACID AS KONTOPARTNERA, Kupac.Naziv AS NazivPartnera, Count(GK.STAVKAID) AS BrojKnjizenja, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS, Izvod.IzvodID, IzvodStavke.NazivPN, IzvodStavke.Doznaka, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje
FROM (((Izvod INNER JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID) INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina) LEFT JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID) LEFT JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
GROUP BY Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, GK.lnkKUPACID, Kupac.Naziv, Izvod.IzvodID, IzvodStavke.NazivPN, IzvodStavke.Doznaka, IzvodStavke.RbStavke, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje
HAVING (((Izvod.ID_SK)=Forms!Izvestaji!cmbSZ) And ((Izvod.Datum)>=Forms!Izvestaji!txtDatumOd And (Izvod.Datum)<=Forms!Izvestaji!txtDatumDo))
ORDER BY Izvod.Datum, IzvodStavke.RbStavke;


=====QUERY=====
frmIzvestaj_Objekti
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv AS Korisnik, ObjektiSaTipomKoef.naziv AS Prostor, Kupac.lnk_ID_SK, ObjektiSaTipomKoef.K1, ObjektiSaTipomKoef.K2, ObjektiSaTipomKoef.K3, ObjektiSaTipomKoef.K4, ObjektiSaTipomKoef.K5, ObjektiSaTipomKoef.SortObj, ObjektiSaTipomKoef.BrojPD, ObjektiSaTipomKoef.TipObj, ObjektiSaTipomKoef.ID_O, ObjektiSaTipomKoef.Ulaz, ObjektiSaTipomKoef.SifraPD, ObjektiSaTipomKoef.Status
FROM Kupac INNER JOIN ObjektiSaTipomKoef ON Kupac.ID_K = ObjektiSaTipomKoef.lnk_ID_K
WHERE (((Kupac.lnk_ID_SK)=[Forms]![Izvestaji]![cmbSZ]))
ORDER BY ObjektiSaTipomKoef.SortObj, ObjektiSaTipomKoef.BrojPD, ObjektiSaTipomKoef.naziv;


=====QUERY=====
frmIzvestaj_Stanari
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv AS Korisnik, First(ObjektiSaTipom.naziv) AS Prostor, Kupac.lnk_ID_SK, SpojRedove("SifraPD","Objekti","lnk_ID_K=" & [ID_K],"BrojPD") AS PD
FROM ObjektiSaTipom RIGHT JOIN Kupac ON ObjektiSaTipom.ID_O = Kupac.DostavaSifraPD
WHERE (((Kupac.lnk_ID_SK)=[Forms]![Izvestaji]![cmbSZ]))
GROUP BY Kupac.ID_K, Kupac.Naziv, Kupac.lnk_ID_SK, SpojRedove("SifraPD","Objekti","lnk_ID_K=" & [ID_K],"BrojPD")
ORDER BY Min(ObjektiSaTipom.SorterFn);


=====QUERY=====
frmIzvestaj_StanariBrCl
-----SQL-----
SELECT Objekti.ID_O, Objekti.naziv AS [Naziv posebnog dela], Kupac.ID_K, Kupac.Naziv AS Korisnik, Objekti.BrStanara AS [Broj clanova domacinstva], Objekti.K1, Objekti.K2, Objekti.K3, Objekti.K4, Objekti.K5
FROM (Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Objekti.BrStanara)>0) And ((Objekti.lnkSkupstinaID)=Forms!Izvestaji!cmbSZ))
ORDER BY TipObjekta.SortObj, Objekti.BrojPD, Objekti.naziv;


=====QUERY=====
frmIzvestaj_StanariStanje
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Sum(frmIzvestajFilterGK.DIZNOS) AS Duguje, Sum(frmIzvestajFilterGK.PIZNOS) AS Potražuje, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Stanje, Objekti.SifraPD AS Prostor, First(Kupac.lnk_ID_SK) AS FirstOflnk_ID_SK, IIf(Round(Sum([DIZNOS]-[PIZNOS]),2)>0,Round(Sum([DIZNOS]-[PIZNOS]),2),0) AS DI, IIf(Round(Sum([DIZNOS]-[PIZNOS]),2)<0,-Round(Sum([DIZNOS]-[PIZNOS]),2),0) AS PI, Objekti.BrojPD, IIf(Round(Sum([DIZNOS]-[PIZNOS]),2)<-0.001,-1,IIf(Round(Sum([DIZNOS]-[PIZNOS]),2)>-0.001 And Round(Sum([DIZNOS]-[PIZNOS]),2)<0.001,0,IIf(Sum([PIZNOS])=0,2,1))) AS Grupa, IIf([IDGrupniRacunMaster]=0,-1,[IDGrupniRacunMaster]=[ID_K]) AS FilterKupac, Nz([Total],0) AS BNR
FROM ((frmIzvestajFilterGK RIGHT JOIN Kupac ON frmIzvestajFilterGK.lnkKUPACID = Kupac.ID_K) LEFT JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O) LEFT JOIN GK_GRP_DOK_COUNT_BRMD ON Kupac.ID_K = GK_GRP_DOK_COUNT_BRMD.lnkKUPACID
WHERE (((Kupac.KONTO)="2040"))
GROUP BY Kupac.ID_K, Kupac.Naziv, Objekti.SifraPD, Objekti.BrojPD, IIf([IDGrupniRacunMaster]=0,-1,[IDGrupniRacunMaster]=[ID_K]), frmIzvestajFilterGK.lnkKUPACID, Nz([Total],0)
HAVING (((First(Kupac.lnk_ID_SK))=[Forms]![Izvestaji]![cmbSZ]) AND ((IIf([IDGrupniRacunMaster]=0,-1,[IDGrupniRacunMaster]=[ID_K]))=-1))
ORDER BY Objekti.BrojPD;


=====QUERY=====
frmIzvestaj_StanariStanje_Duguju
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Sum(frmIzvestajFilterGK.DIZNOS) AS Duguje, Sum(frmIzvestajFilterGK.PIZNOS) AS Potražuje, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Stanje, ObjektiSaTipomPrvi.FirstOfnaziv AS Prostor, First(Kupac.lnk_ID_SK) AS FirstOflnk_ID_SK
FROM (frmIzvestajFilterGK RIGHT JOIN Kupac ON frmIzvestajFilterGK.lnkKUPACID = Kupac.ID_K) LEFT JOIN ObjektiSaTipomPrvi ON Kupac.ID_K = ObjektiSaTipomPrvi.lnk_ID_K
WHERE (((Kupac.KONTO)="2040"))
GROUP BY Kupac.ID_K, Kupac.Naziv, ObjektiSaTipomPrvi.FirstOfnaziv, frmIzvestajFilterGK.lnkKUPACID
HAVING (((Round(Sum([DIZNOS]-[PIZNOS]),2))>0) And ((First(Kupac.lnk_ID_SK))=Forms!Izvestaji!cmbSZ));


=====QUERY=====
frmIzvestajFilterGK
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.DATUM) Between Forms!Izvestaji!txtDatumOd And Forms!Izvestaji!txtDatumDo) And ((GK.TIP_STAVKE)<>98) And ((GK.KNzaTIP)<>98) And ((GK.lnkSkupstinaID)=Forms!Izvestaji!cmbSZ));


=====QUERY=====
frmIzvestajFilterGK_PS
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.DATUM)<[Forms]![Izvestaji]![txtDatumOd]) AND ((GK.TIP_STAVKE)<>98) AND ((GK.KNzaTIP)<>98) AND ((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
frmIzvestajFilterGK_TS
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]) AND ((GK.TIP_STAVKE)<>98) AND ((GK.KNzaTIP)<>98) AND ((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
frmIzvestajFilterGKNoSZ
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.DATUM)>=[Forms]![Izvestaji]![txtDatumOd] And (GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]));


=====QUERY=====
GEN_R_001
-----SQL-----
INSERT INTO RacunStavke ( Objekat, lnkGR, ID_K, ID_O, ID_SK, KOL, JM, JO )
SELECT Objekti.naziv, 1 AS Expr3, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekti.JM, Objekti.JO
FROM Objekti LEFT JOIN Kupac ON Objekti.lnk_ID_K=Kupac.ID_K
WHERE (((Objekti.Status)=0));


=====QUERY=====
GEN_R_002
-----SQL-----
UPDATE RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR=GrupaRacuna.IDGrupaRacuna SET RacunStavke.NBS = GrupaRacuna.NBS, RacunStavke.Cena = RacunStavke.KOL*RacunStavke.JO, RacunStavke.Ukupno = Round(RacunStavke.KOL*RacunStavke.JO*GrupaRacuna.NBS,2)
WHERE (((GrupaRacuna.IDGrupaRacuna)=1));


=====QUERY=====
GEN_R_003
-----SQL-----
INSERT INTO Racun ( ID_K, ID_SK, Ukupno, Co, DatumIzdavanja, MestoIzdavanja, DatumUsluge, Kupac, PBroj_K, Adresa_K, PIB, StanjePredhodniDug, StanjeDug, Valuta, ObavestanjePDug, PozivNaBrojPDug, DatumValute, SvrhaUplate, PozivNaBroj, lnkGR )
SELECT RacunStavke.ID_K, RacunStavke.ID_SK, Sum(RacunStavke.Ukupno) AS SumOfUkupno, Count(RacunStavke.IDRacunStavke) AS CountOfIDRacunStavke, GrupaRacuna.DatumIzdavanja, GrupaRacuna.Mesto, GrupaRacuna.DatumUsluge, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, StanjePP.Stanje, StanjeSveBezPP.Stanje, "RSD" AS Expr1, "*Molimo Vas da izmirite dugovanje koje se odnosi na period do 31.03.2012. uplatom na iskazani tekuci racun sa pozivom na broj  " AS Expr2, StanjePP.FirstOfNAPOMENA, GrupaRacuna.DatumValute, "RACUN ZA ODRŽAVANJE STAMBENOG NASELJA BELVILLE - " & GrupaRacuna.GrupaRacunaFXT AS Expr3, KontrolniBroj(97,[GrupaRacunaFXN] & "-" & Format(RacunStavke.ID_SK,'00') & "-" & Format(RacunStavke.ID_K,'0000')) & "-" & [GrupaRacunaFXN] & "-" & Format(RacunStavke.ID_SK,'00') & "-" & Format(RacunStavke.ID_K,'0000') AS PB, RacunStavke.lnkGR
FROM StanjeSveBezPP RIGHT JOIN (((RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR=GrupaRacuna.IDGrupaRacuna) INNER JOIN Kupac ON RacunStavke.ID_K=Kupac.ID_K) LEFT JOIN StanjePP ON (RacunStavke.ID_SK=StanjePP.lnkSkupstinaID) AND (RacunStavke.ID_K=StanjePP.lnkKUPACID)) ON (StanjeSveBezPP.lnkSkupstinaID=RacunStavke.ID_SK) AND (StanjeSveBezPP.lnkKUPACID=RacunStavke.ID_K)
WHERE (((RacunStavke.lnkGR)=1))
GROUP BY RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.Mesto, GrupaRacuna.DatumUsluge, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, StanjePP.Stanje, StanjeSveBezPP.Stanje, "RSD", "*Molimo Vas da izmirite dugovanje koje se odnosi na period do 31.03.2012. uplatom na iskazani tekuci racun sa pozivom na broj  ", StanjePP.FirstOfNAPOMENA, GrupaRacuna.DatumValute, "RACUN ZA ODRŽAVANJE STAMBENOG NASELJA BELVILLE - " & GrupaRacuna.GrupaRacunaFXT, KontrolniBroj(97,[GrupaRacunaFXN] & "-" & Format(RacunStavke.ID_SK,'00') & "-" & Format(RacunStavke.ID_K,'0000')) & "-" & [GrupaRacunaFXN] & "-" & Format(RacunStavke.ID_SK,'00') & "-" & Format(RacunStavke.ID_K,'0000'), RacunStavke.lnkGR;


=====QUERY=====
GEN_R_010
-----SQL-----
UPDATE Racun SET Racun.StanjePredhodniDug = 0
WHERE ((([Racun].[StanjePredhodniDug]) Is Null) AND ((Racun.lnkGR)=1));


=====QUERY=====
GEN_R_011
-----SQL-----
UPDATE Racun SET Racun.StanjeDug = 0
WHERE ((([Racun].[StanjeDug]) Is Null) AND ((Racun.lnkGR)=1));


=====QUERY=====
GEN_R_012
-----SQL-----
UPDATE Racun SET Racun.UkupnoDug = Racun.Ukupno+Racun.StanjeDug
WHERE (((Racun.lnkGR)=1));


=====QUERY=====
GEN_R_013
-----SQL-----
UPDATE Racun INNER JOIN RacunStavke ON (Racun.ID_K = RacunStavke.ID_K) AND (Racun.lnkGR = RacunStavke.lnkGR) SET RacunStavke.ID_R = Racun.IDRacun
WHERE (((Racun.lnkGR)=8));


=====QUERY=====
GEN_R_020
-----SQL-----
INSERT INTO GK ( BR_NALOG, DATUM, DIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, DPO, PARAMETRI )
SELECT 1 AS Expr3, Racun.DatumIzdavanja, Racun.Ukupno, 3 AS Expr1, Racun.ID_SK, Racun.ID_K, "RAC" & [PozivNaBroj] AS Expr2, Racun.DatumValute, Racun.lnkGR
FROM Racun;


=====QUERY=====
GEN_SUMA_VP
-----SQL-----
SELECT RacunStavke.lnkGR, RacunStavke.ID_O, RacunStavke.Grupa_VP, Sum(RacunStavke.Ukupno) AS SumOfUkupno
FROM RacunStavke
GROUP BY RacunStavke.lnkGR, RacunStavke.ID_O, RacunStavke.Grupa_VP
HAVING (((RacunStavke.Grupa_VP) Is Not Null));


=====QUERY=====
GEN_SUMA_VP_2
-----SQL-----
SELECT RacunStavke.IDRacunStavke, GEN_SUMA_VP.SumOfUkupno, GEN_SUMA_VP.ID_O
FROM RacunStavke INNER JOIN GEN_SUMA_VP ON (RacunStavke.lnkGR=GEN_SUMA_VP.lnkGR) AND (RacunStavke.ID_O=GEN_SUMA_VP.ID_O) AND (RacunStavke.VP=GEN_SUMA_VP.Grupa_VP);


=====QUERY=====
GK Without Matching Kupac
-----SQL-----
SELECT GK.STAVKAID, GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.PIZNOS, GK.DIZNOS, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.lnkIzvodStavkaID, GK.NAPOMENA, GK.PARAMETRI, GK.OPIS, GK.SIFRAKONTA, GK.DPO, GK.SIFRAKN, GK.RDOB, GK.RACID, GK.PRIORITET
FROM GK LEFT JOIN Kupac ON GK.[lnkKUPACID] = Kupac.[ID_K]
WHERE (((Kupac.ID_K) Is Null));


=====QUERY=====
GK_2040
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.BR_NALOG, GK.KONTO
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
GK_2040_RACID_SUM
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, GK.RACID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.KONTO, GK.lnkSkupstinaID, GK.RACID
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
GK_2410
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.KONTO)="2410"));


=====QUERY=====
GK_2410_grpNalog
-----SQL-----
SELECT GK.BR_NALOG, Sum([DIZNOS]-[PIZNOS]) AS SUMA
FROM GK
GROUP BY GK.KONTO, GK.BR_NALOG
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
GK_2410_Sum
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.lnkSkupstinaID
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
GK_4350
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.BR_NALOG, GK.KONTO
HAVING (((GK.KONTO)="4350"));


=====QUERY=====
GK_4900
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.BR_NALOG, GK.KONTO
HAVING (((GK.KONTO)="4900"));


=====QUERY=====
GK_5590
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.BR_NALOG, GK.KONTO
HAVING (((GK.KONTO)="5590"));


=====QUERY=====
GK_97
-----SQL-----
SELECT GK.lnkKUPACID AS [ID-K], GK.lnkSkupstinaID AS SZ, Sum(GK.DIZNOS) AS [D-99], Sum(GK.PIZNOS) AS [P-99], Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2) AS STANJE
FROM GK
WHERE (((GK.KNzaTIP)=97)) OR (((GK.TIP_STAVKE)=97))
GROUP BY GK.lnkKUPACID, GK.lnkSkupstinaID;


=====QUERY=====
GK_98
-----SQL-----
SELECT GK.lnkKUPACID AS [ID-K], GK.lnkSkupstinaID AS SZ, Sum(GK.DIZNOS) AS [D-99], Sum(GK.PIZNOS) AS [P-99], Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2) AS STANJE
FROM GK
WHERE (((GK.KNzaTIP)=98)) OR (((GK.TIP_STAVKE)=98))
GROUP BY GK.lnkKUPACID, GK.lnkSkupstinaID;


=====QUERY=====
GK_99
-----SQL-----
SELECT GK.lnkKUPACID AS [ID-K], GK.lnkSkupstinaID AS SZ, Sum(GK.DIZNOS) AS [D-99], Sum(GK.PIZNOS) AS [P-99], Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2) AS STANJE
FROM GK
WHERE (((GK.KNzaTIP)=99)) OR (((GK.TIP_STAVKE)=99))
GROUP BY GK.lnkKUPACID, GK.lnkSkupstinaID;


=====QUERY=====
GK_ByIzvodStavka
-----SQL-----
SELECT Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkIzvodStavkaID
FROM GK
GROUP BY GK.lnkIzvodStavkaID;


=====QUERY=====
GK_ByRacID
-----SQL-----
SELECT GK.RACID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KONTO, GK.lnkKUPACID, GK.lnkSkupstinaID
FROM GK
GROUP BY GK.RACID, GK.KONTO, GK.lnkKUPACID, GK.lnkSkupstinaID;


=====QUERY=====
GK_DIZNOS
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.DIZNOS)>0));


=====QUERY=====
GK_FILTER_DOBAVLJACI
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.RDOB, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KontoTroska
FROM GK
WHERE (((GK.KONTO) Like "4350*"))
GROUP BY GK.lnkSkupstinaID, GK.RDOB, GK.KontoTroska;


=====QUERY=====
GK_Filter_PD
-----SQL-----
SELECT GK.*, IIf([KNzaTIP]>98 Or [KNzaTIP]<95,0,1)+IIf([TIP_STAVKE]>98 Or [TIP_STAVKE]<95,0,1) AS Flt, IIf(IIf([KNZATIP]>[TIP_STAVKE],[KNZATIP],[TIP_STAVKE])<90,0,IIf([KNZATIP]>[TIP_STAVKE],[KNZATIP],[TIP_STAVKE])) AS GTS
FROM GK;


=====QUERY=====
GK_Filter_PD_Opomene
-----SQL-----
SELECT GK.*, IIf([KNzaTIP]>98 Or [KNzaTIP]<95,0,1)+IIf([TIP_STAVKE]>98 Or [TIP_STAVKE]<95,0,1) AS Flt, IIf(IIf([KNZATIP]>[TIP_STAVKE],[KNZATIP],[TIP_STAVKE])<90,0,IIf([KNZATIP]>[TIP_STAVKE],[KNZATIP],[TIP_STAVKE])) AS GTS
FROM GK
WHERE ((((GK.PIZNOS)<>0) AND ((GK.DATUM)<=#9/12/2026#)) OR (((GK.DIZNOS)<=0) AND ((GK.DATUM)<=#9/12/2026#))) OR (((GK.DIZNOS)<>0) AND ((GK.DATUM)<=#7/31/2026#));


=====QUERY=====
GK_FILTER_PD_Opomene_Razlika
-----SQL-----
SELECT Opomena.lnkGrupaOpomena, GK_Filter_PD_Opomene_Suma.lnkKUPACID, Opomena.Dug, Opomena.SumaPoStavkama, GK_Filter_PD_Opomene_Suma.DIPI, Round([Dug]-[DIPI],2) AS Razlika
FROM GK_Filter_PD_Opomene_Suma INNER JOIN Opomena ON GK_Filter_PD_Opomene_Suma.lnkKUPACID = Opomena.lnkKupac;


=====QUERY=====
GK_Filter_PD_Opomene_Suma
-----SQL-----
SELECT GK_Filter_PD_Opomene.KONTO, GK_Filter_PD_Opomene.lnkKUPACID, Sum([DIZNOS]-[PIZNOS]) AS DIPI
FROM GK_Filter_PD_Opomene
GROUP BY GK_Filter_PD_Opomene.KONTO, GK_Filter_PD_Opomene.lnkKUPACID;


=====QUERY=====
GK_GKTMP_UNION
-----SQL-----
SELECT GK.STAVKAID, GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.PIZNOS, GK.DIZNOS, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.lnkIzvodStavkaID, GK.NAPOMENA, GK.PARAMETRI, GK.OPIS, GK.SIFRAKONTA, GK.DPO, GK.SIFRAKN, GK.RDOB, GK.RACID, GK.PRIORITET, 0 as fromStavkaID
FROM GK

UNION SELECT GK_TMP.STAVKAID, GK_TMP.BR_NALOG, GK_TMP.KONTO, GK_TMP.DATUM, GK_TMP.PIZNOS, GK_TMP.DIZNOS, GK_TMP.TIP_STAVKE, GK_TMP.DOK, GK_TMP.lnkSkupstinaID, GK_TMP.lnkKUPACID, GK_TMP.lnkIzvodStavkaID, GK_TMP.NAPOMENA, GK_TMP.PARAMETRI, GK_TMP.OPIS, GK_TMP.SIFRAKONTA, GK_TMP.DPO, GK_TMP.SIFRAKN, GK_TMP.RDOB, GK_TMP.RACID, GK_TMP.PRIORITET, fromStavkaID 
FROM GK_TMP;


=====QUERY=====
GK_Grp_Dok
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, Sum([DIZNOS]-[PIZNOS]) AS Suma, GK.DOK, GK.RACID, IIf(Sum([DIZNOS]-[PIZNOS])>0,1,0) AS IsDug, IIf(Sum([DIZNOS]-[PIZNOS])<0,1,0) AS IsNotDug
FROM GK
GROUP BY GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, GK.DOK, GK.RACID
HAVING (((GK.KONTO)='2040') And ((Sum(GK.DIZNOS-[PIZNOS]))>1 Or (Sum(GK.DIZNOS-[PIZNOS]))<-1));


=====QUERY=====
GK_GRP_DOK_COUNT
-----SQL-----
SELECT GK_Grp_Dok.lnkSkupstinaID, Skustina.NazivSS, GK_Grp_Dok.lnkKUPACID, Kupac.Naziv
FROM (GK_Grp_Dok INNER JOIN Skustina ON GK_Grp_Dok.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Kupac ON GK_Grp_Dok.lnkKUPACID = Kupac.ID_K
GROUP BY GK_Grp_Dok.lnkSkupstinaID, Skustina.NazivSS, GK_Grp_Dok.lnkKUPACID, Kupac.Naziv
HAVING (((GK_Grp_Dok.lnkSkupstinaID)=Forms!Izvestaji!cmbSZ) And ((Sum(GK_Grp_Dok.IsDug))>0) And ((Sum(GK_Grp_Dok.IsNotDug))>0));


=====QUERY=====
GK_GRP_DOK_COUNT_ALL
-----SQL-----
SELECT GK_Grp_Dok.lnkSkupstinaID, Skustina.NazivSS, GK_Grp_Dok.lnkKUPACID, Kupac.Naziv
FROM (GK_Grp_Dok INNER JOIN Skustina ON GK_Grp_Dok.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Kupac ON GK_Grp_Dok.lnkKUPACID = Kupac.ID_K
GROUP BY GK_Grp_Dok.lnkSkupstinaID, Skustina.NazivSS, GK_Grp_Dok.lnkKUPACID, Kupac.Naziv
HAVING (((Sum(GK_Grp_Dok.IsDug))>0) AND ((Sum(GK_Grp_Dok.IsNotDug))>0));


=====QUERY=====
GK_GRP_DOK_COUNT_BRMD
-----SQL-----
SELECT GK_Grp_Dok.lnkSkupstinaID, GK_Grp_Dok.lnkKUPACID, GK_Grp_Dok.KONTO, Count(GK_Grp_Dok.RACID) AS Total
FROM GK_Grp_Dok
WHERE (((GK_Grp_Dok.IsDug)=1) AND ((GK_Grp_Dok.IsNotDug)=0))
GROUP BY GK_Grp_Dok.lnkSkupstinaID, GK_Grp_Dok.lnkKUPACID, GK_Grp_Dok.KONTO;


=====QUERY=====
GK_GRP_DOK_DOB_ALL
-----SQL-----
SELECT First(GK.STAVKAID) AS FirstOfSTAVKAID, First(GK.DATUM) AS FirstOfDATUM, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2) AS Suma, GK.DOK, GK.PARAMETRI, GK.RACID, GK.RDOB, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))>0,1,0) AS IsDug, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))<0,1,0) AS IsNotDug
FROM GK
GROUP BY GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, GK.DOK, GK.PARAMETRI, GK.RACID, GK.RDOB
HAVING (((GK.KONTO)="4350") AND ((Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2))<>0));


=====QUERY=====
GK_GRP_DOK_DOB_ALL_izvod
-----SQL-----
SELECT First(GK.STAVKAID) AS FirstOfSTAVKAID, First(GK.DATUM) AS FirstOfDATUM, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, Sum(Round([DIZNOS]-[PIZNOS],2)) AS Suma, GK.DOK, GK.PARAMETRI, GK.RDOB, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))>0,1,0) AS IsDug, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))<0,1,0) AS IsNotDug, GK.lnkIzvodStavkaID
FROM GK
GROUP BY GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, GK.DOK, GK.PARAMETRI, GK.RDOB, GK.lnkIzvodStavkaID
HAVING (((GK.KONTO)="4350") AND ((Sum(Round([DIZNOS]-[PIZNOS],2)))<>0));


=====QUERY=====
GK_GRP_DOK_KORISNIK_ALL
-----SQL-----
SELECT First(GK.STAVKAID) AS FirstOfSTAVKAID, First(GK.DATUM) AS FirstOfDATUM, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2) AS Suma, GK.DOK, GK.RACID, GK.PARAMETRI, GK.RDOB, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))>0,1,0) AS IsDug, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))<0,1,0) AS IsNotDug, GK.KontoTroska
FROM GK
GROUP BY GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, GK.DOK, GK.RACID, GK.PARAMETRI, GK.RDOB, GK.KontoTroska
HAVING (((GK.KONTO) Like '204*') AND ((Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2))<>0));


=====QUERY=====
GK_GRP_DOK_KORISNIK_ALL_LISTING
-----SQL-----
SELECT GK_GRP_DOK_KORISNIK_ALL.lnkSkupstinaID, GK_GRP_DOK_KORISNIK_ALL.lnkKUPACID, Sum(GK_GRP_DOK_KORISNIK_ALL.Suma) AS SumOfSuma, Sum(GK_GRP_DOK_KORISNIK_ALL.IsDug) AS SumOfIsDug, Sum(GK_GRP_DOK_KORISNIK_ALL.IsNotDug) AS SumOfIsNotDug
FROM GK_GRP_DOK_KORISNIK_ALL
GROUP BY GK_GRP_DOK_KORISNIK_ALL.lnkSkupstinaID, GK_GRP_DOK_KORISNIK_ALL.lnkKUPACID
HAVING (((GK_GRP_DOK_KORISNIK_ALL.lnkSkupstinaID)=129) AND ((Sum(GK_GRP_DOK_KORISNIK_ALL.IsDug))>0) AND ((Sum(GK_GRP_DOK_KORISNIK_ALL.IsNotDug))>0));


=====QUERY=====
GK_GRP_DOK_KORISNIK_ALL_tmpKT
-----SQL-----
SELECT First(GK.STAVKAID) AS FirstOfSTAVKAID, First(GK.DATUM) AS FirstOfDATUM, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2) AS Suma, GK.DOK, GK.RACID, GK.PARAMETRI, GK.RDOB, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))>0,1,0) AS IsDug, IIf(Sum(Round([DIZNOS]-[PIZNOS],2))<0,1,0) AS IsNotDug
FROM GK
GROUP BY GK.lnkSkupstinaID, GK.lnkKUPACID, GK.KONTO, GK.DOK, GK.RACID, GK.PARAMETRI, GK.RDOB
HAVING (((GK.KONTO)='2040') AND ((Round(Sum(Round([DIZNOS]-[PIZNOS],2)),2))<>0));


=====QUERY=====
GK_Konta_Sum
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS DIPI, TKONTO.GRUPA4, Troskovi_PodKonta_1.Naziv
FROM ((GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN TKONTO ON Troskovi_PodKonta.PodKonto = TKONTO.PodKonto) INNER JOIN Troskovi_PodKonta AS Troskovi_PodKonta_1 ON TKONTO.GRUPA4 = Troskovi_PodKonta_1.PodKonto
GROUP BY GK.lnkSkupstinaID, GK.KONTO, TKONTO.GRUPA4, Troskovi_PodKonta_1.Naziv;


=====QUERY=====
GK_Konta_Sum_Crosstab
-----SQL-----
TRANSFORM Sum(GK_Konta_Sum.[DIPI]) AS SumOfDIPI
SELECT GK_Konta_Sum.[KontoTroska], GK_Konta_Sum.[Naziv], Sum(GK_Konta_Sum.[DIPI]) AS [Total Of DIPI]
FROM GK_Konta_Sum
GROUP BY GK_Konta_Sum.[KontoTroska], GK_Konta_Sum.[Naziv]
PIVOT GK_Konta_Sum.[KONTO];


=====QUERY=====
GK_KORISNIK_RDOB
-----SQL-----
SELECT GK.KONTO, GK.DIZNOS, GK.PIZNOS, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.lnkIzvodStavkaID, GK.RDOB, GK.RACID, GK.KontoTroska, GK.PRIORITET, GK.STAVKAID, GK.DOK, GK.PARAMETRI, GK.SIFRAKONTA, GK.SIFRAKN, GK.BR_NALOG
FROM GK
WHERE (((GK.KONTO)="2040"));


=====QUERY=====
GK_KORISNIK_RDOB_UPOREDO
-----SQL-----
SELECT GK_KORISNIK_RDOB_1.BR_NALOG, GK_KORISNIK_RDOB_1.STAVKAID, GK_KORISNIK_RDOB.lnkSkupstinaID, GK_KORISNIK_RDOB.lnkKUPACID, GK_KORISNIK_RDOB.DIZNOS, GK_KORISNIK_RDOB_1.PIZNOS, GK_KORISNIK_RDOB.RDOB AS SETRSDOB, GK_KORISNIK_RDOB_1.RDOB, GK_KORISNIK_RDOB.PRIORITET AS SETRSPRIORITET, GK_KORISNIK_RDOB_1.PRIORITET, GK_KORISNIK_RDOB.DOK, GK_KORISNIK_RDOB_1.DOK, GK_KORISNIK_RDOB.PARAMETRI, GK_KORISNIK_RDOB_1.PARAMETRI, GK_KORISNIK_RDOB.SIFRAKONTA, GK_KORISNIK_RDOB_1.SIFRAKONTA, GK_KORISNIK_RDOB.SIFRAKN, GK_KORISNIK_RDOB_1.SIFRAKN
FROM GK_KORISNIK_RDOB INNER JOIN GK_KORISNIK_RDOB AS GK_KORISNIK_RDOB_1 ON (GK_KORISNIK_RDOB.RACID = GK_KORISNIK_RDOB_1.RACID) AND (GK_KORISNIK_RDOB.KontoTroska = GK_KORISNIK_RDOB_1.KontoTroska) AND (GK_KORISNIK_RDOB.DIZNOS = GK_KORISNIK_RDOB_1.PIZNOS) AND (GK_KORISNIK_RDOB.lnkKUPACID = GK_KORISNIK_RDOB_1.lnkKUPACID) AND (GK_KORISNIK_RDOB.lnkSkupstinaID = GK_KORISNIK_RDOB_1.lnkSkupstinaID)
WHERE (((GK_KORISNIK_RDOB.RDOB)<>0) AND ((GK_KORISNIK_RDOB_1.RDOB)<>0) AND ((GK_KORISNIK_RDOB.lnkIzvodStavkaID)=0) AND ((GK_KORISNIK_RDOB_1.lnkIzvodStavkaID)<>0) AND (([GK_KORISNIK_RDOB].[RDOB]=[GK_KORISNIK_RDOB_1].[RDOB])=0));


=====QUERY=====
GK_PARTNER_DOK_DATUM_RACUNA
-----SQL-----
SELECT GK.lnkKUPACID, Sum(GK.DIZNOS) AS SumOfDIZNOS, GK.DOK, GK.DATUM
FROM GK
GROUP BY GK.lnkKUPACID, GK.DOK, GK.DATUM, GK.lnkIzvodStavkaID
HAVING (((Sum(GK.DIZNOS))>0) AND ((GK.lnkIzvodStavkaID)=0));


=====QUERY=====
GK_PARTNER_NAJSTARIJI_DUG
-----SQL-----
SELECT GK_PARTNER_NAJSTARIJI_DUG_SUB.lnkKUPACID AS Expr1, Min(GK_PARTNER_NAJSTARIJI_DUG_SUB.DATUM) AS MinOfDATUM
FROM GK_PARTNER_NAJSTARIJI_DUG_SUB
GROUP BY GK_PARTNER_NAJSTARIJI_DUG_SUB.lnkKUPACID;


=====QUERY=====
GK_PARTNER_NAJSTARIJI_DUG_SUB
-----SQL-----
SELECT GK.lnkKUPACID, GK.DOK, Sum([DIZNOS]-[PIZNOS]) AS DIPI, GK_PARTNER_DOK_DATUM_RACUNA.DATUM
FROM GK INNER JOIN GK_PARTNER_DOK_DATUM_RACUNA ON (GK.DOK = GK_PARTNER_DOK_DATUM_RACUNA.DOK) AND (GK.lnkKUPACID = GK_PARTNER_DOK_DATUM_RACUNA.lnkKUPACID)
WHERE (((GK.KONTO)="2040"))
GROUP BY GK.lnkKUPACID, GK.DOK, GK_PARTNER_DOK_DATUM_RACUNA.DATUM
HAVING (((GK.lnkKUPACID)>0) AND ((Sum([DIZNOS]-[PIZNOS]))>0.01));


=====QUERY=====
GK_PARTNER_POSLEDNJA_UPLATA
-----SQL-----
SELECT GK.lnkKUPACID, Max(GK.DATUM) AS MaxOfDATUM
FROM GK
WHERE (((GK.PIZNOS)>0) AND ((GK.lnkIzvodStavkaID)>0))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
GK_PARTNER_SUM_PRINTERBIN
-----SQL-----
SELECT GK_PARTNERI.lnkKUPACID AS IDK, [SumOfDIZNOS]-[SumOfPIZNOS] AS DIPI
FROM PrinterBinLOCAL INNER JOIN GK_PARTNERI ON PrinterBinLOCAL.ID_Item = GK_PARTNERI.lnkKUPACID;


=====QUERY=====
GK_PARTNERI
-----SQL-----
SELECT GK.lnkKUPACID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Round(Sum([DIZNOS]-[PIZNOS]),2) AS DIPI
FROM GK
GROUP BY GK.lnkKUPACID;


=====QUERY=====
GK_PIZNOS
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.PIZNOS)>0) AND ((GK.lnkIzvodStavkaID)>0));


=====QUERY=====
GK_Po_Izvodu
-----SQL-----
SELECT GK.lnkIzvodStavkaID, GK.BR_NALOG, Kupac.ID_K, Kupac.Naziv, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS, GK.DATUM, Count(GK.STAVKAID) AS BrojUlazaUGK
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
GROUP BY GK.lnkIzvodStavkaID, GK.BR_NALOG, Kupac.ID_K, Kupac.Naziv, GK.DATUM
HAVING (((GK.lnkIzvodStavkaID)>0));


=====QUERY=====
GK_POCETNOSTANJE
-----SQL-----
SELECT GK.KONTO, Round(Sum([DIZNOS]-[PIZNOS]),2) AS DIPI, GK.DOK, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.PARAMETRI, GK.SIFRAKONTA, GK.RDOB, GK.RACID, Min(GK.PRIORITET) AS MinOfPRIORITET, GK.KontoTroska, Nz([KnDokID],0) AS KnDokID_NZ
FROM GK
WHERE (((GK.KONTO)="4350"))
GROUP BY GK.KONTO, GK.DOK, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.PARAMETRI, GK.SIFRAKONTA, GK.RDOB, GK.RACID, GK.KontoTroska, Nz([KnDokID],0)
HAVING (((Round(Sum([DIZNOS]-[PIZNOS]),2))<>0) AND ((GK.lnkSkupstinaID)=129));


=====QUERY=====
GK_PRETPLATEIZVODI_GRUPBY_KUPACNALOG
-----SQL-----
SELECT GK_PretplateIzvodi.lnkKUPACID, Count(GK_PretplateIzvodi.STAVKAID) AS CountOfSTAVKAID, GK_PretplateIzvodi.BR_NALOG
FROM GK_PretplateIzvodi
GROUP BY GK_PretplateIzvodi.lnkKUPACID, GK_PretplateIzvodi.BR_NALOG;


=====QUERY=====
gk_R_201207
-----SQL-----
SELECT GK.BR_NALOG, GK.TIP_STAVKE, Left([PARAMETRI],5) & "8" & Right([PARAMETRI],Len([PARAMETRI])-6) AS Expr1
FROM GK
WHERE (((GK.BR_NALOG)=4) AND ((GK.TIP_STAVKE)=3) AND ((GK.PARAMETRI) Is Not Null));


=====QUERY=====
GK_RDOB_4350
-----SQL-----
SELECT GK.RDOB, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS SALDO
FROM GK
WHERE (((GK.KONTO)="4350"))
GROUP BY GK.RDOB, GK.lnkSkupstinaID;


=====QUERY=====
Gk_SumByIzvodStavke
-----SQL-----
SELECT GK.lnkIzvodStavkaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.lnkIzvodStavkaID;


=====QUERY=====
GK-BYMESEC
-----SQL-----
SELECT Skustina.NazivSS, Kupac.Naziv, GK.lnkSkupstinaID, GK.lnkKUPACID, Left([PARAMETRI],4) & "-" & Mid([PARAMETRI],5,2) AS MESEC, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, IIf(Sum([DIZNOS])>0,"R","U") AS RP
FROM (GK INNER JOIN Kupac ON GK.lnkKUPACID=Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK=Skustina.IDSkupstina
WHERE (((Kupac.DOB)=0))
GROUP BY Skustina.NazivSS, Kupac.Naziv, GK.lnkSkupstinaID, GK.lnkKUPACID, Left([PARAMETRI],4) & "-" & Mid([PARAMETRI],5,2), GK.TIP_STAVKE
ORDER BY GK.lnkSkupstinaID, GK.lnkKUPACID, Left([PARAMETRI],4) & "-" & Mid([PARAMETRI],5,2), IIf(Sum([DIZNOS])>0,"R","U") DESC;


=====QUERY=====
GK-BYMESEC_Crosstab
-----SQL-----
TRANSFORM Sum([GK-BYMESEC-RP].Iznos) AS SumOfIznos
SELECT [GK-BYMESEC-RP].lnkSkupstinaID, Skustina.NazivSS, Kupac.Naziv, [GK-BYMESEC-RP].lnkKUPACID
FROM ([GK-BYMESEC-RP] INNER JOIN Skustina ON [GK-BYMESEC-RP].lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Kupac ON [GK-BYMESEC-RP].lnkKUPACID = Kupac.ID_K
GROUP BY [GK-BYMESEC-RP].lnkSkupstinaID, Skustina.NazivSS, Kupac.Naziv, [GK-BYMESEC-RP].lnkKUPACID
ORDER BY [GK-BYMESEC-RP].lnkSkupstinaID, [GK-BYMESEC-RP].lnkKUPACID, [GK-BYMESEC-RP].ZA
PIVOT [GK-BYMESEC-RP].ZA;


=====QUERY=====
GK-BYMESEC-RP
-----SQL-----
SELECT [GK-BYMESEC].*, [MESEC] & "-" & [RP] AS ZA, IIf([SumOfDIZNOS]>0,[SumOfDIZNOS],[SumOfPIZNOS]) AS Iznos
FROM [GK-BYMESEC];


=====QUERY=====
GK-R2305
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, GK.DATUM, Sum(GK.DIZNOS) AS SumOfDIZNOS, GK.PIZNOS, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.lnkIzvodStavkaID, GK.NAPOMENA, GK.PARAMETRI, GK.OPIS, GK.SIFRAKONTA, GK.DPO, GK.SIFRAKN, 77 AS RDB, GK.RACID, GK.PRIORITET, GK.KNzaTIP, GK.RacunIN_ID, GK.KontoTroska, GK.KnDokID
FROM GK
WHERE (((GK.DOK)="R-2305") AND ((GK.TIP_STAVKE)=3) AND ((GK.RDOB)=79 Or (GK.RDOB)=81))
GROUP BY GK.BR_NALOG, GK.KONTO, GK.DATUM, GK.PIZNOS, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.lnkIzvodStavkaID, GK.NAPOMENA, GK.PARAMETRI, GK.OPIS, GK.SIFRAKONTA, GK.DPO, GK.SIFRAKN, GK.RACID, GK.PRIORITET, GK.KNzaTIP, GK.RacunIN_ID, GK.KontoTroska, GK.KnDokID;


=====QUERY=====
GK-R2306
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.TIP_STAVKE)=3) AND ((GK.DOK)="R-2306"));


=====QUERY=====
GrupaOpomenaSaGrupomRacuna
-----SQL-----
SELECT GrupaOpomena.IDGrupaOpomena, GrupaOpomena.IDSZ, GrupaRacuna.GrupaRacunaFXN
FROM GrupaOpomena INNER JOIN GrupaRacuna ON GrupaOpomena.lnkGrupaRacuna = GrupaRacuna.IDGrupaRacuna;


=====QUERY=====
GrupByStavke
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, RacunStavke.lnkGR, Sum(RacunStavke.Ukupno) AS SumOfUkupno, RacunStavke.lnkTVP, Troskovi_VP.Troskovi_Vrste_Opis
FROM (RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR=GrupaRacuna.IDGrupaRacuna) INNER JOIN Troskovi_VP ON RacunStavke.lnkTVP=Troskovi_VP.ID_Troskovi_Vrste
WHERE (((RacunStavke.TipObracuna) Is Not Null))
GROUP BY GrupaRacuna.GrupaRacunaFXN, RacunStavke.lnkGR, RacunStavke.lnkTVP, Troskovi_VP.Troskovi_Vrste_Opis;


=====QUERY=====
GrupByStavke_Crosstab
-----SQL-----
TRANSFORM Sum(GrupByStavke.SumOfUkupno) AS SumOfSumOfUkupno
SELECT GrupByStavke.Troskovi_Vrste_Opis, Sum(GrupByStavke.SumOfUkupno) AS [Total Of SumOfUkupno]
FROM GrupByStavke
GROUP BY GrupByStavke.Troskovi_Vrste_Opis
PIVOT GrupByStavke.GrupaRacunaFXN;


=====QUERY=====
GrupisanjeKonta
-----SQL-----
SELECT Mid([PARAMETRI],3,6) AS MES, GK.BR_NALOG, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]*[IG_V]) AS [IG-DIPIP], Sum([DIZNOS]*[CB_V]) AS [CB-DIPIP], Objekti.lnk_tip, TipObjekta.IG_KONTO, TipObjekta.CB_KONTO, KontrolniBroj(97,Mid([PARAMETRI],3,6) & [IG_KONTO]) & Mid([PARAMETRI],3,6) & [IG_KONTO] AS PAR, KontrolniBroj(97,Mid([PARAMETRI],3,6) & [CB_KONTO]) & Mid([PARAMETRI],3,6) & [CB_KONTO] AS [PAR-CB], GK.DATUM, GK.TIP_STAVKE, GK.lnkSkupstinaID, "IF" & KontrolniBroj(97,Mid([PARAMETRI],3,6) & [IG_KONTO]) & "-" & Mid([PARAMETRI],3,6) & "-" & [IG_KONTO] AS ifpar, "IF" & KontrolniBroj(97,Mid([PARAMETRI],3,6) & [CB_KONTO]) & "-" & Mid([PARAMETRI],3,6) & "-" & [CB_KONTO] AS ifparCB
FROM (Objekti INNER JOIN (GK INNER JOIN Kupac ON GK.lnkKUPACID=Kupac.ID_K) ON Objekti.lnk_ID_K=Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj
WHERE (((Kupac.Naziv)="Imperial Gradnja d.o.o") AND ((GK.NAPOMENA) Is Null))
GROUP BY Mid([PARAMETRI],3,6), GK.BR_NALOG, Objekti.lnk_tip, TipObjekta.IG_KONTO, TipObjekta.CB_KONTO, KontrolniBroj(97,Mid([PARAMETRI],3,6) & [IG_KONTO]) & Mid([PARAMETRI],3,6) & [IG_KONTO], KontrolniBroj(97,Mid([PARAMETRI],3,6) & [CB_KONTO]) & Mid([PARAMETRI],3,6) & [CB_KONTO], GK.DATUM, GK.TIP_STAVKE, GK.lnkSkupstinaID, "IF" & KontrolniBroj(97,Mid([PARAMETRI],3,6) & [IG_KONTO]) & "-" & Mid([PARAMETRI],3,6) & "-" & [IG_KONTO], "IF" & KontrolniBroj(97,Mid([PARAMETRI],3,6) & [CB_KONTO]) & "-" & Mid([PARAMETRI],3,6) & "-" & [CB_KONTO];


=====QUERY=====
GrupisanjeKonta_Storno
-----SQL-----
INSERT INTO GK ( BR_NALOG, DIZNOS, DATUM, TIP_STAVKE, lnkSkupstinaID, NAPOMENA, KONTO, DOK, lnkKUPACID, PARAMETRI )
SELECT GK.BR_NALOG, -[DIZNOS] AS Expr1, GK.DATUM, GK.TIP_STAVKE, GK.lnkSkupstinaID, "SPC" AS Expr2, GK.KONTO, GK.DOK, GK.lnkKUPACID, GK.PARAMETRI
FROM (Objekti INNER JOIN (GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Kupac.Naziv)="Imperial Gradnja d.o.o") AND ((GK.NAPOMENA) Is Null));


=====QUERY=====
GrupisanjeKontaAdd
-----SQL-----
INSERT INTO GK ( BR_NALOG, KONTO, DATUM, TIP_STAVKE, DIZNOS, DOK, lnkSkupstinaID, lnkKUPACID, PARAMETRI, NAPOMENA )
SELECT GrupisanjeKonta.BR_NALOG, 2020 & [IG-DIPIP] AS Expr1, GrupisanjeKonta.DATUM, GrupisanjeKonta.TIP_STAVKE, GrupisanjeKonta.[IG-DIPIP], GrupisanjeKonta.ifpar, GrupisanjeKonta.lnkSkupstinaID, GrupisanjeKonta.IG_KONTO, GrupisanjeKonta.PAR, "ZBIRNI" AS Expr2
FROM GrupisanjeKonta;


=====QUERY=====
GrupisanjeKontaAddCB
-----SQL-----
INSERT INTO GK ( BR_NALOG, KONTO, DATUM, TIP_STAVKE, DIZNOS, DOK, lnkSkupstinaID, lnkKUPACID, PARAMETRI, NAPOMENA )
SELECT GrupisanjeKonta.BR_NALOG, 2020 & [CB-DIPIP] AS Expr1, GrupisanjeKonta.DATUM, GrupisanjeKonta.TIP_STAVKE, GrupisanjeKonta.[CB-DIPIP], GrupisanjeKonta.ifparCB, GrupisanjeKonta.lnkSkupstinaID, GrupisanjeKonta.CB_KONTO, GrupisanjeKonta.[PAR-CB], "ZBIRNI" AS Expr2
FROM GrupisanjeKonta;


=====QUERY=====
GrupIzvodBySK
-----SQL-----
SELECT Izvod.ID_SK, Count(Izvod.IzvodID) AS CountOfIzvodID, Skustina.NazivSS, Skustina.TR
FROM Izvod INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina
GROUP BY Izvod.ID_SK, Skustina.NazivSS, Skustina.TR;


=====QUERY=====
INFO-GENREC-SQL0
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA], [MAX-KV-SVE], [MAX-BRGM], [MAX-KV-K1], [MAX-KV-K2], [MAX-KV-K3] )
SELECT 133 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovor_FLT.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovor_FLT.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis AS Expr1, Troskovi_VP.Sort, Troskovi_VP.AddTxt, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],'') AS Expr2, Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Ugovor_FLT.SumaD, Objekat_Sume_Sve.[MAX-KV-SVE], Objekat_Sume_Sve.[MAX-BRGM], Objekat_Sume_Sve.[MAX-KV-K1], Objekat_Sume_Sve.[MAX-KV-K2], Objekat_Sume_Sve.[MAX-KV-K3]
FROM ((Troskovi_Ugovor_FLT RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovor_FLT.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovor_FLT.lnk_ID_Dob=Kupac.ID_K) INNER JOIN Objekat_Sume_Sve ON Objekti.lnkSkupstinaID=Objekat_Sume_Sve.lnkSkupstinaID
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
izv_kratica_zbirna_kupca
-----SQL-----
SELECT Sum(GK.DIZNOS) AS DI, Sum(GK.PIZNOS) AS PI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, GK.lnkKUPACID, '' AS PARAMETRI, GK.DOK AS DOK, GK.lnkSkupstinaID, First(GK.RACID) AS FirstOfRACID, First(Racun.DatumPrometa) AS FirstOfDatumPrometa, Skustina.NazivSS, Kupac.Naziv, Skustina.PrintNaziv, Skustina.PBrojSZ, Skustina.GradSZ, Skustina.TR, GrupaRacuna.GrupaRacunaFXT
FROM (((GK LEFT JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GK.KONTO) Like "204*") And ((GK.DATUM) Between Forms!Kartica!txtDatumOd And Forms!Kartica!txtDatumDo)) Or (((GK.KONTO) Like "435*") And ((GK.DATUM) Between Forms!Kartica!txtDatumOd And Forms!Kartica!txtDatumDo))
GROUP BY GK.lnkKUPACID, '', GK.DOK, GK.lnkSkupstinaID, Skustina.NazivSS, Kupac.Naziv, Skustina.PrintNaziv, Skustina.PBrojSZ, Skustina.GradSZ, Skustina.TR, GrupaRacuna.GrupaRacunaFXT, Skustina.IDSkupstina, Kupac.ID_K
HAVING (((Skustina.IDSkupstina)=[Forms]![Kartica]![cmbSZ] Or (Skustina.IDSkupstina)=[Forms]![Kartica]![cmbSZ]))
ORDER BY Skustina.IDSkupstina, Kupac.ID_K, GK.DOK;


=====QUERY=====
izv_kratica_zbirna_kupca_bck20251004
-----SQL-----
SELECT Sum(GK.DIZNOS) AS DI, Sum(GK.PIZNOS) AS PI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, GK.lnkKUPACID, '' AS PARAMETRI, GK.DOK AS DOK, GK.lnkSkupstinaID, First(GK.RACID) AS FirstOfRACID, First(Racun.DatumIzdavanja) AS FirstOfDatumIzdavanja, Skustina.NazivSS, Kupac.Naziv
FROM (((GK LEFT JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GK.KONTO) Like "435*"))
GROUP BY GK.lnkKUPACID, '', GK.DOK, GK.lnkSkupstinaID, Skustina.NazivSS, Kupac.Naziv, Skustina.IDSkupstina, Kupac.ID_K
ORDER BY Skustina.IDSkupstina, Kupac.ID_K, GK.DOK;


=====QUERY=====
izv_kratica_zbirna_kupca_det
-----SQL-----
SELECT GK.DATUM, Sum(GK.DIZNOS) AS DI, Sum(GK.PIZNOS) AS PI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, GK.lnkKUPACID, '' AS PARAMETRI, GK.DOK AS DOK, GK.lnkSkupstinaID, GK.RACID, Racun.DatumIzdavanja, Skustina.NazivSS, Kupac.Naziv, TipStavke.TipStavke, [GK].[TIP_STAVKE] & " - " & [GK].[Datum] AS Grupa1, Skustina.IDSkupstina
FROM (((GK LEFT JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipStavke ON GK.TIP_STAVKE = TipStavke.ID_TIP
WHERE (((GK.KONTO) Like "204*") AND ((GK.DATUM) Between [Forms]![Kartica]![txtDatumOd] And [Forms]![Kartica]![txtDatumDo])) OR (((GK.KONTO) Like "435*") AND ((GK.DATUM) Between [Forms]![Kartica]![txtDatumOd] And [Forms]![Kartica]![txtDatumDo]))
GROUP BY GK.DATUM, GK.lnkKUPACID, '', GK.DOK, GK.lnkSkupstinaID, GK.RACID, Racun.DatumIzdavanja, Skustina.NazivSS, Kupac.Naziv, TipStavke.TipStavke, [GK].[TIP_STAVKE] & " - " & [GK].[Datum], Skustina.IDSkupstina, Kupac.ID_K
HAVING (((Skustina.IDSkupstina)=[Forms]![Kartica]![cmbSZ] Or (Skustina.IDSkupstina)=[Forms]![Kartica]![cmbSZ]))
ORDER BY GK.DATUM, Skustina.IDSkupstina, Kupac.ID_K, GK.DOK;


=====QUERY=====
izv_kratica_zbirna_kupca_det_bck20251004
-----SQL-----
SELECT GK.DATUM, Sum(GK.DIZNOS) AS DI, Sum(GK.PIZNOS) AS PI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, GK.lnkKUPACID, '' AS PARAMETRI, GK.DOK AS DOK, GK.lnkSkupstinaID, GK.RACID, Racun.DatumIzdavanja, Skustina.NazivSS, Kupac.Naziv, TipStavke.TipStavke
FROM (((GK LEFT JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipStavke ON GK.TIP_STAVKE = TipStavke.ID_TIP
WHERE (((GK.KONTO) Like "204*"))
GROUP BY GK.DATUM, GK.lnkKUPACID, '', GK.DOK, GK.lnkSkupstinaID, GK.RACID, Racun.DatumIzdavanja, Skustina.NazivSS, Kupac.Naziv, TipStavke.TipStavke, Skustina.IDSkupstina, Kupac.ID_K
ORDER BY GK.DATUM, Skustina.IDSkupstina, Kupac.ID_K, GK.DOK;


=====QUERY=====
izv_kratica_zbirna_kuppca_det_dob
-----SQL-----
SELECT GK.DATUM, Sum(GK.DIZNOS) AS DI, Sum(GK.PIZNOS) AS PI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, GK.lnkKUPACID, '' AS PARAMETRI, GK.DOK AS DOK, GK.lnkSkupstinaID, GK.RACID, Racun.DatumIzdavanja, Skustina.NazivSS, Kupac.Naziv, TipStavke.TipStavke
FROM (((GK LEFT JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipStavke ON GK.TIP_STAVKE = TipStavke.ID_TIP
WHERE (((GK.KONTO) Like "204*"))
GROUP BY GK.DATUM, GK.lnkKUPACID, '', GK.DOK, GK.lnkSkupstinaID, GK.RACID, Racun.DatumIzdavanja, Skustina.NazivSS, Kupac.Naziv, TipStavke.TipStavke, Skustina.IDSkupstina, Kupac.ID_K
ORDER BY GK.DATUM, Skustina.IDSkupstina, Kupac.ID_K, GK.DOK;


=====QUERY=====
Izvestaj_GK_Placanje_Dobavljaca
-----SQL-----
SELECT GK.RDOB, Sum(Round([DIZNOS]-[PIZNOS],2)) AS Stanje, GK.lnkKUPACID, GK.lnkSkupstinaID, GK.PARAMETRI, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.NazivRacuna
FROM GK INNER JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
GROUP BY GK.RDOB, GK.lnkKUPACID, GK.lnkSkupstinaID, GK.PARAMETRI, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.NazivRacuna;


=====QUERY=====
Izvestaj_GK_Placanje_Dobavljaca_Crosstab
-----SQL-----
TRANSFORM Sum(Izvestaj_GK_Placanje_Dobavljaca.Stanje) AS SumOfStanje
SELECT Izvestaj_GK_Placanje_Dobavljaca.lnkSkupstinaID, Izvestaj_GK_Placanje_Dobavljaca.lnkKUPACID, Sum(Izvestaj_GK_Placanje_Dobavljaca.Stanje) AS [Total Of Stanje]
FROM Izvestaj_GK_Placanje_Dobavljaca
GROUP BY Izvestaj_GK_Placanje_Dobavljaca.lnkSkupstinaID, Izvestaj_GK_Placanje_Dobavljaca.lnkKUPACID
PIVOT Izvestaj_GK_Placanje_Dobavljaca.SifraKN;


=====QUERY=====
Izvestaj_GK_Racuni
-----SQL-----
SELECT GK.RACID, Sum(Round([DIZNOS]-[PIZNOS],2)) AS Stanje, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, Racun.ID_K, Racun.Kupac
FROM (Racun INNER JOIN GK ON Racun.IDRacun = GK.RACID) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
GROUP BY GK.RACID, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, Racun.ID_K, Racun.Kupac
HAVING (((GK.RACID)>0));


=====QUERY=====
Izvestaj_GK_Racuni_Crosstab
-----SQL-----
TRANSFORM Sum(Izvestaj_GK_Racuni.Stanje) AS SumOfStanje
SELECT Izvestaj_GK_Racuni.ID_SK, Izvestaj_GK_Racuni.ID_K, Izvestaj_GK_Racuni.Kupac, Sum(Izvestaj_GK_Racuni.Stanje) AS Ukupno
FROM Izvestaj_GK_Racuni
GROUP BY Izvestaj_GK_Racuni.ID_SK, Izvestaj_GK_Racuni.ID_K, Izvestaj_GK_Racuni.Kupac
PIVOT Izvestaj_GK_Racuni.GrupaRacunaFXN;


=====QUERY=====
Izvestaj_Izvod
-----SQL-----
SELECT Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda AS Godina, GK.lnkKUPACID AS KONTOPARTNERA, Kupac.Naziv AS NazivPartnera, Count(GK.STAVKAID) AS BrojKnjizenja, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS, GK.KontoTroska, Troskovi_PodKonta.Naziv AS NazivTroska, IzvodStavke.ID, GK.RacunIN_ID
FROM ((((Izvod INNER JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID) INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina) LEFT JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID) LEFT JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) LEFT JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
GROUP BY Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, GK.lnkKUPACID, Kupac.Naziv, GK.KontoTroska, Troskovi_PodKonta.Naziv, IzvodStavke.ID, GK.RacunIN_ID, IzvodStavke.RbStavke
ORDER BY Izvod.Datum, IzvodStavke.RbStavke;


=====QUERY=====
Izvestaj_Izvod_GRP
-----SQL-----
SELECT Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda AS Godina, GK.lnkKUPACID AS KONTOPARTNERA, Kupac.Naziv AS NazivPartnera, Count(GK.STAVKAID) AS BrojKnjizenja, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS, IzvodStavke.ID
FROM (((Izvod INNER JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID) INNER JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina) LEFT JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID) LEFT JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
GROUP BY Izvod.ID_SK, Skustina.NazivSS, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, GK.lnkKUPACID, Kupac.Naziv, IzvodStavke.ID, IzvodStavke.RbStavke
ORDER BY Izvod.Datum, IzvodStavke.RbStavke;


=====QUERY=====
Izvestaj_Racuni
-----SQL-----
TRANSFORM Sum(Racun.Ukupno) AS SumOfUkupno
SELECT Racun.ID_SK, Racun.ID_K, Racun.Kupac, Sum(Racun.Ukupno) AS [Total Of Ukupno]
FROM Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
GROUP BY Racun.ID_SK, Racun.ID_K, Racun.Kupac
PIVOT GrupaRacuna.GrupaRacunaFXN;


=====QUERY=====
IZVESTAJ_STANJE
-----SQL-----
SELECT GK.lnkKUPACID, Kupac.Naziv, GK.KONTO, Last(GK.DATUM) AS Poslednja_uplata, Sum([DIZNOS]-[PIZNOS]) AS SUMA, GK.lnkSkupstinaID, Skustina.NazivSS, Skustina.TR, Skustina.MB, Skustina.Adresa, Skustina.PBroj, Skustina.PrintNaziv
FROM (GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina
WHERE (((Kupac.DOB)=0))
GROUP BY GK.lnkKUPACID, Kupac.Naziv, GK.KONTO, GK.lnkSkupstinaID, Skustina.NazivSS, Skustina.TR, Skustina.MB, Skustina.Adresa, Skustina.PBroj, Skustina.PrintNaziv
ORDER BY Sum([DIZNOS]-[PIZNOS]) DESC;


=====QUERY=====
IZVOD_UPLATE_RACUN
-----SQL-----
SELECT GK.KONTO, GK.lnkKUPACID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.DATUM, GK.lnkIzvodStavkaID
FROM GK
WHERE (((GK.TIP_STAVKE)=1))
GROUP BY GK.KONTO, GK.lnkKUPACID, GK.DATUM, GK.lnkIzvodStavkaID
HAVING (((GK.KONTO) Like "204*") AND ((GK.DATUM) Between #6/1/2026# And #6/30/2026#))
ORDER BY GK.lnkKUPACID, GK.DATUM;


=====QUERY=====
IzvodiStavke
-----SQL-----
SELECT Izvod.ID_SK, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, IzvodStavke.RbStavke, IzvodStavke.RbNaloga, IzvodStavke.NazivPN, IzvodStavke.BrojRacuna, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, IzvodStavke.Doznaka, IzvodStavke.Sifra, IzvodStavke.PozivNaBroj, IzvodStavke.opt_lnk_Kupac, GK_Po_Izvodu.BR_NALOG, GK_Po_Izvodu.ID_K, GK_Po_Izvodu.Naziv, GK_Po_Izvodu.SumOfPIZNOS, GK_Po_Izvodu.SumOfDIZNOS, GK_Po_Izvodu.BrojUlazaUGK
FROM GK_Po_Izvodu RIGHT JOIN (IzvodStavke INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID) ON GK_Po_Izvodu.lnkIzvodStavkaID = IzvodStavke.ID
ORDER BY Izvod.Datum;


=====QUERY=====
IzvodiStavke_Simple
-----SQL-----
SELECT Izvod.ID_SK, Izvod.Datum, GK_Po_Izvodu.ID_K, GK_Po_Izvodu.Naziv, GK_Po_Izvodu.SumOfPIZNOS, GK_Po_Izvodu.SumOfDIZNOS
FROM GK_Po_Izvodu INNER JOIN (IzvodStavke INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID) ON GK_Po_Izvodu.lnkIzvodStavkaID = IzvodStavke.ID
ORDER BY Izvod.Datum;


=====QUERY=====
Izvodi-Sume
-----SQL-----
SELECT Izvod.Duguje, Izvod.Potrazuje, Sum(IzvodStavke.Zaduzenje) AS SumOfZaduzenje, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje, Izvod.NalogaOdobranja, Izvod.BrojIzvoda, Izvod.ID_SK, Izvod.Datum
FROM IzvodStavke INNER JOIN Izvod ON IzvodStavke.IzvodLNKID=Izvod.IzvodID
GROUP BY Izvod.Duguje, Izvod.Potrazuje, Izvod.NalogaOdobranja, Izvod.BrojIzvoda, Izvod.ID_SK, Izvod.Datum;


=====QUERY=====
IZVOD-NERASPOREDJENA-SREDSTVA
-----SQL-----
SELECT IzvodStavke.ID, Izvod.BrojIzvoda, IzvodStavke.RbStavke, IzvodStavke.DatumRealizacije, IzvodStavke.RbStavke, IzvodStavke.NazivPN, GK.lnkIzvodStavkaID, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, (IIf(First(GK.KONTO) Like "121112?",(([Odobrenje]-[Zaduzenje])*2)+Max([PIZNOS])+Max([DIZNOS]),([Odobrenje]-[Zaduzenje])-(Sum([PIZNOS]-[DIZNOS]))))=0 AS Kontrola, IzvodStavke.IzvodLNKID
FROM (GK RIGHT JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID
GROUP BY IzvodStavke.ID, Izvod.BrojIzvoda, IzvodStavke.RbStavke, IzvodStavke.DatumRealizacije, IzvodStavke.RbStavke, IzvodStavke.NazivPN, GK.lnkIzvodStavkaID, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, IzvodStavke.IzvodLNKID
HAVING ((((IIf(First(GK.KONTO) Like "121112?",(([Odobrenje]-[Zaduzenje])*2)+Max([PIZNOS])+Max([DIZNOS]),([Odobrenje]-[Zaduzenje])-(Sum([PIZNOS]-[DIZNOS]))))=0) Is Null))
ORDER BY IzvodStavke.RbStavke DESC;


=====QUERY=====
IzvodPrilivi
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.Zgrada, IzvodStavke.DatumRealizacije, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje
FROM IzvodStavke INNER JOIN Skustina ON IzvodStavke.ID_SK = Skustina.IDSkupstina
GROUP BY Skustina.IDSkupstina, Skustina.Zgrada, IzvodStavke.DatumRealizacije;


=====QUERY=====
IZVODSTAVKE_SUME
-----SQL-----
SELECT IzvodStavke.IzvodLNKID, IzvodStavke.ID_SK, Sum(IzvodStavke.Zaduzenje) AS SumOfZaduzenje, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje
FROM IzvodStavke
GROUP BY IzvodStavke.IzvodLNKID, IzvodStavke.ID_SK;


=====QUERY=====
IzvodSumaZO
-----SQL-----
SELECT IzvodStavke.RbNaloga, Sum(IzvodStavke.Zaduzenje) AS SumOfZaduzenje, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje, IzvodStavke.ID_SK
FROM IzvodStavke
GROUP BY IzvodStavke.RbNaloga, IzvodStavke.ID_SK;


=====QUERY=====
KILL_RACUN
-----SQL-----
DELETE Racun.*
FROM Table1 INNER JOIN Racun ON Table1.ID = Racun.IDRacun;


=====QUERY=====
KILL_SPC_TODO1-5
-----SQL-----
SELECT Racun.lnkGR, Racun.SPC, RacunStavke.*
FROM Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R
WHERE (((Racun.lnkGR)=116) AND ((Racun.SPC)=1) AND ((RacunStavke.ToDo)=1 Or (RacunStavke.ToDo)=2 Or (RacunStavke.ToDo)=3 Or (RacunStavke.ToDo)=4 Or (RacunStavke.ToDo)=5));


=====QUERY=====
KONTO_TROSKA_PLACANJA
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[pIZNOS]) AS stanje
FROM GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
WHERE (((GK.KONTO) Like "4350*") And ((GK.DATUM)>=Forms!Izvestaji!txtDatumOd And (GK.DATUM)<=Forms!Izvestaji!txtDatumDo))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv
HAVING (((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
KONTO_TROSKA_POTRAZIVANJA_DOB
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS
FROM GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
WHERE (((GK.KONTO) Like "4350*") And ((GK.DATUM)>=Forms!Izvestaji!txtDatumOd And (GK.DATUM)<=Forms!Izvestaji!txtDatumDo))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv
HAVING (((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
KONTO_TROSKA_POTRAZIVANJA_DOB_L2
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS, Kupac.Naziv
FROM (GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.KONTO) Like "4350*") AND ((GK.DATUM)>=[Forms]![Izvestaji]![txtDatumOd] And (GK.DATUM)<=[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv, Kupac.Naziv
HAVING (((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
KONTO_TROSKA_POTRAZIVANJA_STANAR
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
WHERE (((GK.KONTO) Like "2040*"))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv
HAVING (((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
KONTO_TROSKA_POTRAZIVANJA_STANAR_L2
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KONTO
FROM GK LEFT JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska, Troskovi_PodKonta.Naziv, GK.KONTO
HAVING (((GK.lnkSkupstinaID)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
KONTO_TROSKA_SUME
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfPIZNOS AS POTRAZUJU_DOB, KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfDIZNOS AS PLACENO_DOB, KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfDIZNOS AS DUGUJU_KUPCI, KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfPIZNOS AS NAPLACENO_KUPCIMA
FROM (Troskovi_PodKonta LEFT JOIN KONTO_TROSKA_POTRAZIVANJA_STANAR ON Troskovi_PodKonta.PodKonto = KONTO_TROSKA_POTRAZIVANJA_STANAR.KontoTroska) LEFT JOIN KONTO_TROSKA_POTRAZIVANJA_DOB ON Troskovi_PodKonta.PodKonto = KONTO_TROSKA_POTRAZIVANJA_DOB.KontoTroska
WHERE ((Not (KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfPIZNOS) Is Null) AND (Not (KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfDIZNOS) Is Null) AND (Not (KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfDIZNOS) Is Null) AND (Not (KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfPIZNOS) Is Null) AND ((Len([PodKonto]))=4));


=====QUERY=====
KONTO_TROSKA_SUME_L2
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfPIZNOS AS POTRAZUJU_DOB, KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfDIZNOS AS PLACENO_DOB, KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfDIZNOS AS DUGUJU_KUPCI, KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfPIZNOS AS NAPLACENO_KUPCIMA
FROM (Troskovi_PodKonta LEFT JOIN KONTO_TROSKA_POTRAZIVANJA_STANAR ON Troskovi_PodKonta.PodKonto = KONTO_TROSKA_POTRAZIVANJA_STANAR.KontoTroska) LEFT JOIN KONTO_TROSKA_POTRAZIVANJA_DOB ON Troskovi_PodKonta.PodKonto = KONTO_TROSKA_POTRAZIVANJA_DOB.KontoTroska
WHERE ((Not (KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfPIZNOS) Is Null) AND (Not (KONTO_TROSKA_POTRAZIVANJA_DOB.SumOfDIZNOS) Is Null) AND (Not (KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfDIZNOS) Is Null) AND (Not (KONTO_TROSKA_POTRAZIVANJA_STANAR.SumOfPIZNOS) Is Null) AND ((Len([PodKonto]))=4));


=====QUERY=====
KONTOLA-STAVKE-MESEC-RDOB-RDOBPRIKAZ
-----SQL-----
SELECT [KONTROLA-STAVKE-MESEC-RDOB].ID_RDOB, Count([KONTROLA-STAVKE-MESEC-RDOB].GrupaRacunaFXN) AS CountOfGrupaRacunaFXN, [KONTROLA-STAVKE-MESEC-RDOB].ID_SK
FROM [KONTROLA-STAVKE-MESEC-RDOB]
GROUP BY [KONTROLA-STAVKE-MESEC-RDOB].ID_RDOB, [KONTROLA-STAVKE-MESEC-RDOB].ID_SK
HAVING (((Count([KONTROLA-STAVKE-MESEC-RDOB].GrupaRacunaFXN))<>1));


=====QUERY=====
KONTROLA_001_GK_241
-----SQL-----
SELECT frmIzvestajFilterGK_TS.KONTO, Sum([DIZNOS]-[PIZNOS]) AS DIPI
FROM frmIzvestajFilterGK_TS
GROUP BY frmIzvestajFilterGK_TS.KONTO
HAVING (((frmIzvestajFilterGK_TS.KONTO) Like "241*"));


=====QUERY=====
KONTROLA_001_IZVOD_241
-----SQL-----
SELECT I.ID_SK, I.Datum, I.Napomena, I.NovoStanje, X.SumDuguje, X.SumPotrazuje
FROM Izvod AS I INNER JOIN (SELECT 
        Izvod2.Napomena,
        Max(Izvod2.Datum) AS MaxOfDatum, Sum(Duguje) as SumDuguje, Sum(Potrazuje) as SumPotrazuje
    FROM Izvod AS Izvod2
    WHERE Izvod2.ID_SK=[Forms]![Izvestaji]![cmbSZ]
    AND Izvod2.Datum <= [Forms]![Izvestaji]![txtDatumDo]
    GROUP BY Izvod2.Napomena
)  AS X ON (I.Napomena = X.Napomena OR (I.Napomena IS NULL AND X.Napomena IS NULL)) AND (I.Datum = X.MaxOfDatum)
WHERE (((I.ID_SK)=[Forms]![Izvestaji]![cmbSZ]));


=====QUERY=====
KONTROLA_DUPLI_RACUNI_DOB
-----SQL-----
SELECT Dobavljac_Racuni.IDTRRAC, Count(GK.STAVKAID) AS CountOfSTAVKAID, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.MesecRacuna
FROM GK RIGHT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.KONTO) Like "4*"))
GROUP BY Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.MesecRacuna
HAVING (((Count(GK.STAVKAID))>1));


=====QUERY=====
KONTROLA_SUME_TK
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS DIPI, Left([KontoTroska],2) AS KT, Count(GK.STAVKAID) AS CountOfSTAVKAID
FROM GK
GROUP BY GK.KONTO, GK.lnkSkupstinaID, Left([KontoTroska],2);


=====QUERY=====
KONTROLA-FILTER-GK-PARAMETAR
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.PARAMETRI)=[unesi-param]))
ORDER BY IsNull([KontoTroska]), GK.DIZNOS, GK.PIZNOS;


=====QUERY=====
KONTROLA-GK-GRP-RDOB
-----SQL-----
SELECT GK.RDOB, GK.lnkSkupstinaID, Count(GK.STAVKAID) AS CountOfSTAVKAID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
WHERE (((GK.lnkIzvodStavkaID)=0) AND ((GK.RacunIN_ID)=0))
GROUP BY GK.RDOB, GK.lnkSkupstinaID
HAVING (((GK.RDOB)>0));


=====QUERY=====
KONTROLA-IMAM-GK-NEMARSTAVKE
-----SQL-----
SELECT [KONTROLA-RSTAVKE-GRP-RDOB].ID_RDOB, [KONTROLA-GK-GRP-RDOB].RDOB, [KONTROLA-GK-GRP-RDOB].lnkSkupstinaID, [KONTROLA-GK-GRP-RDOB].SumOfDIZNOS, [KONTROLA-GK-GRP-RDOB].SumOfPIZNOS, Dobavljac_Racuni.TipDokumenta
FROM ([KONTROLA-GK-GRP-RDOB] LEFT JOIN [KONTROLA-RSTAVKE-GRP-RDOB] ON [KONTROLA-GK-GRP-RDOB].RDOB = [KONTROLA-RSTAVKE-GRP-RDOB].ID_RDOB) INNER JOIN Dobavljac_Racuni ON [KONTROLA-GK-GRP-RDOB].RDOB = Dobavljac_Racuni.IDTRRAC
WHERE ((([KONTROLA-RSTAVKE-GRP-RDOB].ID_RDOB) Is Null));


=====QUERY=====
KONTROLA-IMAM-RSTAVKE-NEMAGK
-----SQL-----
SELECT [KONTROLA-RSTAVKE-GRP-RDOB].GrupaRacunaFXN, [KONTROLA-RSTAVKE-GRP-RDOB].ID_SK, [KONTROLA-RSTAVKE-GRP-RDOB].ID_RDOB, [KONTROLA-GK-GRP-RDOB].RDOB
FROM [KONTROLA-GK-GRP-RDOB] RIGHT JOIN [KONTROLA-RSTAVKE-GRP-RDOB] ON [KONTROLA-GK-GRP-RDOB].RDOB = [KONTROLA-RSTAVKE-GRP-RDOB].ID_RDOB
WHERE ((([KONTROLA-GK-GRP-RDOB].RDOB) Is Null));


=====QUERY=====
KONTROLA-KNJIZENJA-RACUNA
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID] AS KontrolaSK, Count(GK.STAVKAID) AS BrojKnjizenja, Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv
FROM ((((GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) LEFT JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN GrupaRacuna ON GK.BR_NALOG = GrupaRacuna.NalogKN
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.lnkSkupstinaID, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID], Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv
HAVING (((GrupaRacuna.GrupaRacunaFXN)=[UNESI-MESECRACUNA]) AND ((GrupaRacuna.ID_SK)=[UNESI-SKUPSTINAID]));


=====QUERY=====
KONTROLA-KNJIZENJA-RACUNA-GK-RDOB-ISPRAVKA
-----SQL-----
SELECT "R-" & [MesecRacuna] AS Expr1, GK.DOK, Dobavljac_Racuni.KontoKnjizenja, GK.KontoTroska, Dobavljac_Racuni.IDTRRAC
FROM GK INNER JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((Dobavljac_Racuni.SK_ID)=[SKID]));


=====QUERY=====
KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.IDTRRAC, [MesecRacuna]=[GrupaRacunaFXN] AS Expr1, RacunStavke.ID_RDOB
FROM (RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC
GROUP BY GrupaRacuna.GrupaRacunaFXN, Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.IDTRRAC, [MesecRacuna]=[GrupaRacunaFXN], RacunStavke.ID_RDOB;


=====QUERY=====
KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE-gk
-----SQL-----
SELECT GK.DOK, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].GrupaRacunaFXN, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].MesecRacuna, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].SK_ID, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].IDTRRAC, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].Expr1, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].ID_RDOB
FROM [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE] INNER JOIN GK ON [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].IDTRRAC = GK.RDOB
GROUP BY GK.DOK, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].GrupaRacunaFXN, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].MesecRacuna, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].SK_ID, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].IDTRRAC, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].Expr1, [KONTROLA-KNJIZENJA-RACUNA-RACUNISTAVKE].ID_RDOB;


=====QUERY=====
KONTROLA-KNJIZENJA-RACUNA-SVE
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID] AS KontrolaSK, Count(GK.STAVKAID) AS BrojKnjizenja, Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.IDTRRAC
FROM ((((GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) LEFT JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN GrupaRacuna ON GK.BR_NALOG = GrupaRacuna.NalogKN
WHERE (((GK.KONTO)="2040" Or (GK.KONTO)="4350"))
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.lnkSkupstinaID, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID], Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.IDTRRAC
ORDER BY GrupaRacuna.ID_SK, Dobavljac_Racuni.MesecRacuna;


=====QUERY=====
KONTROLA-RDOB-NEMA-STAVKI
-----SQL-----
SELECT Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.Dobavljac, Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.TipObracuna, Dobavljac_Racuni.MarkerVandrednogRacuna, Dobavljac_Racuni.TipDokumenta
FROM Dobavljac_Racuni LEFT JOIN RacunStavke ON Dobavljac_Racuni.IDTRRAC = RacunStavke.ID_RDOB
WHERE (((RacunStavke.IDRacunStavke) Is Null))
GROUP BY Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.Dobavljac, Dobavljac_Racuni.MesecRacuna, Dobavljac_Racuni.TipObracuna, Dobavljac_Racuni.MarkerVandrednogRacuna, Dobavljac_Racuni.TipDokumenta
HAVING (((Dobavljac_Racuni.MesecRacuna) Like "19*"));


=====QUERY=====
KONTROLA-RSTAVKE-GRP-RDOB
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, RacunStavke.ID_SK, RacunStavke.ID_RDOB, Count(RacunStavke.IDRacunStavke) AS CountOfIDRacunStavke, GrupaRacuna.NalogKN, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, Sum(RacunStavke.IZNOSRACUNA) AS SumOfIZNOSRACUNA
FROM RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna
GROUP BY GrupaRacuna.GrupaRacunaFXN, RacunStavke.ID_SK, RacunStavke.ID_RDOB, GrupaRacuna.NalogKN
HAVING (((GrupaRacuna.NalogKN)<>0));


=====QUERY=====
KONTROLA-RSTAVKE-PB
-----SQL-----
SELECT GrupaRacuna.ID_SK, GrupaRacuna.GrupaRacunaFXN, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, RacunStavke.ID_RDOB, Count(RacunStavke.IDRacunStavke) AS CountOfIDRacunStavke, Replace([PozivNaBroj],"-","") AS Expr1
FROM (RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Racun ON (GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) AND (RacunStavke.lnkGR = Racun.lnkGR)
GROUP BY GrupaRacuna.ID_SK, GrupaRacuna.GrupaRacunaFXN, RacunStavke.ID_RDOB, Replace([PozivNaBroj],"-","")
HAVING (((GrupaRacuna.ID_SK)=106));


=====QUERY=====
KONTROLA-STAVKE-MESEC-RDOB
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, RacunStavke.ID_RDOB, Count(RacunStavke.IDRacunStavke) AS CountOfIDRacunStavke
FROM GrupaRacuna INNER JOIN RacunStavke ON GrupaRacuna.IDGrupaRacuna = RacunStavke.lnkGR
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, RacunStavke.ID_RDOB;


=====QUERY=====
KONTROLA-SUMA-KONTO-2410
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.DATUM, GK.lnkSkupstinaID
FROM GK
GROUP BY GK.KONTO, GK.DATUM, GK.lnkSkupstinaID
HAVING (((GK.KONTO)="2410") AND ((GK.lnkSkupstinaID)=101));


=====QUERY=====
KONTROLA-UPLATE-NA-STORNO-RACUN
-----SQL-----
SELECT Racun.IDRacun, GK_2040_RACID_SUM.lnkSkupstinaID, Racun.Storno, GK_2040_RACID_SUM.SumOfDIZNOS, GK_2040_RACID_SUM.SumOfPIZNOS, Racun.Ukupno, Round([SumOfDIZNOS]-[UKUPNO],2) AS RAZLIKA
FROM GK_2040_RACID_SUM INNER JOIN Racun ON GK_2040_RACID_SUM.RACID = Racun.IDRacun;


=====QUERY=====
KorisnikSZ
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Kupac.lnk_ID_SK
FROM Kupac
WHERE (((Kupac.ID_K)<8000) AND ((Kupac.lnk_ID_SK)=1));


=====QUERY=====
Kupac Without Matching Objekti
-----SQL-----
SELECT Kupac.*
FROM Kupac LEFT JOIN Objekti ON Kupac.[ID_K] = Objekti.[lnk_ID_K]
WHERE (((Kupac.ID_K)<8000) AND ((Objekti.lnk_ID_K) Is Null));


=====QUERY=====
Kupac_Dug
-----SQL-----
SELECT Kupac.ID_K AS lnkKUPACID, Nz([Kupac_Dug_0].[Suma],0) AS Suma0, Nz([Kupac_Dug_98].[Suma],0) AS Suma98
FROM (Kupac LEFT JOIN Kupac_DUG_0 ON Kupac.ID_K = Kupac_DUG_0.lnkKUPACID) LEFT JOIN Kupac_DUG_98 ON Kupac.ID_K = Kupac_DUG_98.lnkKUPACID;


=====QUERY=====
Kupac_DUG_0
-----SQL-----
SELECT GK_Filter_PD.lnkKUPACID, GK_Filter_PD.lnkSkupstinaID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Suma, GK_Filter_PD.KONTO
FROM GK_Filter_PD
WHERE (((GK_Filter_PD.GTS)=0 Or (GK_Filter_PD.GTS)=99))
GROUP BY GK_Filter_PD.lnkKUPACID, GK_Filter_PD.lnkSkupstinaID, GK_Filter_PD.KONTO;


=====QUERY=====
Kupac_DUG_97
-----SQL-----
SELECT GK_Filter_PD.lnkKUPACID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Suma
FROM GK_Filter_PD
WHERE (((GK_Filter_PD.GTS)=97))
GROUP BY GK_Filter_PD.lnkKUPACID;


=====QUERY=====
Kupac_DUG_98
-----SQL-----
SELECT GK_Filter_PD.lnkKUPACID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Suma
FROM GK_Filter_PD
WHERE (((GK_Filter_PD.GTS)=98))
GROUP BY GK_Filter_PD.lnkKUPACID;


=====QUERY=====
Kupac_Dug_OLD
-----SQL-----
SELECT GK.lnkKUPACID, Nz([Kupac_Dug_0].[Suma],0) AS Suma0, Nz([Kupac_Dug_98].[Suma],0) AS Suma98
FROM (GK LEFT JOIN Kupac_DUG_0 ON GK.lnkKUPACID = Kupac_DUG_0.lnkKUPACID) LEFT JOIN Kupac_DUG_98 ON GK.lnkKUPACID = Kupac_DUG_98.lnkKUPACID
GROUP BY GK.lnkKUPACID, Nz([Kupac_Dug_0].[Suma],0), Nz([Kupac_Dug_98].[Suma],0);


=====QUERY=====
Kupac_DUG_tempbackup
-----SQL-----
SELECT GK.lnkKUPACID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Suma
FROM GK
WHERE (((GK.TIP_STAVKE)>98 And (GK.TIP_STAVKE)<95)) OR (((GK.KNzaTIP)>98 And (GK.KNzaTIP)<95))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
Kupac_GK_GRP
-----SQL-----
SELECT GK.lnkKUPACID
FROM GK
GROUP BY GK.lnkKUPACID;


=====QUERY=====
KUPAC_GM
-----SQL-----
SELECT Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.lnk_ID_K, Objekti.HandOverDate, Objekti.Status, Objekti.naziv
FROM Objekti
WHERE (((Objekti.lnk_tip)=4));


=====QUERY=====
KUPAC_GRUPNI_RACUN_MASTERID
-----SQL-----
SELECT Kupac_1.ID_K AS MASTER_ID_K, Count(Kupac.ID_K) AS BROj_PODRACUNA, Kupac_1.lnk_ID_SK
FROM Kupac INNER JOIN Kupac AS Kupac_1 ON Kupac.IDGrupniRacunMaster = Kupac_1.ID_K
GROUP BY Kupac_1.ID_K, Kupac_1.lnk_ID_SK;


=====QUERY=====
KUPAC_NEMA_EMAIL_SLANJE
-----SQL-----
SELECT MailAktivni.IDPartner, MailNeAktivni.FirstOfeMail, Kupac.ID_K, Kupac.Naziv, Objekti.SifraPD
FROM ((Kupac LEFT JOIN MailAktivni ON Kupac.ID_K = MailAktivni.IDPartner) LEFT JOIN MailNeAktivni ON Kupac.ID_K = MailNeAktivni.IDPartner) LEFT JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O
WHERE (((MailAktivni.IDPartner) Is Null) AND ((Kupac.ID_K)<8000));


=====QUERY=====
Kupac_Opomena_001
-----SQL-----
SELECT GK_Filter_PD.*
FROM GK_Filter_PD
WHERE (((GK_Filter_PD.DATUM)<=[DATUMDI]) AND ((GK_Filter_PD.DIZNOS)>0) AND ((GK_Filter_PD.Flt)=0)) OR (((GK_Filter_PD.DATUM)<=[DATUMPI]) AND ((GK_Filter_PD.DIZNOS)<0) AND ((GK_Filter_PD.Flt)=0)) OR (((GK_Filter_PD.DATUM)<=[DATUMPI]) AND ((GK_Filter_PD.PIZNOS)<>0) AND ((GK_Filter_PD.Flt)=0));


=====QUERY=====
Kupac_PBPD
-----SQL-----
SELECT Kupac.ID_K, Replace([PBPD],"-","") AS PB
FROM Kupac
WHERE (((Kupac.PBPD) Is Not Null));


=====QUERY=====
Kupac_PD
-----SQL-----
SELECT Kupac_PBPD.ID_K, Sum([DIZNOS]-[PIZNOS]) AS PD, Count(GK.STAVKAID) AS CountOfSTAVKAID
FROM Kupac_PBPD INNER JOIN GK ON Kupac_PBPD.ID_K=GK.lnkKUPACID
GROUP BY Kupac_PBPD.ID_K;


=====QUERY=====
Kupac_PD_GDATUM
-----SQL-----
SELECT Kupac_PBPD.ID_K, Sum([DIZNOS]-[PIZNOS]) AS PD, Count(GK.STAVKAID) AS CountOfSTAVKAID
FROM Kupac_PBPD INNER JOIN GK ON Kupac_PBPD.ID_K = GK.lnkKUPACID
GROUP BY Kupac_PBPD.ID_K;


=====QUERY=====
KUPAC_STAN
-----SQL-----
SELECT Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.lnk_ID_K, Objekti.HandOverDate, Objekti.Status, Objekti.naziv
FROM Objekti
WHERE (((Objekti.lnk_tip)=1));


=====QUERY=====
KUPAC_TS
-----SQL-----
SELECT Kupac.ID_K AS IDK, Round(Nz([PD],0),2) AS PS, Round(Nz([Suma],0)-Nz([PD],0),2) AS STANJE, Round([Suma],2) AS SumaR
FROM Kupac_DUG RIGHT JOIN (Kupac_PD RIGHT JOIN Kupac ON Kupac_PD.ID_K = Kupac.ID_K) ON Kupac_DUG.lnkKUPACID = Kupac.ID_K;


=====QUERY=====
KUPAC_TS_GDATUM
-----SQL-----
SELECT Kupac.ID_K AS IDK, Round(Nz([PD],0),2) AS BLOK, Round(Nz([Suma],0)-Nz([PD],0),2) AS STANJE, Round([Suma],2) AS SumaR
FROM Kupac_DUG RIGHT JOIN (Kupac_PD RIGHT JOIN Kupac ON Kupac_PD.ID_K=Kupac.ID_K) ON Kupac_DUG.lnkKUPACID=Kupac.ID_K;


=====QUERY=====
KupacBezSlanja
-----SQL-----
SELECT Kupac.chkSkipPrintRacunGrupa, Kupac.ID_K, Kupac.lnk_ID_SK, Kupac.Naziv
FROM Kupac
WHERE (((Kupac.chkSkipPrintRacunGrupa)=-1));


=====QUERY=====
Kupac-Email-CR
-----SQL-----
SELECT Kupac.ID_K, IIf(Not IsNull([ID_K]),ConcatRelated("eMail","Mail","IDPartner = " & [ID_K])) AS Emails
FROM Kupac INNER JOIN Mail ON Kupac.ID_K = Mail.IDPartner;


=====QUERY=====
KupacLanjeEmailGroup
-----SQL-----
SELECT KupacSlanjeMail.IDPartner
FROM KupacSlanjeMail
GROUP BY KupacSlanjeMail.IDPartner;


=====QUERY=====
kupac-objekti-cr
-----SQL-----
SELECT Kupac.ID_K, Kupac.lnk_ID_SK, Kupac.Naziv, ConcatRelated("naziv","Objekti","lnk_ID_K = " & Nz([ID_K],0)) AS Objekti
FROM Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K
GROUP BY Kupac.ID_K, Kupac.lnk_ID_SK, Kupac.Naziv;


=====QUERY=====
KupacSlanjeMail
-----SQL-----
SELECT Mail.IDPartner, Mail.SendMailRacun, Mail.eMail
FROM Mail
WHERE (((Mail.SendMailRacun)=-1));


=====QUERY=====
Kupac-Za-Utuzenje
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, AdresaDostave.Adresa AS AdresaStana, AdresaDostave.Zgrada, Kupac.Adresa, Kupac.PBroj, Kupac.KupacGrad, Kupac.napomena, [Kupac-Email-CR].Emails, [kupac-objekti-cr].Objekti, AdresaDostave.SifraPD
FROM (((PrinterBinLOCAL INNER JOIN Kupac ON PrinterBinLOCAL.ID_Item = Kupac.ID_K) LEFT JOIN [Kupac-Email-CR] ON Kupac.ID_K = [Kupac-Email-CR].ID_K) LEFT JOIN [kupac-objekti-cr] ON Kupac.ID_K = [kupac-objekti-cr].ID_K) LEFT JOIN AdresaDostave ON Kupac.ID_K = AdresaDostave.ID_K
WHERE (((PrinterBinLOCAL.TypeIndex)=11));


=====QUERY=====
KupciSkupstine
-----SQL-----
SELECT Kupac.ID_K, Objekti.lnkSkupstinaID
FROM Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K;


=====QUERY=====
Last_GrupaOpomena
-----SQL-----
SELECT Last(GrupaOpomena.IDGrupaOpomena) AS LastOfIDGrupaOpomena
FROM GrupaOpomena;


=====QUERY=====
LastRacuniTroskovi
-----SQL-----
SELECT Troskovi_Racuni.MesecRacuna
FROM Troskovi_Racuni
GROUP BY Troskovi_Racuni.MesecRacuna, Right([MesecRacuna],2), Left([MesecRacuna],2)
ORDER BY Right([MesecRacuna],2) DESC , Left([MesecRacuna],2) DESC;


=====QUERY=====
LIST_001_SKUPSTINA-OBJEKTI
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS AS Skupstina, Skustina.RB AS SortRB, Count(Objekti.ID_O) AS BrojObjekata
FROM Skustina INNER JOIN Objekti ON Skustina.IDSkupstina = Objekti.lnkSkupstinaID
GROUP BY Skustina.IDSkupstina, Skustina.NazivSS, Skustina.RB;


=====QUERY=====
LIST_002_SKUPSTINA-KUPCI
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS AS Skupstina, Skustina.RB AS SortRB, Count(Kupac.ID_K) AS BrojKupaca
FROM Kupac INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina
GROUP BY Skustina.IDSkupstina, Skustina.NazivSS, Skustina.RB;


=====QUERY=====
LISTING_KUPAC_OBJEKTI
-----SQL-----
SELECT KUPAC_STAN.naziv, Kupac.Naziv, KUPAC_STAN.HandOverDate, KUPAC_STAN.Status, KUPAC_GM.naziv, KUPAC_GM.HandOverDate, KUPAC_GM.Status, Kupac.lnk_ID_SK
FROM (KUPAC_STAN INNER JOIN Kupac ON KUPAC_STAN.lnk_ID_K = Kupac.ID_K) LEFT JOIN KUPAC_GM ON Kupac.ID_K = KUPAC_GM.lnk_ID_K
ORDER BY KUPAC_STAN.HandOverDate;


=====QUERY=====
LISTING_KUPAC_OBJEKTI_CNT
-----SQL-----
SELECT KUPAC_STAN.naziv, Kupac.Naziv, KUPAC_STAN.HandOverDate, KUPAC_STAN.Status, Kupac.lnk_ID_SK, Count(KUPAC_GM.ID_O) AS CountOfID_O
FROM (KUPAC_STAN INNER JOIN Kupac ON KUPAC_STAN.lnk_ID_K = Kupac.ID_K) LEFT JOIN KUPAC_GM ON Kupac.ID_K = KUPAC_GM.lnk_ID_K
GROUP BY KUPAC_STAN.naziv, Kupac.Naziv, KUPAC_STAN.HandOverDate, KUPAC_STAN.Status, Kupac.lnk_ID_SK
ORDER BY KUPAC_STAN.HandOverDate;


=====QUERY=====
Mail_Crosstab
-----SQL-----
TRANSFORM First(Mail.[eMail]) AS FirstOfeMail
SELECT Mail.[IDPartner], First(Mail.[eMail]) AS [Total Of eMail]
FROM Mail
GROUP BY Mail.[IDPartner]
PIVOT Mail.[SendMailRacun];


=====QUERY=====
MailAktivni
-----SQL-----
SELECT Mail.IDPartner, First(Mail.eMail) AS FirstOfeMail, Count(Mail.IDeMail) AS CountOfIDeMail
FROM Mail
WHERE (((Mail.SendMailRacun)=-1))
GROUP BY Mail.IDPartner;


=====QUERY=====
MailGrupByIDK
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, SpojRedove("email","mail","IDPartner=" & Nz([ID_K],0),"SortOrder","; ") AS MailGrup, SpojRedove("email","mail","SendMailRacun=-1 AND IDPartner=" & Nz([ID_K],0),"SortOrder","; ") AS MailGrupRacun
FROM Kupac;


=====QUERY=====
MailNeAktivni
-----SQL-----
SELECT Mail.IDPartner, First(Mail.eMail) AS FirstOfeMail, Count(Mail.IDeMail) AS CountOfIDeMail
FROM Mail
WHERE (((Mail.SendMailRacun)=0))
GROUP BY Mail.IDPartner;


=====QUERY=====
MultiMailInOne
-----SQL-----
SELECT Mail.*
FROM Mail
WHERE (((Mail.eMail) Like '*;*'));


=====QUERY=====
Nalog_Tip_98
-----SQL-----
SELECT GK.TIP_STAVKE, GK.lnkKUPACID, GK.lnkSkupstinaID, GK.PARAMETRI, GK.DOK
FROM GK
WHERE (((GK.TIP_STAVKE)=98));


=====QUERY=====
Objekat_Sume
-----SQL-----
SELECT Objekti.lnkSkupstinaID AS ID_SK, Objekti.lnk_tip, Sum([K1]*[K2]) AS [SUM-K1K2], Sum(Objekti.K2) AS [SUM-K2], Sum(Objekti.K3) AS [SUM-K3], Sum(Objekti.K4) AS [SUM-K4], Sum(Objekti.K5) AS [SUM-K5]
FROM Objekti
GROUP BY Objekti.lnkSkupstinaID, Objekti.lnk_tip;


=====QUERY=====
Objekat_Sume_Po_TipovimaIzRacuna
-----SQL-----
SELECT Dobavljaci_Racun_TipObjekta.RacunID, Objekat_Sume.ID_SK, Sum(Objekat_Sume.[SUM-K1K2]) AS [SUM-K1K2], Sum(Objekat_Sume.[SUM-K2]) AS [SUM-K2], Sum(Objekat_Sume.[SUM-K3]) AS [SUM-K3], Sum(Objekat_Sume.[SUM-K4]) AS [SUM-K4], Sum(Objekat_Sume.[SUM-K5]) AS [SUM-K5]
FROM Dobavljac_Racuni INNER JOIN (Objekat_Sume INNER JOIN Dobavljaci_Racun_TipObjekta ON Objekat_Sume.lnk_tip = Dobavljaci_Racun_TipObjekta.TipObjekta) ON (Dobavljac_Racuni.IDTRRAC = Dobavljaci_Racun_TipObjekta.RacunID) AND (Dobavljac_Racuni.SK_ID = Objekat_Sume.ID_SK)
GROUP BY Dobavljaci_Racun_TipObjekta.RacunID, Objekat_Sume.ID_SK;


=====QUERY=====
Objekat_Sume_Sve
-----SQL-----
SELECT Objekti.lnkSkupstinaID AS ID_SK, Sum(Objekti.K1) AS [SUM-K1], Sum(Objekti.K2) AS [SUM-K2], Sum(Objekti.K3) AS [SUM-K3], Sum(Objekti.K4) AS [SUM-K4], Sum(Objekti.K5) AS [SUM-K5]
FROM Objekti
GROUP BY Objekti.lnkSkupstinaID;


=====QUERY=====
Objekti_By_KupacID
-----SQL-----
SELECT Objekti.lnk_ID_K, Objekti.lnkSkupstinaID, Count(Objekti.ID_O) AS CountOfID_O
FROM Objekti
GROUP BY Objekti.lnk_ID_K, Objekti.lnkSkupstinaID;


=====QUERY=====
Objekti_Crosstab
-----SQL-----
TRANSFORM Count(Objekti.ID_O) AS CountOfID_O
SELECT Objekti.lnkSkupstinaID, Count(Objekti.ID_O) AS [Total Of ID_O]
FROM Objekti
GROUP BY Objekti.lnkSkupstinaID
PIVOT Objekti.lnk_tip;


=====QUERY=====
Objekti_Prebrojavanje
-----SQL-----
TRANSFORM Count(Objekti.ID_O) AS CountOfID_O
SELECT Objekti.lnkSkupstinaID, Sum(Objekti.K1) AS SumOfK1, Sum(Objekti.K2) AS SumOfK2, Sum(Objekti.K3) AS SumOfK3, Sum(Objekti.K4) AS SumOfK4, Sum(Objekti.K5) AS SumOfK5
FROM Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
GROUP BY Objekti.lnkSkupstinaID
PIVOT TipObjekta.TipObj;


=====QUERY=====
Objekti_Sort
-----SQL-----
SELECT Objekti.lnkSkupstinaID, TipObjekta.SortObj, Objekti.naziv, Objekti.ID_O, Objekti.lnk_ID_K, [SortObj] & [naziv] AS SortN
FROM Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
ORDER BY Objekti.lnkSkupstinaID, TipObjekta.SortObj, Objekti.naziv;


=====QUERY=====
OBJEKTI_SS_TIP
-----SQL-----
SELECT Skustina.RB, Skustina.NazivSS, Objekti.ID_O, Objekti.kolicina, TipObjekta.Print, Objekti.lnk_ID_K, [kolicina]*[JO] AS CENA
FROM (Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID=Skustina.IDSkupstina) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj
ORDER BY Skustina.RB;


=====QUERY=====
OBJEKTI_SS_TIP_Crosstab
-----SQL-----
TRANSFORM Sum(OBJEKTI_SS_TIP.kolicina) AS SumOfkolicina
SELECT OBJEKTI_SS_TIP.RB, OBJEKTI_SS_TIP.NazivSS, Sum(OBJEKTI_SS_TIP.CENA) AS SumOfCENA
FROM OBJEKTI_SS_TIP
GROUP BY OBJEKTI_SS_TIP.RB, OBJEKTI_SS_TIP.NazivSS
ORDER BY OBJEKTI_SS_TIP.RB
PIVOT OBJEKTI_SS_TIP.Print;


=====QUERY=====
OBJEKTI_STAT
-----SQL-----
SELECT Skustina.RB, Skustina.NazivSS, Objekti.ID_O, Objekti.kolicina AS Expr1, TipObjekta.Print, Objekti.lnk_ID_K
FROM (Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
ORDER BY Skustina.RB;


=====QUERY=====
Objekti_Za_Racun
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Objekti.lnk_ID_K, Objekti.lnk_tip, TipObjekta.SortObj, Count(Objekti.ID_O) AS BrojObjekata, Sum(Objekti.K1) AS SumOfK1, Sum(Objekti.K2) AS SumOfK2, Sum(Objekti.K3) AS SumOfK3, Sum(Objekti.K4) AS SumOfK4, Sum(Objekti.K5) AS SumOfK5, Sum([K2]*[K1]) AS K21, Sum([K2]*[K3]) AS K23, Sum([K2]*[K4]) AS K24, Sum([K2]*[K5]) AS K25
FROM Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Objekti.Status)=1))
GROUP BY Objekti.lnkSkupstinaID, Objekti.lnk_ID_K, Objekti.lnk_tip, TipObjekta.SortObj
ORDER BY TipObjekta.SortObj;


=====QUERY=====
OBJEKTI-GM-KUPAC
-----SQL-----
SELECT Objekti.lnk_tip, Kupac.Naziv, Objekti.SifraPD, Objekti.lnkSkupstinaID, Kupac.ID_K, Objekti.ID_O
FROM Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
WHERE (((Objekti.lnk_tip)=4));


=====QUERY=====
oBJEKTIgpm
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.naziv, Objekti.lnk_tip, TipObjekta.TipObj, TipObjekta.SortObj, Objekti.BrojPD, Kupac.Naziv, Objekti.SifraPD, Objekti.Status, Objekti.K1, Objekti.K2
FROM (Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
WHERE (((Objekti.lnk_tip)=4));


=====QUERY=====
ObjektiKupca
-----SQL-----
SELECT Objekti.lnk_ID_K, Objekti.lnkSkupstinaID, Objekti.SifraPD, TipObjekta.SortObj, TipObjekta.Print
FROM Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
ORDER BY TipObjekta.SortObj, Objekti.BrojPD;


=====QUERY=====
ObjektiSaTipom
-----SQL-----
SELECT Objekti.ID_O, Objekti.lnk_ID_K, Objekti.naziv, TipObjekta.TipObj, TipObjekta.SortObj, Objekti.BrojPD, Objekti.BrojPD, [SortObj]*1000000+Nz([BrojPD],0) & [naziv] AS SorterFn
FROM Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
ORDER BY Objekti.BrojPD, [SortObj]*1000000+Nz([BrojPD],0) & [naziv];


=====QUERY=====
ObjektiSaTipomKoef
-----SQL-----
SELECT Objekti.ID_O, Objekti.lnk_ID_K, Objekti.naziv, TipObjekta.TipObj, TipObjekta.SortObj, Objekti.K1, Objekti.K2, Objekti.K3, Objekti.K4, Objekti.K5, Objekti.BrojPD, Objekti.BrStanara, Objekti.Ulaz, Objekti.SifraPD, Objekti.Status
FROM Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
ORDER BY TipObjekta.SortObj, Objekti.BrojPD;


=====QUERY=====
ObjektiSaTipomPrvi
-----SQL-----
SELECT ObjektiSaTipom.lnk_ID_K, First(ObjektiSaTipom.naziv) AS FirstOfnaziv, Min(ObjektiSaTipom.BrojPD) AS MinOfBrojPD
FROM ObjektiSaTipom
GROUP BY ObjektiSaTipom.lnk_ID_K
ORDER BY Min(ObjektiSaTipom.BrojPD);


=====QUERY=====
OBJEKTI-STAN-KUPAC
-----SQL-----
SELECT Objekti.lnk_tip, Kupac.Naziv, Objekti.SifraPD, Objekti.lnkSkupstinaID, Kupac.ID_K, Objekti.ID_O
FROM Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
WHERE (((Objekti.lnk_tip)=1));


=====QUERY=====
ObjektiSviPoKupcu
-----SQL-----
SELECT Kupac.ID_K, Kupac.lnk_ID_SK, [Kupac].[Naziv] & ' - ' & [NazivSS] AS Expr1, Kupac.Naziv, Skustina.NazivSS, ConcatRelated('naziv','Objekti','lnk_ID_K=' & [ID_K]) AS Objekti
FROM (Kupac LEFT JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) LEFT JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina
WHERE (((Kupac.lnk_ID_SK)=152))
GROUP BY Kupac.ID_K, Kupac.lnk_ID_SK, [Kupac].[Naziv] & ' - ' & [NazivSS], Kupac.Naziv, Skustina.NazivSS;


=====QUERY=====
OpomenaExportListing
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.Zgrada, Skustina.NazivSS, Kupac.ID_K, Kupac.Naziv, Kupac.Adresa, Kupac.PBroj, Kupac.PIB, Kupac.MB, Opomena.PozivNaBroj, Opomena.BNR, Opomena.Dug, Kupac.DostavaSifraPD, Objekti.SifraPD, Opomena.SumaPoStavkama, Kupac.KupacGrad, Skustina.Adresa, SzUlaz.Adresa, SzUlaz.Ulaz
FROM ((((PrinterBinLOCAL INNER JOIN Opomena ON PrinterBinLOCAL.ID_Item = Opomena.IDOpomena) INNER JOIN Kupac ON Opomena.lnkKupac = Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) INNER JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O) INNER JOIN SzUlaz ON Objekti.Ulaz = SzUlaz.Ulaz
WHERE (((PrinterBinLOCAL.TypeIndex)=2));


=====QUERY=====
OPOMENA-FILL-TROSKOVI
-----SQL-----
UPDATE Opomena SET Opomena.Troskovi = 4500, Opomena.Ukupno = [Dug]+4500
WHERE (((Opomena.lnkGrupaOpomena)=[GRUPA OPOMENA ID]));


=====QUERY=====
OpomenaZaRacun
-----SQL-----
SELECT GrupaOpomena.lnkGrupaRacuna, Opomena.lnkKupac, Opomena.txtRacunOp, Opomena.Dug, Opomena.ActivnaOpomena
FROM GrupaOpomena INNER JOIN Opomena ON GrupaOpomena.IDGrupaOpomena = Opomena.lnkGrupaOpomena
WHERE (((Opomena.ActivnaOpomena)=-1));


=====QUERY=====
OPOMENE_PARTNERI_SALDA
-----SQL-----
SELECT GK_PARTNERI.lnkKUPACID, GK_PARTNERI.SumOfDIZNOS, GK_PARTNERI.SumOfPIZNOS, GK_PARTNERI.DIPI
FROM Opomena INNER JOIN GK_PARTNERI ON Opomena.lnkKupac = GK_PARTNERI.lnkKUPACID
WHERE (((Opomena.lnkGrupaOpomena)=20)) OR (((Opomena.lnkGrupaOpomena)=21)) OR (((Opomena.lnkGrupaOpomena)=22));


=====QUERY=====
OpomeneListingContact
-----SQL-----
SELECT Skustina.Zgrada, Kupac.ID_K, Kupac.Naziv, Kupac.Adresa, Kupac.PBroj, Kupac.MB, Kupac.PIB, Kupac.napomena, [chkSkipPrintRacunGrupa]=0 AS ŠtampaniRacun, Objekti.naziv AS DostavaŠtampani, SzUlaz.Adresa AS DostavaŠtampaniAdresa, Mail.eMail, Mail.SendMailRacun AS RacunNaEmail, Kupac.KupacGrad
FROM (PrinterBinLOCAL INNER JOIN Opomena ON PrinterBinLOCAL.ID_Item = Opomena.IDOpomena) INNER JOIN (((Kupac LEFT JOIN Mail ON Kupac.ID_K = Mail.IDPartner) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) LEFT JOIN (Objekti LEFT JOIN SzUlaz ON Objekti.Ulaz = SzUlaz.Ulaz) ON Kupac.DostavaSifraPD = Objekti.ID_O) ON Opomena.lnkKupac = Kupac.ID_K
WHERE (((PrinterBinLOCAL.TypeIndex)=2))
ORDER BY Kupac.ID_K, Mail.SortOrder;


=====QUERY=====
PARKING
-----SQL-----
SELECT Skustina.Zgrada, Objekti.SifraPD, Objekti.netoKV
FROM Skustina INNER JOIN Objekti ON Skustina.IDSkupstina = Objekti.lnkSkupstinaID
WHERE (((Objekti.netoKV)>12.5) AND ((Objekti.lnk_tip)=4))
ORDER BY Skustina.Zgrada, Objekti.BrojPD;


=====QUERY=====
Prebrojavanje
-----SQL-----
SELECT Objekti.lnkSkupstinaID, TipObjekta.TipObj, Objekti.lnk_tip, Count(Objekti.ID_O) AS CountOfID_O, Sum(Objekti.K1) AS SumOfK1, Sum(Objekti.K2) AS SumOfK2, Sum(Objekti.Koeficijent) AS SumOfKoeficijent
FROM Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
GROUP BY Objekti.lnkSkupstinaID, TipObjekta.TipObj, Objekti.lnk_tip;


=====QUERY=====
PREBROJAVANJE-GK-PO-SK
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS, Count(GK.STAVKAID) AS CountOfSTAVKAID
FROM Skustina LEFT JOIN GK ON Skustina.IDSkupstina = GK.lnkSkupstinaID
GROUP BY Skustina.IDSkupstina, Skustina.NazivSS;


=====QUERY=====
prenos-povezivanje
-----SQL-----
SELECT PRENOS.*, Kupac.ID_K, Kupac.Naziv, Kupac.lnk_ID_SK
FROM Kupac RIGHT JOIN PRENOS ON Kupac.ExterniKonto = PRENOS.PB;


=====QUERY=====
Presek_Filter_Opomena
-----SQL-----
SELECT Opomena.*
FROM Opomena
WHERE (((Opomena.lnkGrupaOpomena)=Forms!Presek_Izvestaj!Combo43));


=====QUERY=====
Presek_Filter_Opomena_EXPORT
-----SQL-----
SELECT Presek_Filter_Opomena_R.IDSkupstina, Presek_Filter_Opomena_R.NazivSS, Presek_Filter_Opomena_R.ID_K, Presek_Filter_Opomena_R.Naziv, Nz([R1],0)-Nz([R2],0) AS [Prethodno stanje], Nz([R2],0) AS [Poslednji izdat racun], Nz([R3],0) AS [Uplaceno u tekucem mesecu], Nz([R4],0) AS [Preostali dug], Nz([R5],0) AS [Prethodni dug za BLOK 67], Nz([R6],0) AS [Uplaceno za BLOK 67], Nz([R7],0) AS [Preostali dug za BLOK 67], Presek_Filter_Opomena_R.CKOP AS [Opomena pred tužbu]
FROM Presek_Filter_Opomena_R;


=====QUERY=====
Presek_Filter_Opomena_EXPORT_BySKID
-----SQL-----
SELECT Presek_Filter_Opomena_R.IDSkupstina, Presek_Filter_Opomena_R.NazivSS, Presek_Filter_Opomena_R.ID_K, Presek_Filter_Opomena_R.Naziv, Nz([R1],0)-Nz([R2],0) AS [Prethodno stanje], Nz([R2],0) AS [Poslednji izdat racun], Nz([R3],0) AS [Uplaceno u tekucem mesecu], Nz([R4],0) AS [Preostali dug], Nz([R5],0) AS [Prethodni dug za BLOK 67], Nz([R6],0) AS [Uplaceno za BLOK 67], Nz([R7],0) AS [Preostali dug za BLOK 67], Presek_Filter_Opomena_R.CKOP AS [Opomena pred tužbu]
FROM Presek_Filter_Opomena_R
WHERE (((Presek_Filter_Opomena_R.IDSkupstina)=[Forms]![Presek_Izvestaj]![Combo37]));


=====QUERY=====
Presek_Filter_Opomena_R
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS, Kupac.ID_K, Kupac.Naziv, Presek_Sub_001.Suma AS R1, Presek_Sub_002.DIZNOS AS R2, Presek_Sub_003.Suma AS R3, Presek_Sub_004.Suma AS R4, Presek_Sub_005.Suma AS R5, Presek_Sub_006.Suma AS R6, Presek_Sub_007.Suma AS R7, Skustina.PrintNaziv, Not IsNull([IDOpomena]) AS CKOP
FROM Presek_Filter_Opomena RIGHT JOIN ((Presek_Sub_007 RIGHT JOIN (Presek_Sub_006 RIGHT JOIN (Presek_Sub_005 RIGHT JOIN (Presek_Sub_004 RIGHT JOIN (Presek_Sub_003 RIGHT JOIN (Presek_Sub_002 RIGHT JOIN ((Presek_Sub_001 RIGHT JOIN Kupac ON Presek_Sub_001.lnkKUPACID=Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK=Skustina.IDSkupstina) ON Presek_Sub_002.lnkKUPACID=Kupac.ID_K) ON Presek_Sub_003.lnkKUPACID=Kupac.ID_K) ON Presek_Sub_004.lnkKUPACID=Kupac.ID_K) ON Presek_Sub_005.lnkKUPACID=Kupac.ID_K) ON Presek_Sub_006.lnkKUPACID=Kupac.ID_K) ON Presek_Sub_007.lnkKUPACID=Kupac.ID_K) INNER JOIN Objekti_By_KupacID ON Kupac.ID_K=Objekti_By_KupacID.lnk_ID_K) ON Presek_Filter_Opomena.lnkKupac=Kupac.ID_K
WHERE (((Kupac.Naziv) Not Like "nepoznat*"))
ORDER BY Kupac.ID_K;


=====QUERY=====
Presek_Opomene_Sub_001
-----SQL-----
SELECT Kupac.lnk_ID_SK AS SZ, GK.lnkKUPACID, Sum([DIZNOS]-[PIZNOS]) AS Suma
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.PARAMETRI) Is Null) And ((GK.datum)<=Forms!Opomene_WorkInProgres_Status!Datum) And ((Kupac.ID_K)<8000)) Or (((GK.PARAMETRI) Not Like Nz(kupac.PBPD_F,"")) And ((GK.datum)<=Forms!Opomene_WorkInProgres_Status!Datum) And ((Kupac.ID_K)<8000))
GROUP BY Kupac.lnk_ID_SK, GK.lnkKUPACID
HAVING (((Sum([DIZNOS]-[PIZNOS]))>=[Forms]![Opomene_WorkInProgres_Status]![MinIznos] Or (Sum([DIZNOS]-[PIZNOS]))>=[Forms]![Opomene_WorkInProgres_Status]![MinIznos]));


=====QUERY=====
Presek_Opomene_Sub_002
-----SQL-----
SELECT GK.lnkKUPACID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Suma, IIf(IsNull([PARAMETRI]),'',Mid([PARAMETRI],3,10)) AS rPAR, Presek_Opomene_Sub_001.Suma AS U, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM (GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Presek_Opomene_Sub_001 ON Kupac.ID_K = Presek_Opomene_Sub_001.lnkKUPACID
WHERE (((GK.datum)<=[Forms]![Opomene_WorkInProgres_Status]![Datum]))
GROUP BY GK.lnkKUPACID, IIf(IsNull([PARAMETRI]),'',Mid([PARAMETRI],3,10)), Presek_Opomene_Sub_001.Suma;


=====QUERY=====
Presek_Opomene_Sub_003
-----SQL-----
SELECT Presek_Opomene_Sub_002.lnkKUPACID, Presek_Opomene_Sub_002.Suma, Presek_Opomene_Sub_002.rPAR, Presek_Opomene_Sub_002.U
FROM Presek_Opomene_Sub_002
WHERE (((Presek_Opomene_Sub_002.Suma)<-[Forms]![Opomene_WorkInProgres_Status]![TolerancijaDuga]));


=====QUERY=====
Presek_Opomene_Sub_004
-----SQL-----
SELECT Presek_Opomene_Sub_002.lnkKUPACID, Count(Presek_Opomene_Sub_002.Suma) AS CountOfSuma
FROM Presek_Opomene_Sub_002
WHERE (((Presek_Opomene_Sub_002.Suma)>[Forms]![Opomene_WorkInProgres_Status]![TolerancijaDuga]))
GROUP BY Presek_Opomene_Sub_002.lnkKUPACID
HAVING (((Count(Presek_Opomene_Sub_002.Suma))>=[Forms]![Opomene_WorkInProgres_Status]![MinBNR]));


=====QUERY=====
Presek_Opomene_Sub_005
-----SQL-----
SELECT Presek_Opomene_Sub_001.SZ, Presek_Opomene_Sub_004.lnkKUPACID, Presek_Opomene_Sub_004.CountOfSuma AS BNR, Presek_Opomene_Sub_001.Suma
FROM Presek_Opomene_Sub_004 INNER JOIN Presek_Opomene_Sub_001 ON Presek_Opomene_Sub_004.lnkKUPACID = Presek_Opomene_Sub_001.lnkKUPACID;


=====QUERY=====
Presek_Opomene_Sub_006
-----SQL-----
SELECT Presek_Opomene_Sub_002.*, Presek_Opomene_Sub_005.BNR, IIf(IsNull([rPAR]),0,Mid([rPAR],5,2)) AS M, IIf(IsNull([rPAR]),Year(Forms!Opomene_WorkInProgres_Status!Datum),Mid([rPAR],1,4)) AS Y, IIf(IsNull([rPAR]) Or [rPar]='',300,CInt(Mid([rPAR],5,2))+100) AS Mtxt
FROM Presek_Opomene_Sub_002 INNER JOIN Presek_Opomene_Sub_005 ON Presek_Opomene_Sub_002.lnkKUPACID = Presek_Opomene_Sub_005.lnkKUPACID;


=====QUERY=====
Presek_Opomene_Sub_007
-----SQL-----
INSERT INTO Opomena ( lnkKupac, BNR, Dug, lnkGrupaOpomena, PozivNaBroj )
SELECT Presek_Opomene_Sub_005.lnkKUPACID, Presek_Opomene_Sub_005.BNR, Presek_Opomene_Sub_005.Suma, Forms!Opomene_WorkInProgres_Status!IDGrupaOpomena AS Expr1, KontrolniBroj(97,[SZ] & "-" & [lnkKUPACID] & "-P" & Format([Forms]![Opomene_WorkInProgres_Status]![Datum],"yyyymmdd")) & "-" & [SZ] & "-" & [lnkKUPACID] & "-P" & Format([Forms]![Opomene_WorkInProgres_Status]![Datum],"yyyymmdd") AS KB
FROM Presek_Opomene_Sub_005;


=====QUERY=====
Presek_Opomene_Sub_008
-----SQL-----
INSERT INTO OpomenaStavke ( lnkOpomena, lnkKupacID, M, Y, Di, Pi, Suma, mTXT )
SELECT Opomena.IDOpomena, Presek_Opomene_Sub_006.lnkKUPACID, Presek_Opomene_Sub_006.M, Presek_Opomene_Sub_006.Y, Presek_Opomene_Sub_006.SumOfDIZNOS, Presek_Opomene_Sub_006.SumOfPIZNOS, Round([SumOfDIZNOS]-[SumOfPIZNOS],2) AS Sm, [Poruka] & ' ' & [Y] & '.' AS Expr2
FROM Opomena INNER JOIN (Presek_Opomene_Sub_006 INNER JOIN _Recnik ON Presek_Opomene_Sub_006.Mtxt = [_Recnik].Index) ON Opomena.lnkKupac = Presek_Opomene_Sub_006.lnkKUPACID
WHERE (((Opomena.lnkGrupaOpomena)=[Forms]![Opomene_WorkInProgres_Status]![IDGrupaOpomena]) AND (([_Recnik].Jezik)="SR-LAT-NC"));


=====QUERY=====
Presek_Opomene_Sub_009
-----SQL-----
DELETE Opomena.lnkGrupaOpomena, OpomenaStavke.*
FROM OpomenaStavke INNER JOIN Opomena ON OpomenaStavke.lnkOpomena = Opomena.IDOpomena
WHERE (((Opomena.lnkGrupaOpomena)=[Forms]![Opomene_WorkInProgres_Status]![IDGrupaOpomena]));


=====QUERY=====
Presek_Opomene_Sub_010
-----SQL-----
DELETE Opomena.lnkGrupaOpomena, Opomena.*
FROM Opomena
WHERE (((Opomena.lnkGrupaOpomena)=[Forms]![Opomene_WorkInProgres_Status]![IDGrupaOpomena]));


=====QUERY=====
Presek_Sub_000
-----SQL-----
SELECT GK.lnkKUPACID, Sum([DIZNOS]-[PIZNOS]) AS Suma
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.KNzaTIP)=98) And ((GK.TIP_STAVKE)=98) And ((GK.DATUM)<=Forms!Presek_Izvestaj!Text13))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
Presek_Sub_001
-----SQL-----
SELECT GK.lnkKUPACID, Sum([DIZNOS]-[PIZNOS]) AS Suma
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.KNzaTIP)<>98) And ((GK.TIP_STAVKE)<>98) And ((GK.DATUM)<=Forms!Presek_Izvestaj!Text13))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
Presek_Sub_002
-----SQL-----
SELECT GK.lnkKUPACID, Sum(GK.DIZNOS) AS SumOfDIZNOS
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.DATUM)=Forms!Presek_Izvestaj!Text41) And ((GK.TIP_STAVKE)=3))
GROUP BY GK.lnkKUPACID
HAVING (((Sum(GK.DIZNOS))>0));


=====QUERY=====
Presek_Sub_003
-----SQL-----
SELECT GK.lnkKUPACID, Sum([pIZNOS]-[dIZNOS]) AS Suma
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.KNzaTIP)<>98) And ((GK.DATUM)>Forms!Presek_Izvestaj!Text13 And (GK.DATUM)<=Forms!Presek_Izvestaj!Text15) And ((GK.TIP_STAVKE)<>3 And (GK.TIP_STAVKE)<>98))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
Presek_Sub_004
-----SQL-----
SELECT GK.lnkKUPACID, Sum([DIZNOS]-[PIZNOS]) AS Suma, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.TIP_STAVKE)<>98) And ((GK.DATUM)<=Forms!Presek_Izvestaj!Text15) And ((GK.KNzaTIP)<>98))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
Presek_Sub_005
-----SQL-----
SELECT GK.lnkKUPACID, Sum([DIZNOS]-[PIZNOS]) AS Suma, Kupac.lnk_ID_SK AS SZ
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
WHERE (((GK.PARAMETRI) Like kupac.PBPD_F) And ((GK.DATUM)<Forms!Presek_Izvestaj!Text13))
GROUP BY GK.lnkKUPACID, Kupac.lnk_ID_SK;


=====QUERY=====
Presek_Sub_006
-----SQL-----
SELECT GK.lnkKUPACID, Sum([pIZNOS]-[dIZNOS]) AS Suma
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID=Kupac.ID_K
WHERE (((GK.PARAMETRI) Like kupac.PBPD_F) And ((GK.DATUM)>=Forms!Presek_Izvestaj!Text13 And (GK.DATUM)<=Forms!Presek_Izvestaj!Text15))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
Presek_Sub_007
-----SQL-----
SELECT GK.lnkKUPACID, Sum([DIZNOS]-[PIZNOS]) AS Suma
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID=Kupac.ID_K
WHERE (((GK.PARAMETRI) Like kupac.PBPD_F) And ((GK.DATUM)<=Forms!Presek_Izvestaj!Text15))
GROUP BY GK.lnkKUPACID;


=====QUERY=====
PRESEK-BENEFITI
-----SQL-----
SELECT IMPORT_BENEFIT.PDI AS [BW UNIT], Objekti.Ulaz, TipObjekta_1.TipObj AS [Unit Type], Objekti.SifraPD AS Unit, Objekti.K1 AS KV, Kupac.Naziv AS Invoice, oBJEKTIgpm.SifraPD AS [Unit GPS], oBJEKTIgpm.K1 AS [KV GPS], oBJEKTIgpm.K2 AS [K2 GPS], MailGrupByIDK.MailGrupRacun, MailGrupByIDK.MailGrup
FROM MailGrupByIDK RIGHT JOIN (oBJEKTIgpm RIGHT JOIN (((Objekti RIGHT JOIN IMPORT_BENEFIT ON Objekti.SifraPD = IMPORT_BENEFIT.PDI) LEFT JOIN TipObjekta AS TipObjekta_1 ON Objekti.lnk_tip = TipObjekta_1.IDTipObj) LEFT JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) ON oBJEKTIgpm.lnk_ID_K = Objekti.lnk_ID_K) ON MailGrupByIDK.ID_K = Objekti.lnk_ID_K
ORDER BY Objekti.lnkSkupstinaID, Objekti.BrojPD, oBJEKTIgpm.BrojPD;


=====QUERY=====
Provera Budzeta
-----SQL-----
SELECT Troskovi_Budzet.ID_Troskovi_Budzet, Troskovi_Budzet.Sort, Troskovi_Budzet.Suma, Sum(Troskovi_Ugovori.Vrednost) AS SumOfVrednost, Troskovi_Budzet.TipObracuna, Troskovi_Ugovori.TipObracuna, Troskovi_Budzet.KontoKorisnika, Troskovi_Ugovori.lnk_ID_Dob
FROM Troskovi_Ugovori INNER JOIN Troskovi_Budzet ON Troskovi_Ugovori.IzBudzeta = Troskovi_Budzet.ID_Troskovi_Budzet
GROUP BY Troskovi_Budzet.ID_Troskovi_Budzet, Troskovi_Budzet.Sort, Troskovi_Budzet.Suma, Troskovi_Budzet.TipObracuna, Troskovi_Ugovori.TipObracuna, Troskovi_Budzet.KontoKorisnika, Troskovi_Ugovori.lnk_ID_Dob
ORDER BY Troskovi_Budzet.Sort;


=====QUERY=====
qKupacTip
-----SQL-----
SELECT Kupac.*, Objekti.*, TipObjekta.*
FROM (Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj;


=====QUERY=====
qKupacTip_Crosstab
-----SQL-----
TRANSFORM Count(qKupacTip.ID_O) AS CountOfID_O
SELECT qKupacTip.ID_K, qKupacTip.lnkSkupstinaID
FROM qKupacTip
GROUP BY qKupacTip.ID_K, qKupacTip.lnkSkupstinaID
PIVOT qKupacTip.TipObj;


=====QUERY=====
qKupacTip_FilterBy
-----SQL-----
SELECT qKupacTip_Crosstab.ID_K, qKupacTip_Crosstab.lnkSkupstinaID, qKupacTip_Crosstab.G, qKupacTip_Crosstab.GPM, qKupacTip_Crosstab.Lokali, qKupacTip_Crosstab.OPM, qKupacTip_Crosstab.Stanovi
FROM qKupacTip_Crosstab
WHERE (((qKupacTip_Crosstab.lnkSkupstinaID)=3) AND ((qKupacTip_Crosstab.G) Is Not Null) AND ((qKupacTip_Crosstab.Stanovi) Is Not Null)) OR (((qKupacTip_Crosstab.lnkSkupstinaID)=3) AND ((qKupacTip_Crosstab.GPM) Is Not Null) AND ((qKupacTip_Crosstab.Stanovi) Is Not Null));


=====QUERY=====
QRCODE
-----SQL-----
SELECT 'PR' AS K, '01' AS V, '1' AS C, TrZaQRcode([Skustina].[TR]) AS R, Left([Skustina].[PrintNaziv] & Chr(13) & Chr(10) & [Skustina].[PBroj],70) AS N, 'RSD' & Format(IIf(NZ([MarkerVanderdnihRacuna],'')='',IIf([PrethodniDug]+[Ukupno]>0,[PrethodniDug]+[Ukupno],0),[Ukupno]),'Fixed') AS I, Left([Kupac] & Chr(13) & Chr(10) & [Adresa_K] & ', ' & [PBroj_K],70) AS P, 221 AS SF, Left([Skustina_1].[Doznaka] & ' ' & [GrupaRacunaFXT],35) AS S, QRCodePozivNaBroj(97,[PozivNaBroj]) AS RO, Racun.IDRacun AS SKIP_IDR, Skustina.Folder AS SKIP_FOLDER, Racun.lnkGR AS SKIP_GR, Racun.PozivNaBroj AS SKIP_PB
FROM ((GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) INNER JOIN Skustina AS Skustina_1 ON Racun.ID_SK = Skustina_1.IDSkupstina) INNER JOIN Skustina ON Skustina_1.InvoiceIssuer = Skustina.IDSkupstina
WHERE (((Racun.IDRacun)=30591));


=====QUERY=====
QRCODE_OPOMENA
-----SQL-----
SELECT 'PR' AS K, '01' AS V, '1' AS C, TrZaQRcode([Skustina].[TR]) AS R, Left([Skustina].[PrintNaziv] & Chr(13) & Chr(10) & [Skustina].[PBroj],70) AS N, 'RSD' & Format([Opomena].[Ukupno],'Fixed') AS I, Left([Kupac].[Naziv] & Chr(13) & Chr(10) & IIf(IsNull([Kupac].[Adresa]),[SzUlaz].[Adresa],[Kupac].[Adresa]) & ', ' & IIf(IsNull([Kupac].[Adresa]),[Skustina_1].[PBroj],[Kupac].[PBroj] & ' ' & [Kupac].[KupacGrad]),70) AS P, 221 AS SF, Left([GrupaOpomena].[Doznaka],35) AS S, QRCodePozivNaBroj(97,[PozivNaBroj]) AS RO, Opomena.IDOpomena AS SKIP_IDR, Skustina.Folder AS SKIP_FOLDER, Opomena.lnkGrupaOpomena AS SKIP_GR, Opomena.PozivNaBroj AS SKIP_PB
FROM (Skustina AS Skustina_1 RIGHT JOIN ((Skustina INNER JOIN ((Kupac INNER JOIN (Opomena INNER JOIN PrinterBinLOCAL ON Opomena.IDOpomena = PrinterBinLOCAL.ID_Item) ON Kupac.ID_K = Opomena.lnkKupac) INNER JOIN GrupaOpomena ON Opomena.lnkGrupaOpomena = GrupaOpomena.IDGrupaOpomena) ON Skustina.IDSkupstina = GrupaOpomena.IDSZ) LEFT JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O) ON Skustina_1.IDSkupstina = Objekti.lnkSkupstinaID) LEFT JOIN SzUlaz ON Objekti.Ulaz = SzUlaz.Ulaz
WHERE (((Opomena.lnkGrupaOpomena)=16) AND ((PrinterBinLOCAL.TypeIndex)=2));


=====QUERY=====
qryTemplateKnjizenja
-----SQL-----
SELECT TemplateIzvodaKnjizenje.IDTemplate, TemplateIzvodaKnjizenje.Template
FROM TemplateIzvodaKnjizenje
GROUP BY TemplateIzvodaKnjizenje.IDTemplate, TemplateIzvodaKnjizenje.Template;


=====QUERY=====
qryTipDokumenta
-----SQL-----
SELECT tblShortList.Index, tblShortList.Cat1, tblShortList.ShortName, tblShortList.Caption
FROM tblShortList
WHERE (((tblShortList.TableFrom)="Dobavljac_Racuni"));


=====QUERY=====
Query1
-----SQL-----
SELECT Racun.*
FROM Racun
WHERE (((Racun.lnkGR)=11) AND ((ID_K)=1002) AND ((Storno)=0));


=====QUERY=====
Query10
-----SQL-----
UPDATE Objekti SET Objekti.IONaslov = "Investiciono održavanje"
WHERE (((Objekti.IO) Is Not Null));


=====QUERY=====
Query100
-----SQL-----
SELECT GrupaRacuna.DatumIzdavanja
FROM GrupaRacuna
WHERE (((GrupaRacuna.IDGrupaRacuna)=1));


=====QUERY=====
Query101
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna AS IDGR, GrupaRacuna.GrupaRacunaFXN AS YYMM, GrupaRacuna.GrupaRacunaFXT AS Mesec, [IDSkupstina] & ' - ' & [NazivSS] AS SZ, GrupaRacuna.DatumIzdavanja, GrupaRacuna.NalogKN, Skustina.IDSkupstina, Count(Racun.IDRacun) AS BrojRacuna, GrupaRacuna.MarkerVanderdnihRacuna AS MVR
FROM (GrupaRacuna INNER JOIN Skustina ON GrupaRacuna.ID_SK = Skustina.IDSkupstina) INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((GrupaRacuna.GrupaRacunaFXN)='2002'))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, [IDSkupstina] & ' - ' & [NazivSS], GrupaRacuna.DatumIzdavanja, GrupaRacuna.NalogKN, Skustina.IDSkupstina, GrupaRacuna.MarkerVanderdnihRacuna
ORDER BY GrupaRacuna.GrupaRacunaFXN, [IDSkupstina] & ' - ' & [NazivSS], GrupaRacuna.DatumIzdavanja DESC;


=====QUERY=====
Query102
-----SQL-----
SELECT GrupaOpomena.Naslov, GrupaOpomena.Datum, GrupaOpomena.IDSZ
FROM GrupaOpomena
WHERE (((GrupaOpomena.Naslov)="w") AND ((GrupaOpomena.Datum)="w") AND ((GrupaOpomena.IDSZ)="w"));


=====QUERY=====
Query103
-----SQL-----
SELECT '' AS STAVKAID, '' AS BR_NALOG, '' AS DATUM, '' AS TIP_STAVKE, '' AS Napomena, Sum(GK.PIZNOS) AS PI, Sum(GK.DIZNOS) AS DI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, GK.lnkKUPACID, '' AS PARAMETRI, GK.DOK AS DOK, GK.lnkSkupstinaID
FROM GK
GROUP BY GK.lnkKUPACID, '', GK.DOK, GK.lnkSkupstinaID, '', '', '', '', ''
ORDER BY GK.DOK;


=====QUERY=====
Query104
-----SQL-----
SELECT GK.*
FROM GK;


=====QUERY=====
Query105
-----SQL-----
SELECT GK.*
FROM GK;


=====QUERY=====
Query106
-----SQL-----
SELECT GK.DOK
FROM GK
WHERE (((GK.DOK)='')) OR (((GK.DOK) Is Null));


=====QUERY=====
Query107
-----SQL-----
SELECT Len([KONTO]) AS Expr1, GK.*
FROM GK;


=====QUERY=====
Query108
-----SQL-----
SELECT Kupac.lnk_ID_SK, Kupac.ID_K, Opomena.Dug, Opomena.SumaPoStavkama
FROM Opomena INNER JOIN Kupac ON Opomena.lnkKupac = Kupac.ID_K
WHERE ((([Dug]=[SumaPoStavkama])=0))
ORDER BY Kupac.lnk_ID_SK, Kupac.ID_K;


=====QUERY=====
Query109
-----SQL-----
SELECT IzvodStavke.ID, Sum(([DIZNOS]-[PIZNOS])) AS GKSuma, ([Zaduzenje]-[Odobrenje]) AS IzvodStavkaSuma
FROM IzvodStavke INNER JOIN GK ON IzvodStavke.ID = GK.lnkIzvodStavkaID
GROUP BY IzvodStavke.ID
HAVING (((IzvodStavke.ID)=19473));


=====QUERY=====
Query11
-----SQL-----
INSERT INTO RacunStavke_Troskovi ( ID_R, lnkGR, ID_K, ID_SK, Ukupno )
SELECT Racuni_Troskovi.IDRacun_T, Racuni_Troskovi.lnkGrp, Troskovi_Ugovori.lnk_ID_Dob, Troskovi_Ugovori.lnk_ID_SK, Round(Troskovi_Ugovori.Vrednost*(1+Troskovi_Ugovori.PDV)*Racuni_Troskovi.nbs,2) AS Cena
FROM Troskovi_Ugovori INNER JOIN Racuni_Troskovi ON Troskovi_Ugovori.lnk_ID_SK=Racuni_Troskovi.lnk_IDSK
WHERE (((Racuni_Troskovi.lnkGrp)=4));


=====QUERY=====
Query110
-----SQL-----
SELECT IzvodStavke.ID, [Zaduzenje]-[Odobrenje] AS Expr1, Sum([DIZNOS]-[PIZNOS]) AS Expr2
FROM GK INNER JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID
GROUP BY IzvodStavke.ID, [Zaduzenje]-[Odobrenje]
HAVING (((IzvodStavke.ID)=19473));


=====QUERY=====
Query111
-----SQL-----
SELECT OpomenaSablonEx.SablonText
FROM OpomenaSablonEx
WHERE (((OpomenaSablonEx.IDOpomenaSablon)=1) AND ((OpomenaSablonEx.KeyName)="rptHeaderText"))
ORDER BY OpomenaSablonEx.KeyIndex;


=====QUERY=====
Query112
-----SQL-----
SELECT Dobavljac_Racuni.IDTRRAC, [NazivRacuna] & " /  " & [Naziv] AS Expr1, Dobavljac_Racuni.MesecRacuna
FROM Kupac INNER JOIN Dobavljac_Racuni ON Kupac.ID_K = Dobavljac_Racuni.DobavljacKonto
WHERE (((Dobavljac_Racuni.MesecRacuna)='[Forms]![Racun_Det]![GrupaRacunaFXT]'));


=====QUERY=====
Query113
-----SQL-----
SELECT Opomena.IDOpomena, Opomena.lnkKupac AS IDK, GrupaOpomena.Datum, Skustina.Folder
FROM ((PrinterBinLOCAL INNER JOIN Opomena ON PrinterBinLOCAL.ID_Item = Opomena.IDOpomena) INNER JOIN GrupaOpomena ON Opomena.lnkGrupaOpomena = GrupaOpomena.IDGrupaOpomena) INNER JOIN Skustina ON GrupaOpomena.IDSZ = Skustina.IDSkupstina
WHERE (((PrinterBinLOCAL.TypeIndex)=2));


=====QUERY=====
Query114
-----SQL-----
SELECT IzvodStavke.ID, [IzvodStavke].[ID_SK]=[Izvod].[ID_SK] AS Expr1, IzvodStavke.ID_SK, Izvod.ID_SK, IzvodStavke.NazivPN
FROM IzvodStavke INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID
WHERE ((([IzvodStavke].[ID_SK]=[Izvod].[ID_SK])=0));


=====QUERY=====
Query115
-----SQL-----
SELECT Izvod.Datum, Izvod.IzvodID, Skustina.IDSkupstina, Izvod.ID_SK, IzvodStavke.NazivPN, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje, Izvod.NalogZaKnjizenje, IzvodStavke.opt_lnk_Kupac, IzvodStavke.ID, IzvodStavke.DatumRealizacije
FROM (Izvod LEFT JOIN Skustina ON Izvod.ID_SK = Skustina.IDSkupstina) LEFT JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID
WHERE (((Skustina.IDSkupstina) Is Null));


=====QUERY=====
Query116
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Count(Racun.IDRacun) AS CountOfIDRacun
FROM Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
GROUP BY GrupaRacuna.GrupaRacunaFXN;


=====QUERY=====
Query117
-----SQL-----
SELECT GK.BR_NALOG, GK.lnkSkupstinaID
FROM GK
GROUP BY GK.BR_NALOG, GK.lnkSkupstinaID;


=====QUERY=====
Query118
-----SQL-----
SELECT Dobavljac_Racuni.*, GK.*, Nalog.*
FROM (Dobavljac_Racuni INNER JOIN GK ON Dobavljac_Racuni.IDTRRAC = GK.RacunIN_ID) INNER JOIN Nalog ON GK.BR_NALOG = Nalog.Br_Nalog
WHERE (((Dobavljac_Racuni.TipDokumenta)=2));


=====QUERY=====
Query119
-----SQL-----
SELECT Dobavljac_Racuni.*
FROM Dobavljac_Racuni
WHERE (((Dobavljac_Racuni.TipObracuna)=2));


=====QUERY=====
Query12
-----SQL-----
INSERT INTO GK ( BR_NALOG, KONTO, DATUM, PIZNOS, TIP_STAVKE, DOK, lnkSkupstinaID, lnkKUPACID, PARAMETRI )
SELECT [mNalog] AS Expr1, "4330" & Format([ID_K],"0000") AS mKonto, [mDatum] AS mDatum, RacunStavke_Troskovi.Ukupno, 4 AS mTip, "UF" & [mMarker] AS mDOK, RacunStavke_Troskovi.ID_SK, RacunStavke_Troskovi.ID_K, [mMarker] AS mPar
FROM RacunStavke_Troskovi
WHERE (((RacunStavke_Troskovi.lnkGR)=1));


=====QUERY=====
Query120
-----SQL-----
SELECT GK.BR_NALOG, Count(IzvodStavke.ID) AS CountOfID, IzvodStavke.IzvodLNKID, Sum(GK.DIZNOS) AS SumOfZaduzenje, Sum(GK.PIZNOS) AS SumOfOdobrenje, [Zaduzenje]<>0 AS DUGUJE, [Odobrenje]<>0 AS POTRAZUJE, IzvodStavke.ID_SK, 'IZVOD ' & [BrojIzvoda] & '/' & Year([Izvod].[Datum]) AS DOKUMENT, Izvod.NalogZaKnjizenje, Izvod.Datum
FROM (GK RIGHT JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID
GROUP BY GK.BR_NALOG, IzvodStavke.IzvodLNKID, [Zaduzenje]<>0, [Odobrenje]<>0, IzvodStavke.ID_SK, 'IZVOD ' & [BrojIzvoda] & '/' & Year([Izvod].[Datum]), Izvod.NalogZaKnjizenje, Izvod.Datum
HAVING (((IzvodStavke.IzvodLNKID)=19));


=====QUERY=====
Query121
-----SQL-----
SELECT GK.DATUM, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.DOK
FROM GK
GROUP BY GK.DATUM, GK.DOK, GK.KONTO, GK.lnkSkupstinaID
HAVING (((GK.KONTO)="2410") AND ((GK.lnkSkupstinaID)=101))
ORDER BY GK.DATUM;


=====QUERY=====
Query122
-----SQL-----
SELECT GK.KONTO, GK.RDOB
FROM GK
GROUP BY GK.KONTO, GK.RDOB
HAVING (((GK.KONTO)="5590"));


=====QUERY=====
Query123
-----SQL-----
SELECT Count(Query122.KONTO) AS CountOfKONTO, Query122.RDOB
FROM Query122
GROUP BY Query122.RDOB
HAVING (((Count(Query122.KONTO))>1));


=====QUERY=====
Query124
-----SQL-----
SELECT ERROR_082_GK_4900.SumOfDIZNOS, ERROR_082_GK_4900.SumOfPIZNOS, ERROR_081_RACUNI_SUME.ID_SK, ERROR_082_GK_4900.lnkIzvodStavkaID, ERROR_081_RACUNI_SUME.SumOfUkupno, ERROR_081_RACUNI_SUME.DatumIzdavanja, ERROR_081_RACUNI_SUME.GrupaRacunaFXT, ERROR_081_RACUNI_SUME.GrupaRacunaFXN, ERROR_081_RACUNI_SUME.MarkerVanderdnihRacuna, ERROR_081_RACUNI_SUME.NalogKN, ERROR_081_RACUNI_SUME.CountOfIDRacun
FROM ERROR_081_RACUNI_SUME INNER JOIN ERROR_082_GK_4900 ON ERROR_081_RACUNI_SUME.NalogKN = ERROR_082_GK_4900.BR_NALOG;


=====QUERY=====
Query125
-----SQL-----
SELECT [ERROR_075_GK-RDOB-GRPSK].RDOB, Count([ERROR_075_GK-RDOB-GRPSK].lnkSkupstinaID) AS CountOflnkSkupstinaID, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv
FROM ([ERROR_075_GK-RDOB-GRPSK] INNER JOIN Dobavljac_Racuni ON [ERROR_075_GK-RDOB-GRPSK].RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K
GROUP BY [ERROR_075_GK-RDOB-GRPSK].RDOB, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv
HAVING (((Count([ERROR_075_GK-RDOB-GRPSK].lnkSkupstinaID))>1));


=====QUERY=====
Query126
-----SQL-----
SELECT GK.lnkKUPACID, GK.SIFRAKONTA, [lnkKUPACID]=[SIFRAKONTA] AS Expr1, GK.RDOB, GK.lnkIzvodStavkaID
FROM GK
WHERE (((GK.RDOB)>0));


=====QUERY=====
Query127
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.RDOB, GK.SIFRAKONTA, GK.lnkKUPACID, GK.lnkSkupstinaID
FROM GK
GROUP BY GK.KONTO, GK.RDOB, GK.SIFRAKONTA, GK.lnkKUPACID, GK.lnkSkupstinaID
HAVING (((GK.KONTO)="4350"));


=====QUERY=====
Query128
-----SQL-----
SELECT GrupaRacuna.NalogKN, GrupaRacuna.IDGrupaRacuna, GK.lnkIzvodStavkaID, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, Racun.ID_SK, GK.lnkSkupstinaID, Nalog.SZID, Nalog.Napomena, Nalog.OpisNaloga
FROM (GK INNER JOIN (GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) ON GK.RACID = Racun.IDRacun) INNER JOIN Nalog ON GK.BR_NALOG = Nalog.Br_Nalog
WHERE (((GrupaRacuna.ID_SK=[lnkSkupstinaID])=0)) Or ((([lnkSkupstinaID]=[SZID])=0)) Or (((Racun.ID_SK=GrupaRacuna.ID_SK)=0))
GROUP BY GrupaRacuna.NalogKN, GrupaRacuna.IDGrupaRacuna, GK.lnkIzvodStavkaID, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, Racun.ID_SK, GK.lnkSkupstinaID, Nalog.SZID, Nalog.Napomena, Nalog.OpisNaloga;


=====QUERY=====
Query129
-----SQL-----
UPDATE Table2 INNER JOIN Nalog ON Table2.id = Nalog.Br_Nalog SET Nalog.SZID = [Field1];


=====QUERY=====
Query13
-----SQL-----
INSERT INTO GK ( BR_NALOG, KONTO, DATUM, DIZNOS, TIP_STAVKE, DOK, lnkSkupstinaID, PARAMETRI )
SELECT GK.BR_NALOG, '5300' & Format([lnkSkupstinaID],'00') AS mKonto, GK.DATUM, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID, GK.PARAMETRI
FROM GK
GROUP BY GK.BR_NALOG, '5300' & Format([lnkSkupstinaID],'00'), GK.DATUM, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID, GK.PARAMETRI
HAVING (((GK.BR_NALOG)=1559));


=====QUERY=====
Query130
-----SQL-----
UPDATE Dobavljac_Racuni INNER JOIN Table4 ON Dobavljac_Racuni.IDTRRAC = Table4.ID SET Dobavljac_Racuni.DatumKnjizenja = [DatumIzdavanja], Dobavljac_Racuni.NalogKnjizenja = [NalogKN];


=====QUERY=====
Query131
-----SQL-----
SELECT KnjiznaDokumenta.*, Kupac.KONTO AS KRKONTO, Skustina.Konto AS SZKONTO, KnjiznaDokumenta.NalogKN, KnjiznaDokumenta.IDDokument, KnjiznaDokumenta.IDTipDok, KnjiznaDokumenta.IDKR
FROM (KnjiznaDokumenta INNER JOIN Kupac ON KnjiznaDokumenta.IDKR = Kupac.ID_K) INNER JOIN Skustina ON KnjiznaDokumenta.IDSZ = Skustina.IDSkupstina
WHERE (((KnjiznaDokumenta.IDDokument)=1));


=====QUERY=====
Query132
-----SQL-----
SELECT Racun.DatumIzdavanja, Racun.Ukupno, Racun.lnkOpomenaID
FROM GrupaRacuna, Racun;


=====QUERY=====
Query133
-----SQL-----
SELECT [DokumentOpis] & [DokumentNaziv] AS Expr1, KnjiznaDokumenta.DokumentNaziv
FROM KnjiznaDokumenta
WHERE ((([DokumentOpis] & [DokumentNaziv])="asasa"));


=====QUERY=====
Query134
-----SQL-----
UPDATE Table4 INNER JOIN Dobavljac_Racuni ON Table4.ID = Dobavljac_Racuni.IDTRRAC SET Dobavljac_Racuni.IznosRacunaKN = [Field1];


=====QUERY=====
Query135
-----SQL-----
SELECT KnjiznaDokumenta.*, KnjiznaDokumenta.Iznos, KorisnikSZ.ID_K, KorisnikSZ.Naziv
FROM KnjiznaDokumenta RIGHT JOIN KorisnikSZ ON KnjiznaDokumenta.IDKR = KorisnikSZ.ID_K;


=====QUERY=====
Query136
-----SQL-----
SELECT KnjiznaDokumenta.*, KorisnikSZ_FILTER.ID_K, KorisnikSZ_FILTER.Naziv
FROM KnjiznaDokumenta RIGHT JOIN (SELECT Kupac.ID_K, Kupac.Naziv, Kupac.lnk_ID_SK FROM Kupac WHERE (((Kupac.ID_K)<8000) AND ((Kupac.lnk_ID_SK)=101)))  AS KorisnikSZ_FILTER ON KnjiznaDokumenta.IDKR = KorisnikSZ_FILTER.ID_K;


=====QUERY=====
Query137
-----SQL-----
SELECT KnjiznaDokumenta.*, KorisnikSZ_FILTER.ID_K, KorisnikSZ_FILTER.Naziv
FROM KnjiznaDokumenta RIGHT JOIN (SELECT Kupac.ID_K, Kupac.Naziv, Kupac.lnk_ID_SK FROM Kupac WHERE (((Kupac.ID_K)<8000) AND ((Kupac.lnk_ID_SK)=101)))  AS KorisnikSZ_FILTER ON KnjiznaDokumenta.IDKR = KorisnikSZ_FILTER.ID_K;


=====QUERY=====
Query138
-----SQL-----
SELECT GK_GRP_DOK_DOB_ALL.lnkSkupstinaID, GK_GRP_DOK_DOB_ALL.lnkKUPACID, GK_GRP_DOK_DOB_ALL.RDOB, GK_GRP_DOK_DOB_ALL.Suma, GK_GRP_DOK_DOB_ALL.IsDug, GK_GRP_DOK_DOB_ALL.IsNotDug
FROM GK_GRP_DOK_DOB_ALL
WHERE (((GK_GRP_DOK_DOB_ALL.lnkSkupstinaID)=159) AND ((GK_GRP_DOK_DOB_ALL.lnkKUPACID)=9100));


=====QUERY=====
Query139
-----SQL-----
SELECT GK.*, GK.KontoTroska
FROM GK
WHERE (((GK.TIP_STAVKE)>90) AND ((GK.lnkIzvodStavkaID)=0) AND ((GK.KnDokID)=0) AND ((GK.KONTO)="2040" Or (GK.KONTO)="4350"));


=====QUERY=====
Query14
-----SQL-----
SELECT Racun.IDRacun, Racun.ID_K, Racun.Co, Racun.lnkGR
FROM Racun
WHERE (((Racun.ID_K)=2015) AND ((Racun.Storno)=0))
ORDER BY Racun.lnkGR DESC;


=====QUERY=====
Query140
-----SQL-----
SELECT GK_GRP_DOK_KORISNIK_ALL.lnkSkupstinaID, GK_GRP_DOK_KORISNIK_ALL.lnkKUPACID, GK_GRP_DOK_KORISNIK_ALL.DOK, GK_GRP_DOK_KORISNIK_ALL.RACID, GK_GRP_DOK_KORISNIK_ALL.PARAMETRI
FROM GK_GRP_DOK_KORISNIK_ALL
WHERE (((GK_GRP_DOK_KORISNIK_ALL.lnkSkupstinaID)=159) AND ((GK_GRP_DOK_KORISNIK_ALL.lnkKUPACID)=2615) AND ((GK_GRP_DOK_KORISNIK_ALL.DOK)="R-2007") AND ((GK_GRP_DOK_KORISNIK_ALL.RACID)=33043) AND ((GK_GRP_DOK_KORISNIK_ALL.PARAMETRI)="7115926152007"));


=====QUERY=====
Query141
-----SQL-----
SELECT GK.DATUM, Dobavljac_Racuni.DatumRacuna, Dobavljac_Racuni.DatumKnjizenja, Dobavljac_Racuni.IDTRRAC, GK.*
FROM GK LEFT JOIN Dobavljac_Racuni ON GK.RacunIN_ID = Dobavljac_Racuni.IDTRRAC
WHERE (((GK.DATUM) Is Null));


=====QUERY=====
Query142
-----SQL-----
DELETE Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.*
FROM Dobavljac_Racuni
WHERE (((Dobavljac_Racuni.IDTRRAC)=1));


=====QUERY=====
Query143
-----SQL-----
SELECT Sum([DIZNOS]-[PIZNOS]) AS Suma, GK_Filter_PD_Opomene.DOK, Sum(GK_Filter_PD_Opomene.DIZNOS) AS SumOfDIZNOS, Sum(GK_Filter_PD_Opomene.PIZNOS) AS SumOfPIZNOS, First(GK_Filter_PD_Opomene.DATUM) AS FirstOfDATUM, GK_Filter_PD_Opomene.RACID, First(IIf(IsNull([DPO]),[DATUM],[DPO])) AS Dospece, GrupaRacuna.GrupaRacunaFXT, Racun.RBR, Racun.DatumIzdavanja, Racun.DatumValute, Racun.SvrhaUplate2
FROM (GK_Filter_PD_Opomene LEFT JOIN Racun ON GK_Filter_PD_Opomene.RACID = Racun.IDRacun) LEFT JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
GROUP BY GK_Filter_PD_Opomene.DOK, GK_Filter_PD_Opomene.PARAMETRI, GK_Filter_PD_Opomene.RACID, GrupaRacuna.GrupaRacunaFXT, Racun.RBR, Racun.DatumIzdavanja, Racun.DatumValute, Racun.SvrhaUplate2, GK_Filter_PD_Opomene.KONTO, GK_Filter_PD_Opomene.lnkKUPACID, GK_Filter_PD_Opomene.lnkSkupstinaID
HAVING (((Sum([DIZNOS]-[PIZNOS]))>1) AND ((GK_Filter_PD_Opomene.KONTO)='2040') AND ((GK_Filter_PD_Opomene.lnkKUPACID)=1063) AND ((GK_Filter_PD_Opomene.lnkSkupstinaID)=102));


=====QUERY=====
Query144
-----SQL-----
SELECT GK_Filter_PD_Opomene.lnkSkupstinaID, GK_Filter_PD_Opomene.lnkKUPACID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS Suma
FROM GK_Filter_PD_Opomene
GROUP BY GK_Filter_PD_Opomene.lnkSkupstinaID, GK_Filter_PD_Opomene.lnkKUPACID, GK_Filter_PD_Opomene.KONTO
HAVING (((GK_Filter_PD_Opomene.lnkSkupstinaID)=114) AND ((GK_Filter_PD_Opomene.lnkKUPACID)=1455) AND ((GK_Filter_PD_Opomene.KONTO)="2040"));


=====QUERY=====
Query145
-----SQL-----
SELECT Settings_FormGrid.LayOutIndex
FROM Settings_FormGrid
GROUP BY Settings_FormGrid.LayOutIndex, Settings_FormGrid.FormName, Settings_FormGrid.FormParent, Settings_FormGrid.CompName, Settings_FormGrid.mUserID
HAVING (((Settings_FormGrid.LayOutIndex)>0) And ((Settings_FormGrid.FormName)='RacuniDobavljaca_Sub') And ((Settings_FormGrid.FormParent)='Racuni_Dob_List') And ((Settings_FormGrid.CompName)='AJSA') And ((Settings_FormGrid.mUserID)=1))
ORDER BY Settings_FormGrid.LayOutIndex;


=====QUERY=====
Query146
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.RDOB)=4589));


=====QUERY=====
Query147
-----SQL-----
SELECT Sum(GK_GRP_DOK_KORISNIK_ALL.Suma) AS SumOfSuma
FROM GK_GRP_DOK_KORISNIK_ALL
GROUP BY GK_GRP_DOK_KORISNIK_ALL.lnkSkupstinaID, GK_GRP_DOK_KORISNIK_ALL.lnkKUPACID, GK_GRP_DOK_KORISNIK_ALL.IsDug, GK_GRP_DOK_KORISNIK_ALL.IsNotDug
HAVING (((GK_GRP_DOK_KORISNIK_ALL.lnkSkupstinaID)=129) AND ((GK_GRP_DOK_KORISNIK_ALL.lnkKUPACID)=1887));


=====QUERY=====
Query148
-----SQL-----
SELECT VIRMAN.*
FROM VIRMAN
WHERE (((VIRMAN.IDVirman)=1) AND ((VIRMAN.Arhivirano) Is Null));


=====QUERY=====
Query149
-----SQL-----
SELECT Dobavljac_Racuni.*, Skustina.NazivSS, Kupac.Naziv, TipObracuna.Napomena, qryTipDokumenta.Cat1, qryTipDokumenta.ShortName
FROM (((Dobavljac_Racuni INNER JOIN Skustina ON Dobavljac_Racuni.SK_ID = Skustina.IDSkupstina) INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) INNER JOIN qryTipDokumenta ON Dobavljac_Racuni.TipDokumenta = qryTipDokumenta.Index
WHERE (((Dobavljac_Racuni.SK_ID)=121) AND ((Dobavljac_Racuni.MesecRacuna)='1907') AND ((Dobavljac_Racuni.DobavljacKonto)=9102));


=====QUERY=====
Query15
-----SQL-----
SELECT Skustina.Folder
FROM Skustina
WHERE (((Skustina.IDSkupstina)=1));


=====QUERY=====
Query150
-----SQL-----
SELECT GK_RDOB_4350.*
FROM GK_RDOB_4350
WHERE (((GK_RDOB_4350.RDOB)=724));


=====QUERY=====
Query151
-----SQL-----
SELECT IzvodStavke.ID, IzvodStavke.DatumRealizacije, IzvodStavke.RbStavke, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje
FROM IzvodStavke
WHERE (((IzvodStavke.ID)=1));


=====QUERY=====
Query152
-----SQL-----
SELECT Izvod.IzvodID, Izvod.ID_SK, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, Izvod.Duguje, Izvod.Potrazuje, Sum(IzvodStavke.Zaduzenje) AS SumOfZaduzenje, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje, [PrethodnoStanjeIzvoda]-[NovoStanje] AS Expr2
FROM Izvod INNER JOIN IzvodStavke ON Izvod.IzvodID = IzvodStavke.IzvodLNKID
GROUP BY Izvod.IzvodID, Izvod.ID_SK, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, Izvod.Duguje, Izvod.Potrazuje, [PrethodnoStanjeIzvoda]-[NovoStanje];


=====QUERY=====
Query153
-----SQL-----
SELECT Izvod.IzvodID, GK_2410.BR_NALOG, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, [PrethodnoStanjeIzvoda]-[NovoStanje] AS IZVOD_STANJE, Sum([PIZNOS]-[DIZNOS]) AS GK_STANJE, ([PrethodnoStanjeIzvoda]-[NovoStanje])=Sum([PIZNOS]-[DIZNOS]) AS Expr1
FROM Izvod INNER JOIN GK_2410 ON Izvod.NalogZaKnjizenje = GK_2410.BR_NALOG
WHERE (((Izvod.ID_SK)=121))
GROUP BY Izvod.IzvodID, GK_2410.BR_NALOG, Izvod.Datum, Izvod.BrojIzvoda, Izvod.SufixIzvoda, [PrethodnoStanjeIzvoda]-[NovoStanje]
HAVING (((([PrethodnoStanjeIzvoda]-[NovoStanje])=Sum([PIZNOS]-[DIZNOS]))=0))
ORDER BY Izvod.Datum;


=====QUERY=====
Query154
-----SQL-----
SELECT GK_2410.BR_NALOG, GK_2410.DATUM, Sum(GK_2410.DIZNOS) AS SumOfDIZNOS, Sum(GK_2410.PIZNOS) AS SumOfPIZNOS, Izvod.IzvodID, Izvod.BrojIzvoda, Izvod.SufixIzvoda, Izvod.Datum, Izvod.PrethodnoStanjeIzvoda, Izvod.NovoStanje, Izvod.NalogZaKnjizenje
FROM Izvod RIGHT JOIN GK_2410 ON Izvod.NalogZaKnjizenje = GK_2410.BR_NALOG
GROUP BY GK_2410.BR_NALOG, GK_2410.TIP_STAVKE, GK_2410.DATUM, GK_2410.lnkSkupstinaID, Izvod.IzvodID, Izvod.BrojIzvoda, Izvod.SufixIzvoda, Izvod.Datum, Izvod.PrethodnoStanjeIzvoda, Izvod.NovoStanje, Izvod.NalogZaKnjizenje
HAVING (((GK_2410.lnkSkupstinaID)=121))
ORDER BY GK_2410.TIP_STAVKE DESC , GK_2410.DATUM;


=====QUERY=====
Query155
-----SQL-----
SELECT Objekti.*
FROM Objekti
WHERE (((Objekti.IDGrupnogRacuna) Is Not Null And (Objekti.IDGrupnogRacuna)>0));


=====QUERY=====
Query156
-----SQL-----
INSERT INTO Racun ( lnkGR, ID_K, ID_SK, DatumIzdavanja, DatumUsluge, DatumValute, DatumPrometa, MestoIzdavanja, Kupac, PBroj_K, Adresa_K, PIB, PrethodniDug, PD_iznos, Ukupno )
SELECT Racun.lnkGR, Kupac.IDGrupniRacunMaster, Racun.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Nz([Suma0],0) AS PDD, Nz([Suma98],0) AS PDN, Sum(Racun.Ukupno) AS SumOfUkupno
FROM Kupac_DUG RIGHT JOIN (Kupac AS Kupac_1 INNER JOIN ((GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K) ON Kupac_1.ID_K = Kupac.IDGrupniRacunMaster) ON Kupac_DUG.lnkKUPACID = Kupac_1.ID_K
GROUP BY Racun.lnkGR, Kupac.IDGrupniRacunMaster, Racun.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Kupac_DUG.Suma0, Kupac_DUG.Suma98
HAVING (((Racun.lnkGR)=1686) AND (Not (Kupac.IDGrupniRacunMaster) Is Null) AND ((Racun.ID_SK)=116));


=====QUERY=====
Query157
-----SQL-----
INSERT INTO GK ( BR_NALOG, KONTO, DATUM, DIZNOS, PIZNOS, TIP_STAVKE, DOK, lnkSkupstinaID, lnkKUPACID, lnkIzvodStavkaID, NAPOMENA, PARAMETRI, OPIS, SIFRAKONTA, DPO, SIFRAKN, RDOB, RDOB, RACID, PRIORITET, KNzaTIP, RacunIN_ID, KontoTroska, KnDokID )
SELECT 1 AS NewNalog, GK.KONTO, GK.DATUM, [DIZNOS]*-1 AS DISTORNO, [PIZNOS]*-1 AS PISTORNO, GK.TIP_STAVKE, GK.DOK, GK.lnkSkupstinaID, GK.lnkKUPACID, GK.lnkIzvodStavkaID, GK.NAPOMENA, GK.PARAMETRI, GK.OPIS, GK.SIFRAKONTA, GK.DPO, GK.SIFRAKN, GK.RDOB, GK.RDOB, GK.RACID, GK.PRIORITET, GK.KNzaTIP, GK.RacunIN_ID, GK.KontoTroska, GK.KnDokID
FROM GK INNER JOIN (Kupac AS Kupac_1 INNER JOIN ((GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K) ON Kupac_1.ID_K = Kupac.IDGrupniRacunMaster) ON GK.RACID = Racun.IDRacun
WHERE (((Racun.lnkGR)=1686));


=====QUERY=====
Query158
-----SQL-----
SELECT 
FROM GrupaRacuna INNER JOIN (Racun INNER JOIN KUPAC_GRUPNI_RACUN_MASTERID ON Racun.ID_K = KUPAC_GRUPNI_RACUN_MASTERID.MASTER_ID_K) ON GrupaRacuna.ID_SK = KUPAC_GRUPNI_RACUN_MASTERID.lnk_ID_SK;


=====QUERY=====
Query159
-----SQL-----
UPDATE (Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN KUPAC_GRUPNI_RACUN_MASTERID ON Racun.ID_K = KUPAC_GRUPNI_RACUN_MASTERID.MASTER_ID_K SET Racun.RBR = [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN], Racun.PozivNaBroj = KontrolniBroj(97,[RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]) & '-' & [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]
WHERE (((Racun.lnkGR)=1686));


=====QUERY=====
Query16
-----SQL-----
SELECT Kupac.ID_K, Kupac.lnk_ID_SK, Skustina.IDSkupstina, [lnk_ID_SK]=[IDSkupstina] AS Expr1
FROM (Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina
WHERE ((([lnk_ID_SK]=[IDSkupstina])=0));


=====QUERY=====
Query160
-----SQL-----
UPDATE Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna SET Racun.RBR = [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN], Racun.PozivNaBroj = KontrolniBroj(97,[RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]) & '-' & [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]
WHERE (((Racun.RBR) Is Null) AND ((Racun.lnkGR)=1738) AND ((Racun.ID_SK)=116));


=====QUERY=====
Query161
-----SQL-----
SELECT Racun.lnkGR
FROM Racun INNER JOIN Racun AS Racun_1 ON Racun.ID_K = Racun_1.IDKGrupniRacun;


=====QUERY=====
Query162
-----SQL-----
UPDATE Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna SET Racun.RBR = [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN], Racun.PozivNaBroj = KontrolniBroj(97,[RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]) & '-' & [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]
WHERE (((Racun.RBR) Is Null) AND ((Racun.lnkGR)=1740) AND ((Racun.ID_SK)=116));


=====QUERY=====
Query163
-----SQL-----
UPDATE Racun INNER JOIN Racun AS Racun_1 ON (Racun_1.ID_K = Racun.IDKGrupniRacun) AND (Racun.lnkGR = Racun_1.lnkGR) SET Racun.Storno = True, Racun.SPC = [Racun_1].[IDRacun]
WHERE (((Racun.lnkGR)=1741));


=====QUERY=====
Query164
-----SQL-----
UPDATE (Racun INNER JOIN RacunStavke ON (Racun.lnkGR = RacunStavke.lnkGR) AND (Racun.ID_K = RacunStavke.ID_K) AND (Racun.ID_SK = RacunStavke.ID_SK)) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna SET RacunStavke.ID_R = RACUN.IDRacun, Racun.RBR = [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN], Racun.PozivNaBroj = KontrolniBroj(97,[RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]) & '-' & [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN]
WHERE (((Racun.lnkGR)=1741) AND ((Racun.ID_SK)=116));


=====QUERY=====
Query165
-----SQL-----
SELECT Racun_1.*, RacunStavke.*, Skustina.Adresa AS sAdresa, Skustina.PBroj AS sPBroj, Skustina.MB AS sMB, Skustina.PIB AS sPIB, Skustina.PrintNaziv, Skustina.TR, Skustina.PDtext, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.MarkerVanderdnihRacuna, Skustina.NazivSS, OpomenaZaRacun.txtRacunOp, OpomenaZaRacun.Dug, Racun.IDRacun AS IDRCPC, Racun.SvrhaUplate2 AS SPCSvrhaUplate2, Racun.Ukupno AS SPCUkupno
FROM (RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun) INNER JOIN (PrinterBinLOCAL INNER JOIN (((Racun AS Racun_1 LEFT JOIN OpomenaZaRacun ON (Racun_1.ID_K = OpomenaZaRacun.lnkKupac) AND (Racun_1.lnkGR = OpomenaZaRacun.lnkGrupaRacuna)) INNER JOIN Skustina ON Racun_1.ID_SK = Skustina.IDSkupstina) INNER JOIN GrupaRacuna ON Racun_1.lnkGR = GrupaRacuna.IDGrupaRacuna) ON PrinterBinLOCAL.ID_Item = Racun_1.IDRacun) ON Racun.SPC = Racun_1.IDRacun;


=====QUERY=====
Query166
-----SQL-----
SELECT Mail.eMail
FROM Mail
WHERE (((Mail.IDPartner)=1226))
ORDER BY Mail.SortOrder;


=====QUERY=====
Query167
-----SQL-----
SELECT Kupac.lnk_ID_SK, Count(Kupac.ID_K) AS CountOfID_K
FROM Mail INNER JOIN Kupac ON Mail.IDPartner = Kupac.ID_K
GROUP BY Kupac.lnk_ID_SK;


=====QUERY=====
Query168
-----SQL-----
SELECT eMail_PartnerRetMail([ID_K]) AS Expr1
FROM PrinterBinLOCAL INNER JOIN Racun ON PrinterBinLOCAL.ID_Item = Racun.IDRacun
WHERE (((PrinterBinLOCAL.TypeIndex)=1) AND ((eMail_PartnerRetMail([ID_K])) Is Not Null And (eMail_PartnerRetMail([ID_K]))<>""));


=====QUERY=====
Query169
-----SQL-----
SELECT Mail.IDPartner, Mail.eMail, eMail_PartnerRetMail([ID_K]) AS Expr1, Racun.ID_K, Racun.SvrhaUplate
FROM PrinterBinLOCAL INNER JOIN (Racun INNER JOIN Mail ON Racun.ID_K = Mail.IDPartner) ON PrinterBinLOCAL.ID_Item = Racun.IDRacun;


=====QUERY=====
Query17
-----SQL-----
SELECT Racun.lnkGR, Max(CInt(Mid([PozivNaBroj],13,Len([PozivNaBroj])-12))) AS Expr1
FROM Racun
GROUP BY Racun.lnkGR
HAVING (((Racun.lnkGR)=41));


=====QUERY=====
Query170
-----SQL-----
SELECT Kupac.lnk_ID_SK, Count(Mail.IDeMail) AS CountOfIDeMail, Kupac.ID_K
FROM Mail INNER JOIN Kupac ON Mail.IDPartner = Kupac.ID_K
GROUP BY Kupac.lnk_ID_SK, Kupac.ID_K
HAVING (((Count(Mail.IDeMail))>1));


=====QUERY=====
Query171
-----SQL-----
UPDATE Racun SET Racun.SvrhaUplate = "Racun za održavanje zgrade";


=====QUERY=====
Query172
-----SQL-----
SELECT Racun.IDRacun, Racun.RBR, Racun.SvrhaUplate, Racun.ID_K, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.Godina, GrupaRacuna.Mesec, Skustina.Folder
FROM (((Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina) INNER JOIN PrinterBinLOCAL ON Racun.IDRacun = PrinterBinLOCAL.ID_Item) INNER JOIN eMail_Partner ON Racun.ID_K = eMail_Partner.IDPartner
WHERE (((PrinterBinLOCAL.TypeIndex)=1));


=====QUERY=====
Query173
-----SQL-----
SELECT Racun.*, RacunStavke.*, Skustina.Adresa AS sAdresa, Skustina.PBroj AS sPBroj, Skustina.MB AS sMB, Skustina.PIB AS sPIB, Skustina.PrintNaziv, Skustina.TR, Skustina.PDtext, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.MarkerVanderdnihRacuna, Skustina.NazivSS, OpomenaZaRacun.txtRacunOp, OpomenaZaRacun.Dug, PrinterBinLOCAL.TypeIndex
FROM (PrinterBinLOCAL INNER JOIN (((RacunStavke INNER JOIN (Racun INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina) ON RacunStavke.ID_R = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) LEFT JOIN OpomenaZaRacun ON (Racun.ID_K = OpomenaZaRacun.lnkKupac) AND (Racun.lnkGR = OpomenaZaRacun.lnkGrupaRacuna)) ON PrinterBinLOCAL.ID_Item = Racun.IDRacun) INNER JOIN Kupac ON RacunStavke.ID_K = Kupac.ID_K
WHERE (((PrinterBinLOCAL.TypeIndex)=1) AND ((Kupac.chkSkipPrintRacunGrupa)=0));


=====QUERY=====
Query174
-----SQL-----
SELECT Racun.*, RacunStavke.*, Skustina.Adresa AS sAdresa, Skustina.PBroj AS sPBroj, Skustina.MB AS sMB, Skustina.PIB AS sPIB, Skustina.PrintNaziv, Skustina.TR, Skustina.PDtext, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.MarkerVanderdnihRacuna, Skustina.NazivSS, OpomenaZaRacun.txtRacunOp, OpomenaZaRacun.Dug, PrinterBinLOCAL.TypeIndex, Racun.IDKGrupniRacun
FROM PrinterBinLOCAL INNER JOIN (((RacunStavke INNER JOIN (Racun INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina) ON RacunStavke.ID_R = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) LEFT JOIN OpomenaZaRacun ON (Racun.lnkGR = OpomenaZaRacun.lnkGrupaRacuna) AND (Racun.ID_K = OpomenaZaRacun.lnkKupac)) ON PrinterBinLOCAL.ID_Item = Racun.IDRacun
WHERE (((PrinterBinLOCAL.TypeIndex)=1) AND ((Racun.IDKGrupniRacun)=0 Or (Racun.IDKGrupniRacun) Is Null));


=====QUERY=====
Query175
-----SQL-----
UPDATE Racun INNER JOIN Racun AS Racun_1 ON (Racun.lnkGR = Racun_1.lnkGR) AND (Racun_1.ID_K = Racun.IDKGrupniRacun) SET Racun.Storno = True, Racun.SPC = [Racun_1].[IDRacun]
WHERE (((Racun.lnkGR)=50));


=====QUERY=====
Query176
-----SQL-----
INSERT INTO GK ( BR_NALOG, Konto, DATUM, DIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, DPO, PARAMETRI, RDOB, RACID, SIFRAKN, PRIORITET, KontoTroska, SIFRAKONTA )
SELECT 306 AS Nalog, Kupac.konto AS KONTO, Racun_1.DatumIzdavanja, RacunStavke.UkupnoRSD, 3 AS TIP, Racun.ID_SK, Racun_1.ID_K, 'R-' & [GrupaRacunaFXN] AS DOK, Racun_1.DatumValute, Replace([Racun_1].[PozivNaBroj],'-','') AS PB, RacunStavke.ID_RDOB, Racun_1.IDRacun, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.PrioritetNaplate, Dobavljac_Racuni.KontoKnjizenja, Racun_1.ID_K
FROM ((((Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R) INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Racun AS Racun_1 ON Racun.SPC = Racun_1.IDRacun) INNER JOIN Kupac ON Racun_1.ID_K = Kupac.ID_K
WHERE (((RacunStavke.UkupnoRSD)<>0) AND ((Racun.lnkGR)=50));


=====QUERY=====
Query177
-----SQL-----
SELECT Racun.IDRacun
FROM Racun AS Racun_1 INNER JOIN Racun ON Racun_1.SPC = Racun.IDRacun
GROUP BY Racun.IDRacun
HAVING (((Racun.IDRacun)=45768));


=====QUERY=====
Query178
-----SQL-----
SELECT Racun_1.*, RacunStavke.*, Skustina.Adresa AS sAdresa, Skustina.PBroj AS sPBroj, Skustina.MB AS sMB, Skustina.PIB AS sPIB, Skustina.PrintNaziv, Skustina.TR, Skustina.PDtext, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.MarkerVanderdnihRacuna, Skustina.NazivSS, OpomenaZaRacun.txtRacunOp, OpomenaZaRacun.Dug, Racun.IDRacun AS IDRCPC, Racun.SvrhaUplate2 AS SPCSvrhaUplate2, Racun.Ukupno AS SPCUkupno
FROM (PrinterBinLOCAL INNER JOIN ((RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun) INNER JOIN (((Racun AS Racun_1 LEFT JOIN OpomenaZaRacun ON (Racun_1.ID_K = OpomenaZaRacun.lnkKupac) AND (Racun_1.lnkGR = OpomenaZaRacun.lnkGrupaRacuna)) INNER JOIN Skustina ON Racun_1.ID_SK = Skustina.IDSkupstina) INNER JOIN GrupaRacuna ON Racun_1.lnkGR = GrupaRacuna.IDGrupaRacuna) ON Racun.SPC = Racun_1.IDRacun) ON PrinterBinLOCAL.ID_Item = Racun_1.IDRacun) INNER JOIN Kupac ON RacunStavke.ID_K = Kupac.ID_K
WHERE (((RacunStavke.UkupnoRSD)<>0) AND ((PrinterBinLOCAL.TypeIndex)=1) AND ((Kupac.chkSkipPrintRacunGrupa)=0));


=====QUERY=====
Query179
-----SQL-----
SELECT Izvod.IzvodID, Izvod.Rasknjizen
FROM Izvod
WHERE (((Izvod.Rasknjizen)=False));


=====QUERY=====
Query18
-----SQL-----
SELECT Skustina.NazivSS, Kupac.ID_K, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.MB, Kupac.PIB, Kupac.Telefon, Kupac.eMail, Kupac.TR_K, Kupac.napomena
FROM (Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID=Skustina.IDSkupstina) INNER JOIN Kupac ON Objekti.lnk_ID_K=Kupac.ID_K
WHERE (((Skustina.IDSkupstina)=1));


=====QUERY=====
Query180
-----SQL-----
SELECT GK.BR_NALOG, Count(IzvodStavke.ID) AS CountOfID, IzvodStavke.IzvodLNKID, Sum(GK.DIZNOS) AS SumOfZaduzenje, Sum(GK.PIZNOS) AS SumOfOdobrenje, [GK].[DIZNOS]<>0 AS DUGUJE, [GK].[PIZNOS]<>0 AS POTRAZUJE, IzvodStavke.ID_SK, 'IZVOD ' & [BrojIzvoda] & '/' & Year([Izvod].[Datum]) AS DOKUMENT, Izvod.NalogZaKnjizenje, Izvod.Datum
FROM (GK RIGHT JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID) INNER JOIN Izvod ON IzvodStavke.IzvodLNKID = Izvod.IzvodID
GROUP BY GK.BR_NALOG, IzvodStavke.IzvodLNKID, [GK].[DIZNOS]<>0, [GK].[PIZNOS]<>0, IzvodStavke.ID_SK, 'IZVOD ' & [BrojIzvoda] & '/' & Year([Izvod].[Datum]), Izvod.NalogZaKnjizenje, Izvod.Datum, [Zaduzenje]<>0, [Odobrenje]<>0
HAVING (((IzvodStavke.IzvodLNKID)=2456));


=====QUERY=====
Query181
-----SQL-----
SELECT Kupac.ID_K, [Kupac].[Naziv] & ' - ' & [NazivSS] AS Expr1
FROM (Kupac LEFT JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) LEFT JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina
WHERE (((Kupac.konto)<>"2040") AND ((Nz([lnk_ID_SK],0))=113 Or (Nz([lnk_ID_SK],0))=0))
GROUP BY Kupac.ID_K, [Kupac].[Naziv] & ' - ' & [NazivSS], Kupac.lnk_ID_SK, Kupac.Naziv, Skustina.NazivSS
ORDER BY [Kupac].[Naziv] & ' - ' & [NazivSS];


=====QUERY=====
Query182
-----SQL-----
SELECT Racun.DatumIzdavanja, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, Racun.ID_SK, Dobavljac_Racuni.IDTRRAC
FROM ((Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R) INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K
WHERE (((Dobavljac_Racuni.TipDokumenta)=1))
GROUP BY Racun.DatumIzdavanja, Racun.ID_SK, Dobavljac_Racuni.IDTRRAC, Racun.lnkGR
HAVING (((Sum(RacunStavke.UkupnoRSD))<>0) AND ((Racun.ID_SK)=170) AND ((Racun.lnkGR)=2393));


=====QUERY=====
Query183
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.DATUM, GK.lnkSkupstinaID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID] AS KontrolaSK, Count(GK.STAVKAID) AS BrojKnjizenja, Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.IDTRRAC
FROM ((((GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) LEFT JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN GrupaRacuna ON GK.BR_NALOG = GrupaRacuna.NalogKN
WHERE (((GK.KONTO)='2040' Or (GK.KONTO)='4350'))
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.ID_SK, GK.BR_NALOG, GK.DATUM, GK.lnkSkupstinaID, GK.KontoTroska, GK.DOK, Dobavljac_Racuni.MesecRacuna, Troskovi_PodKonta.Naziv, Skustina.NazivSS, [IDSkupstina]=[SK_ID], Dobavljac_Racuni.DobavljacKonto, Kupac.Naziv, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.IznosRacunaKN, Dobavljac_Racuni.IDTRRAC
HAVING (((GrupaRacuna.ID_SK)=170))
ORDER BY GrupaRacuna.ID_SK, Dobavljac_Racuni.MesecRacuna;


=====QUERY=====
Query184
-----SQL-----
SELECT Kupac.ID_K, Skustina.IDSkupstina, Objekti.ID_O, Objekti.lnk_tip, [ID_K]+2000 AS Expr1
FROM (Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K;


=====QUERY=====
Query185
-----SQL-----
SELECT Kupac.ID_K, Kupac.MB
FROM Kupac
WHERE ((Not (Kupac.MB) Is Null));


=====QUERY=====
Query186
-----SQL-----
SELECT Racun.RBR, Racun.DatumIzdavanja, Racun.ID_K, Racun.Kupac, Racun.Adresa_K, Racun.PBroj_K, Racun.Grad_K, Racun.PIB, Racun.MB, Racun.Lokacija, Racun.SvrhaUplate2, Racun.Suma, Racun.PDVStopa, Racun.PDVIznos, Racun.Ukupno, Racun.PozivNaBroj
FROM Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R
WHERE (((RacunStavke.lnkGR)=11))
ORDER BY Racun.tmpID;


=====QUERY=====
Query187
-----SQL-----
SELECT RacunStavke.lnkGR, RacunStavke.Naziv, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.PDVStopa, RacunStavke.PDVIznos, RacunStavke.UkupnoRSD, RacunStavke.IZNOSRACUNA, RacunStavke.K1
FROM RacunStavke
WHERE (((RacunStavke.Naziv)="Održavanje zgrade za stanove"));


=====QUERY=====
Query188
-----SQL-----
SELECT Racun.RBR, Racun.DatumIzdavanja, Racun.ID_K, Racun.Kupac, Racun.Ukupno, Racun.Lokacija
FROM Racun
WHERE (((Racun.lnkGR)=13))
ORDER BY Racun.tmpID;


=====QUERY=====
Query189
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_SK, K1, K2, K3, K4, K5, Sort, Naziv, TipObracuna, DobavljacKonto, ID_RDOB, K1xK2, K2xK3, K2xK4, K2xK5, PDVStopa, NBS, JM, ID_O )
SELECT GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.K21, Objekti_Za_Racun.K23, Objekti_Za_Racun.K24, Objekti_Za_Racun.K25, IIf([SZPDV]<>0,[Dobavljac_Racuni].[PDV],0) AS RacunPDV, GrupaRacuna.NBS, TipObracuna.JM, Objekti_Za_Racun.BrojObjekata
FROM ((GrupaRacuna INNER JOIN ((Objekti_Za_Racun INNER JOIN (Kupac INNER JOIN (Dobavljaci_Racun_TipObjekta INNER JOIN Dobavljac_Racuni ON Dobavljaci_Racun_TipObjekta.RacunID = Dobavljac_Racuni.IDTRRAC) ON Kupac.ID_K = Dobavljac_Racuni.DobavljacKonto) ON (Objekti_Za_Racun.lnk_tip = Dobavljaci_Racun_TipObjekta.TipObjekta) AND (Objekti_Za_Racun.lnkSkupstinaID = Dobavljac_Racuni.SK_ID)) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) ON GrupaRacuna.GrupaRacunaFXN = Dobavljac_Racuni.MesecRacuna) INNER JOIN TipRacunaDobavljac ON Dobavljac_Racuni.TipDokumenta = TipRacunaDobavljac.Index) INNER JOIN SZ_IzdavalacRacuna ON Objekti_Za_Racun.lnkSkupstinaID = SZ_IzdavalacRacuna.IDSkupstina
WHERE (((Dobavljac_Racuni.MarkerVandrednogRacuna) Is Null) AND ((TipRacunaDobavljac.Cat1)=1) AND ((TipObracuna.IDTipObr)<>99))
GROUP BY GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.K21, Objekti_Za_Racun.K23, Objekti_Za_Racun.K24, Objekti_Za_Racun.K25, IIf([SZPDV]<>0,[Dobavljac_Racuni].[PDV],0), GrupaRacuna.NBS, TipObracuna.JM, Objekti_Za_Racun.BrojObjekata, Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna
HAVING (((GrupaRacuna.IDGrupaRacuna)=13) AND ((Objekti_Za_Racun.lnkSkupstinaID)=101))
ORDER BY Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna;


=====QUERY=====
Query19
-----SQL-----
SELECT Max(GrupaRacuna.IDGrupaRacuna) AS MaxOfIDGrupaRacuna
FROM GrupaRacuna;


=====QUERY=====
Query190
-----SQL-----
SELECT Sum(Racun.Ukupno) AS SumOfUkupno, Sum(RacunStavke_Suma.SumOfUkupnoRSD) AS SumOfSumOfUkupnoRSD, Racun.lnkGR
FROM Racun RIGHT JOIN RacunStavke_Suma ON Racun.IDRacun = RacunStavke_Suma.ID_R
GROUP BY Racun.lnkGR
HAVING (((Racun.lnkGR)=13));


=====QUERY=====
Query191
-----SQL-----
SELECT Objekti_1.ID_O, Objekti.Napomena, Kupac.IDMaster, Objekti_1.naziv, Kupac.Naziv, Objekti_1.Napomena, Objekti_1.Status
FROM (Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.IDMaster) INNER JOIN Objekti AS Objekti_1 ON Kupac.ID_K = Objekti_1.lnk_ID_K
WHERE (((Objekti.Napomena) Like "*") AND ((Objekti_1.Napomena) Is Null))
ORDER BY Kupac.IDMaster;


=====QUERY=====
Query192
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Objekti.naziv, Objekti.Ulaz, TipObjekta.TipObj, Objekti.BrojPD, Objekti.SifraPD, Objekti.K1, Objekti.K2, Objekti.Napomena, TipObjekta_1.SortObj, Objekti_1.BrojPD, TipObjekta.SortObj
FROM (TipObjekta INNER JOIN (Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) ON TipObjekta.IDTipObj = Objekti.lnk_tip) INNER JOIN (Objekti AS Objekti_1 INNER JOIN TipObjekta AS TipObjekta_1 ON Objekti_1.lnk_tip = TipObjekta_1.IDTipObj) ON Kupac.IDMaster = Objekti_1.lnk_ID_K
WHERE (((Objekti.Status)=0))
ORDER BY Objekti.Ulaz, TipObjekta_1.SortObj, Objekti_1.BrojPD, TipObjekta.SortObj;


=====QUERY=====
Query193
-----SQL-----
SELECT Round(Sum([DIZNOS]-[PIZNOS]),2) AS STANJE, GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI, GK.DOK, GK.KontoTroska, GK.RDOB, GK.RACID
FROM GK
GROUP BY GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI, GK.DOK, GK.KontoTroska, GK.RDOB, GK.RACID, GK.lnkSkupstinaID, GK.lnkKUPACID
HAVING (((Round(Sum([DIZNOS]-[PIZNOS]),2))<>0) AND ((GK.PARAMETRI)='2710117082305') AND ((GK.lnkSkupstinaID)=101) AND ((GK.lnkKUPACID)=1708))
ORDER BY First(GK.DATUM), GK.PRIORITET, GK.SIFRAKN;


=====QUERY=====
Query194
-----SQL-----
SELECT SHEET1_FIX.*, Kupac.Naziv, Objekti.SifraPD, Objekti.K1, Objekti.KV
FROM (Objekti INNER JOIN SHEET1_FIX ON Objekti.SifraPD = SHEET1_FIX.NFT) INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K;


=====QUERY=====
Query195
-----SQL-----
SELECT Objekti.ID_O, Kupac.Naziv, Objekti.SifraPD, Objekti.K1, Objekti.KV, Objekti.lnk_tip, Objekti.BrojPD, Objekti.Ulaz
FROM Objekti LEFT JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
WHERE (((Objekti.lnk_tip)=4));


=====QUERY=====
Query196
-----SQL-----
SELECT SHEET1_FIX.*, Query195.Naziv, Query195.BrojPD
FROM SHEET1_FIX LEFT JOIN Query195 ON SHEET1_FIX.UN = Query195.BrojPD;


=====QUERY=====
Query197
-----SQL-----
DELETE Sheet1.[Unit number]
FROM Sheet1
WHERE (((Sheet1.[Unit number]) Is Null));


=====QUERY=====
Query198
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Kupac1.Naziv, [KUPAC].[Naziv]=[KUPAC1].[Naziv] AS Expr1
FROM Kupac INNER JOIN Kupac1 ON Kupac.ID_K = Kupac1.ID_K;


=====QUERY=====
Query199
-----SQL-----
SELECT Izvod.IzvodID, Izvod.NalogZaKnjizenje, IzvodStavke.*
FROM Izvod INNER JOIN IzvodStavke ON (Izvod.ID_SK = IzvodStavke.ID_SK) AND (Izvod.Datum = IzvodStavke.DatumRealizacije)
WHERE (((IzvodStavke.ID_SK)=101));


=====QUERY=====
Query2
-----SQL-----
SELECT RacunStavke.ID_R, Racun.Storno
FROM Racun INNER JOIN RacunStavke ON (Racun.lnkGR=RacunStavke.lnkGR) AND (Racun.ID_K=RacunStavke.ID_K) AND (Racun.ID_SK=RacunStavke.ID_SK)
WHERE (((RacunStavke.ID_R)=0) AND ((Racun.lnkGR)=11) AND ((Racun.ID_K)=1003) AND ((Racun.Storno)=0));


=====QUERY=====
Query20
-----SQL-----
SELECT Table1.Field1, [GK-99].Suma, Table1.Suma, [Table1].[Suma]=[GK-99].[Suma] AS Expr1, [GK-99].Suma
FROM [GK-99] RIGHT JOIN Table1 ON [GK-99].lnkKUPACID = Table1.Field1;


=====QUERY=====
Query200
-----SQL-----
SELECT RacunStavke.lnkGR, RacunStavke.ID_R, Racun.ID_K, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, Sum(RacunStavke.PDVIznos) AS SumOfPDVIznos, Sum(RacunStavke.Suma) AS SumOfSuma, Racun.PrethodniDug, Racun.PD_iznos, Racun.Ukupno, Racun.PDVIznos, Racun.Suma, Sum([RacunStavke].[Suma])<>[Racun].[Suma] AS Expr1, Racun.IDRacun
FROM Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R
GROUP BY RacunStavke.lnkGR, RacunStavke.ID_R, Racun.ID_K, Racun.PrethodniDug, Racun.PD_iznos, Racun.Ukupno, Racun.PDVIznos, Racun.Suma, Racun.IDRacun
HAVING (((RacunStavke.lnkGR)=22));


=====QUERY=====
Query201
-----SQL-----
SELECT 
FROM Benefiti;


=====QUERY=====
Query202
-----SQL-----
SELECT [BENEFIT-LISTING].*, Objekti.Status
FROM [BENEFIT-LISTING] RIGHT JOIN Objekti ON [BENEFIT-LISTING].ID_O = Objekti.ID_O
WHERE ((([BENEFIT-LISTING].MesecYYMM)="2307" Or ([BENEFIT-LISTING].MesecYYMM) Is Null) AND (([BENEFIT-LISTING].ID_O) Is Null));


=====QUERY=====
Query203
-----SQL-----
SELECT [BENEFITI-LISTING-2307].ID_O, Objekti.Status, Objekti.naziv, Objekti.Ulaz, Objekti.ID_O
FROM [BENEFITI-LISTING-2307] RIGHT JOIN Objekti ON [BENEFITI-LISTING-2307].ID_O = Objekti.ID_O
WHERE ((([BENEFITI-LISTING-2307].ID_O) Is Null))
ORDER BY Objekti.ID_O;


=====QUERY=====
Query204
-----SQL-----
UPDATE Objekti INNER JOIN Table1 ON Objekti.ID_O = Table1.IDO SET Objekti.Status = 1;


=====QUERY=====
Query205
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, RacunStavke.*, Objekti.naziv
FROM (RacunStavke LEFT JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) RIGHT JOIN Objekti ON RacunStavke.ID_O = Objekti.ID_O
WHERE (((GrupaRacuna.GrupaRacunaFXN)="2304"));


=====QUERY=====
Query206
-----SQL-----
SELECT Objekti.K5, Table2.BK, Table2.ULAZ, Table2.PD, Objekti.ID_O, Objekti.lnk_tip, Objekti.naziv
FROM Objekti RIGHT JOIN Table2 ON (Objekti.BrojPD = Table2.PD) AND (Objekti.Ulaz = Table2.ULAZ)
WHERE (((Objekti.lnk_tip)=1));


=====QUERY=====
Query207
-----SQL-----
SELECT Racun.RBR, Racun.DatumIzdavanja, Racun.ID_K, Racun.Kupac, RacunStavke.UkupnoRSD, RacunStavke.Naziv, Racun.Adresa_K, RacunStavke.Kolicina
FROM RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun
WHERE (((Racun.lnkGR)=19) AND ((RacunStavke.Naziv) Like "*TAG*"));


=====QUERY=====
Query208
-----SQL-----
SELECT RacunStavke.*
FROM RacunStavke;


=====QUERY=====
Query209
-----SQL-----
SELECT [GK-R2306].*
FROM [BENEFITI-LISTING-2306] INNER JOIN [GK-R2306] ON [BENEFITI-LISTING-2306].ID_K = [GK-R2306].lnkKUPACID;


=====QUERY=====
Query21
-----SQL-----
SELECT Objekti.lnk_ID_K, Count(Objekti.lnk_tip) AS CountOflnk_tip
FROM Objekti
GROUP BY Objekti.lnk_ID_K
HAVING (((Count(Objekti.lnk_tip))=1));


=====QUERY=====
Query210
-----SQL-----
SELECT Kupac_Dug.lnkKUPACID, Kupac_Dug.Suma0
FROM Kupac_Dug;


=====QUERY=====
Query211
-----SQL-----
SELECT Racun.IDRacun, Racun.ID_K, Kupac_Dug.Suma0, Racun.PrethodniDug, Racun.ZaUplatu, Racun.PD_iznos, [Suma0]=[PrethodniDug] AS Expr1
FROM Racun LEFT JOIN Kupac_Dug ON Racun.ID_K = Kupac_Dug.lnkKUPACID
WHERE ((([Suma0]=[PrethodniDug])=0) AND ((Racun.lnkGR)=19));


=====QUERY=====
Query212
-----SQL-----
SELECT TipObjekta.Print, Objekti.naziv, Objekti.BrojPD, Objekti.SifraPD, TipObjekta.TipObj, TipObjekta.IDTipObj, "Garažno mesto / Parking " & [SifraPD] AS Expr1, Kupac.Adresa, Skustina.Adresa, Kupac.Adresa, [Skustina].[Adresa] & Chr(13) & Chr(10) & [Objekti].[Naziv] AS Expr2
FROM ((Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina;


=====QUERY=====
Query213
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Objekti.lnk_tip, Count(Objekti.ID_O) AS CountOfID_O, Count(Kupac.ID_K) AS CountOfID_K
FROM Objekti LEFT JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
GROUP BY Objekti.lnkSkupstinaID, Objekti.lnk_tip;


=====QUERY=====
Query214
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS, Skustina.Zgrada, Objekti.ID_O, Objekti.naziv, Objekti.SifraPD, Objekti.BrojPD, Objekti.K1, Objekti.K2, Objekti.lnk_tip
FROM Skustina INNER JOIN Objekti ON Skustina.IDSkupstina = Objekti.lnkSkupstinaID
ORDER BY Skustina.IDSkupstina, Objekti.BrojPD;


=====QUERY=====
Query215
-----SQL-----
SELECT Kupac.ID_K, Kupac.lnk_ID_SK, Kupac.Naziv, ConcatRelated('naziv','Objekti','lnk_ID_K=' & [ID_K]) AS ObjektiNaziv, ConcatRelated('ulaz','Objekti','lnk_ID_K=' & [ID_K]) AS LokacijaObjekata
FROM Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K
GROUP BY Kupac.ID_K, Kupac.lnk_ID_SK, Kupac.Naziv, Objekti.Status
HAVING (((Kupac.ID_K)=1001) AND ((Objekti.Status)=1));


=====QUERY=====
Query216
-----SQL-----
SELECT Objekti.Ulaz, Skustina.Zgrada
FROM Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina;


=====QUERY=====
Query217
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GrupaRacuna INNER JOIN GK ON GrupaRacuna.NalogKN = GK.BR_NALOG
GROUP BY GrupaRacuna.GrupaRacunaFXN, GK.KONTO
HAVING (((GrupaRacuna.GrupaRacunaFXN)=[YYMM]));


=====QUERY=====
Query218
-----SQL-----
SELECT Objekti.Ulaz, Table2.ULAZ, Objekti.ID_O, Objekti.naziv, Objekti.BrojPD, Objekti.SifraPD, Table2.sIFRA
FROM Objekti RIGHT JOIN Table2 ON Objekti.SifraPD = Table2.sIFRA
ORDER BY Objekti.BrojPD;


=====QUERY=====
Query219
-----SQL-----
SELECT Objekti.lnk_tip, Kupac.ID_K, Objekti.SifraPD, Objekti.Ulaz, Objekti.lnkSkupstinaID
FROM Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
WHERE (((Objekti.lnk_tip)=1) AND ((Objekti.lnkSkupstinaID)=102));


=====QUERY=====
Query219-2
-----SQL-----
SELECT Objekti.lnk_tip, Kupac.ID_K, Objekti.SifraPD, Objekti.ID_O, Objekti.Ulaz, Objekti.lnkSkupstinaID
FROM Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K
WHERE (((Objekti.lnk_tip)=4) AND ((Objekti.lnkSkupstinaID)=102));


=====QUERY=====
Query22
-----SQL-----
SELECT Table1.Field1, [Field2]-[Field3]+[Field4]-[Field5] AS Expr1, Table1.Suma
FROM Table1;


=====QUERY=====
Query220
-----SQL-----
SELECT Query219.Ulaz, [Query219-2].ID_O, [Query219-2].Ulaz
FROM Query219 INNER JOIN [Query219-2] ON Query219.ID_K = [Query219-2].ID_K;


=====QUERY=====
Query221
-----SQL-----
SELECT Objekti.Ulaz, Table3.ulaz
FROM Table3 INNER JOIN Objekti ON Table3.ido = Objekti.ID_O;


=====QUERY=====
Query222
-----SQL-----
UPDATE ((Benefiti INNER JOIN GrupaRacuna ON Benefiti.MesecYYMM = GrupaRacuna.GrupaRacunaFXN) INNER JOIN Racun ON (Benefiti.KupacID = Racun.ID_K) AND (GrupaRacuna.IDGrupaRacuna = Racun.lnkGR)) INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R SET RacunStavke.UkupnoRSD = 0
WHERE (((GrupaRacuna.IDGrupaRacuna)=37) AND ((RacunStavke.DobavljacKonto)=9001));


=====QUERY=====
Query223
-----SQL-----
SELECT Racun.Lokacija, Kupac.LokacijaDostava, [Lokacija] & " /" & [LokacijaDostava] AS Expr1, GrupaRacuna.GrupaRacunaFXN
FROM (Racun INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GrupaRacuna.GrupaRacunaFXN)=[YYMM]));


=====QUERY=====
Query224
-----SQL-----
SELECT Objekti.lnk_ID_K, Objekti.lnkSkupstinaID, Count(Objekti.ID_O) AS CountOfID_O
FROM Objekti
GROUP BY Objekti.lnk_ID_K, Objekti.lnkSkupstinaID
ORDER BY Count(Objekti.ID_O);


=====QUERY=====
Query225
-----SQL-----
SELECT [OBJEKTI-GM-KUPAC].*
FROM [OBJEKTI-GM-KUPAC] LEFT JOIN [OBJEKTI-STAN-KUPAC] ON [OBJEKTI-GM-KUPAC].ID_K = [OBJEKTI-STAN-KUPAC].ID_K
WHERE ((([OBJEKTI-STAN-KUPAC].ID_O) Is Null));


=====QUERY=====
Query226
-----SQL-----
SELECT Skustina.IDSkupstina AS IDSZ, Skustina.NazivSS AS [Stambena zajednica], Skustina.Zgrada, TipObjekta.Print AS [Vrsta posebnog dela], Sum(Objekti.K1) AS [K1 - m2], Sum(Objekti.K2) AS [K2 - keoficijent], Sum(Objekti.K3) AS [K3 - broj]
FROM (Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
GROUP BY Skustina.IDSkupstina, Skustina.NazivSS, Skustina.Zgrada, TipObjekta.Print, Objekti.Status
HAVING (((Objekti.Status)=1))
ORDER BY Skustina.IDSkupstina;


=====QUERY=====
Query227
-----SQL-----
SELECT BenefitUpdate.*, Objekti.ID_O, Benefiti.MesecYYMM
FROM (Objekti INNER JOIN BenefitUpdate ON Objekti.SifraPD = BenefitUpdate.UnitApp) INNER JOIN Benefiti ON Objekti.ID_O = Benefiti.ObjekatID;


=====QUERY=====
Query228
-----SQL-----
INSERT INTO RacunObjekti ( IDRacun, IDObjekat )
SELECT Racun.IDRacun, Objekti.ID_O
FROM Objekti INNER JOIN Racun ON Objekti.lnk_ID_K = Racun.ID_K
WHERE (((Objekti.Status)=1) AND ((Racun.lnkGR)=1) AND ((Racun.ID_SK)=2));


=====QUERY=====
Query229
-----SQL-----
DELETE RacunObjekti.*, Racun.lnkGR
FROM RacunObjekti INNER JOIN Racun ON RacunObjekti.IDRacun = Racun.IDRacun
WHERE (((Racun.lnkGR)=1));


=====QUERY=====
Query23
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna )
SELECT 1 AS Expr1, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnk_ID_K, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovori.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovori.TipObracuna
FROM Troskovi_Ugovori RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovori.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query230
-----SQL-----
SELECT KupacBezSlanja.ID_K, KupacBezSlanja.lnk_ID_SK, KupacBezSlanja.Naziv, KupacLanjeEmailGroup.IDPartner
FROM KupacBezSlanja LEFT JOIN KupacLanjeEmailGroup ON KupacBezSlanja.ID_K = KupacLanjeEmailGroup.IDPartner
WHERE (((KupacLanjeEmailGroup.IDPartner) Is Null));


=====QUERY=====
Query231
-----SQL-----
SELECT Promene.IDChange, Promene.Description, Mail.SendMailRacun, Mail.eMail, Mail.IDeMail
FROM Mail LEFT JOIN Promene ON Mail.IDPartner = Promene.IDK;


=====QUERY=====
Query232
-----SQL-----
SELECT RacunStavke.ID_SK, Skustina.Zgrada, RacunStavke.ID_K, Kupac.Naziv, RacunStavke.Naziv, RacunStavke.Kolicina, RacunStavke.CenaE, RacunStavke.NBS, RacunStavke.Iznos, RacunStavke.Suma, RacunStavke.UkupnoRSD
FROM (RacunStavke INNER JOIN Kupac ON RacunStavke.ID_K = Kupac.ID_K) INNER JOIN Skustina ON RacunStavke.ID_SK = Skustina.IDSkupstina
ORDER BY RacunStavke.ID_SK, RacunStavke.ID_K, RacunStavke.Sort;


=====QUERY=====
Query233
-----SQL-----
SELECT Mail.SendMailRacun, Mail.eMail
FROM Mail INNER JOIN Table5 ON Mail.IDeMail = Table5.id;


=====QUERY=====
Query234
-----SQL-----
SELECT Objekti.SifraPD, Kupac.LokacijaDostava
FROM Objekti INNER JOIN Kupac ON Objekti.ID_O = Kupac.DostavaSifraPD;


=====QUERY=====
Query235
-----SQL-----
SELECT Table6.*, Objekti.*, Kupac.Naziv
FROM (Objekti INNER JOIN Table6 ON Objekti.SifraPD = Table6.UNIT) INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K;


=====QUERY=====
Query236
-----SQL-----
SELECT Racun.lnkGR, Racun.RBR, Kupac.chkSkipPrintRacunGrupa
FROM Racun INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K
WHERE (((Racun.lnkGR)=33 Or (Racun.lnkGR)=34 Or (Racun.lnkGR)=35) AND ((Kupac.chkSkipPrintRacunGrupa)=0));


=====QUERY=====
Query237
-----SQL-----
SELECT RacunStavke.ID_R, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD
FROM RacunStavke
GROUP BY RacunStavke.ID_R
HAVING (((RacunStavke.ID_R)=6173));


=====QUERY=====
Query238
-----SQL-----
SELECT Racun.IDRacun, Kupac.chkSkipPrintRacunGrupa
FROM Racun INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K
WHERE (((Kupac.chkSkipPrintRacunGrupa)=0) AND ((Racun.lnkGR)=33 Or (Racun.lnkGR)=34 Or (Racun.lnkGR)=35));


=====QUERY=====
Query239
-----SQL-----
SELECT Racun.PIB
FROM Racun
WHERE (((Racun.PIB) Is Not Null));


=====QUERY=====
Query24
-----SQL-----
UPDATE RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR=GrupaRacuna.IDGrupaRacuna SET RacunStavke.NBS = GrupaRacuna.NBS, RacunStavke.CenaE = IIf([TipObracuna]=1,[MAX-SUMA]/[MAX-KV]*[KV],IIf([TipObracuna]=2,[MAX-SUMA]/[MAX-KOEF]*[KOEF],0)), RacunStavke.Ukupno = GrupaRacuna.NBS*(IIf([TipObracuna]=1,[MAX-SUMA]/[MAX-KV]*[KV],IIf([TipObracuna]=2,[MAX-SUMA]/[MAX-KOEF]*[KOEF],0))), RacunStavke.Naziv = IIf([AddTXT]=2,[NAZIV] & ", 1/" & [MAX-CN] & ", KOEFICIJENT " & Format([koef],'#,#0.0'),IIf([AddTXT]=1,[NAZIV] & ", " & [KV] & "/" & [MAX-KV] & " m2",[Naziv])), RacunStavke.VP = IIf(Len([vp])=1,[VP]);


=====QUERY=====
Query240
-----SQL-----
SELECT Racun.*, RacunStavke.*, Skustina.Adresa AS sAdresa, Skustina.IDSkupstina, Skustina.PBroj AS sPBroj, Skustina.MB AS sMB, Skustina.PIB AS sPIB, Skustina.PrintNaziv, Skustina.TR, Skustina.PDtext, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.MarkerVanderdnihRacuna, Skustina.NazivSS, OpomenaZaRacun.txtRacunOp, OpomenaZaRacun.Dug, PrinterBinLOCAL.TypeIndex, Skustina_1.Doznaka, Skustina.RacunInfoReklamacija
FROM (((PrinterBinLOCAL INNER JOIN (((RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) LEFT JOIN OpomenaZaRacun ON (Racun.ID_K = OpomenaZaRacun.lnkKupac) AND (Racun.lnkGR = OpomenaZaRacun.lnkGrupaRacuna)) ON PrinterBinLOCAL.ID_Item = Racun.IDRacun) INNER JOIN Skustina AS Skustina_1 ON Racun.ID_SK = Skustina_1.IDSkupstina) INNER JOIN Skustina ON Skustina_1.InvoiceIssuer = Skustina.IDSkupstina) INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K
WHERE (((PrinterBinLOCAL.TypeIndex)=1) AND ((Racun.IDKGrupniRacun)=0 Or (Racun.IDKGrupniRacun) Is Null) AND ((RacunStavke.Suma)<>0) AND ((Kupac.chkSkipPrintRacunGrupa)=0));


=====QUERY=====
Query241
-----SQL-----
UPDATE (Racun INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K) INNER JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O SET Racun.SortRacun = [BrojPD];


=====QUERY=====
Query242
-----SQL-----
SELECT Racun.lnkGR
FROM Racun
WHERE (((Racun.lnkGR)=37));


=====QUERY=====
Query243
-----SQL-----
SELECT;


=====QUERY=====
Query244
-----SQL-----
SELECT Racun.lnkGR, GetBenefitText([ID_K]) AS txt, BenefitHave.MesecYYMM
FROM Racun INNER JOIN BenefitHave ON (Racun.ID_K = BenefitHave.KupacID) AND (Racun.lnkGR = BenefitHave.IDGrupaRacuna);


=====QUERY=====
Query245
-----SQL-----
SELECT Racun.IDRacun, BenefitUpdateRacun.TXT, Racun.lnkGR
FROM Racun INNER JOIN BenefitUpdateRacun ON Racun.IDRacun = BenefitUpdateRacun.IDRacun
WHERE (((Racun.lnkGR)=27));


=====QUERY=====
Query246
-----SQL-----
SELECT Racun.ID_K, Racun.Kupac, Racun.RBR, Racun.Ukupno, Dobavljac_Racuni.Napomena, RacunStavke.Suma, Racun.Lokacija, RacunStavke.Sort
FROM (RacunStavke INNER JOIN Racun ON RacunStavke.ID_R = Racun.IDRacun) INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((Racun.ID_K)=1497))
ORDER BY Racun.RBR, RacunStavke.Sort;


=====QUERY=====
Query247
-----SQL-----
DELETE RacunStavke.*
FROM RacunStavke INNER JOIN RacunStavkeKill ON RacunStavke.IDRacunStavke = RacunStavkeKill.IDRS;


=====QUERY=====
Query248
-----SQL-----
SELECT BenefitRacunStavkeArhive_2404.ID_K, Count(BenefitRacunStavkeArhive_2404.IDRacunStavke) AS CountOfIDRacunStavke
FROM BenefitRacunStavkeArhive_2404
GROUP BY BenefitRacunStavkeArhive_2404.ID_K;


=====QUERY=====
Query249
-----SQL-----
SELECT Benefiti.MesecYYMM, Benefiti.KupacID, Count(Benefiti.IDBenefit) AS CountOfIDBenefit
FROM Benefiti
GROUP BY Benefiti.MesecYYMM, Benefiti.KupacID
HAVING (((Benefiti.MesecYYMM)="2404"));


=====QUERY=====
Query25
-----SQL-----
SELECT 
FROM RacunStavke, GEN_SUMA_VP;


=====QUERY=====
Query250
-----SQL-----
SELECT PrinterBinLOCAL.*, Opomena.lnkKupac, Kupac.chkSkipPrintRacunGrupa, Mail.IDPartner, Mail.eMail, Mail.SendMailRacun, Kupac.lnk_ID_SK
FROM ((PrinterBinLOCAL INNER JOIN Opomena ON PrinterBinLOCAL.ID_Item = Opomena.IDOpomena) LEFT JOIN Mail ON Opomena.lnkKupac = Mail.IDPartner) LEFT JOIN Kupac ON Opomena.lnkKupac = Kupac.ID_K
WHERE (((PrinterBinLOCAL.TypeIndex)=2))
ORDER BY Mail.IDPartner;


=====QUERY=====
Query251
-----SQL-----
SELECT Skustina.NazivSS, Kupac.Naziv AS Kupac, Kupac.KupacGrad AS Grad_K, Opomena.*, OpomenaStavke.*, GrupaOpomena.Datum, Kupac.Naziv, Kupac.PBroj AS PBroj_K, Kupac.Adresa AS Adresa_K, IIf(Kupac.PIB<>'','PIB: ' & Kupac.PIB,'') AS rPIB, Skustina.PIB AS SZPIB, Skustina.MB AS SZMB, Skustina.DatumUgovora, GrupaOpomena.DatumPI, Skustina.TR, Skustina.IDSkupstina, Skustina.eMail, Objekti.SifraPD AS Lokacija, [Skustina].[Adresa] & ', ' & [Skustina].[PBroj] AS SZAdresa, GrupaOpomena.lnkSablonOpomene
FROM (((((OpomenaStavke INNER JOIN Opomena ON OpomenaStavke.lnkOpomena = Opomena.IDOpomena) INNER JOIN GrupaOpomena ON Opomena.lnkGrupaOpomena = GrupaOpomena.IDGrupaOpomena) INNER JOIN Kupac ON Opomena.lnkKupac = Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) LEFT JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O) INNER JOIN PrinterBinLOCAL ON Opomena.IDOpomena = PrinterBinLOCAL.ID_Item
WHERE (((Opomena.ActivnaOpomena)=-1) AND ((PrinterBinLOCAL.TypeIndex)=2))
ORDER BY OpomenaStavke.DOK;


=====QUERY=====
Query252
-----SQL-----
SELECT Promene.*
FROM (PrinterBinLOCAL INNER JOIN Opomena ON PrinterBinLOCAL.ID_Item = Opomena.IDOpomena) INNER JOIN Promene ON Opomena.lnkKupac = Promene.IDK;


=====QUERY=====
Query253
-----SQL-----
SELECT Sum([DIZNOS]-[PIZNOS]) AS Suma, GK_Filter_PD_Opomene.DOK, Sum(GK_Filter_PD_Opomene.DIZNOS) AS SumOfDIZNOS, Sum(GK_Filter_PD_Opomene.PIZNOS) AS SumOfPIZNOS, First(GK_Filter_PD_Opomene.DATUM) AS FirstOfDATUM, GK_Filter_PD_Opomene.RACID, First(IIf(IsNull([DPO]),[DATUM],[DPO])) AS Dospece, Racun.DatumIzdavanja, Racun.SvrhaUplate2
FROM (GK_Filter_PD_Opomene LEFT JOIN Racun ON GK_Filter_PD_Opomene.RACID = Racun.IDRacun) LEFT JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
GROUP BY GK_Filter_PD_Opomene.DOK, GK_Filter_PD_Opomene.RACID, Racun.DatumIzdavanja, Racun.SvrhaUplate2, GK_Filter_PD_Opomene.KONTO, GK_Filter_PD_Opomene.lnkKUPACID, GK_Filter_PD_Opomene.lnkSkupstinaID, Racun.RBR
HAVING (((Sum([DIZNOS]-[PIZNOS]))>1) AND ((GK_Filter_PD_Opomene.KONTO)='2040') AND ((GK_Filter_PD_Opomene.lnkKUPACID)=1443) AND ((GK_Filter_PD_Opomene.lnkSkupstinaID)=103));


=====QUERY=====
Query254
-----SQL-----
SELECT GK.DOK, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM (GK LEFT JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina
GROUP BY GK.DOK, GK.lnkKUPACID
HAVING (((GK.lnkKUPACID)=9001))
ORDER BY GK.DOK;


=====QUERY=====
Query255
-----SQL-----
SELECT Opomena.*
FROM ((Opomena INNER JOIN PrinterBinLOCAL ON Opomena.IDOpomena = PrinterBinLOCAL.ID_Item) INNER JOIN Kupac ON Opomena.lnkKupac = Kupac.ID_K) INNER JOIN tblShortList_MailSufixPrintOnly ON Kupac.chkSkipPrintRacunGrupa = tblShortList_MailSufixPrintOnly.Index
WHERE (((PrinterBinLOCAL.TypeIndex)=2));


=====QUERY=====
Query256
-----SQL-----
SELECT Kupac.*, Opomena.*, GrupaOpomena.Datum
FROM ((Opomena INNER JOIN Kupac ON Opomena.lnkKupac = Kupac.ID_K) INNER JOIN GrupaOpomena ON Opomena.lnkGrupaOpomena = GrupaOpomena.IDGrupaOpomena) INNER JOIN PrinterBinLOCAL ON Opomena.IDOpomena = PrinterBinLOCAL.ID_Item
WHERE (((PrinterBinLOCAL.TypeIndex)=2));


=====QUERY=====
Query257
-----SQL-----
SELECT GK.KONTO, GK.DOK, GK.KontoTroska, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.KONTO, GK.DOK, GK.KontoTroska
HAVING (((GK.KONTO)="2040") AND ((GK.KontoTroska) Like "1114*"))
ORDER BY GK.DOK, GK.KontoTroska;


=====QUERY=====
Query258
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, GK.DOK, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.BR_NALOG, GK.KONTO, GK.DOK
HAVING (((GK.BR_NALOG)=[nalog]));


=====QUERY=====
Query259
-----SQL-----
SELECT GK.KONTO, GK.DOK, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkSkupstinaID
FROM GK
WHERE (((GK.TIP_STAVKE)<>1))
GROUP BY GK.KONTO, GK.DOK, GK.lnkSkupstinaID;


=====QUERY=====
Query26
-----SQL-----
UPDATE RacunStavke INNER JOIN GEN_SUMA_VP ON RacunStavke.VP=GEN_SUMA_VP.Grupa_VP SET RacunStavke.Ukupno = GEN_SUMA_VP.SumOfUkupno;


=====QUERY=====
Query260
-----SQL-----
SELECT Mail.eMail
FROM Mail INNER JOIN Kupac ON Mail.IDPartner = Kupac.ID_K
WHERE (((Mail.SendMailRacun)=-1) AND ((Kupac.lnk_ID_SK)=103));


=====QUERY=====
Query261
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXT, IzvodPrilivi.IDSkupstina, IzvodPrilivi.Zgrada, IzvodPrilivi.DatumRealizacije, IzvodPrilivi.SumOfOdobrenje
FROM IzvodPrilivi LEFT JOIN GrupaRacuna ON (IzvodPrilivi.DatumRealizacije = GrupaRacuna.DatumValute) AND (IzvodPrilivi.IDSkupstina = GrupaRacuna.ID_SK);


=====QUERY=====
Query262
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.DOK
FROM GK
WHERE (((GK.lnkIzvodStavkaID)=0))
GROUP BY GK.KONTO, GK.DOK
HAVING (((GK.KONTO)<>"2410"));


=====QUERY=====
Query263
-----SQL-----
SELECT Opomena.lnkGrupaOpomena, GK_PARTNERI.lnkKUPACID, GK_PARTNERI.SumOfDIZNOS, GK_PARTNERI.SumOfPIZNOS, Opomena.ActivnaOpomena
FROM Opomena INNER JOIN GK_PARTNERI ON Opomena.lnkKupac = GK_PARTNERI.lnkKUPACID;


=====QUERY=====
Query264
-----SQL-----
SELECT Racun.IDRacun, Racun.RBR, Racun.SvrhaUplate, Racun.ID_K, GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.Godina, GrupaRacuna.Mesec, Skustina.Folder, Skustina.IDSkupstina, tblShortList_MailSufixPrintOnly.Caption
FROM (((((Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina) INNER JOIN PrinterBinLOCAL ON Racun.IDRacun = PrinterBinLOCAL.ID_Item) INNER JOIN eMail_Partner ON Racun.ID_K = eMail_Partner.IDPartner) INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K) INNER JOIN tblShortList_MailSufixPrintOnly ON Kupac.chkSkipPrintRacunGrupa = tblShortList_MailSufixPrintOnly.Index
WHERE (((PrinterBinLOCAL.TypeIndex)=1));


=====QUERY=====
Query265
-----SQL-----
SELECT Opomena.lnkKupac, Last(Racun.Lokacija) AS LastOfLokacija
FROM Racun INNER JOIN Opomena ON Racun.ID_K = Opomena.lnkKupac
WHERE (((Opomena.lnkGrupaOpomena)=20 Or (Opomena.lnkGrupaOpomena)=21 Or (Opomena.lnkGrupaOpomena)=22))
GROUP BY Opomena.lnkKupac;


=====QUERY=====
Query266
-----SQL-----
SELECT GK.BR_NALOG, Nalog.OpisNaloga, GK.TIP_STAVKE, GK.KONTO
FROM GK INNER JOIN Nalog ON GK.BR_NALOG = Nalog.Br_Nalog
WHERE (((Nalog.OpisNaloga) Like "prek*"));


=====QUERY=====
Query267
-----SQL-----
SELECT Racun.*, Racun.IDKGrupniRacun
FROM Racun;


=====QUERY=====
Query268
-----SQL-----
SELECT GK.*
FROM GK;


=====QUERY=====
Query269
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.Zgrada, Skustina.NazivSS, Kupac.ID_K, Kupac.Naziv, Objekti.Ulaz, IIf(IsNull([kupac].[Adresa]),[Skustina].[adresa],[kupac].[Adresa]) AS AdresaUlica, IIf(IsNull([kupac].[PBroj]),[Skustina].[PBrojSZ],[kupac].[PBroj]) AS PostanskiBroj, IIf(IsNull([kupac].[KupacGrad]),[Skustina].[GradSZ],[kupac].[KupacGrad]) AS Grad, GK_PARTNERI.SumOfDIZNOS, GK_PARTNERI.SumOfPIZNOS
FROM ((Kupac INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) LEFT JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O) LEFT JOIN GK_PARTNERI ON Kupac.ID_K = GK_PARTNERI.lnkKUPACID
WHERE (((Kupac.ID_K) Not Like "*BEOGRAD NA VODI D.O.O.*") AND ((GK_PARTNERI.SumOfPIZNOS)=0));


=====QUERY=====
Query27
-----SQL-----
UPDATE RacunStavke INNER JOIN TEMP_GEN ON RacunStavke.IDRacunStavke=TEMP_GEN.ID SET RacunStavke.Ukupno = TEMP_GEN.SUMA;


=====QUERY=====
Query270
-----SQL-----
SELECT Dobavljac_Racuni.Napomena, GK.*
FROM GK INNER JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((Dobavljac_Racuni.Napomena)="Upravljanje lokali"));


=====QUERY=====
Query271
-----SQL-----
SELECT IzvodStavke.ID_SK, Sum(IzvodStavke.Odobrenje) AS SumOfOdobrenje, Sum(IzvodStavke.Zaduzenje) AS SumOfZaduzenje, Sum([Odobrenje]-[Zaduzenje]) AS DIPI
FROM IzvodStavke
WHERE (((IzvodStavke.DatumRealizacije)<=[Forms]![Izvestaji]![txtDatumDo]))
GROUP BY IzvodStavke.ID_SK;


=====QUERY=====
Query272
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, GK.BR_NALOG, GK.DATUM, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum([DIZNOS]-[PIZNOS]) AS DIPI
FROM GK
GROUP BY GK.KONTO, GK.lnkSkupstinaID, GK.BR_NALOG, GK.DATUM
HAVING (((GK.KONTO)="2410"));


=====QUERY=====
Query273
-----SQL-----
SELECT GK.KONTO, GK.BR_NALOG, GK.DATUM, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK
GROUP BY GK.KONTO, GK.BR_NALOG, GK.DATUM, GK.lnkSkupstinaID
HAVING (((GK.KONTO)="4350") AND ((GK.lnkSkupstinaID)=101));


=====QUERY=====
Query274
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkSkupstinaID, GK.KontoTroska
FROM GK
GROUP BY GK.KONTO, GK.lnkSkupstinaID, GK.KontoTroska;


=====QUERY=====
Query275
-----SQL-----
SELECT GK.lnkSkupstinaID, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.TrosakNa
FROM GK LEFT JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
GROUP BY GK.lnkSkupstinaID, GK.KONTO, Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.TrosakNa;


=====QUERY=====
Query276
-----SQL-----
SELECT Len([PodKonto]) AS Expr1, Troskovi_PodKonta.*
FROM Troskovi_PodKonta;


=====QUERY=====
Query277
-----SQL-----
SELECT GK.KONTO, GK.lnkSkupstinaID, TKONTO.GRUPA1, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS
FROM GK LEFT JOIN TKONTO ON GK.KontoTroska = TKONTO.PodKonto
GROUP BY GK.KONTO, GK.lnkSkupstinaID, TKONTO.GRUPA1;


=====QUERY=====
Query278
-----SQL-----
SELECT Objekti.naziv, Objekti.lnk_tip, TransferGM.[No], TransferGM.Kv, TransferGM.Desc, Objekti.K1, TransferGM.ID, TransferGM.Kv, Objekti.Napomena, TransferGM.Desc
FROM Objekti INNER JOIN TransferGM ON Objekti.naziv = TransferGM.unit;


=====QUERY=====
Query279
-----SQL-----
SELECT Objekti.naziv, Kupac.Naziv, Objekti.K1, Objekti.Status, tblShortList_StatusObjekta.Caption, Objekti.HandOverDate
FROM (Kupac RIGHT JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) LEFT JOIN tblShortList_StatusObjekta ON Objekti.Status = tblShortList_StatusObjekta.Index
WHERE (((Objekti.lnk_tip)=4) AND ((Objekti.lnkSkupstinaID)=231 Or (Objekti.lnkSkupstinaID)=232));


=====QUERY=====
Query28
-----SQL-----
DELETE TEMP_GEN.*
FROM TEMP_GEN;


=====QUERY=====
Query280
-----SQL-----
SELECT Kupac.ID_K, Kupac.lnk_ID_SK, Kupac.Naziv, TransferIDK.UNIT, Objekti.lnk_ID_K, Objekti.ID_O
FROM (Kupac INNER JOIN TransferIDK ON Kupac.Naziv = TransferIDK.CUST) INNER JOIN Objekti ON TransferIDK.UNIT = Objekti.naziv;


=====QUERY=====
Query281
-----SQL-----
SELECT Objekti.ID_O, Objekti.naziv, Objekti.lnk_ID_K, TransferKupac.Kupac, Kupac.ID_K, Kupac.Naziv, Kupac_1.Naziv
FROM ((TransferKupac INNER JOIN Objekti ON TransferKupac.Unit = Objekti.naziv) INNER JOIN Kupac ON TransferKupac.Kupac = Kupac.Naziv) LEFT JOIN Kupac AS Kupac_1 ON Objekti.lnk_ID_K = Kupac_1.ID_K;


=====QUERY=====
Query282
-----SQL-----
SELECT Objekti.ID_O, Objekti.HandOverDate
FROM Objekti INNER JOIN TransferHO ON Objekti.naziv = TransferHO.uNIT;


=====QUERY=====
Query283
-----SQL-----
SELECT Skustina.Folder, Objekti.naziv, Kupac.Naziv, Objekti.K1, Objekti.HandOverDate, tblShortList_StatusObjekta.Caption
FROM ((Objekti LEFT JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) LEFT JOIN tblShortList_StatusObjekta ON Objekti.Status = tblShortList_StatusObjekta.Index
WHERE (((Objekti.lnk_tip)=4) AND ((Objekti.HandOverDate) Is Null))
ORDER BY Skustina.Folder, Objekti.BrojPD;


=====QUERY=====
Query284
-----SQL-----
SELECT [lnk_ID_SK]=[lnkSkupstinaID] AS Expr1, Objekti.lnkSkupstinaID, Kupac.ID_K, Kupac.Naziv, Objekti.lnk_tip, Objekti.naziv, Objekti.HandOverDate
FROM Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K;


=====QUERY=====
Query285
-----SQL-----
SELECT KUPAC_GM.lnk_ID_K
FROM KUPAC_GM;


=====QUERY=====
Query286
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Len([Naziv]) AS Expr1
FROM Kupac;


=====QUERY=====
Query287
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS, Skustina.Zgrada
FROM Skustina;


=====QUERY=====
Query288
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Skustina.Zgrada, TipObjekta.Print, Sum(Objekti.K1) AS SumOfK1, Sum(Objekti.K2) AS SumOfK2, Sum(Objekti.K3) AS SumOfK3
FROM (Objekti INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina
GROUP BY Objekti.lnkSkupstinaID, Skustina.Zgrada, Objekti.lnk_tip, TipObjekta.Print, TipObjekta.SortObj
ORDER BY Objekti.lnkSkupstinaID, TipObjekta.SortObj;


=====QUERY=====
Query289
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.Zgrada, TipObjekta.TipObj, TipObjekta.Print, IIf([Status]=1,"AKTIVAN","") AS STATUSO, Sum(Objekti.K1) AS SumOfK1, Sum(Objekti.K2) AS SumOfK2, Sum(Objekti.K3) AS SumOfK3
FROM (Skustina INNER JOIN Objekti ON Skustina.IDSkupstina = Objekti.lnkSkupstinaID) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
GROUP BY Skustina.IDSkupstina, Skustina.Zgrada, TipObjekta.TipObj, TipObjekta.Print, IIf([Status]=1,"AKTIVAN",""), TipObjekta.SortObj
ORDER BY TipObjekta.SortObj;


=====QUERY=====
Query29
-----SQL-----
SELECT RacunStavke.lnkGR, RacunStavke.ID_O, RacunStavke.Ukupno, RacunStavke.VP
FROM RacunStavke
WHERE (((RacunStavke.lnkGR)=1) AND ((RacunStavke.ID_O)=3) AND ((RacunStavke.VP)=3));


=====QUERY=====
Query290
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.Zgrada, TipObjekta.TipObj, TipObjekta.Print, Objekti.naziv, Objekti.K1, Objekti.K2, Objekti.K3
FROM (Skustina INNER JOIN Objekti ON Skustina.IDSkupstina = Objekti.lnkSkupstinaID) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Objekti.Status)<>1))
ORDER BY Skustina.IDSkupstina, TipObjekta.SortObj;


=====QUERY=====
Query291
-----SQL-----
SELECT RacunStavke.lnkGR, RacunStavke.ID_SK, Count(RacunStavke.IDRacunStavke) AS CountOfIDRacunStavke
FROM RacunStavke
GROUP BY RacunStavke.lnkGR, RacunStavke.ID_SK;


=====QUERY=====
Query292
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Objekti.lnk_tip, Kupac.Naziv, Objekti.naziv, Kupac.chkSkipPrintRacunGrupa
FROM Kupac INNER JOIN Objekti ON Kupac.DostavaSifraPD = Objekti.ID_O
WHERE (((Objekti.lnkSkupstinaID)=232) AND ((Kupac.chkSkipPrintRacunGrupa)=-1));


=====QUERY=====
Query293
-----SQL-----
SELECT [IDRacun] & "-" & [IDObjekat] AS Expr1, RacunObjekti.IDObjekat, RacunObjekti.IDRacun, Count(RacunObjekti.IDRacun) AS CountOfIDRacun
FROM RacunObjekti
GROUP BY [IDRacun] & "-" & [IDObjekat], RacunObjekti.IDObjekat, RacunObjekti.IDRacun;


=====QUERY=====
Query294
-----SQL-----
SELECT GK.BR_NALOG, GK.lnkKUPACID AS IDK, Skustina.Zgrada, GK.DATUM, GK.DOK, Troskovi_PodKonta.Naziv, GK.PIZNOS, GK.KontoTroska
FROM (GK INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
WHERE (((GK.BR_NALOG)=532 Or (GK.BR_NALOG)=531) AND ((GK.lnkKUPACID)=9001));


=====QUERY=====
Query295
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.TrosakNa, Len([PodKonto]) AS Expr1
FROM Troskovi_PodKonta;


=====QUERY=====
Query296
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS, Skustina.Zgrada, Kupac.ID_K, Kupac.Naziv, Objekti.naziv, Kupac.PIB, Objekti.Ulaz, SzUlaz.Adresa, Objekti.Status, TipObjekta.Print, Objekti.K1
FROM Skustina, ((Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN SzUlaz ON Objekti.Ulaz = SzUlaz.Ulaz
ORDER BY Skustina.IDSkupstina, Kupac.ID_K;


=====QUERY=====
Query297
-----SQL-----
SELECT Objekti.naziv, TipObjekta.TipObj, Kupac.Naziv, Mail.eMail, TipObjekta.Print
FROM (Objekti INNER JOIN (Kupac INNER JOIN Mail ON Kupac.ID_K = Mail.IDPartner) ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Mail.SendMailRacun)=-1))
ORDER BY Kupac.ID_K, TipObjekta.SortObj, Objekti.BrojPD;


=====QUERY=====
Query298
-----SQL-----
SELECT Round(Sum([DIZNOS]-[PIZNOS]),2) AS STANJE, GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI, GK.DOK, GK.KontoTroska, GK.RDOB, GK.RACID
FROM GK
GROUP BY GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI, GK.DOK, GK.KontoTroska, GK.RDOB, GK.RACID, GK.lnkSkupstinaID, GK.lnkKUPACID
HAVING (((Round(Sum([DIZNOS]-[PIZNOS]),2))<>0) AND ((GK.PARAMETRI) = '8610110112512') AND ((GK.lnkSkupstinaID) = 101) AND ((GK.lnkKUPACID) = 1011))
ORDER BY First(GK.DATUM), GK.PRIORITET, GK.SIFRAKN;


=====QUERY=====
Query299
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Sum(Objekti.K1) AS SumOfK1, Objekti.lnk_tip
FROM Objekti
WHERE (((Objekti.Status)=1))
GROUP BY Objekti.lnkSkupstinaID, Objekti.lnk_tip;


=====QUERY=====
Query3
-----SQL-----
SELECT OpomenaStavke.*, Opomena.BNR, OpomenaStavke.Suma, OpomenaStavke.lnkKupacID, OpomenaStavke.M
FROM OpomenaStavke INNER JOIN Opomena ON OpomenaStavke.lnkOpomena = Opomena.IDOpomena
WHERE (((Opomena.lnkGrupaOpomena)=1) AND ((OpomenaStavke.lnkKupacID)=1002))
ORDER BY OpomenaStavke.M;


=====QUERY=====
Query30
-----SQL-----
INSERT INTO Racun ( ID_K, ID_SK, DatumIzdavanja, MestoIzdavanja, DatumUsluge, Kupac, PBroj_K, Adresa_K, PIB, StanjePredhodniDug, StanjeDug, Valuta, PozivNaBrojPDug, DatumValute, SvrhaUplate, PozivNaBroj, lnkGR, ID_O, objekatNaziv, SvrhaUplate2 )
SELECT RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, UCase(GrupaRacuna.Mesto) AS GrupaRacunaMesto, GrupaRacuna.DatumUsluge, UCase(Kupac.Naziv) AS KupacNaziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, KUPAC_TS.BLOK, KUPAC_TS.STANJE, 'RSD' AS Expr1, Kupac.PBPD, GrupaRacuna.DatumValute, TipObjekta.NaslovRC AS Expr3, KontrolniBroj(97,GrupaRacuna.Godina & GrupaRacuna.Mesec & '-' & Format(RacunStavke.ID_K,'0000')) & "-" & GrupaRacuna.Godina & GrupaRacuna.Mesec & '-' & Format(RacunStavke.ID_K,'0000') AS PB, RacunStavke.lnkGR, RacunStavke.ID_O, UCase([objekti].[naziv]) AS ObjNaziv, UCase(Objekti.naziv & ", " & Objekti.Adresa & " " & Objekti.Ulaz & " - " & GrupaRacuna.GrupaRacunaFXT) AS Expr2
FROM ((((RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Kupac ON RacunStavke.ID_K = Kupac.ID_K) INNER JOIN KUPAC_TS ON RacunStavke.ID_K = KUPAC_TS.IDK) INNER JOIN Objekti ON RacunStavke.ID_O = Objekti.ID_O) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((RacunStavke.lnkGR)=49) AND ((RacunStavke.ID_R)=0))
GROUP BY RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, UCase(GrupaRacuna.Mesto), GrupaRacuna.DatumUsluge, UCase(Kupac.Naziv), Kupac.PBroj, Kupac.Adresa, Kupac.PIB, KUPAC_TS.BLOK, KUPAC_TS.STANJE, 'RSD', Kupac.PBPD, GrupaRacuna.DatumValute, TipObjekta.NaslovRC, KontrolniBroj(97,GrupaRacuna.Godina & GrupaRacuna.Mesec & '-' & Format(RacunStavke.ID_K,'0000')) & "-" & GrupaRacuna.Godina & GrupaRacuna.Mesec & '-' & Format(RacunStavke.ID_K,'0000'), RacunStavke.lnkGR, RacunStavke.ID_O, UCase([objekti].[naziv]), UCase(Objekti.naziv & ", " & Objekti.Adresa & " " & Objekti.Ulaz & " - " & GrupaRacuna.GrupaRacunaFXT);


=====QUERY=====
Query300
-----SQL-----
SELECT GK.lnkSkupstinaID, "ODRŽAVANJE TR, PROVIZIJE ZA " & GetYYMMfromDate([Datum],"-",4) AS Naziv, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkKUPACID, PoslednjiDanMesecaOdDatuma([Datum]) AS PDM, FormatDatumLikeSort(PoslednjiDanMesecaOdDatuma([Datum])) & " TR " & " " & GetYYMMfromDate([Datum],"-",4) AS RBR, GetYYMMfromDate([Datum]) AS YYMM
FROM GK
GROUP BY GK.lnkSkupstinaID, "ODRŽAVANJE TR, PROVIZIJE ZA " & GetYYMMfromDate([Datum],"-",4), GK.lnkKUPACID, PoslednjiDanMesecaOdDatuma([Datum]), FormatDatumLikeSort(PoslednjiDanMesecaOdDatuma([Datum])) & " TR " & " " & GetYYMMfromDate([Datum],"-",4), GetYYMMfromDate([Datum])
HAVING (((GK.lnkKUPACID)=9003));


=====QUERY=====
Query301
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN] AS Dokument, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK.DIZNOS) AS SumOfDIZNOS, Racun.IDRacun, Round(Sum([DIZNOS])-[Ukupno],2) AS Razlika, Racun.Ukupno
FROM GK INNER JOIN (GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) ON GK.RACID = Racun.IDRacun
WHERE (((Racun.Storno)=False) AND ((GK.DATUM) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]))
GROUP BY GrupaRacuna.IDGrupaRacuna, GrupaRacuna.ID_SK, "R-" & [GrupaRacunaFXN], GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, Racun.IDRacun, Racun.Ukupno
HAVING (((GrupaRacuna.DatumPrometa) Between [Forms]![Izvestaji]![txtDatumOd] And [Forms]![Izvestaji]![txtDatumDo]) AND ((Round(Sum([DIZNOS])-[Ukupno],2))<>0))
ORDER BY GrupaRacuna.DatumIzdavanja;


=====QUERY=====
Query302
-----SQL-----
SELECT GK.BR_NALOG, GK.TIP_STAVKE, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.RDOB
FROM GK
GROUP BY GK.BR_NALOG, GK.TIP_STAVKE, GK.KONTO, GK.RDOB
HAVING (((GK.TIP_STAVKE)=3)) OR (((GK.TIP_STAVKE)=4));


=====QUERY=====
Query303
-----SQL-----
SELECT GK.lnkKUPACID, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Racun.Ukupno, Racun.IDRacun, Round([Ukupno],2)-Round(Sum([DIZNOS]),2) AS kontrola
FROM Racun INNER JOIN GK ON Racun.IDRacun = GK.RACID
GROUP BY GK.lnkKUPACID, Racun.Ukupno, Racun.IDRacun
HAVING (((Round([Ukupno],2)-Round(Sum([DIZNOS]),2))<>0));


=====QUERY=====
Query304
-----SQL-----
SELECT Racun.IDRacun, GK.RACID, GK.DOK
FROM Racun RIGHT JOIN GK ON Racun.IDRacun = GK.RACID
GROUP BY Racun.IDRacun, GK.RACID, GK.DOK
HAVING (((Racun.IDRacun) Is Null));


=====QUERY=====
Query305
-----SQL-----
SELECT GK.BR_NALOG, GK.KONTO, GK.KontoTroska, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Round(Sum([DIZNOS]-[PIZNOS]),2) AS DIPI
FROM GK
GROUP BY GK.BR_NALOG, GK.KONTO, GK.KontoTroska
HAVING (((GK.BR_NALOG)=167));


=====QUERY=====
Query306
-----SQL-----
SELECT Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.KONTO, GK.lnkKUPACID, Year([DATUM]) AS Expr1, Month([Datum]) AS Expr2, GK.lnkSkupstinaID, "ODRŽAVANJE TR, PROVIZIJE ZA " & Year([DATUM]) & "-" & Format(Month([Datum]),"00") AS INFO, Last(GK.DATUM) AS LastOfDATUM, DateAdd("d",-1,DateAdd("m",1,Year([DATUM]) & "-" & Month([Datum]) & "-" & 1)) AS DP
FROM GK
GROUP BY GK.KONTO, GK.lnkKUPACID, Year([DATUM]), Month([Datum]), GK.lnkSkupstinaID, "ODRŽAVANJE TR, PROVIZIJE ZA " & Year([DATUM]) & "-" & Format(Month([Datum]),"00"), DateAdd("d",-1,DateAdd("m",1,Year([DATUM]) & "-" & Month([Datum]) & "-" & 1))
HAVING (((GK.lnkKUPACID)=9003));


=====QUERY=====
Query307
-----SQL-----
SELECT GK.lnkKUPACID, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, Round(Sum([PIZNOS]),2) AS SP, Round(Sum([DIZNOS]),2) AS SD, Count(GK.STAVKAID) AS BrojStavki
FROM GK
WHERE (((GK.KONTO) Like '204*'))
GROUP BY GK.lnkKUPACID, GK.DOK, GK.RDOB, GK.lnkSkupstinaID
HAVING (((Round(Sum([DIZNOS]-[PIZNOS]),2))<>0) AND ((Round(Sum([DIZNOS]),2))>-0.01));


=====QUERY=====
Query308
-----SQL-----
SELECT GrupaRacuna.Mesec, GrupaRacuna.Godina, GrupaRacuna.ID_SK, Objekti.lnk_tip, Objekti.ID_O, Objekti.naziv
FROM ((Racun INNER JOIN RacunObjekti ON Racun.IDRacun = RacunObjekti.IDRacun) INNER JOIN Objekti ON RacunObjekti.IDObjekat = Objekti.ID_O) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GrupaRacuna.Mesec)="02") AND ((GrupaRacuna.Godina)="2026") AND ((GrupaRacuna.ID_SK)=231) AND ((Objekti.lnk_tip)=4));


=====QUERY=====
Query308-2
-----SQL-----
SELECT GrupaRacuna.Mesec, GrupaRacuna.Godina, GrupaRacuna.ID_SK, Objekti.lnk_tip, Objekti.ID_O, Objekti.naziv
FROM ((Racun INNER JOIN RacunObjekti ON Racun.IDRacun = RacunObjekti.IDRacun) INNER JOIN Objekti ON RacunObjekti.IDObjekat = Objekti.ID_O) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE (((GrupaRacuna.Mesec)="03") AND ((GrupaRacuna.Godina)="2026") AND ((GrupaRacuna.ID_SK)=231) AND ((Objekti.lnk_tip)=4));


=====QUERY=====
Query309
-----SQL-----
UPDATE Racun SET Racun.UkupnoRacun = [Ukupno], Racun.Ukupno = [Ukupno]+[KamataIznos];


=====QUERY=====
Query31
-----SQL-----
SELECT Troskovi_Ugovori.ID_Troskovi_Ugovori, Kupac.Naziv, Troskovi_VP.Troskovi_Vrste, Troskovi_VP.Troskovi_Vrste_Opis, Troskovi_Ugovori.Vrednost, Troskovi_Ugovori.TipObjekta, Troskovi_VP.TipObjektaRep
FROM (Troskovi_Ugovori INNER JOIN Troskovi_VP ON Troskovi_Ugovori.lnk_TV = Troskovi_VP.ID_Troskovi_Vrste) INNER JOIN Kupac ON Troskovi_Ugovori.lnk_ID_Dob = Kupac.ID_K;


=====QUERY=====
Query310
-----SQL-----
SELECT ZK.IDSK, Sum(ZK.DIZNOS) AS SumOfDIZNOS, ZK.PARTNERID
FROM ZK
WHERE (((ZK.IDGR)=33))
GROUP BY ZK.IDSK, ZK.PARTNERID;


=====QUERY=====
Query311
-----SQL-----
UPDATE (SELECT ZK.IDSK, Sum(ZK.DIZNOS) AS SumOfDIZNOS, ZK.PARTNERID, ZK.IDGR
FROM ZK
WHERE (((ZK.IDGR)=33))
GROUP BY ZK.IDSK, ZK.PARTNERID, ZK.IDGR
)  AS ZKGRUP INNER JOIN Racun ON (ZKGRUP.PARTNERID = Racun.ID_K) AND (ZKGRUP.IDGR = Racun.lnkGR) SET Racun.KamataIznos = [ZKGRUP].[SumOfDIZNOS];


=====QUERY=====
Query312
-----SQL-----
UPDATE Racun SET Racun.Ukupno = [UkupnoRacun]+[KamataIznos]
WHERE (((Racun.lnkGR)=33));


=====QUERY=====
Query313
-----SQL-----
UPDATE Racun INNER JOIN (SELECT
        PARTNERID,
        IDGR,
        Sum(DIZNOS) AS SumOfDIZNOS
    FROM ZK
    WHERE IDGR = 34
    GROUP BY PARTNERID, IDGR
)  AS ZKGRUP ON (Racun.lnkGR = ZKGRUP.IDGR) AND (Racun.ID_K = ZKGRUP.PARTNERID) SET Racun.KamataIznos = ZKGRUP.SumOfDIZNOS;


=====QUERY=====
Query314
-----SQL-----
INSERT INTO Racun ( lnkGR, ID_K, ID_SK, DatumIzdavanja, DatumUsluge, DatumValute, DatumPrometa, MestoIzdavanja, Kupac, PBroj_K, Adresa_K, PIB, MB, PrethodniDug, PD_iznos, Ukupno, UkupnoRacun, KamataIznos, DatumStanja, RacunShema, Lokacija )
SELECT Racun.lnkGR, Kupac.IDGrupniRacunMaster, Racun.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, GrupaRacuna.Mesto, Kupac_1.Naziv, Kupac_1.PBroj, Kupac_1.Adresa, Kupac_1.PIB, Kupac_1.PIB, Nz([Suma0],0) AS PDD, Nz([Suma98],0) AS PDN, Sum(Racun.Ukupno) AS SumOfUkupno, Sum(Racun.UkupnoRacun) AS SumOfUR, Sum(Racun.KamataIznos) AS SumOfKI, GrupaRacuna.DatumStanja, 'GR' AS RacunShema, Skustina.Folder
FROM (Kupac_DUG RIGHT JOIN (Kupac AS Kupac_1 INNER JOIN ((GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K) ON Kupac_1.ID_K = Kupac.IDGrupniRacunMaster) ON Kupac_DUG.lnkKUPACID = Kupac_1.ID_K) INNER JOIN Skustina ON GrupaRacuna.ID_SK = Skustina.IDSkupstina
GROUP BY Racun.lnkGR, Kupac.IDGrupniRacunMaster, Racun.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, GrupaRacuna.Mesto, Kupac_1.Naziv, Kupac_1.PBroj, Kupac_1.Adresa, Kupac_1.PIB, GrupaRacuna.DatumStanja, Skustina.Folder, Kupac_DUG.Suma0, Kupac_DUG.Suma98
HAVING (((Racun.lnkGR)=35) AND (Not (Kupac.IDGrupniRacunMaster) Is Null));


=====QUERY=====
Query315
-----SQL-----
INSERT INTO GK ( BR_NALOG, Konto, DATUM, DIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, DPO, PARAMETRI, RACID, KontoTroska, SIFRAKONTA )
SELECT " & novNalog & " AS Nalog, ZK.KONTO, ZK.DATUM, ZK.DIZNOS, ZK.TIPSTAVKE, ZK.IDSK, ZK.PARTNERID, 'R-' & [ZK].[PARAMETRI] AS DOK, ZK.DATUMVALUTE, Replace([RACUN].[PozivNaBroj],'-','') AS PB, ZK.RACUNID, ZK.PODKONTO, ZK.PARTNERID
FROM ZK INNER JOIN Racun ON ZK.RACUNID = Racun.IDRacun
WHERE (((ZK.DIZNOS)<>0) AND ((ZK.IDSK)=101) AND ((ZK.IDGR)=164));


=====QUERY=====
Query316
-----SQL-----
UPDATE Racun SET Racun.KamataIznos = Nz(DSum('DIZNOS','ZK','PARTNERID=' & [ID_K] & ' AND IDGR=' & [lnkGR]),0)
WHERE (((Racun.[lnkGR])=34));


=====QUERY=====
Query317
-----SQL-----
INSERT INTO GK ( BR_NALOG, Konto, DATUM, PIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, DPO, KontoTroska, SIFRAKONTA )
SELECT " & novNalog & " AS Nalog, '4909' AS KONTO, ZK.DATUM, Sum(ZK.DIZNOS) AS SumOfDIZNOS, ZK.TIPSTAVKE, ZK.IDSK, 0 AS PARTNERID, 'R-' & [ZK].[PARAMETRI] AS DOK, ZK.DATUM, ZK.PODKONTO, Troskovi_PodKonta_DefDob.DefDob
FROM (ZK INNER JOIN Racun ON ZK.RACUNID = Racun.IDRacun) INNER JOIN Troskovi_PodKonta_DefDob ON (ZK.IDSK = Troskovi_PodKonta_DefDob.IDSZ) AND (ZK.PODKONTO = Troskovi_PodKonta_DefDob.Konto)
GROUP BY " & novNalog & ", '4909', ZK.DATUM, ZK.TIPSTAVKE, ZK.IDSK, 0, 'R-' & [ZK].[PARAMETRI], ZK.DATUM, ZK.PODKONTO, Troskovi_PodKonta_DefDob.DefDob, ZK.IDGR
HAVING (((Sum(ZK.DIZNOS))<>0) AND ((ZK.IDSK)=101) AND ((ZK.IDGR)=164));


=====QUERY=====
Query318
-----SQL-----
SELECT GK.DPO, GK.STAVKAID, GK.KONTO, GK.lnkKUPACID, GK.PIZNOS, Nz([KontoTroska],'') AS KT
FROM GK
WHERE (((GK.KONTO)="2040") AND ((GK.lnkKUPACID)=1050) AND ((Nz([KontoTroska],''))=''))
ORDER BY GK.DPO, GK.STAVKAID;


=====QUERY=====
Query319
-----SQL-----
SELECT ZK.PODKONTO
FROM ZK
WHERE (((ZK.RACUNID)>0))
GROUP BY ZK.PODKONTO;


=====QUERY=====
Query32
-----SQL-----
SELECT RacunStavke.lnkGR, RacunStavke_1.lnkGR, RacunStavke.Ukupno, RacunStavke_1.Ukupno
FROM RacunStavke INNER JOIN RacunStavke AS RacunStavke_1 ON (RacunStavke.ID_K=RacunStavke_1.ID_K) AND (RacunStavke.ID_O=RacunStavke_1.ID_O) AND (RacunStavke.lnkTVP=RacunStavke_1.lnkTVP)
WHERE (((RacunStavke.lnkGR)=101) AND ((RacunStavke_1.lnkGR)=102));


=====QUERY=====
Query320
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkKUPACID
FROM GK
GROUP BY GK.KONTO, GK.lnkKUPACID
HAVING (((GK.KONTO)="2049"));


=====QUERY=====
Query321
-----SQL-----
SELECT GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkSkupstinaID, GK.KontoTroska, Right(Year([DATUM]),2) & Format(Month([DATUM]),'00') AS YYMM, GK.lnkKUPACID
FROM GK
GROUP BY GK.KONTO, GK.lnkSkupstinaID, GK.KontoTroska, Right(Year([DATUM]),2) & Format(Month([DATUM]),'00'), GK.lnkKUPACID
HAVING (((GK.KontoTroska)="21611") AND ((Right(Year([DATUM]),2) & Format(Month([DATUM]),'00')) Like '26*'))
ORDER BY Right(Year([DATUM]),2) & Format(Month([DATUM]),'00'), GK.lnkKUPACID;


=====QUERY=====
Query322
-----SQL-----
INSERT INTO Dobavljac_Racuni ( TipDokumenta, SK_ID, RacunNO, NazivRacuna, DobavljacKonto, TipObracuna, PrioritetNaplate, KontoKnjizenja )
SELECT Dobavljac_Racuni.TipDokumenta, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.DobavljacKonto, Dobavljac_Racuni.TipObracuna, Dobavljac_Racuni.PrioritetNaplate, Dobavljac_Racuni.KontoKnjizenja, *
FROM Dobavljac_Racuni
WHERE (((Dobavljac_Racuni.MesecRacuna)='0000') AND ((Dobavljac_Racuni.TipDokumenta)=9));


=====QUERY=====
Query324
-----SQL-----
SELECT Racun.Kupac, RacunStavke.*
FROM Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R
WHERE (((Racun.Kupac) Like "*BEOGRAD NA VODI*") AND ((Racun.lnkGR)>171));


=====QUERY=====
Query33
-----SQL-----
INSERT INTO RacunStavke ( KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, TipObracuna, ToDo, Sort, AddTXT, Grupa_VP )
SELECT Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovori.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Troskovi_Ugovori.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Sort, Troskovi_VP.AddTxt, Troskovi_VP.Grupa_VP
FROM (Troskovi_Ugovori RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovori.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovori.lnk_ID_Dob=Kupac.ID_K
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query34
-----SQL-----
SELECT Troskovi_Racuni.MesecRacuna, Troskovi_Racuni.RacunNO
FROM Troskovi_Racuni
GROUP BY Troskovi_Racuni.MesecRacuna, Troskovi_Racuni.RacunNO
HAVING (((Troskovi_Racuni.MesecRacuna)="1013"));


=====QUERY=====
Query35
-----SQL-----
SELECT Troskovi_VP.*, Troskovi_Ugovor.IzRacuna, Troskovi_Ugovor.LINK, Troskovi_Ugovor.MesecRacuna
FROM Troskovi_Ugovor RIGHT JOIN Troskovi_VP ON Troskovi_Ugovor.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste
WHERE (((Troskovi_VP.TipObjektaRep)=1) AND ((Troskovi_Ugovor.MesecRacuna)="1013" Or (Troskovi_Ugovor.MesecRacuna) Is Null))
ORDER BY Troskovi_VP.Sort;


=====QUERY=====
Query36
-----SQL-----
SELECT Troskovi_VP.*, Troskovi_Ugovor_FRMTROSKOVIVP.*
FROM Troskovi_VP LEFT JOIN Troskovi_Ugovor_FRMTROSKOVIVP ON Troskovi_VP.ID_Troskovi_Vrste=Troskovi_Ugovor_FRMTROSKOVIVP.lnk_TV
WHERE (((Troskovi_VP.TipObjektaRep)=Forms!Troskovi_VP!cmbMesec))
ORDER BY Troskovi_VP.Sort;


=====QUERY=====
Query37
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA] )
SELECT " & linkGrupeRacuna & " AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovori.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovor.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis, Troskovi_VP.Sort, Troskovi_VP.AddTXT, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],''), Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Budzet.Suma
FROM ((Troskovi_Ugovor RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovor.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovor.lnk_ID_Dob=Kupac.ID_K) LEFT JOIN Troskovi_Budzet ON Troskovi_Ugovor.IzBudzeta=Troskovi_Budzet.ID_Troskovi_Budzet
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query38
-----SQL-----
SELECT Troskovi_Ugovor.ID_Troskovi_Ugovori, Troskovi_Ugovor.Vrednost, Troskovi_Ugovor.SumaD, Troskovi_Racuni.TipObracuna, Troskovi_Racuni.DobavljacKonto, Troskovi_Racuni.MesecRacuna
FROM Troskovi_Ugovor INNER JOIN Troskovi_Racuni ON (Troskovi_Ugovor.IzRacuna=Troskovi_Racuni.RacunNO) AND (Troskovi_Racuni.MesecRacuna=Troskovi_Ugovor.MesecRacuna)
WHERE (((Troskovi_Racuni.MesecRacuna)=Forms!GrupaRacuna_Add!Combo37));


=====QUERY=====
Query39
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA] )
SELECT 107 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovori.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovori.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis, Troskovi_VP.Sort, Troskovi_VP.AddTXT, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],''), Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Budzet.Suma
FROM ((Troskovi_Ugovori RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovori.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovori.lnk_ID_Dob=Kupac.ID_K) LEFT JOIN Troskovi_Budzet ON Troskovi_Ugovori.IzBudzeta=Troskovi_Budzet.ID_Troskovi_Budzet
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query4
-----SQL-----
DELETE Opomena.BNR, Opomena.*
FROM Opomena
WHERE (((Opomena.BNR)<0));


=====QUERY=====
Query40
-----SQL-----
DELETE Troskovi_Ugovor.MesecRacuna, Troskovi_Ugovor.IzRacuna
FROM Troskovi_Ugovor
WHERE (((Troskovi_Ugovor.MesecRacuna)="1113") AND ((Troskovi_Ugovor.IzRacuna)=0)) OR (((Troskovi_Ugovor.MesecRacuna)="1113") AND ((Troskovi_Ugovor.IzRacuna) Is Null));


=====QUERY=====
Query41
-----SQL-----
SELECT Troskovi_Racuni.*, Troskovi_Racuni.MesecRacuna
FROM Troskovi_Racuni
WHERE (((Troskovi_Racuni.RacunNO)=1) AND ((Troskovi_Racuni.MesecRacuna)="1113"));


=====QUERY=====
Query42
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA] )
SELECT 110 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovor.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovor.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis AS Expr1, Troskovi_VP.Sort, Troskovi_VP.AddTxt, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],'') AS Expr2, Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Ugovor.SumaD
FROM (Troskovi_Ugovor RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovor.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovor.lnk_ID_Dob=Kupac.ID_K
WHERE (((Objekti.Status)=0) AND ((Troskovi_Ugovor.MesecRacuna)="1113"))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query43
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA] )
SELECT 110 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovor_FLT.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovor_FLT.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis AS Expr1, Troskovi_VP.Sort, Troskovi_VP.AddTxt, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],'') AS Expr2, Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Ugovor_FLT.SumaD
FROM (Troskovi_Ugovor_FLT RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovor_FLT.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovor_FLT.lnk_ID_Dob=Kupac.ID_K
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query44
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA] )
SELECT 112 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovor_FLT.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovor_FLT.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis, Troskovi_VP.Sort, Troskovi_VP.AddTXT, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],''), Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Ugovor_FLT.SumaD
FROM (Troskovi_Ugovor_FLT RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip = Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip = TipObjekta.IDTipObj) ON Troskovi_Ugovor_FLT.lnk_TV = Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovor_FLT.lnk_ID_Dob = Kupac.ID_K
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query45
-----SQL-----
INSERT INTO GK ( BR_NALOG, KONTO, DATUM, PIZNOS, TIP_STAVKE, DOK, lnkSkupstinaID, lnkKUPACID, PARAMETRI )
SELECT 1 AS mNalog, '4330' & Format(IIf([DobavljacKonto]=0,9917,[DobavljacKonto]),'0000') AS mKonto, #12/30/1899# AS mDatum, Round(Sum(IIf([IznosE]>0,[IznosE]*[NBS],[IznosD])),2) AS Ukupno, 4 AS mTip, 'UF' & 114 AS mDOK, '27' AS mSK, Format(IIf([DobavljacKonto]=0,9917,[DobavljacKonto]),'0000') AS Konto, 114 AS mPar
FROM Troskovi_Racuni INNER JOIN GrupaRacuna ON Troskovi_Racuni.MesecRacuna=GrupaRacuna.GrupaRacunaFXN
WHERE (((GrupaRacuna.IDGrupaRacuna)=111))
GROUP BY Troskovi_Racuni.RacunNO, IIf([DobavljacKonto]=0,9917,[DobavljacKonto]), [mDatum]
HAVING (((Round(Sum(IIf([IznosE]>0,[IznosE]*[NBS],[IznosD])),2))>0));


=====QUERY=====
Query46
-----SQL-----
INSERT INTO GK ( BR_NALOG, KONTO, DATUM, PIZNOS, TIP_STAVKE, DOK, lnkSkupstinaID, lnkKUPACID, PARAMETRI )
SELECT 5280 AS mNalog, '4330' & Format(IIf([DobavljacKonto]=0,9917,[DobavljacKonto]),'0000') AS mKonto, #1/1/2014# AS mDatum, Round(Sum(IIf([IznosE]>0,[IznosE]*[NBS],[IznosD])),2) AS Ukupno, 4 AS mTip, 'UF' & 114 AS mDOK, '27' AS mSK, Format(IIf([DobavljacKonto]=0,9917,[DobavljacKonto]),'0000') AS Konto, 114 AS mPar
FROM Troskovi_Racuni INNER JOIN GrupaRacuna ON Troskovi_Racuni.MesecRacuna=GrupaRacuna.GrupaRacunaFXN
WHERE (((GrupaRacuna.IDGrupaRacuna)=111))
GROUP BY Troskovi_Racuni.RacunNO, 1, IIf([DobavljacKonto]=0,9917,[DobavljacKonto]), [mDatum]
HAVING (((Sum(IIf([IznosE]>0,[IznosE]*[NBS],[IznosD])))>0));


=====QUERY=====
Query47
-----SQL-----
SELECT Count(GK.STAVKAID) AS CountOfSTAVKAID, Nalog.Br_Nalog
FROM GK RIGHT JOIN Nalog ON GK.BR_NALOG = Nalog.Br_Nalog
GROUP BY Nalog.Br_Nalog
HAVING (((Count(GK.STAVKAID))=0));


=====QUERY=====
Query48
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA] )
SELECT 116 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovor_FLT.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovor_FLT.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis AS Expr1, Troskovi_VP.Sort, Troskovi_VP.AddTxt, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],'') AS Expr2, Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Ugovor_FLT.SumaD
FROM (Troskovi_Ugovor_FLT RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip=Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip=TipObjekta.IDTipObj) ON Troskovi_Ugovor_FLT.lnk_TV=Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovor_FLT.lnk_ID_Dob=Kupac.ID_K
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query49
-----SQL-----
SELECT RacunStavke.Ukupno
FROM RacunStavke
WHERE (((RacunStavke.lnkGR)=116) AND ((RacunStavke.lnkTVP)=56));


=====QUERY=====
Query5
-----SQL-----
SELECT OpomenaStavke.lnkKupacID, Sum(OpomenaStavke.Di) AS SumOfDi, Sum(OpomenaStavke.Pi) AS SumOfPi, Sum(OpomenaStavke.Suma) AS SumOfSuma, Opomena.Dug, Opomena.BNR
FROM OpomenaStavke INNER JOIN Opomena ON OpomenaStavke.lnkOpomena = Opomena.IDOpomena
GROUP BY OpomenaStavke.lnkKupacID, Opomena.Dug, Opomena.BNR;


=====QUERY=====
Query50
-----SQL-----
SELECT Sum(Objekti.kolicina) AS SUMKV, Sum([kolicina]*[K1]) AS KVK1, Sum([kolicina]*[K2]) AS KVK2, Sum([kolicina]*[K3]) AS KVK3, Sum(Objekti.BRGM) AS SUMABRGM
FROM Objekti;


=====QUERY=====
Query51
-----SQL-----
UPDATE RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna SET RacunStavke.NBS = GrupaRacuna.NBS, RacunStavke.CenaE = IIf([TipObracuna]=1,[MAX-SUMA]/[MAX-KV]*[KV], IIf([TipObracuna]=2,[MAX-SUMA]/[MAX-KOEF]*[KOEF], IIf([TipObracuna]=3,[MAX-SUMA]/[MAX-KV-K1]*[K1]*[KV], IIf([TipObracuna]=4,[MAX-SUMA]/[MAX-KV-K2]*[K2]*[KV], IIf([TipObracuna]=5,[MAX-SUMA]/[MAX-KV-K3]*[K3]*[KV], IIf([TipObracuna]=6,[MAX-SUMA]/[MAX-BRGM]*[BRGM], 0)))))), RacunStavke.Ukupno = GrupaRacuna.NBS*(IIf([TipObracuna]=1,[MAX-SUMA]/[MAX-KV]*[KV], IIf([TipObracuna]=2,[MAX-SUMA]/[MAX-KOEF]*[KOEF], IIf([TipObracuna]=3,[MAX-SUMA]/[MAX-KV-K1]*[K1]*[KV], IIf([TipObracuna]=3,[MAX-SUMA]/[MAX-KV-K2]*[K2]*[KV], IIf([TipObracuna]=3,[MAX-SUMA]/[MAX-KV-K3]*[K3]*[KV], IIf([TipObracuna]=3,[MAX-SUMA]/[MAX-BRGM]*[BRGM],0))))))), RacunStavke.Naziv = IIf([AddTXT]=2,[NAZIV] & ', 1/' & [MAX-CN] & ', KOEFICIJENT ' & Format([koef],'#,##0.00'), IIf([AddTXT]=1,[NAZIV] & ', ' & [KV] & '/' & [MAX-KV-SVE] & ' m2', IIf([AddTXT]=4,[NAZIV] & ', 1/' & [MAX-CN], IIf([AddTXT]=7,[NAZIV] & ', ' & [BRGM] & '/' & [MAX-BRGM], IIf([AddTXT]=5,[NAZIV] & ', KOEFICIJENT ' & Format([koef],'#,##0.00') &' /' & format([DSP-SUMA],'#,##0.00'), IIf([AddTXT]=6,[Naziv] & ' /' & format([DSP-SUMA],'#,##0.00'), IIf([AddTXT]=3,[Naziv] & iif(isNull([DobAlias]),' ', ', ' & [DobAlias]) &' /' & format([DSP-SUMA],'#,##0.00'), [Naziv])))))))
WHERE (((GrupaRacuna.IDGrupaRacuna)=134));


=====QUERY=====
Query52
-----SQL-----
SELECT RacunStavke.*
FROM RacunStavke
WHERE (((RacunStavke.lnkGR)=207) AND ((RacunStavke.ID_K)=7123))
ORDER BY RacunStavke.Sort;


=====QUERY=====
Query53
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_O, ID_SK, KV, [MAX-CN], [MAX-KV], [MAX-SUMA], [MAX-KOEF], KOEF, K1, K2, K3, BRGM, Bold, TipObracuna, ToDo, Naziv, Sort, AddTXT, Grupa_VP, VP, lnkTVP, DobAlias, [DSP-SUMA], [MAX-KV-SVE], [MAX-BRGM], [MAX-KV-K1], [MAX-KV-K2], [MAX-KV-K3] )
SELECT 207 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, Objekti.kolicina, Objekat_Sume.[MAX-CN], Objekat_Sume.[MAX-KV], Troskovi_Ugovor_FLT.Vrednost, Objekat_Sume.[MAX-KO], Objekti.Koeficijent, Objekti.K1, Objekti.K2, Objekti.K3, Objekti.BRGM, Len([Troskovi_Vrste])=1 AS IsBoldFont, Troskovi_Ugovor_FLT.TipObracuna, Troskovi_VP.ToDo, Troskovi_VP.Troskovi_Vrste & '  ' & Troskovi_VP.Troskovi_Vrste_Opis AS Expr1, Troskovi_VP.Sort, Troskovi_VP.AddTXT, Troskovi_VP.Grupa_VP, IIf(IsNumeric([Troskovi_Vrste]),[Troskovi_Vrste],'') AS Expr2, Troskovi_VP.ID_Troskovi_Vrste, Kupac.Alias, Troskovi_Ugovor_FLT.SumaD, Objekat_Sume_Sve.[MAX-KV-SVE], Objekat_Sume.[MAX-BRGM], Objekat_Sume.[MAX-KV-K1], Objekat_Sume.[MAX-KV-K2], Objekat_Sume.[MAX-KV-K3]
FROM ((Troskovi_Ugovor_FLT RIGHT JOIN (Objekat_Sume INNER JOIN ((Objekti INNER JOIN Troskovi_VP ON Objekti.lnk_tip = Troskovi_VP.TipObjektaRep) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) ON Objekat_Sume.lnk_tip = TipObjekta.IDTipObj) ON Troskovi_Ugovor_FLT.lnk_TV = Troskovi_VP.ID_Troskovi_Vrste) LEFT JOIN Kupac ON Troskovi_Ugovor_FLT.lnk_ID_Dob = Kupac.ID_K) INNER JOIN Objekat_Sume_Sve ON Objekti.lnkSkupstinaID = Objekat_Sume_Sve.lnkSkupstinaID
WHERE (((Objekti.Status)=0))
ORDER BY TipObjekta.SortObj, Objekti.naziv, Troskovi_VP.Troskovi_Vrste;


=====QUERY=====
Query54
-----SQL-----
SELECT Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, Objekti.naziv, Objekti.lnk_ID_K
FROM (Dobavljac_Racuni INNER JOIN Dobavljaci_Racun_TipObjekta ON Dobavljac_Racuni.IDTRRAC = Dobavljaci_Racun_TipObjekta.RacunID) INNER JOIN Objekti ON (Dobavljac_Racuni.SK_ID = Objekti.lnkSkupstinaID) AND (Dobavljaci_Racun_TipObjekta.TipObjekta = Objekti.lnk_tip)
WHERE (((Dobavljac_Racuni.SK_ID)=101));


=====QUERY=====
Query55
-----SQL-----
INSERT INTO Racun ( lnkGR, ID_K, Kupac, Naziv_Slanja, PBroj_K, Adresa_K, PIB_Naziv )
SELECT [linkGrupeRacuna] AS IDGRP, Kupac.ID_K, Kupac.Naziv, IIf(IsNull([Naziv_Slanja]),[kUPAC].[NAZIV],[Naziv_Slanja]) AS NazivSlanja, Kupac.PBroj, Kupac.Adresa, Kupac.PIB
FROM Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K
GROUP BY [linkGrupeRacuna], Kupac.ID_K, Kupac.Naziv, IIf(IsNull([Naziv_Slanja]),[kUPAC].[NAZIV],[Naziv_Slanja]), Kupac.PBroj, Kupac.Adresa, Objekti.lnkSkupstinaID, Kupac.PIB;


=====QUERY=====
Query56
-----SQL-----
SELECT TipObracuna.KOLICINA, TipObracuna.IZNOS, Objekat_Sume_Sve.*, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.IznosPoKoefRSD, RacunStavke.*, RacunStavke.IDRacunStavke
FROM ((RacunStavke INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Objekat_Sume_Sve ON RacunStavke.ID_SK = Objekat_Sume_Sve.ID_SK) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr
WHERE (((RacunStavke.lnkGR)=216));


=====QUERY=====
Query57
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Dobavljaci_Racun_TipObjekta.TipObjekta
FROM GrupaRacuna INNER JOIN ((Kupac INNER JOIN (Dobavljaci_Racun_TipObjekta INNER JOIN Dobavljac_Racuni ON Dobavljaci_Racun_TipObjekta.RacunID = Dobavljac_Racuni.IDTRRAC) ON Kupac.ID_K = Dobavljac_Racuni.DobavljacKonto) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) ON GrupaRacuna.GrupaRacunaFXT = Dobavljac_Racuni.MesecRacuna
GROUP BY GrupaRacuna.IDGrupaRacuna, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.SortMarkerYYYYMM, Dobavljaci_Racun_TipObjekta.TipObjekta
HAVING (((GrupaRacuna.IDGrupaRacuna)=216))
ORDER BY Dobavljac_Racuni.SortMarkerYYYYMM;


=====QUERY=====
Query58
-----SQL-----
SELECT Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.NazivRacuna, TipObracuna.*
FROM Dobavljac_Racuni INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr;


=====QUERY=====
Query59
-----SQL-----
SELECT 
FROM RacunStavke INNER JOIN Racun ON (RacunStavke.ID_SK = Racun.ID_SK) AND (RacunStavke.ID_K = Racun.ID_K) AND (RacunStavke.lnkGR = Racun.lnkGR);


=====QUERY=====
Query6
-----SQL-----
INSERT INTO RacunStavke ( Objekat, lnkGR, ID_K, ID_O, ID_SK, KOL, JM, JO, Sort )
SELECT 'Investiciono održavanje' AS mIO, 14 AS GRPRC, Objekti.lnk_ID_K, Objekti.ID_O, Objekti.lnkSkupstinaID, 1 AS mKol, 'kom' AS mJM, Objekti.IO, TipObjekta.SortObj
FROM (Objekti LEFT JOIN Kupac ON Objekti.lnk_ID_K=Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj
WHERE (((Objekti.IO)>0) AND ((Objekti.Status)=0));


=====QUERY=====
Query60
-----SQL-----
INSERT INTO Racun ( lnkGR, ID_K, ID_SK, DatumIzdavanja, DatumUsluge, DatumValute, MestoIzdavanja, Kupac, PBroj_K, Adresa_K, PIB, PrethodniDug, Ukupno )
SELECT RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Nz([Suma],0) AS PD, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD
FROM ((RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Kupac ON RacunStavke.ID_K = Kupac.ID_K) LEFT JOIN Kupac_DUG ON RacunStavke.ID_K = Kupac_DUG.lnkKUPACID
GROUP BY RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Kupac_DUG.Suma
HAVING (((RacunStavke.lnkGR)=216) AND ((Sum(RacunStavke.UkupnoRSD))>0));


=====QUERY=====
Query61
-----SQL-----
UPDATE (Racun INNER JOIN RacunStavke ON (Racun.ID_SK = RacunStavke.ID_SK) AND (Racun.ID_K = RacunStavke.ID_K) AND (Racun.lnkGR = RacunStavke.lnkGR)) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna SET RacunStavke.ID_R = RACUN.IDRacun, Racun.PozivNaBroj = KontrolniBroj(97,[ID_SK] & '-' & Format([ID_K]) & '-' & [GrupaRacunaFXT]) & '-' & [ID_SK] & '-' & Format([ID_K]) & '-' & [GrupaRacunaFXT]
WHERE (((Racun.lnkGR)=216) AND ((Racun.ID_SK)=101));


=====QUERY=====
Query62
-----SQL-----
UPDATE Racun INNER JOIN [kupac-objekti-cr] ON Racun.ID_K = [kupac-objekti-cr].ID_K SET Racun.SvrhaUplate2 = [Objekti];


=====QUERY=====
Query63
-----SQL-----
UPDATE Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna SET Racun.PozivNaBroj = KontrolniBroj(97,[ID_SK] & '-' & Format([ID_K]) & '-' & [GrupaRacunaFXT]) & '-' & [RACUN].[ID_SK] & '-' & Format([ID_K]) & '-' & [GrupaRacunaFXT]
WHERE (((Racun.lnkGR)=216) AND ((Racun.ID_SK)=101));


=====QUERY=====
Query64
-----SQL-----
INSERT INTO GK ( BR_NALOG, Konto, DATUM, DIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, DPO, PARAMETRI, RDOB, RACID, SIFRAKN, PRIORITET, SIFRAKONTA )
SELECT 14 AS Nalog, '2040' & [Racun].[ID_K] AS KONTO, Racun.DatumIzdavanja, RacunStavke.UkupnoRSD, 3 AS TIP, Racun.ID_SK, Racun.ID_K, 'IF' & [PozivNaBroj] AS DOK, Racun.DatumValute, Replace([PozivNaBroj],'-','') AS PB, RacunStavke.ID_RDOB, Racun.IDRacun, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.PrioritetNaplate, Racun.ID_K
FROM (Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R) INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC
WHERE (((Racun.ID_SK)=101) AND ((Racun.lnkGR)=216));


=====QUERY=====
Query65
-----SQL-----
INSERT INTO GK ( BR_NALOG, Konto, DATUM, PIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, PARAMETRI, RDOB, RACID, SIFRAKN, SIFRAKONTA )
SELECT 14 AS Nalog, '4350' & [RACUNSTAVKE].[DobavljacKonto] AS KONTO, Racun.DatumIzdavanja, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, 3 AS TIP, Racun.ID_SK, RacunStavke.DobavljacKonto, [SifraKN] & '-' & [MesecRacuna] AS DOK, [SifraKN] & '-' & [MesecRacuna] AS PB, RacunStavke.ID_RDOB, RacunStavke.ID_RDOB, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.DobavljacKonto
FROM (Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R) INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC
GROUP BY 14, '4350' & [RACUNSTAVKE].[DobavljacKonto], Racun.DatumIzdavanja, 3, Racun.ID_SK, RacunStavke.DobavljacKonto, [SifraKN] & '-' & [MesecRacuna], [SifraKN] & '-' & [MesecRacuna], RacunStavke.ID_RDOB, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.DobavljacKonto, RacunStavke.ID_RDOB, Racun.lnkGR
HAVING (((Sum(RacunStavke.UkupnoRSD))<>0) AND ((Racun.ID_SK)=101) AND ((Racun.lnkGR)=216));


=====QUERY=====
Query66
-----SQL-----
SELECT Sum([DIZNOS]-[PIZNOS]) AS STANJE, GK.PARAMETRI
FROM GK
GROUP BY GK.PARAMETRI, GK.lnkKUPACID
HAVING (((Sum([DIZNOS]-[PIZNOS]))<>0) AND ((GK.lnkKUPACID)=1003))
ORDER BY First(GK.DATUM);


=====QUERY=====
Query67
-----SQL-----
SELECT Sum([DIZNOS]-[PIZNOS]) AS STANJE, GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI
FROM GK
GROUP BY GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI
HAVING (((GK.PARAMETRI)='9810110031901'))
ORDER BY GK.PRIORITET, GK.SIFRAKN;


=====QUERY=====
Query68
-----SQL-----
INSERT INTO Dobavljac_Racuni ( TMPprevID, SK_ID, RacunNO, NazivRacuna, Napomena, Dobavljac, DobavljacKonto, TipObracuna, MesecRacuna, IznosRacunaEUR, IznosRacunaRSD, IznosPoKoefEUR, IznosPoKoefRSD, PrioritetNaplate, SifraKN )
SELECT Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.Napomena, Dobavljac_Racuni.Dobavljac, Dobavljac_Racuni.DobavljacKonto, Dobavljac_Racuni.TipObracuna, '1902' AS MR, Dobavljac_Racuni.IznosRacunaEUR, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.IznosPoKoefEUR, Dobavljac_Racuni.IznosPoKoefRSD, Dobavljac_Racuni.PrioritetNaplate, Dobavljac_Racuni.SifraKN
FROM Dobavljac_Racuni
WHERE (((Dobavljac_Racuni.MesecRacuna)='1901'));


=====QUERY=====
Query69
-----SQL-----
INSERT INTO Dobavljaci_Racun_TipObjekta ( RacunID, TipObjekta )
SELECT Dobavljac_Racuni.IDTRRAC, Dobavljaci_Racun_TipObjekta.TipObjekta
FROM Dobavljac_Racuni INNER JOIN Dobavljaci_Racun_TipObjekta ON Dobavljac_Racuni.TMPprevID = Dobavljaci_Racun_TipObjekta.RacunID
WHERE (((Dobavljac_Racuni.MesecRacuna)='1902'));


=====QUERY=====
Query7
-----SQL-----
SELECT Objekti.*, Objekti.IO
FROM Objekti
WHERE (((Objekti.ID_O)=1));


=====QUERY=====
Query70
-----SQL-----
SELECT Count(PrinterBinLOCAL.IDlocalPrinterbin) AS CountOfIDlocalPrinterbin
FROM PrinterBinLOCAL
WHERE (((PrinterBinLOCAL.TypeIndex)=1));


=====QUERY=====
Query71
-----SQL-----
SELECT Sum([DIZNOS]-[PIZNOS]) AS STANJE, GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI
FROM GK
GROUP BY GK.PRIORITET, GK.SIFRAKN, GK.PARAMETRI
HAVING (((Sum([DIZNOS]-[PIZNOS]))<>0) AND ((GK.PARAMETRI)='9810110031901'))
ORDER BY GK.PRIORITET, GK.SIFRAKN;


=====QUERY=====
Query72
-----SQL-----
SELECT GK.BR_NALOG, IzvodStavke.ID, IzvodStavke.IzvodLNKID, (([Odobrenje]-[Zaduzenje])-(Sum([PIZNOS]-[DIZNOS])))=0 AS Kontrola, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje
FROM GK RIGHT JOIN IzvodStavke ON GK.lnkIzvodStavkaID = IzvodStavke.ID
GROUP BY GK.BR_NALOG, IzvodStavke.ID, IzvodStavke.IzvodLNKID, IzvodStavke.Zaduzenje, IzvodStavke.Odobrenje
HAVING (((IzvodStavke.IzvodLNKID)=1) AND (((([Odobrenje]-[Zaduzenje])-(Sum([PIZNOS]-[DIZNOS])))=0) Is Null Or ((([Odobrenje]-[Zaduzenje])-(Sum([PIZNOS]-[DIZNOS])))=0)=0));


=====QUERY=====
Query73
-----SQL-----
SELECT tblIzvestaj.*, tblIzvestajSub.*
FROM tblIzvestaj INNER JOIN tblIzvestajSub ON tblIzvestaj.ID = tblIzvestajSub.ID_I
WHERE (((tblIzvestaj.ID)=6));


=====QUERY=====
Query74
-----SQL-----
UPDATE GK SET GK.KNzaTIP = 0
WHERE (((GK.KNzaTIP) Is Null));


=====QUERY=====
Query75
-----SQL-----
INSERT INTO Racun ( lnkGR, ID_K, ID_SK, DatumIzdavanja, DatumUsluge, DatumValute, DatumPrometa, MestoIzdavanja, Kupac, PBroj_K, Adresa_K, PIB, PrethodniDug, PD_iznos, Ukupno )
SELECT RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Nz([Suma0],0) AS PDD, Nz([Suma98],0) AS PDN, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD
FROM ((RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Kupac ON RacunStavke.ID_K = Kupac.ID_K) LEFT JOIN Kupac_DUG ON RacunStavke.ID_K = Kupac_DUG.lnkKUPACID
GROUP BY RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.DatumPrometa, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Nz([Suma0],0), Nz([Suma98],0)
HAVING (((RacunStavke.lnkGR)=208) AND ((RacunStavke.ID_SK)=126) AND ((Sum(RacunStavke.UkupnoRSD))>0));


=====QUERY=====
Query76
-----SQL-----
SELECT Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna
FROM ((Dobavljac_Racuni INNER JOIN Skustina ON Dobavljac_Racuni.SK_ID = Skustina.IDSkupstina) INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr;


=====QUERY=====
Query77
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_SK, K1, K2, K3, K4, K5, Sort, Naziv, TipObracuna, DobavljacKonto, ID_RDOB )
SELECT GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC
FROM GrupaRacuna INNER JOIN ((Objekti_Za_Racun INNER JOIN (Kupac INNER JOIN (Dobavljaci_Racun_TipObjekta INNER JOIN Dobavljac_Racuni ON Dobavljaci_Racun_TipObjekta.RacunID = Dobavljac_Racuni.IDTRRAC) ON Kupac.ID_K = Dobavljac_Racuni.DobavljacKonto) ON (Objekti_Za_Racun.lnk_tip = Dobavljaci_Racun_TipObjekta.TipObjekta) AND (Dobavljac_Racuni.SK_ID = Objekti_Za_Racun.lnkSkupstinaID)) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) ON GrupaRacuna.GrupaRacunaFXN = Dobavljac_Racuni.MesecRacuna
GROUP BY GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna
HAVING (((GrupaRacuna.IDGrupaRacuna) = 597) AND ((Objekti_Za_Racun.lnkSkupstinaID)=118))
ORDER BY Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna;


=====QUERY=====
Query78
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_SK, K1, K2, K3, K4, K5, Sort, Naziv, TipObracuna, DobavljacKonto, ID_RDOB, K1xK2, K2xK3, K2xK4, K2xK5 )
SELECT GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.K21, Objekti_Za_Racun.K23, Objekti_Za_Racun.K24, Objekti_Za_Racun.K25
FROM GrupaRacuna INNER JOIN ((Objekti_Za_Racun INNER JOIN (Kupac INNER JOIN (Dobavljaci_Racun_TipObjekta INNER JOIN Dobavljac_Racuni ON Dobavljaci_Racun_TipObjekta.RacunID = Dobavljac_Racuni.IDTRRAC) ON Kupac.ID_K = Dobavljac_Racuni.DobavljacKonto) ON (Objekti_Za_Racun.lnk_tip = Dobavljaci_Racun_TipObjekta.TipObjekta) AND (Objekti_Za_Racun.lnkSkupstinaID = Dobavljac_Racuni.SK_ID)) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) ON GrupaRacuna.GrupaRacunaFXN = Dobavljac_Racuni.MesecRacuna
GROUP BY GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna, Objekti_Za_Racun.K21, Objekti_Za_Racun.K23, Objekti_Za_Racun.K24, Objekti_Za_Racun.K25
HAVING (((GrupaRacuna.IDGrupaRacuna)=597) AND ((Objekti_Za_Racun.lnkSkupstinaID)=118))
ORDER BY Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna;


=====QUERY=====
Query79
-----SQL-----
UPDATE (Racun INNER JOIN RacunStavke ON (Racun.ID_SK = RacunStavke.ID_SK) AND (Racun.ID_K = RacunStavke.ID_K) AND (Racun.lnkGR = RacunStavke.lnkGR)) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna SET RacunStavke.ID_R = RACUN.IDRacun, Racun.RBR = [RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN] & '-' & [MarkerVanderdnihRacuna], Racun.PozivNaBroj = KontrolniBroj(97,[RACUN].[ID_SK] & '-' & Format([RACUN].[ID_K]) & '-' & [GrupaRacunaFXN] & '-' & [MarkerVanderdnihRacuna]) & '-' & [RACUN].[ID_SK] & '-' & [RACUN].[ID_K] & '-' & [GrupaRacunaFXN] & '-' & [MarkerVanderdnihRacuna]
WHERE (((Racun.lnkGR)=724) AND ((Racun.ID_SK)=104));


=====QUERY=====
Query8
-----SQL-----
INSERT INTO RacunStavke ( Objekat, lnkGR, ID_K, ID_SK, KOL, JM, JO, Sort )
SELECT 'Rezervni fond' AS mRF, " & linkGrupeRacuna & " AS GRPRC, Objekti.lnk_ID_K, Objekti.lnkSkupstinaID, Count(Objekti.ID_O) AS CountOfID_O, 'kom' AS mJM, [RF_DIN]/[NBSKURS] AS EURO, 10 AS mSort
FROM (Objekti LEFT JOIN Kupac ON Objekti.lnk_ID_K=Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj
GROUP BY 'Rezervni fond', " & linkGrupeRacuna & ", Objekti.lnk_ID_K, Objekti.lnkSkupstinaID, 'kom', [RF_DIN]/[NBSKURS], 10, Objekti.Status
HAVING ((([RF_DIN]/[NBSKURS])>0) AND ((Objekti.Status)=0));


=====QUERY=====
Query80
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_SK, K1, K2, K3, K4, K5, Sort, Naziv, TipObracuna, DobavljacKonto, ID_RDOB, K1xK2, K2xK3, K2xK4, K2xK5 )
SELECT GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.K21, Objekti_Za_Racun.K23, Objekti_Za_Racun.K24, Objekti_Za_Racun.K25
FROM GrupaRacuna INNER JOIN ((Objekti_Za_Racun INNER JOIN (Kupac INNER JOIN (Dobavljaci_Racun_TipObjekta INNER JOIN Dobavljac_Racuni ON Dobavljaci_Racun_TipObjekta.RacunID = Dobavljac_Racuni.IDTRRAC) ON Kupac.ID_K = Dobavljac_Racuni.DobavljacKonto) ON (Objekti_Za_Racun.lnk_tip = Dobavljaci_Racun_TipObjekta.TipObjekta) AND (Objekti_Za_Racun.lnkSkupstinaID = Dobavljac_Racuni.SK_ID)) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) ON GrupaRacuna.GrupaRacunaFXN = Dobavljac_Racuni.MesecRacuna
WHERE (((Dobavljac_Racuni.MarkerVandrednogRacuna) Is Null))
GROUP BY GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.RacunNO, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.K21, Objekti_Za_Racun.K23, Objekti_Za_Racun.K24, Objekti_Za_Racun.K25, Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna
HAVING (((GrupaRacuna.IDGrupaRacuna)=733) AND ((Objekti_Za_Racun.lnkSkupstinaID)=104))
ORDER BY Objekti_Za_Racun.SortObj, Dobavljac_Racuni.MesecRacuna;


=====QUERY=====
Query81
-----SQL-----
SELECT GrupaRacuna.*, GrupaRacuna.NalogKN
FROM GrupaRacuna
WHERE (((GrupaRacuna.IDGrupaRacuna)=666));


=====QUERY=====
Query82
-----SQL-----
SELECT Kupac.lnk_ID_SK
FROM Kupac
WHERE (((Kupac.lnk_ID_SK)=22)) OR (((Kupac.lnk_ID_SK)=0)) OR (((Kupac.lnk_ID_SK) Is Null));


=====QUERY=====
Query83
-----SQL-----
SELECT Dobavljac_Racuni.*, Kupac.KONTO, Dobavljac_Racuni.RBR, Dobavljac_Racuni.DatumKnjizenja
FROM Dobavljac_Racuni INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K;


=====QUERY=====
Query84
-----SQL-----
SELECT GK.PARAMETRI, GK.KontoTroska, GK.KONTO, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.BR_NALOG, GK.DATUM
FROM GK
WHERE (((GK.lnkSkupstinaID)=101))
GROUP BY GK.PARAMETRI, GK.KontoTroska, GK.KONTO, GK.BR_NALOG, GK.DATUM
HAVING (((GK.PARAMETRI) Is Not Null) AND ((GK.KontoTroska) Is Null) AND ((GK.KONTO) Like "2040*"));


=====QUERY=====
Query85
-----SQL-----
SELECT Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.KontoKnjizenja, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, Sum(GK_1.DIZNOS) AS SumOfDIZNOS1, Sum(GK_1.PIZNOS) AS SumOfPIZNOS1, Sum(GK_FILTER_DOBAVLJACI.SumOfDIZNOS) AS SumOfSumOfDIZNOS, Sum(GK_FILTER_DOBAVLJACI.SumOfPIZNOS) AS SumOfSumOfPIZNOS, GK_FILTER_DOBAVLJACI.KontoTroska
FROM (GK AS GK_1 INNER JOIN (GK INNER JOIN Dobavljac_Racuni ON (Dobavljac_Racuni.SK_ID = GK.lnkSkupstinaID) AND (GK.RDOB = Dobavljac_Racuni.IDTRRAC)) ON (GK_1.RDOB = Dobavljac_Racuni.IDTRRAC) AND (GK_1.lnkSkupstinaID = Dobavljac_Racuni.SK_ID)) INNER JOIN GK_FILTER_DOBAVLJACI ON (Dobavljac_Racuni.IDTRRAC = GK_FILTER_DOBAVLJACI.RDOB) AND (Dobavljac_Racuni.SK_ID = GK_FILTER_DOBAVLJACI.lnkSkupstinaID)
WHERE (((GK.KONTO) Like "2040*") AND ((GK_1.KONTO) Like "4350*"))
GROUP BY Dobavljac_Racuni.SK_ID, Dobavljac_Racuni.KontoKnjizenja, GK_FILTER_DOBAVLJACI.KontoTroska
HAVING (((Dobavljac_Racuni.SK_ID)=101));


=====QUERY=====
Query86
-----SQL-----
SELECT Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.lnkSkupstinaID, GK.KontoTroska
FROM GK
WHERE (((GK.TIP_STAVKE)=99) AND ((GK.KONTO) Like "2040*")) OR (((GK.KNzaTIP)=99) AND ((GK.KONTO) Like "2040*"))
GROUP BY GK.lnkSkupstinaID, GK.KontoTroska
HAVING (((GK.lnkSkupstinaID)=101)) OR (((GK.lnkSkupstinaID)=101));


=====QUERY=====
Query87
-----SQL-----
INSERT INTO GK ( BR_NALOG, Konto, DATUM, PIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, PARAMETRI, RDOB, SIFRAKN, SIFRAKONTA, KontoTroska, RacunIN_ID )
SELECT 5550 AS Nalog, [KUPAC].[KONTO] AS KONTO, Racun.DatumIzdavanja, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD, 3 AS TIP, Racun.ID_SK, RacunStavke.DobavljacKonto, 'R-' & [MesecRacuna] AS DOK, Dobavljac_Racuni.PozivNaBroj, RacunStavke.ID_RDOB, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.DobavljacKonto, Dobavljac_Racuni.KontoKnjizenja, Dobavljac_Racuni.IDTRRAC
FROM ((Racun INNER JOIN RacunStavke ON Racun.IDRacun = RacunStavke.ID_R) INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K
GROUP BY 5550, [KUPAC].[KONTO], Racun.DatumIzdavanja, 3, Racun.ID_SK, RacunStavke.DobavljacKonto, 'R-' & [MesecRacuna], Dobavljac_Racuni.PozivNaBroj, RacunStavke.ID_RDOB, Dobavljac_Racuni.SifraKN, Dobavljac_Racuni.DobavljacKonto, Racun.lnkGR, Dobavljac_Racuni.KontoKnjizenja, Dobavljac_Racuni.IDTRRAC
HAVING (((Sum(RacunStavke.UkupnoRSD))<>0) AND ((Racun.ID_SK)=152) AND ((Racun.lnkGR)=786));


=====QUERY=====
Query88
-----SQL-----
UPDATE GK SET GK.KONTO = Left([KONTO],4);


=====QUERY=====
Query89
-----SQL-----
SELECT '' AS STAVKAID, '' AS BR_NALOG, '' AS DATUM, Sum(GK.PIZNOS) AS PI, Sum(GK.DIZNOS) AS DI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, IIf([KNZATIP]>0,[KNZATIP],[TIP_STAVKE]) AS TS, GK.SIFRAKONTA, GK.SIFRAKN, GK.RDOB, GK.RACID, GK.RacunIN_ID, GK.KontoTroska, GK.lnkKUPACID, GK.lnkSkupstinaID, GK.DOK, GK.PARAMETRI, GK.PRIORITET
FROM GK
GROUP BY IIf([KNZATIP]>0,[KNZATIP],[TIP_STAVKE]), GK.SIFRAKONTA, GK.SIFRAKN, GK.RDOB, GK.RACID, GK.RacunIN_ID, GK.KontoTroska, GK.lnkKUPACID, GK.lnkSkupstinaID, GK.DOK, GK.PARAMETRI, GK.PRIORITET
HAVING (((GK.lnkKUPACID)=1006) AND ((GK.lnkSkupstinaID)=101) AND ((GK.DOK) Like 'R-1912'))
ORDER BY GK.DOK, GK.PRIORITET;


=====QUERY=====
Query9
-----SQL-----
INSERT INTO RacunStavke ( Objekat, lnkGR, ID_K, ID_SK, KOL, JM, JO, Sort )
SELECT 'Rezervni fond' AS mRF, 14 AS GRPRC, Objekti.lnk_ID_K, Objekti.lnkSkupstinaID, Count(Objekti.ID_O) AS mKol, 'kom' AS mJM, [RF_DIN]/113.2836 AS EURO, 10 AS mSort
FROM (Objekti LEFT JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
GROUP BY 'Rezervni fond', 14, Objekti.lnk_ID_K, Objekti.lnkSkupstinaID, 'kom', [RF_DIN]/113.2836, 10, Objekti.Status, Objekti.RF_DIN
HAVING (((Objekti.Status)=0) AND ((Objekti.RF_DIN)>0) AND ((Objekti.lnk_ID_K)=1102));


=====QUERY=====
Query90
-----SQL-----
SELECT Dobavljac_Racuni.IDTRRAC, Dobavljac_Racuni.RacunNO AS Rno, Dobavljac_Racuni.NazivRacuna, Kupac.Naziv AS Dobavljac, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.IznosPoKoefRSD, Dobavljac_Racuni.PrioritetNaplate, TipObracuna.TipObracuna, Dobavljac_Racuni.MarkerVandrednogRacuna AS MVR
FROM ((Dobavljac_Racuni INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) INNER JOIN TipRacunaDobavljac ON Dobavljac_Racuni.TipDokumenta = TipRacunaDobavljac.Index
WHERE (((Dobavljac_Racuni.MarkerVandrednogRacuna)=Forms!Troskovi_Racuni!cmbRV) And ((Dobavljac_Racuni.MesecRacuna)=Forms!Troskovi_Racuni!cmbFilterMY) And ((Dobavljac_Racuni.SK_ID)=Forms!Troskovi_Racuni!cmbSZ) And ((TipRacunaDobavljac.Cat1)=1))
ORDER BY Dobavljac_Racuni.RacunNO;


=====QUERY=====
Query91
-----SQL-----
SELECT Dobavljac_Racuni.MarkerVandrednogRacuna
FROM Dobavljac_Racuni
WHERE (((Dobavljac_Racuni.MarkerVandrednogRacuna) Is Not Null));


=====QUERY=====
Query92
-----SQL-----
SELECT GK.lnkKUPACID, Kupac.Naziv, Sum(GK.DIZNOS) AS SumOfDIZNOS, Sum(([DIZNOS]-[PIZNOS])) AS Suma, GK.DOK, GK.RDOB, GK.KontoTroska
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K
GROUP BY GK.lnkKUPACID, Kupac.Naziv, GK.DOK, GK.RDOB, GK.KontoTroska;


=====QUERY=====
Query93
-----SQL-----
SELECT GK.DOK, Dobavljac_Racuni.MesecRacuna, GrupaRacuna.GrupaRacunaFXN, [GrupaRacunaFXN]<>[MesecRacuna] AS Expr1, GK.lnkSkupstinaID, GK.*
FROM ((GK INNER JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN Racun ON GK.RACID = Racun.IDRacun) INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
WHERE ((([GrupaRacunaFXN]<>[MesecRacuna])=-1));


=====QUERY=====
Query94
-----SQL-----
SELECT GK.lnkKUPACID, GK.lnkSkupstinaID, Sum([DIZNOS]-[PIZNOS]) AS Saldo, GK.PARAMETRI, GK.KONTO, GK.DOK
FROM GK
GROUP BY GK.lnkKUPACID, GK.lnkSkupstinaID, GK.PARAMETRI, GK.KONTO, GK.DOK
HAVING (((GK.lnkSkupstinaID)=101) AND ((GK.KONTO)="2040"));


=====QUERY=====
Query95
-----SQL-----
SELECT '' AS STAVKAID, '' AS BR_NALOG, '' AS DATUM, Sum(GK.PIZNOS) AS PI, Sum(GK.DIZNOS) AS DI, Round(Sum([DIZNOS]-[PIZNOS]),2) AS SPD, IIf([KNZATIP]>0,[KNZATIP],[TIP_STAVKE]) AS TS, GK.SIFRAKONTA, GK.SIFRAKN, GK.RDOB, GK.RACID, GK.RacunIN_ID, GK.KontoTroska, GK.lnkKUPACID, GK.lnkSkupstinaID, GK.DOK, GK.PARAMETRI, GK.PRIORITET
FROM GK
GROUP BY IIf([KNZATIP]>0,[KNZATIP],[TIP_STAVKE]), GK.SIFRAKONTA, GK.SIFRAKN, GK.RDOB, GK.RACID, GK.RacunIN_ID, GK.KontoTroska, GK.lnkKUPACID, GK.lnkSkupstinaID, GK.DOK, GK.PARAMETRI, GK.PRIORITET
HAVING (((GK.DOK) Like 'POCETNO STANJE 2019') And ((GK.lnkKUPACID)=1001) And ((GK.lnkSkupstinaID)=101))
ORDER BY GK.DOK, GK.PRIORITET;


=====QUERY=====
Query96
-----SQL-----
SELECT Sum(GK.DIZNOS) AS SumOfDIZNOS, GK.lnkSkupstinaID, GK.DATUM
FROM GK
WHERE (((GK.KONTO)="2040") AND ((GK.TIP_STAVKE)=3))
GROUP BY GK.lnkSkupstinaID, GK.DATUM
HAVING (((Sum(GK.DIZNOS))>0));


=====QUERY=====
Query97
-----SQL-----
SELECT Kupac_DUG_0.*
FROM Kupac_DUG_0
WHERE (((Kupac_DUG_0.lnkSkupstinaID)=101) AND ((Kupac_DUG_0.Suma)>1000) AND ((Kupac_DUG_0.lnkKUPACID)<8000));


=====QUERY=====
Query98
-----SQL-----
UPDATE GK SET GK.RacunIN_ID = 0
WHERE (((GK.RacunIN_ID) Is Null));


=====QUERY=====
Query99
-----SQL-----
SELECT Sum(GK_Filter_PD.[DIZNOS]-[PIZNOS]) AS Suma, GK_Filter_PD.DOK, Sum(GK_Filter_PD.DIZNOS) AS SumOfDIZNOS, Sum(GK_Filter_PD.PIZNOS) AS SumOfPIZNOS, GK_Filter_PD.PARAMETRI
FROM GK_Filter_PD
GROUP BY GK_Filter_PD.KONTO, GK_Filter_PD.lnkKUPACID, GK_Filter_PD.lnkSkupstinaID, GK_Filter_PD.DOK, GK_Filter_PD.PARAMETRI
HAVING (((GK_Filter_PD.KONTO)="2040") And ((GK_Filter_PD.lnkKUPACID)=1001) And ((GK_Filter_PD.lnkSkupstinaID)=101) And ((Sum(GK_Filter_PD.DIZNOS-[PIZNOS]))>0));


=====QUERY=====
R-201211
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.PARAMETRI) Like "201211*") AND ((GK.TIP_STAVKE)=3));


=====QUERY=====
R-201211-P
-----SQL-----
SELECT GK.*
FROM GK
WHERE (((GK.PARAMETRI) Like "201211*") AND ((GK.TIP_STAVKE)=3));


=====QUERY=====
RACUN_2304
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Racun.lnkGR, Racun.Ukupno, Racun.RBR, Racun.ID_K
FROM GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((GrupaRacuna.GrupaRacunaFXN)="2304"));


=====QUERY=====
RACUN_2305
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Racun.lnkGR, Racun.Ukupno, Racun.RBR, Racun.ID_K
FROM GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((GrupaRacuna.GrupaRacunaFXN)="2305"));


=====QUERY=====
RACUN_2306
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Racun.lnkGR, Racun.Ukupno, Racun.RBR, Racun.ID_K
FROM GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR
WHERE (((GrupaRacuna.GrupaRacunaFXN)="2306"));


=====QUERY=====
RACUNI_PRINTLISTA
-----SQL-----
SELECT Racun.ID_SK, Skustina.NazivSS, Racun.ID_K, Racun.Kupac, Racun.PIB, Racun.Ukupno, Racun.StanjePredhodniDug, Racun.StanjeDug, Racun.UkupnoDug
FROM (PrinterBinLOCAL INNER JOIN Racun ON PrinterBinLOCAL.ID_Item = Racun.IDRacun) INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina
ORDER BY Racun.ID_SK, Racun.ID_K;


=====QUERY=====
RACUNI_PRINTLISTA_Ex
-----SQL-----
SELECT Skustina.NazivSS AS [prodavac           (skupština stanara)], Racun.Kupac, Racun.DatumUsluge AS [Datum pružanja usluge], Racun.Ukupno AS [Zaduženje za mesec], Racun.DatumIzdavanja AS [Datum izdavanja racuna], Racun.ID_K AS ID, Racun.PozivNaBroj AS [Model i poziv na broj], Racun.PozivNaBrojPDug AS [Poziv na broj za dug]
FROM (PrinterBinLOCAL INNER JOIN Racun ON PrinterBinLOCAL.ID_Item = Racun.IDRacun) INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina
ORDER BY Racun.ID_SK, Racun.Kupac, Racun.ID_K;


=====QUERY=====
RacunPAR
-----SQL-----
SELECT Racun.*, Replace([PozivNaBroj],"-","") AS PAR
FROM Racun;


=====QUERY=====
RacunRacunStavke
-----SQL-----
SELECT GrupaRacuna.IDGrupaRacuna, Objekti.lnk_tip, GrupaRacuna.GrupaRacunaFXN, RacunStavke.Naziv, RacunStavke.lnkTVP, RacunStavke.DobAlias, RacunStavke.Ukupno, Troskovi_VP.Troskovi_Vrste, Troskovi_VP.Troskovi_Vrste_Opis, RacunStavke.Sort, [objekatNaziv] & " TC" & [uLAZ] AS [ON], Objekti.Ulaz
FROM (((Racun INNER JOIN RacunStavke ON Racun.IDRacun=RacunStavke.ID_R) INNER JOIN GrupaRacuna ON Racun.lnkGR=GrupaRacuna.IDGrupaRacuna) INNER JOIN Troskovi_VP ON RacunStavke.lnkTVP=Troskovi_VP.ID_Troskovi_Vrste) INNER JOIN Objekti ON Racun.ID_O=Objekti.ID_O
WHERE (((Objekti.lnk_tip)=3) AND ((GrupaRacuna.GrupaRacunaFXN)="1013"))
ORDER BY Objekti.Ulaz, Racun.objekatNaziv;


=====QUERY=====
RacunRacunStavke_Crosstab
-----SQL-----
TRANSFORM Sum(RacunRacunStavke.Ukupno) AS Suma
SELECT RacunRacunStavke.Sort, RacunRacunStavke.Troskovi_Vrste_Opis
FROM RacunRacunStavke
GROUP BY RacunRacunStavke.Sort, RacunRacunStavke.Troskovi_Vrste_Opis
ORDER BY RacunRacunStavke.Sort
PIVOT RacunRacunStavke.[ON];


=====QUERY=====
RacunRacunStavke_Crosstab-2
-----SQL-----
TRANSFORM Sum(RacunRacunStavke.Ukupno) AS Suma
SELECT RacunRacunStavke.Ulaz, RacunRacunStavke.[ON]
FROM RacunRacunStavke
GROUP BY RacunRacunStavke.Ulaz, RacunRacunStavke.[ON]
ORDER BY RacunRacunStavke.Ulaz, RacunRacunStavke.[ON], RacunRacunStavke.Sort
PIVOT RacunRacunStavke.Sort;


=====QUERY=====
RacunSaGrupomRacuna
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Racun.*, GrupaRacuna.GrupaRacunaFXT
FROM Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna;


=====QUERY=====
RacunStavke_Crosstab
-----SQL-----
TRANSFORM Sum(RacunStavke.Ukupno) AS SumOfUkupno
SELECT RacunStavke.ID_O, Sum(RacunStavke.Ukupno) AS [Total Of Ukupno]
FROM RacunStavke
WHERE (((RacunStavke.lnkGR)=57))
GROUP BY RacunStavke.ID_O
PIVOT RacunStavke.Grupa_VP;


=====QUERY=====
RacunStavke_Suma
-----SQL-----
SELECT RacunStavke.ID_R, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD
FROM RacunStavke
GROUP BY RacunStavke.ID_R;


=====QUERY=====
RacunStavkeSuma
-----SQL-----
SELECT RacunStavke.ID_R, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD
FROM RacunStavke
GROUP BY RacunStavke.ID_R;


=====QUERY=====
REPORT_KV_PROSEK
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Skustina.Zgrada, Count(Objekti.ID_O) AS CountOfID_O, TipObjekta.TipObj, Sum(Objekti.K1) AS SumOfK1, Sum(Objekti.netoKV) AS SumOfnetoKV, Sum(Objekti.terasa) AS SumOfterasa, Sum(Objekti.netoKVsaTerasom) AS SumOfnetoKVsaTerasom
FROM (Objekti INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
GROUP BY Objekti.lnkSkupstinaID, Skustina.Zgrada, TipObjekta.TipObj, TipObjekta.SortObj
ORDER BY Objekti.lnkSkupstinaID, TipObjekta.SortObj;


=====QUERY=====
REVIEW-IZVOD-ISPLATE
-----SQL-----
SELECT Skustina.Zgrada, IzvodStavke.DatumRealizacije, IzvodStavke.ID_SK, IzvodStavke.NazivPN, IzvodStavke.BrojRacuna, IzvodStavke.Doznaka, IzvodStavke.Zaduzenje, GK.KONTO, GK.KontoTroska, Troskovi_PodKonta.Naziv, GK.RDOB
FROM (GK RIGHT JOIN (Skustina INNER JOIN IzvodStavke ON Skustina.IDSkupstina = IzvodStavke.ID_SK) ON GK.lnkIzvodStavkaID = IzvodStavke.ID) LEFT JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto
WHERE (((IzvodStavke.Zaduzenje)>0))
ORDER BY IzvodStavke.DatumRealizacije, IzvodStavke.ID_SK;


=====QUERY=====
REVIEW-PROMENE
-----SQL-----
SELECT Promene.IDChange, Promene.DateOfRequest, Promene.DateOfExecution, Skustina.Zgrada AS SZ, Promene.IDK, Kupac.Naziv, Promene.IDO, Promene.Description, Promene.PreviusValue, Promene.NewValue, Promene.FieldsRelated, Promene.RequestType, Promene.RequestBy, Promene.RequestThrou, ConcatRelated("SifraPD","Objekti","lnk_ID_K = " & Nz([ID_K],-1)) AS PD
FROM (Kupac LEFT JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) RIGHT JOIN Promene ON Kupac.ID_K = Promene.IDK;


=====QUERY=====
REVIEW-RACUNA
-----SQL-----
SELECT Racun.RBR, Racun.DatumIzdavanja, Racun.ID_K, Racun.Kupac, Racun.Adresa_K, Racun.PBroj_K, Racun.Grad_K, Racun.PIB, Racun.MB, Racun.Lokacija, Racun.SvrhaUplate2, Racun.Suma, Racun.PDVStopa, Racun.PDVIznos, Racun.Ukupno, Racun.PozivNaBroj
FROM Racun INNER JOIN SortiranjeRacunaRacun ON Racun.IDRacun = SortiranjeRacunaRacun.IDRacun
WHERE (((Racun.lnkGR)=37))
ORDER BY SortiranjeRacunaRacun.tmpID;


=====QUERY=====
REVIEW-RACUNA-NEFAKTURISESE
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Objekti.naziv, Objekti.Ulaz, TipObjekta.TipObj, Objekti.BrojPD, Objekti.SifraPD, Objekti.K1, Objekti.K2, Objekti.Napomena
FROM (Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE (((Objekti.Status)=0));


=====QUERY=====
RunMeKillMe
-----SQL-----
INSERT INTO GK ( BR_NALOG, Konto, DATUM, PIZNOS, TIP_STAVKE, lnkSkupstinaID, lnkKUPACID, DOK, DPO, KontoTroska, SIFRAKONTA )
SELECT 4282 AS Nalog, '4909' AS KONTO, ZK.DATUM, Sum(ZK.DIZNOS) AS SumOfDIZNOS, ZK.TIPSTAVKE, ZK.IDSK, 0 AS PARTNERID, 'R-' & [ZK].[PARAMETRI] AS DOK, ZK.DATUM, ZK.PODKONTO, Troskovi_PodKonta_DefDob.DefDob
FROM (ZK INNER JOIN Racun ON ZK.RACUNID = Racun.IDRacun) INNER JOIN Troskovi_PodKonta_DefDob ON (ZK.IDSK = Troskovi_PodKonta_DefDob.IDSZ) AND (ZK.PODKONTO = Troskovi_PodKonta_DefDob.Konto)
GROUP BY 4282, '4909', ZK.TIPSTAVKE, ZK.IDSK, 0, 'R-' & [ZK].[PARAMETRI], ZK.DATUM, ZK.PODKONTO, Troskovi_PodKonta_DefDob.DefDob, ZK.IDGR
HAVING (((Sum(ZK.DIZNOS))<>0) AND ((ZK.IDSK)=101) AND ((ZK.IDGR)=164));


=====QUERY=====
Setting_FilterUser_Update
-----SQL-----
UPDATE Settings SET Settings.FilterUser = 0
WHERE (((Settings.FilterUser) Is Null));


=====QUERY=====
SettingMail_FixVal
-----SQL-----
SELECT Settings_eMail.*, Replace([SettingVal],"#sz-email#",[email]) AS FixVal
FROM Settings_eMail INNER JOIN Skustina ON Settings_eMail.SZID = Skustina.IDSkupstina;


=====QUERY=====
SHEET1_FIX
-----SQL-----
SELECT Sheet1.[Building name], Sheet1.Usage, Sheet1.[Unit number], "BW-ARI-" & [uNIT NUMBER] AS NFT, Sheet1.[Gross Area (EE)], Sheet1.[Owner / Tenant], Sheet1.[HO date], Sheet1.[FM Benefits start date], Sheet1.[FM benefits duration], Sheet1.[Date of FM charging start], Sheet1.UN
FROM Sheet1
WHERE (((Sheet1.Usage)="parking"));


=====QUERY=====
sk_pr
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS, Kupac.ID_K, Kupac.Naziv, Presek_Sub_001.Suma AS R1, Presek_Sub_002.DIZNOS AS R2, Presek_Sub_003.Suma AS R3, Presek_Sub_004.Suma AS R4, Presek_Sub_005.Suma AS R5, Presek_Sub_006.Suma AS R6, Presek_Sub_007.Suma AS R7, Skustina.PrintNaziv, RemoveSpecCHR([Naziv]) AS mN
FROM Presek_Sub_007 RIGHT JOIN (Presek_Sub_006 RIGHT JOIN (Presek_Sub_005 RIGHT JOIN (Presek_Sub_004 RIGHT JOIN (Presek_Sub_003 RIGHT JOIN (Presek_Sub_002 RIGHT JOIN ((Presek_Sub_001 RIGHT JOIN Kupac ON Presek_Sub_001.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) ON Presek_Sub_002.lnkKUPACID = Kupac.ID_K) ON Presek_Sub_003.lnkKUPACID = Kupac.ID_K) ON Presek_Sub_004.lnkKUPACID = Kupac.ID_K) ON Presek_Sub_005.lnkKUPACID = Kupac.ID_K) ON Presek_Sub_006.lnkKUPACID = Kupac.ID_K) ON Presek_Sub_007.lnkKUPACID = Kupac.ID_K
ORDER BY Kupac.ID_K;


=====QUERY=====
SK-IZVOD
-----SQL-----
SELECT Skustina.Folder, Izvod.IzvodID, Izvod.ID_SK
FROM Skustina INNER JOIN Izvod ON Skustina.IDSkupstina = Izvod.ID_SK;


=====QUERY=====
SortiranjeRacuna
-----SQL-----
SELECT Kupac.IDMaster, Objekti.naziv, Objekti.ID_O, Kupac.ID_K, Objekti.BRGM, TipObjekta.TipObj, Objekti_1.Ulaz, TipObjekta.SortObj, Objekti.BrojPD, Objekti_1.naziv, TipObjekta_1.SortObj, Objekti_1.BrojPD, Kupac.Naziv
FROM (((Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.IDMaster) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Objekti AS Objekti_1 ON Kupac.ID_K = Objekti_1.lnk_ID_K) INNER JOIN TipObjekta AS TipObjekta_1 ON Objekti_1.lnk_tip = TipObjekta_1.IDTipObj
ORDER BY Objekti_1.Ulaz, TipObjekta.SortObj, Objekti.BrojPD, TipObjekta_1.SortObj, Objekti_1.BrojPD;


=====QUERY=====
SortiranjeRacunaRacun
-----SQL-----
SELECT Kupac.IDMaster, Objekti.naziv, Objekti.ID_O, Kupac.ID_K, Objekti.BRGM, TipObjekta.TipObj, Objekti_1.Ulaz, TipObjekta.SortObj, Objekti.BrojPD, Objekti_1.naziv, TipObjekta_1.SortObj, Objekti_1.BrojPD, Kupac.Naziv, Racun.IDRacun, Racun.lnkGR, Racun.tmpID
FROM ((((Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.IDMaster) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) INNER JOIN Objekti AS Objekti_1 ON Kupac.ID_K = Objekti_1.lnk_ID_K) INNER JOIN TipObjekta AS TipObjekta_1 ON Objekti_1.lnk_tip = TipObjekta_1.IDTipObj) INNER JOIN Racun ON Kupac.ID_K = Racun.ID_K
WHERE (((Racun.lnkGR)=37))
ORDER BY Objekti_1.Ulaz, TipObjekta.SortObj, Objekti.BrojPD, TipObjekta_1.SortObj, Objekti_1.BrojPD;


=====QUERY=====
Spajanje_Tip98_PDPB_Kupac
-----SQL-----
SELECT Kupac.PBPD, Right([DOK],Len([DOK])-2) AS Expr1
FROM Kupac INNER JOIN Nalog_Tip_98 ON Kupac.ID_K = Nalog_Tip_98.lnkKUPACID;


=====QUERY=====
SPISAK-PD-KORISNIK-TIP
-----SQL-----
SELECT Objekti.Ulaz, Objekti.SifraPD, TipObjekta.TipObj, TipObjekta.SortObj, Objekti.BrojPD, Objekti.naziv, Objekti.K1, IIf([kUPAC].[napomena]="INVESTITOR","INVESTITOR",IIf(Nz([PIB],0)=0,"FIZICKO","PRAVNO")) AS KORISNIK, Kupac.ID_K, Kupac.Naziv, Kupac.PIB, Objekti.K2, IIf([kUPAC].[napomena]="INVESTITOR","PRAVNO",IIf(Nz([PIB],0)=0,"FIZICKO","PRAVNO")) AS FP
FROM (Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
ORDER BY Objekti.Ulaz, TipObjekta.SortObj, Objekti.BrojPD, Objekti.naziv;


=====QUERY=====
SPISAK-PD-KORISNIK-TIP-SUM
-----SQL-----
SELECT [SPISAK-PD-KORISNIK-TIP].Ulaz, [SPISAK-PD-KORISNIK-TIP].TipObj, [SPISAK-PD-KORISNIK-TIP].KORISNIK, Count([SPISAK-PD-KORISNIK-TIP].SifraPD) AS CountOfSifraPD, Sum([SPISAK-PD-KORISNIK-TIP].K1) AS SumOfK1, Sum([SPISAK-PD-KORISNIK-TIP].K2) AS SumOfK2, [SPISAK-PD-KORISNIK-TIP].SortObj, [SPISAK-PD-KORISNIK-TIP].FP
FROM [SPISAK-PD-KORISNIK-TIP]
GROUP BY [SPISAK-PD-KORISNIK-TIP].Ulaz, [SPISAK-PD-KORISNIK-TIP].TipObj, [SPISAK-PD-KORISNIK-TIP].KORISNIK, [SPISAK-PD-KORISNIK-TIP].SortObj, [SPISAK-PD-KORISNIK-TIP].FP
ORDER BY [SPISAK-PD-KORISNIK-TIP].Ulaz, [SPISAK-PD-KORISNIK-TIP].SortObj, [SPISAK-PD-KORISNIK-TIP].KORISNIK;


=====QUERY=====
SPISAK-PD-KORISNIK-TIP-SUM2
-----SQL-----
SELECT [SPISAK-PD-KORISNIK-TIP-SUM].TipObj, [SPISAK-PD-KORISNIK-TIP-SUM].KORISNIK, Sum([SPISAK-PD-KORISNIK-TIP-SUM].SumOfK1) AS SumOfSumOfK1, Sum([SPISAK-PD-KORISNIK-TIP-SUM].SumOfK2) AS SumOfSumOfK2
FROM [SPISAK-PD-KORISNIK-TIP-SUM]
GROUP BY [SPISAK-PD-KORISNIK-TIP-SUM].TipObj, [SPISAK-PD-KORISNIK-TIP-SUM].KORISNIK, [SPISAK-PD-KORISNIK-TIP-SUM].SortObj
ORDER BY [SPISAK-PD-KORISNIK-TIP-SUM].SortObj;


=====QUERY=====
SPISAK-PD-KORISNIK-TIP-SUM3
-----SQL-----
SELECT [SPISAK-PD-KORISNIK-TIP-SUM].TipObj, [SPISAK-PD-KORISNIK-TIP-SUM].KORISNIK, Sum([SPISAK-PD-KORISNIK-TIP-SUM].SumOfK1) AS SumOfSumOfK1, Sum([SPISAK-PD-KORISNIK-TIP-SUM].SumOfK2) AS SumOfSumOfK2
FROM [SPISAK-PD-KORISNIK-TIP-SUM]
GROUP BY [SPISAK-PD-KORISNIK-TIP-SUM].TipObj, [SPISAK-PD-KORISNIK-TIP-SUM].KORISNIK, [SPISAK-PD-KORISNIK-TIP-SUM].SortObj
ORDER BY [SPISAK-PD-KORISNIK-TIP-SUM].SortObj;


=====QUERY=====
SPISAK-STANARA-EXPORT
-----SQL-----
SELECT Objekti.lnkSkupstinaID, Kupac.Naziv, TipObjekta.TipObj, Objekti.BrojPD, Objekti.SifraPD, Objekti.naziv
FROM (Kupac RIGHT JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj;


=====QUERY=====
StanjePP
-----SQL-----
SELECT GK.lnkKUPACID, GK.lnkSkupstinaID, Sum([DIZNOS]-[PIZNOS]) AS Stanje, First(GK.NAPOMENA) AS FirstOfNAPOMENA
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID=Kupac.ID_K
WHERE ((([PARAMETRI]=kupac.PBPD_F)=-1))
GROUP BY GK.lnkKUPACID, GK.lnkSkupstinaID;


=====QUERY=====
StanjeSveBezPP
-----SQL-----
SELECT GK.lnkKUPACID, GK.lnkSkupstinaID, Sum([DIZNOS]-[PIZNOS]) AS Stanje
FROM GK INNER JOIN Kupac ON GK.lnkKUPACID=Kupac.ID_K
WHERE (((nz([PARAMETRI],0)=nz(kupac.PBPD_F,"F"))=0))
GROUP BY GK.lnkKUPACID, GK.lnkSkupstinaID;


=====QUERY=====
STAT_DOBAVLJACI_RACUN
-----SQL-----
SELECT Dobavljac_Racuni.SK_ID, Skustina.NazivSS, Dobavljac_Racuni.NazivRacuna, Dobavljac_Racuni.Dobavljac, Kupac.Naziv, Dobavljac_Racuni.TipObracuna, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.IznosPoKoefRSD, TipObracuna.TipObracuna, TipObracuna.Napomena
FROM ((Dobavljac_Racuni INNER JOIN Skustina ON Dobavljac_Racuni.SK_ID = Skustina.IDSkupstina) INNER JOIN Kupac ON Dobavljac_Racuni.DobavljacKonto = Kupac.ID_K) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr
ORDER BY Dobavljac_Racuni.Dobavljac;


=====QUERY=====
STAT_STANJE
-----SQL-----
SELECT Kupac.Naziv, Kupac.ID_K, Sum([DIZNOS]-[PIZNOS]) AS Stanje, Skustina.NazivSS, Last(GK.DATUM) AS LastOfDATUM
FROM (GK INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina
GROUP BY Kupac.Naziv, Kupac.ID_K, Skustina.NazivSS
ORDER BY Sum([DIZNOS]-[PIZNOS]) DESC;


=====QUERY=====
STAT0003-KONTROLOA-OBJ-KUP
-----SQL-----
SELECT Kupac.ID_K, Kupac.lnk_ID_SK, Skustina_1.NazivSS, Objekti.naziv, Objekti.lnkSkupstinaID, Skustina.NazivSS
FROM ((Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN Skustina AS Skustina_1 ON Kupac.lnk_ID_SK = Skustina_1.IDSkupstina
WHERE ((([lnkSkupstinaID]=[lnk_ID_SK])=0));


=====QUERY=====
StornoFaktura2040
-----SQL-----
SELECT GK.KONTO, GK.BR_NALOG
FROM GK
WHERE (((GK.DIZNOS)<0))
GROUP BY GK.KONTO, GK.BR_NALOG
HAVING (((GK.KONTO)="2040"));


=====QUERY=====
STT001-UPLATE-ZA-BLOK
-----SQL-----
SELECT Kupac.PBPD_F, Kupac.ID_K, Kupac.Naziv, Skustina.NazivSS, GK.DATUM, GK.PIZNOS
FROM (GK INNER JOIN Kupac ON GK.PARAMETRI = Kupac.PBPD_F) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina
WHERE (((GK.DIZNOS)=0) AND ((GK.PIZNOS)<>0))
ORDER BY GK.DATUM;


=====QUERY=====
STT001-UPLATE-ZA-BLOK-STANJE
-----SQL-----
SELECT Kupac.PBPD_F, Kupac.ID_K, Kupac.Naziv, Skustina.NazivSS, Sum(Round(Round([DIZNOS],2)-Round([PIZNOS],2),2)) AS STANJE
FROM (GK INNER JOIN Kupac ON GK.PARAMETRI=Kupac.PBPD_F) INNER JOIN Skustina ON Kupac.lnk_ID_SK=Skustina.IDSkupstina
GROUP BY Kupac.PBPD_F, Kupac.ID_K, Kupac.Naziv, Skustina.NazivSS;


=====QUERY=====
STT001-UPLATE-ZA-BLOK-SVE
-----SQL-----
SELECT Kupac.PBPD_F, Kupac.ID_K, Kupac.Naziv, Skustina.NazivSS, GK.DATUM, GK.PIZNOS, [STT001-UPLATE-ZA-BLOK-STANJE].STANJE
FROM ((GK RIGHT JOIN Kupac ON GK.PARAMETRI=Kupac.PBPD_F) LEFT JOIN Skustina ON Kupac.lnk_ID_SK=Skustina.IDSkupstina) RIGHT JOIN [STT001-UPLATE-ZA-BLOK-STANJE] ON Kupac.ID_K=[STT001-UPLATE-ZA-BLOK-STANJE].ID_K
WHERE (((GK.PIZNOS)<>0) AND ((GK.DIZNOS)=0))
ORDER BY GK.DATUM;


=====QUERY=====
STT001-UPLATE-ZA-BLOK-SVE-BEZ0
-----SQL-----
SELECT Kupac.PBPD_F, Kupac.ID_K, Kupac.Naziv, Skustina.NazivSS, GK.DATUM, GK.PIZNOS, [STT001-UPLATE-ZA-BLOK-STANJE].STANJE
FROM ((GK INNER JOIN Kupac ON GK.PARAMETRI=Kupac.PBPD_F) INNER JOIN Skustina ON Kupac.lnk_ID_SK=Skustina.IDSkupstina) INNER JOIN [STT001-UPLATE-ZA-BLOK-STANJE] ON Kupac.ID_K=[STT001-UPLATE-ZA-BLOK-STANJE].ID_K
WHERE (((GK.PIZNOS)<>0) AND (([STT001-UPLATE-ZA-BLOK-STANJE].STANJE)>0.01) AND ((GK.DIZNOS)=0))
ORDER BY GK.DATUM;


=====QUERY=====
SubKonto_3
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, GK.KONTO, GK.BR_NALOG, GK.DATUM, TipStavke.TipStavke, Kupac.Naziv, GK.lnkSkupstinaID, GK.NAPOMENA, GK.DIZNOS, GK.PIZNOS, GK.DOK, [DIZNOS]-[PIZNOS] AS DIPI, GK.PARAMETRI, Nalog.OpisNaloga
FROM (((GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.PodKonto) INNER JOIN TipStavke ON GK.TIP_STAVKE = TipStavke.ID_TIP) INNER JOIN Kupac ON GK.lnkKUPACID = Kupac.ID_K) INNER JOIN Nalog ON GK.BR_NALOG = Nalog.Br_Nalog
WHERE (((Troskovi_PodKonta.PodKonto)="31911"));


=====QUERY=====
suma racuna
-----SQL-----
SELECT Racun.lnkGR, Sum(Racun.Ukupno) AS SumOfUkupno, Count(Racun.IDRacun) AS CountOfIDRacun
FROM Racun
WHERE (((Racun.SPC)=0))
GROUP BY Racun.lnkGR;


=====QUERY=====
SumaPoTabeliRashoda
-----SQL-----
SELECT Sum(Troskovi_Vrednosti.Men) AS SumOfMen, Sum(Troskovi_Vrednosti.Teh) AS SumOfTeh, Sum(Troskovi_Vrednosti.Obz) AS SumOfObz, Sum(Troskovi_Vrednosti.Cis) AS SumOfCis, Sum(Troskovi_Vrednosti.Zel) AS SumOfZel, Sum(Troskovi_Vrednosti.Fin) AS SumOfFin, Sum(Troskovi_Vrednosti.U_P) AS SumOfU_P, Sum(Troskovi_Vrednosti.P_stan) AS SumOfP_stan, Sum(Troskovi_Vrednosti.P_posl) AS SumOfP_posl, Sum(Troskovi_Vrednosti.Gar_G) AS SumOfGar_G, Sum(Troskovi_Vrednosti.Gar_GM) AS SumOfGar_GM, Sum(Troskovi_Vrednosti.OP) AS SumOfOP
FROM Troskovi_Vrednosti;


=====QUERY=====
SUMA-RACUNA
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, Skustina.Zgrada, Count(Racun.IDRacun) AS CountOfIDRacun, Sum(Kupac.chkSkipPrintRacunGrupa) AS SumOfchkSkipPrintRacunGrupa
FROM ((GrupaRacuna INNER JOIN Racun ON GrupaRacuna.IDGrupaRacuna = Racun.lnkGR) INNER JOIN Skustina ON Racun.ID_SK = Skustina.IDSkupstina) INNER JOIN Kupac ON Racun.ID_K = Kupac.ID_K
GROUP BY GrupaRacuna.GrupaRacunaFXN, Skustina.Zgrada;


=====QUERY=====
SumaRacunaPoTipu1
-----SQL-----
SELECT Troskovi_Racuni.RacunNO, Sum(Troskovi_Racuni.SumaD) AS SumOfSumaD, Troskovi_Racuni.MesecRacuna, 1 AS TipObjekta
FROM Troskovi_Racuni
GROUP BY Troskovi_Racuni.RacunNO, Troskovi_Racuni.MesecRacuna, 1, Troskovi_Racuni.G
HAVING (((Troskovi_Racuni.G)=-1));


=====QUERY=====
SumaRacunaPoTipu3
-----SQL-----
SELECT Troskovi_Racuni.RacunNO, Sum(Troskovi_Racuni.SumaD) AS SumOfSumaD, Troskovi_Racuni.MesecRacuna, 3 AS TipObjekta
FROM Troskovi_Racuni
GROUP BY Troskovi_Racuni.RacunNO, Troskovi_Racuni.MesecRacuna, 3, Troskovi_Racuni.S
HAVING (((Troskovi_Racuni.S)=-1));


=====QUERY=====
SumaRacunaPoTipu4
-----SQL-----
SELECT Troskovi_Racuni.RacunNO, Sum(Troskovi_Racuni.SumaD) AS SumOfSumaD, Troskovi_Racuni.MesecRacuna, 4 AS TipObjekta
FROM Troskovi_Racuni
GROUP BY Troskovi_Racuni.RacunNO, Troskovi_Racuni.MesecRacuna, 4, Troskovi_Racuni.L
HAVING (((Troskovi_Racuni.L)=-1));


=====QUERY=====
SUME RACUNA
-----SQL-----
SELECT Racun.lnkGR, Sum(Racun.Ukupno) AS SumOfUkupno
FROM Racun
GROUP BY Racun.lnkGR;


=====QUERY=====
SUME RACUNISTAVKE
-----SQL-----
SELECT RacunStavke.lnkGR, RacunStavke.Grupa_VP, Sum(RacunStavke.CenaE) AS SumOfCenaE
FROM RacunStavke
GROUP BY RacunStavke.lnkGR, RacunStavke.Grupa_VP;


=====QUERY=====
SZ_IzdavalacRacuna
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina_1.SZPDV
FROM Skustina INNER JOIN Skustina AS Skustina_1 ON Skustina.InvoiceIssuer = Skustina_1.IDSkupstina;


=====QUERY=====
tblShortList_IzvestajGrupaDugovanja
-----SQL-----
SELECT tblShortList.Index, tblShortList.Caption
FROM tblShortList
WHERE (((tblShortList.TableFrom)="IzvestajGrupaDugovanja"));


=====QUERY=====
tblShortList_KnjiiznaDokumenta
-----SQL-----
SELECT tblShortList.Index, [IndexValue] & " - " & [Caption] AS Expr1, tblShortList.ShortName, tblShortList.IndexValue, tblShortList.Description
FROM tblShortList
WHERE (((tblShortList.TableFrom)="KnjiznaDokumenta"));


=====QUERY=====
tblShortList_MailSufixPrintOnly
-----SQL-----
SELECT tblShortList.Index, tblShortList.Caption
FROM tblShortList
WHERE (((tblShortList.TableFrom)="MailSufixPrintOnly"));


=====QUERY=====
tblShortList_OpomeneVrsta
-----SQL-----
SELECT tblShortList.Index, tblShortList.Caption, tblShortList.ShortName
FROM tblShortList
WHERE (((tblShortList.TableFrom)="OpomeneVrsta"));


=====QUERY=====
tblShortList_RacunListaPrintOption
-----SQL-----
SELECT tblShortList.Index, tblShortList.Caption, tblShortList.ShortName, tblShortList.Description
FROM tblShortList
WHERE (((tblShortList.TableFrom)="RacunListaPrintOption"))
ORDER BY tblShortList.Cat1;


=====QUERY=====
tblShortList_StatusObjekta
-----SQL-----
SELECT tblShortList.Index, tblShortList.Caption, tblShortList.ShortName
FROM tblShortList
WHERE (((tblShortList.TableFrom)="UnitStatus"));


=====QUERY=====
tblShortList_TipVirmana
-----SQL-----
SELECT tblShortList.Index, tblShortList.Caption, tblShortList.ShortName, tblShortList.Description
FROM tblShortList
WHERE (((tblShortList.TableFrom)="TipVirmana"));


=====QUERY=====
TEMP_FIX_MAXI_UPDATE_NALOG_IZVODA
-----SQL-----
UPDATE Izvod INNER JOIN Nalog ON Izvod.NalogZaKnjizenje = Nalog.Br_Nalog SET Nalog.OpisNaloga = 'IZVOD ' & [BrojIzvoda] & '/' & Year([Izvod].[Datum]), Nalog.SZID = [ID_SK];


=====QUERY=====
TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA
-----SQL-----
SELECT GK.lnkSkupstinaID, Skustina.Zgrada, GK.DATUM, TipStavke.TipStavke, Sum(GK.PIZNOS) AS SumOfPIZNOS, GK.TIP_STAVKE
FROM ((GK INNER JOIN Dobavljac_Racuni ON GK.RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN TipStavke ON GK.TIP_STAVKE = TipStavke.ID_TIP) INNER JOIN Skustina ON GK.lnkSkupstinaID = Skustina.IDSkupstina
GROUP BY Dobavljac_Racuni.DobavljacKonto, GK.lnkSkupstinaID, Skustina.Zgrada, GK.DATUM, TipStavke.TipStavke, GK.TIP_STAVKE
HAVING (((Dobavljac_Racuni.DobavljacKonto)=9034) AND ((GK.TIP_STAVKE)=1 Or (GK.TIP_STAVKE)=8));


=====QUERY=====
TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA_Crosstab
-----SQL-----
TRANSFORM Sum(TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA.[SumOfPIZNOS]) AS SumOfSumOfPIZNOS
SELECT TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA.[DATUM], TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA.[TipStavke], Sum(TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA.[SumOfPIZNOS]) AS [Total Of SumOfPIZNOS]
FROM TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA
GROUP BY TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA.[DATUM], TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA.[TipStavke]
PIVOT TEMP_GK_KNJIZENJE_ZA_DOB_PO_DANIMA.[Zgrada];


=====QUERY=====
TEMP_PROMENA_KONTA_SVE
-----SQL-----
UPDATE GK INNER JOIN Troskovi_PodKonta ON GK.KontoTroska = Troskovi_PodKonta.SifraKnj_Prethodni SET GK.KontoTroska = [PodKonto];


=====QUERY=====
TEMP_PROMENA_KONTA_SVE_A
-----SQL-----
UPDATE Dobavljac_Racuni INNER JOIN Troskovi_PodKonta ON Dobavljac_Racuni.KontoKnjizenja = Troskovi_PodKonta.SifraKnj_Prethodni SET Dobavljac_Racuni.KontoKnjizenja = [PodKonto];


=====QUERY=====
TEMP_PROMENA_KONTA1001
-----SQL-----
UPDATE GK SET GK.KontoTroska = "0001"
WHERE (((GK.KontoTroska)="1001"));


=====QUERY=====
TEMP_PROMENA_KONTA1001A
-----SQL-----
UPDATE Dobavljac_Racuni SET Dobavljac_Racuni.KontoKnjizenja = "0001"
WHERE (((Dobavljac_Racuni.KontoKnjizenja)="1001"));


=====QUERY=====
TEMP_PROMENA_KONTA1002
-----SQL-----
UPDATE GK SET GK.KontoTroska = "0002"
WHERE (((GK.KontoTroska)="1002"));


=====QUERY=====
TEMP_PROMENA_KONTA1002A
-----SQL-----
UPDATE Dobavljac_Racuni SET Dobavljac_Racuni.KontoKnjizenja = "0002"
WHERE (((Dobavljac_Racuni.KontoKnjizenja)="1002"));


=====QUERY=====
TEMP_X_NALOG_PROMENA DATUMA
-----SQL-----
SELECT Nalog.TipNaloga, Nalog.Datum, GK.DATUM, GK.DPO
FROM GK INNER JOIN Nalog ON GK.BR_NALOG = Nalog.Br_Nalog
WHERE (((Nalog.TipNaloga)=8) AND ((Nalog.Datum)=#7/1/2026#));


=====QUERY=====
temp-filter-kupac-objekat-garaze
-----SQL-----
SELECT Kupac.ID_K, Kupac.Naziv, Kupac.Adresa, Objekti.lnk_ID_K, Objekti.naziv, Objekti.lnkSkupstinaID, Kupac.lnk_ID_SK
FROM Kupac INNER JOIN Objekti ON Kupac.ID_K = Objekti.lnk_ID_K;


=====QUERY=====
TEMP-OPOMENA-KB
-----SQL-----
SELECT KontrolniBroj(97,[IDSkupstina] & "-" & [lnkKupac] & "-P" & Format([Datum],"yyyymmdd")) & "-" & [IDSkupstina] & "-" & [lnkKupac] & "-P" & Format([Datum],"yyyymmdd") AS KB, Skustina.IDSkupstina, Opomena.PozivNaBroj
FROM ((Opomena INNER JOIN Kupac ON Opomena.lnkKupac = Kupac.ID_K) INNER JOIN Skustina ON Kupac.lnk_ID_SK = Skustina.IDSkupstina) INNER JOIN GrupaOpomena ON Opomena.lnkGrupaOpomena = GrupaOpomena.IDGrupaOpomena;


=====QUERY=====
TEMP-R1-BRISI
-----SQL-----
INSERT INTO RacunStavke ( lnkGR, ID_K, ID_SK, K1, K2, K3, K4, K5, Naziv, TipObracuna, DobavljacKonto, ID_RDOB )
SELECT GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC
FROM GrupaRacuna INNER JOIN ((Objekti_Za_Racun INNER JOIN (Kupac INNER JOIN (Dobavljaci_Racun_TipObjekta INNER JOIN Dobavljac_Racuni ON Dobavljaci_Racun_TipObjekta.RacunID = Dobavljac_Racuni.IDTRRAC) ON Kupac.ID_K = Dobavljac_Racuni.DobavljacKonto) ON (Dobavljac_Racuni.SK_ID = Objekti_Za_Racun.lnkSkupstinaID) AND (Objekti_Za_Racun.lnk_tip = Dobavljaci_Racun_TipObjekta.TipObjekta)) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) ON GrupaRacuna.GrupaRacunaFXT = Dobavljac_Racuni.MesecRacuna
GROUP BY GrupaRacuna.IDGrupaRacuna, Objekti_Za_Racun.lnk_ID_K, Objekti_Za_Racun.lnkSkupstinaID, Objekti_Za_Racun.SumOfK1, Objekti_Za_Racun.SumOfK2, Objekti_Za_Racun.SumOfK3, Objekti_Za_Racun.SumOfK4, Objekti_Za_Racun.SumOfK5, Dobavljac_Racuni.NazivRacuna, TipObracuna.IDTipObr, Kupac.ID_K, Dobavljac_Racuni.IDTRRAC, Objekti_Za_Racun.SortObj, Dobavljac_Racuni.SortMarkerYYYYMM
HAVING (((GrupaRacuna.IDGrupaRacuna)=216))
ORDER BY Objekti_Za_Racun.SortObj, Dobavljac_Racuni.SortMarkerYYYYMM;


=====QUERY=====
TEMP-R2-BRISI
-----SQL-----
INSERT INTO Racun ( lnkGR, ID_K, ID_SK, DatumIzdavanja, DatumUsluge, DatumValute, MestoIzdavanja, Kupac, PBroj_K, Adresa_K, PIB, PrethodniDug, Ukupno )
SELECT RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Nz([Suma],0) AS PD, Sum(RacunStavke.UkupnoRSD) AS SumOfUkupnoRSD
FROM ((RacunStavke INNER JOIN GrupaRacuna ON RacunStavke.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN Kupac ON RacunStavke.ID_K = Kupac.ID_K) LEFT JOIN Kupac_DUG ON RacunStavke.ID_K = Kupac_DUG.lnkKUPACID
GROUP BY RacunStavke.lnkGR, RacunStavke.ID_K, RacunStavke.ID_SK, GrupaRacuna.DatumIzdavanja, GrupaRacuna.DatumUsluge, GrupaRacuna.DatumValute, GrupaRacuna.Mesto, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB, Nz([Suma],0)
HAVING (((RacunStavke.lnkGR)=216));


=====QUERY=====
TEMP-R3-BRISI
-----SQL-----
SELECT TipObracuna.KOLICINA, TipObracuna.IZNOS, Objekat_Sume_Po_TipovimaIzRacuna.*, Dobavljac_Racuni.IznosRacunaRSD, Dobavljac_Racuni.IznosPoKoefRSD, RacunStavke.*
FROM ((RacunStavke INNER JOIN Dobavljac_Racuni ON RacunStavke.ID_RDOB = Dobavljac_Racuni.IDTRRAC) INNER JOIN TipObracuna ON Dobavljac_Racuni.TipObracuna = TipObracuna.IDTipObr) INNER JOIN Objekat_Sume_Po_TipovimaIzRacuna ON Dobavljac_Racuni.IDTRRAC = Objekat_Sume_Po_TipovimaIzRacuna.RacunID
WHERE (((RacunStavke.lnkGR)=216));


=====QUERY=====
TEMP-R4-BRISI
-----SQL-----
UPDATE Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna SET Racun.PozivNaBroj = KontrolniBroj(97,[ID_SK] & '-' & Format([ID_K]) & '-' & [GrupaRacunaFXT]) & '-' & [ID_SK] & '-' & Format([ID_K]) & '-' & [GrupaRacunaFXT];


=====QUERY=====
TipDokumenta
-----SQL-----
SELECT Kupac.PBroj
FROM Kupac
GROUP BY Kupac.PBroj
ORDER BY Kupac.PBroj;


=====QUERY=====
TipRacunaDobavljac
-----SQL-----
SELECT tblShortList.Index, tblShortList.TableFrom, tblShortList.Caption, tblShortList.Cat1
FROM tblShortList
GROUP BY tblShortList.Index, tblShortList.TableFrom, tblShortList.Caption, tblShortList.Cat1
HAVING (((tblShortList.TableFrom)="Dobavljac_Racuni"));


=====QUERY=====
TK_SPISAK_KONTA
-----SQL-----
SELECT TK4.PodKonto, TK4.Naziv, TK3.PodKonto, TK3.Naziv, TK2.PodKonto, TK2.Naziv, TK1.PodKonto, TK1.Naziv
FROM ((TK1 INNER JOIN TK2 ON TK1.PodKonto = TK2.Prethodni) INNER JOIN TK3 ON TK2.PodKonto = TK3.Prethodni) INNER JOIN TK4 ON TK3.PodKonto = TK4.Prethodni
ORDER BY TK4.PodKonto;


=====QUERY=====
TK1
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, Len([PodKonto]) AS Nivo
FROM Troskovi_PodKonta
WHERE (((Len([PodKonto]))=1));


=====QUERY=====
TK2
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, Len([PodKonto]) AS Nivo, Left([PodKonto],1) AS Prethodni
FROM Troskovi_PodKonta
WHERE (((Len([PodKonto]))=2));


=====QUERY=====
TK3
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, Len([PodKonto]) AS Nivo, Left([PodKonto],2) AS Prethodni
FROM Troskovi_PodKonta
WHERE (((Len([PodKonto]))=3));


=====QUERY=====
TK4
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, Troskovi_PodKonta.Naziv, Len([PodKonto]) AS Nivo, Left([PodKonto],3) AS Prethodni
FROM Troskovi_PodKonta
WHERE (((Len([PodKonto]))=4));


=====QUERY=====
TKONTO
-----SQL-----
SELECT Troskovi_PodKonta.PodKonto, IIf(Nz([TROSAKNA],"")<>"",[TROSAKNA],[PODKONTO]) AS TKONTO, Left(IIf(Nz([TROSAKNA],"")<>"",[TROSAKNA],[PODKONTO]),2) AS GRUPA1, Left(IIf(Nz([TROSAKNA],"")<>"",[TROSAKNA],[PODKONTO]),4) AS GRUPA4, Troskovi_PodKonta.TrosakNa
FROM Troskovi_PodKonta
WHERE (((Len([PodKonto]))=5));


=====QUERY=====
tmpQuery
-----SQL-----
SELECT Skustina.IDSkupstina, Skustina.NazivSS, Kupac.ID_K, Kupac.Naziv, Kupac.MB, Objekti.naziv, Objekti.kolicina, Objekti.JM
FROM ((Objekti INNER JOIN Kupac ON Objekti.lnk_ID_K = Kupac.ID_K) INNER JOIN Skustina ON Objekti.lnkSkupstinaID = Skustina.IDSkupstina) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj
WHERE ((Kupac.Naziv like '*adil*') AND ((Kupac.ID_K)<9900));


=====QUERY=====
Troskovi_Qry
-----SQL-----
SELECT Skustina.NazivSS, [ID_Troskovi_Vrste] & " - " & [Troskovi_Vrste] & " - " & [Naziv] AS Ugovor, Troskovi_Ugovori.Vrednost
FROM Troskovi_VP INNER JOIN ((Skustina INNER JOIN Troskovi_Ugovori ON Skustina.IDSkupstina = Troskovi_Ugovori.lnk_ID_SK) INNER JOIN Kupac ON Troskovi_Ugovori.lnk_ID_Dob = Kupac.ID_K) ON Troskovi_VP.ID_Troskovi_Vrste = Troskovi_Ugovori.lnk_TV;


=====QUERY=====
Troskovi_Qry_Crosstab
-----SQL-----
TRANSFORM Sum(Troskovi_Qry.Vrednost) AS SumOfVrednost
SELECT Troskovi_Qry.NazivSS, Sum(Troskovi_Qry.Vrednost) AS [Total Of Vrednost]
FROM Troskovi_Qry
GROUP BY Troskovi_Qry.NazivSS
PIVOT Troskovi_Qry.Ugovor;


=====QUERY=====
Troskovi_Qry_Crosstab_Suma
-----SQL-----
TRANSFORM Sum(Troskovi_Qry.Vrednost) AS SumOfVrednost
SELECT "SVE SKUPŠTINE" AS SVI, Sum(Troskovi_Qry.Vrednost) AS [Total Of Vrednost]
FROM Troskovi_Qry
GROUP BY "SVE SKUPŠTINE"
PIVOT Troskovi_Qry.Ugovor;


=====QUERY=====
Troskovi_Ugovor_FLT
-----SQL-----
SELECT Troskovi_Ugovor.*
FROM Troskovi_Ugovor
WHERE (((Troskovi_Ugovor.MesecRacuna)='0918'));


=====QUERY=====
Troskovi_Ugovor_FRMTROSKOVIVP
-----SQL-----
SELECT Troskovi_Ugovor.*
FROM Troskovi_Ugovor
WHERE (((Troskovi_Ugovor.MesecRacuna)=Forms!Troskovi_VP!Combo18));


=====QUERY=====
Troskovi_VP_SORTIRANO
-----SQL-----
SELECT Troskovi_VP.*
FROM Troskovi_VP
WHERE (((Troskovi_VP.TipObjektaRep)=3 Or (Troskovi_VP.TipObjektaRep)=4))
ORDER BY Troskovi_VP.TipObjektaRep, Troskovi_VP.Sort, Troskovi_VP.Grupa_VP;


=====QUERY=====
TV_MIX
-----SQL-----
SELECT Troskovi_VP.*, Troskovi_Ugovor.IzRacuna, Troskovi_Ugovor.LINK
FROM Troskovi_Ugovor RIGHT JOIN Troskovi_VP ON Troskovi_Ugovor.lnk_TV = Troskovi_VP.ID_Troskovi_Vrste;


=====QUERY=====
UPDATE_ADRESA
-----SQL-----
SELECT Racun.lnkGR, Racun.Adresa_K, Racun.Grad_K, Racun.PBroj_K, GrupaRacuna.GrupaRacunaFXN, AdresaDostave.*, Racun.lnkGR, AdresaDostave.ID_K, Racun.ID_K
FROM (Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna) INNER JOIN AdresaDostave ON Racun.ID_K = AdresaDostave.ID_K
WHERE (((Racun.Adresa_K) Is Null) AND ((GrupaRacuna.GrupaRacunaFXN)="2401"));


=====QUERY=====
UPDATE_FIX_SIFRAPD
-----SQL-----
SELECT Objekti.SifraPD, GetSortPDfromSifraPD([SifraPD]) AS PD, GetSortPDfromSifraPDGM([SifraPD]) AS PDGM, Objekti.BrojPD, Objekti.lnk_tip
FROM Objekti
WHERE (((Objekti.BrojPD)=0) AND ((Objekti.lnk_tip)=4));


=====QUERY=====
Update_GK_Datum_DatumPrometa
-----SQL-----
UPDATE GrupaRacuna INNER JOIN GK ON GrupaRacuna.NalogKN = GK.BR_NALOG SET GK.DATUM = [DatumPrometa]
WHERE ((([DATUM]=[DatumPrometa])=0));


=====QUERY=====
UPDATE_KNDOKID
-----SQL-----
UPDATE GK SET GK.KnDokID = 0
WHERE (((GK.KnDokID) Is Null));


=====QUERY=====
UPDATE-PDV-20
-----SQL-----
UPDATE Troskovi_Ugovori SET Troskovi_Ugovori.PDV = 0.2;


=====QUERY=====
xTmp_1_dell
-----SQL-----
INSERT INTO Racuni_Troskovi ( lnkGrp, lnk_IDSK, SumaE, SumaDin, NBS, Marker )
SELECT 1 AS mGRUPA, Troskovi_Ugovori.lnk_ID_SK, Sum([Vrednost]*(1+[PDV])) AS Suma, Sum([Vrednost]*(1+[PDV]))*[mKurs] AS Suma2, [mKurs] AS mKurs, 1208 AS mMesec
FROM Troskovi_Ugovori
GROUP BY 1, Troskovi_Ugovori.lnk_ID_SK, [mKurs], 1208;


=====QUERY=====
xxx_dell_povecanjepdva
-----SQL-----
UPDATE GK SET GK.PIZNOS = Round([PIZNOS]/1.18*1.2,2), GK.DIZNOS = Round([DIZNOS]/1.18*1.2,2)
WHERE (((GK.BR_NALOG)=2080));


=====QUERY=====
ZBIRNIRACUN-KREIRAJ
-----SQL-----
INSERT INTO Racun ( lnkGR, DatumIzdavanja, MestoIzdavanja, DatumUsluge, ID_K, ID_SK, Kupac, PBroj_K, Adresa_K, PIB_Naziv, PIB, Ukupno, StanjeDug, UkupnoDug, DatumValute, SvrhaUplate, Napomena, UpravljanjeZgradom, PozivNaBroj )
SELECT Racun.lnkGR, Racun.DatumIzdavanja, Racun.MestoIzdavanja, Racun.DatumUsluge, Kupac.ID_K, Racun.ID_SK, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB_N, Kupac.PIB, Sum(Racun.Ukupno*[IG_V]) AS [NR-UKUPNO], KUPAC_TS.STANJE, KUPAC_TS.STANJE+(Sum(Racun.Ukupno*[IG_V])) AS UD, Racun.DatumValute, Racun.SvrhaUplate, Racun.Napomena, Racun.UpravljanjeZgradom, KontrolniBroj(97,Mid([PozivNaBroj],3,7) & KUPAC.ID_K) & Mid([PozivNaBroj],3,7) & KUPAC.ID_K AS PAR
FROM (Kupac INNER JOIN ((Racun INNER JOIN Objekti ON Racun.ID_O = Objekti.ID_O) INNER JOIN TipObjekta ON Objekti.lnk_tip = TipObjekta.IDTipObj) ON Kupac.ID_K = TipObjekta.IG_KONTO) INNER JOIN KUPAC_TS ON Kupac.ID_K = KUPAC_TS.IDK
WHERE (((Racun.SPC)=1))
GROUP BY Racun.lnkGR, Racun.DatumIzdavanja, Racun.MestoIzdavanja, Racun.DatumUsluge, Kupac.ID_K, Racun.ID_SK, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB_N, Kupac.PIB, KUPAC_TS.STANJE, Racun.DatumValute, Racun.SvrhaUplate, Racun.Napomena, Racun.UpravljanjeZgradom, KontrolniBroj(97,Mid([PozivNaBroj],3,7) & KUPAC.ID_K) & Mid([PozivNaBroj],3,7) & KUPAC.ID_K, Objekti.lnk_tip
HAVING (((Racun.lnkGR)=117));


=====QUERY=====
ZBIRNIRACUN-KREIRAJ2
-----SQL-----
INSERT INTO Racun ( lnkGR, DatumIzdavanja, MestoIzdavanja, DatumUsluge, ID_K, ID_SK, Kupac, PBroj_K, Adresa_K, PIB_Naziv, PIB, Ukupno, StanjeDug, UkupnoDug, DatumValute, SvrhaUplate, Napomena, UpravljanjeZgradom, PozivNaBroj )
SELECT Racun.lnkGR, Racun.DatumIzdavanja, Racun.MestoIzdavanja, Racun.DatumUsluge, Kupac.ID_K, Racun.ID_SK, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB_N, Kupac.PIB, Sum(Racun.Ukupno*[CB_V]) AS [NR-UKUPNO], KUPAC_TS.STANJE, KUPAC_TS.STANJE+(Sum(Racun.Ukupno*[CB_V])) AS UD, Racun.DatumValute, Racun.SvrhaUplate, Racun.Napomena, Racun.UpravljanjeZgradom, KontrolniBroj(97,Mid([PozivNaBroj],3,7) & KUPAC.ID_K) & Mid([PozivNaBroj],3,7) & KUPAC.ID_K AS PAR
FROM ((Racun INNER JOIN Objekti ON Racun.ID_O=Objekti.ID_O) INNER JOIN TipObjekta ON Objekti.lnk_tip=TipObjekta.IDTipObj) INNER JOIN (Kupac INNER JOIN KUPAC_TS ON Kupac.ID_K=KUPAC_TS.IDK) ON TipObjekta.CB_KONTO=Kupac.ID_K
WHERE (((Racun.SPC)=1))
GROUP BY Racun.lnkGR, Racun.DatumIzdavanja, Racun.MestoIzdavanja, Racun.DatumUsluge, Kupac.ID_K, Racun.ID_SK, Kupac.Naziv, Kupac.PBroj, Kupac.Adresa, Kupac.PIB_N, Kupac.PIB, KUPAC_TS.STANJE, Racun.DatumValute, Racun.SvrhaUplate, Racun.Napomena, Racun.UpravljanjeZgradom, KontrolniBroj(97,Mid([PozivNaBroj],3,7) & KUPAC.ID_K) & Mid([PozivNaBroj],3,7) & KUPAC.ID_K, Objekti.lnk_tip
HAVING (((Racun.lnkGR)=117));


=====QUERY=====
ZbirRacuna
-----SQL-----
SELECT GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, Sum(Racun.Ukupno) AS SumOfUkupno, GrupaRacuna.ID_SK
FROM Racun INNER JOIN GrupaRacuna ON Racun.lnkGR = GrupaRacuna.IDGrupaRacuna
GROUP BY GrupaRacuna.GrupaRacunaFXN, GrupaRacuna.GrupaRacunaFXT, GrupaRacuna.ID_SK;


=====QUERY=====
ZK_LINK_RDOB_FOR_UPDATE
-----SQL-----
SELECT ZK.IDzk, Dobavljac_Racuni.IDTRRAC, Troskovi_PodKonta_DefDob.DefDob
FROM (ZK INNER JOIN Troskovi_PodKonta_DefDob ON (ZK.IDSK = Troskovi_PodKonta_DefDob.IDSZ) AND (ZK.PODKONTO = Troskovi_PodKonta_DefDob.Konto)) INNER JOIN Dobavljac_Racuni ON (ZK.PARAMETRI = Dobavljac_Racuni.MesecRacuna) AND (Troskovi_PodKonta_DefDob.IDSZ = Dobavljac_Racuni.SK_ID) AND (Dobavljac_Racuni.KontoKnjizenja = ZK.KNPODKONTO)
WHERE (((Dobavljac_Racuni.TipDokumenta)=9) AND ((Dobavljac_Racuni.MesecRacuna)='2606'));


=====QUERY=====
ZK_podkonta_obrada
-----SQL-----
SELECT ZK.IDSK, ZK.PODKONTO, ZK.PARAMETRI, Troskovi_PodKonta_DefDob.DefDob, Sum(ZK.DIZNOS) AS SumOfDIZNOS
FROM Troskovi_PodKonta_DefDob RIGHT JOIN ZK ON (Troskovi_PodKonta_DefDob.IDSZ = ZK.IDSK) AND (Troskovi_PodKonta_DefDob.Konto = ZK.PODKONTO)
WHERE (((ZK.RACUNID)>0))
GROUP BY ZK.IDSK, ZK.PODKONTO, ZK.PARAMETRI, Troskovi_PodKonta_DefDob.DefDob;


=====QUERY=====
ZK_UPDATE_001
-----SQL-----
SELECT ZK.KNPODKONTO, Troskovi_PodKonta.Kamata
FROM ZK INNER JOIN Troskovi_PodKonta ON ZK.PODKONTO = Troskovi_PodKonta.PodKonto;


=====QUERY=====
ZK_UPDATE_02
-----SQL-----
SELECT ZK.KNPODKONTO, Troskovi_PodKonta_DefDob.DefDob, ZK.KNDOB
FROM ZK INNER JOIN Troskovi_PodKonta_DefDob ON (Troskovi_PodKonta_DefDob.IDSZ = ZK.IDSK) AND (ZK.PODKONTO = Troskovi_PodKonta_DefDob.Konto);


=====QUERY=====
ZKGroupByGrID
-----SQL-----
SELECT ZK.PARTNERID, ZK.IDGR, Sum(ZK.DIZNOS) AS SumOfDIZNOS
FROM ZK
WHERE (((ZK.[IDGR])=77))
GROUP BY ZK.PARTNERID, ZK.IDGR;


```
