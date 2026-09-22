using SzApp.Contracts.MasterData;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.MasterData;

public static class MasterDataValidation
{
    public static Dictionary<string, string[]> Validate(CreateCompanyRequest request) => ValidateCompany(
        request.PartnerId,
        request.ManagerId,
        request.ShortName,
        request.PrintName,
        request.RelativeFolderName);

    public static Dictionary<string, string[]> Validate(UpdateCompanyRequest request)
    {
        var errors = ValidateCompany(
            request.PartnerId,
            request.ManagerId,
            request.ShortName,
            request.PrintName,
            request.RelativeFolderName);
        if (!ETagCodec.TryDecode(request.RowVersion, out _))
        {
            errors[nameof(request.RowVersion)] = ["RowVersion mora biti validan Base64 ETag."];
        }
        return errors;
    }

    public static Dictionary<string, string[]> Validate(SavePartnerRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, nameof(request.ShortName), request.ShortName, 100);
        Required(errors, nameof(request.Name), request.Name, 255);
        OptionalMax(errors, nameof(request.RegistrationNumber), request.RegistrationNumber, 10);
        OptionalMax(errors, nameof(request.TaxNumber), request.TaxNumber, 10);
        OptionalMax(errors, nameof(request.Jbkjs), request.Jbkjs, 10);
        OptionalMax(errors, nameof(request.IdCardNumber), request.IdCardNumber, 20);
        OptionalMax(errors, nameof(request.Jmbg), request.Jmbg, 15);
        Required(errors, nameof(request.Language), request.Language, 10);
        return errors;
    }

    public static Dictionary<string, string[]> Validate(SaveAddressRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, nameof(request.StreetAddress), request.StreetAddress, 255);
        OptionalMax(errors, nameof(request.PostalCode), request.PostalCode, 20);
        Required(errors, nameof(request.City), request.City, 255);
        Required(errors, nameof(request.CountryCode), request.CountryCode, 2);
        if (!string.IsNullOrWhiteSpace(request.CountryCode) && request.CountryCode.Trim().Length != 2)
        {
            errors[nameof(request.CountryCode)] = ["CountryCode mora imati tačno dva znaka."];
        }
        return errors;
    }

    public static Dictionary<string, string[]> Validate(SaveStaffAccessRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.StaffId <= 0)
        {
            errors[nameof(request.StaffId)] = ["StaffId mora biti pozitivan."];
        }
        if (!Enum.TryParse<StaffRole>(request.StaffRole, true, out _))
        {
            errors[nameof(request.StaffRole)] = ["StaffRole mora biti Upravnik, Moderator ili Review."];
        }
        return errors;
    }

    public static Dictionary<string, string[]> ValidateContractPeriod(
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        DateOnly? invoiceFrom,
        DateOnly? invoiceTo)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (effectiveTo < effectiveFrom)
        {
            errors[nameof(effectiveTo)] = ["Datum završetka ugovora ne može biti pre početka."];
        }
        if (invoiceTo.HasValue && !invoiceFrom.HasValue)
        {
            errors[nameof(invoiceFrom)] = ["Početak fakturisanja je obavezan kada postoji kraj fakturisanja."];
        }
        if (invoiceFrom.HasValue && invoiceTo < invoiceFrom)
        {
            errors[nameof(invoiceTo)] = ["Kraj fakturisanja ne može biti pre početka fakturisanja."];
        }
        return errors;
    }

    private static Dictionary<string, string[]> ValidateCompany(
        int partnerId,
        int? managerId,
        string shortName,
        string printName,
        string? relativeFolderName)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (partnerId <= 0)
        {
            errors[nameof(partnerId)] = ["PartnerId mora biti pozitivan."];
        }
        if (managerId <= 0)
        {
            errors[nameof(managerId)] = ["ManagerId mora biti pozitivan kada je naveden."];
        }
        Required(errors, nameof(shortName), shortName, 50);
        Required(errors, nameof(printName), printName, 50);
        OptionalMax(errors, nameof(relativeFolderName), relativeFolderName, 50);
        return errors;
    }

    private static void Required(
        IDictionary<string, string[]> errors,
        string field,
        string? value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = ["Polje je obavezno."];
        }
        else if (value.Trim().Length > maxLength)
        {
            errors[field] = [$"Polje može imati najviše {maxLength} znakova."];
        }
    }

    private static void OptionalMax(
        IDictionary<string, string[]> errors,
        string field,
        string? value,
        int maxLength)
    {
        if (value?.Trim().Length > maxLength)
        {
            errors[field] = [$"Polje može imati najviše {maxLength} znakova."];
        }
    }
}

public static class ETagCodec
{
    public static string Encode(byte[] rowVersion) => $"\"{Convert.ToBase64String(rowVersion)}\"";

    public static bool TryDecode(string? value, out byte[] rowVersion)
    {
        rowVersion = [];
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (normalized.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[2..].Trim();
        }
        normalized = normalized.Trim('"');

        try
        {
            rowVersion = Convert.FromBase64String(normalized);
            return rowVersion.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

