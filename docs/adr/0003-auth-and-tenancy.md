# ADR 0003: Autentikacija i tenant izolacija

Status: Accepted

Koristi se ASP.NET Core Identity sa `int` ključem i secure, HttpOnly, SameSite cookie
sesijom. Globalna rola je `Root`; `Upravnik`, `Moderator` i `Review` se dodeljuju po
kompaniji kroz `StaffAccess`.

Svaki company-scoped endpoint proverava i route `companyId` i pripadnost povezanih
entiteta. `CompanyId IS NULL` ne znači automatski javni pristup: globalni red mora biti
eksplicitno dozvoljen konkretnom use-case-u.
