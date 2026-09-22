# ADR 0001: Modularni monolit

Status: Accepted

Backend je .NET 10 modularni monolit. Poslovni moduli su organizovani kao vertikalni
feature folderi, dok EF Core model, javni ugovori i finansijske primitive ostaju u
zajedničkim projektima. Ne uvode se CQRS framework, message broker ili mikroservisi dok
merena potreba to ne opravda.

Frontend je jedan React/TypeScript SPA. OpenAPI je izvor generisanog API klijenta.
