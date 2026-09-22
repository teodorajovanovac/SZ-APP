namespace SzApp.Domain.Platform;

public interface IImportAllowlistRegistry
{
    bool IsAllowedTarget(string table, string field);
    bool IsAllowedLookup(string table, string keyField, string valueField);
}

public sealed class ImportAllowlistRegistry : IImportAllowlistRegistry
{
    private static readonly IReadOnlyDictionary<string, HashSet<string>> Targets =
        new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Partner"] = new(["ShortName", "Name", "RegistrationNumber", "TaxNumber", "Jbkjs", "Language"], StringComparer.OrdinalIgnoreCase),
            ["Address"] = new(["Address", "PostalCode", "City", "CountryCode"], StringComparer.OrdinalIgnoreCase),
            ["BankStatement"] = new(["StatementNumber", "StatementDate", "PreviousBalance", "NewBalance"], StringComparer.OrdinalIgnoreCase),
            ["BankStatementLine"] = new(["Amount", "ValueDate", "Description", "PaymentReference"], StringComparer.OrdinalIgnoreCase)
        };

    public bool IsAllowedTarget(string table, string field) =>
        Targets.TryGetValue(table, out var fields) && fields.Contains(field);

    public bool IsAllowedLookup(string table, string keyField, string valueField) =>
        table.Equals("Partner", StringComparison.OrdinalIgnoreCase) &&
        new[] { "TaxNumber", "RegistrationNumber", "Id" }.Contains(keyField, StringComparer.OrdinalIgnoreCase) &&
        valueField.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
        table.Equals("BankAccount", StringComparison.OrdinalIgnoreCase) &&
        keyField.Equals("AccountNumber", StringComparison.OrdinalIgnoreCase) &&
        valueField.Equals("Id", StringComparison.OrdinalIgnoreCase);
}

