# ADR 0004: Dinamički SQL

Status: Accepted

Interaktivne analize smeju izvršavati samo jednu parametrizovanu `SELECT` naredbu preko
read-only SQL naloga, uz timeout, maksimalan broj redova i audit. DML, DDL, multi-statement
batch, komentari koji skrivaju naredbe i report pre-execute SQL nisu dozvoljeni.

Legacy action query-ji postaju imenovane aplikativne komande. Nazivi funkcija, tabela i
polja iz konfiguracionih tabela razrešavaju se kroz allowlist registry, nikada kroz
reflection ili direktno spajanje SQL stringova.
