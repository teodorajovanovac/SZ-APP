namespace SzApp.Domain.LedgerBanking;

/// <summary>Legacy GK.TIP_STAVKE codes (9.2). Stored as ShortList(TableName=LedgerLineType).IndexValue.</summary>
public static class LedgerLineTypes
{
    public const string ShortListTable = "LedgerLineType";
    public const int BankStatement = 1;
    public const int Invoice = 3;
    public const int SupplierInvoice = 4;
    public const int Storno = 7;
    public const int Rebooking = 8;
    public const int ForeignCommission = 11;
    public const int Compensation = 91;
    public const int TakenOverBalance = 94;
    public const int Loan = 97;
    public const int TakenOverDebt = 98;
    public const int OpeningBalance = 99;
}

public static class LedgerAccounts
{
    public const string Customers = "2040";
    public const string Revenue = "4900";
    public const string Suppliers = "4350";
    public const string Expenses = "5590";
}

/// <summary>Legacy Dobavljac_Racuni.TipDokumenta, stored as ShortList(SupplierDocumentType).IndexValue.</summary>
public static class SupplierDocumentTypes
{
    public const int Planned = 1;   // "Predviđeni troškovi" -- posted together with the invoice batch
    public const int Actual = 2;    // "Izvršeni troškovi" -- posted on its own
    public const int CreditNote = 3; // odobrenje -- same as 2 with sides swapped, line type 7
}

public sealed record PostingLine(
    string Account,
    decimal Debit,
    decimal Credit,
    int LineType,
    DateOnly PostingDate,
    int? PartnerAccountId = null,
    string? SubAccountId = null,
    DateOnly? DueDate = null,
    string? DocumentRef = null,
    string? Parameters = null,
    int? InvoiceId = null,
    int? SupplierInvoiceId = null,
    int? CollectionPriority = null,
    string? Note = null,
    int? BankStatementLineId = null,
    int? ClosesDocumentType = null,
    string? Description = null);

/// <summary>One invoice's share of one supplier invoice (sum of that invoice's lines for it).</summary>
public sealed record InvoicePostingSource(
    int InvoiceId,
    int CustomerPartnerAccountId,
    string CustomerAccount,
    DateOnly DueDate,
    string? PaymentReference,
    int? SupplierInvoiceId,
    decimal Amount);

public sealed record SupplierPostingInfo(
    int Id,
    int DocumentType,
    int SupplierPartnerAccountId,
    string SupplierAccount,
    string? SubAccountId,
    int CollectionPriority,
    string? PaymentReference,
    string Caption,
    string? CodeName = null,
    decimal Amount = 0m,
    DateOnly InvoiceDate = default,
    DateOnly? PaymentDate = null,
    string? ClosesAccount = null);

public static class DocumentPostingRules
{
    public const int ParametersMaxLength = 25;

    public static string InvoiceDocumentRef(int periodYYMM) => $"R-{periodYYMM:0000}";

    /// <summary>Legacy <c>Replace(PozivNaBroj,'-','')</c>, clipped to the column width.</summary>
    public static string? CleanPaymentReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var clean = value.Replace("-", string.Empty, StringComparison.Ordinal).Trim();
        return clean.Length > ParametersMaxLength ? clean[..ParametersMaxLength] : clean;
    }

    /// <summary>
    /// 9.2 invoice batch posting (legacy <c>KnjizenjeRacuna</c>): 2040 D per invoice × supplier
    /// invoice (× its sub-account), 4900 C per supplier invoice, and for supplier document type 1
    /// also 4350 C (partner = supplier) + 5590 D. Zero groups are skipped. Result is balanced.
    /// </summary>
    public static IReadOnlyList<PostingLine> BuildInvoiceBatch(
        DateOnly postingDate,
        string documentRef,
        IEnumerable<InvoicePostingSource> sources,
        IReadOnlyDictionary<int, SupplierPostingInfo> suppliers)
    {
        var customerLines = new List<PostingLine>();
        foreach (var group in sources
                     .GroupBy(x => (x.InvoiceId, x.SupplierInvoiceId))
                     .OrderBy(x => x.Key.InvoiceId).ThenBy(x => x.Key.SupplierInvoiceId ?? 0))
        {
            var amount = FinanceRounding.Money(group.Sum(x => x.Amount));
            if (amount == 0m) continue;
            var first = group.First();
            var supplier = SupplierFor(suppliers, group.Key.SupplierInvoiceId);
            customerLines.Add(new PostingLine(
                first.CustomerAccount, amount, 0m, LedgerLineTypes.Invoice, postingDate,
                PartnerAccountId: first.CustomerPartnerAccountId,
                SubAccountId: supplier?.SubAccountId,
                DueDate: first.DueDate,
                DocumentRef: documentRef,
                Parameters: CleanPaymentReference(first.PaymentReference),
                InvoiceId: first.InvoiceId,
                SupplierInvoiceId: group.Key.SupplierInvoiceId,
                CollectionPriority: supplier?.CollectionPriority));
        }

        return customerLines.Concat(BuildSupplierSide(customerLines, postingDate, documentRef, suppliers, LedgerLineTypes.Invoice)).ToArray();
    }

    /// <summary>
    /// P9 storno of one invoice (legacy <c>KnjizenjeRacunaStorno</c>): the invoice's posted 2040
    /// lines copied with negative amounts on the same side, plus the matching negative 4900 (and
    /// 4350/5590 for supplier document type 1) rebuilt from them. All lines type 7, storno date.
    /// </summary>
    public static IReadOnlyList<PostingLine> BuildInvoiceStorno(
        DateOnly stornoDate,
        IEnumerable<PostingLine> postedCustomerLines,
        IReadOnlyDictionary<int, SupplierPostingInfo> suppliers)
    {
        var customer = postedCustomerLines
            .Select(x => x with { Debit = -x.Debit, Credit = -x.Credit, LineType = LedgerLineTypes.Storno, PostingDate = stornoDate })
            .ToArray();
        var documentRef = customer.FirstOrDefault()?.DocumentRef;
        return customer.Concat(BuildSupplierSide(customer, stornoDate, documentRef, suppliers, LedgerLineTypes.Storno)).ToArray();
    }

    /// <summary>Generic red storno of any journal: same lines, negated, line type 7.</summary>
    public static IReadOnlyList<PostingLine> Negate(IEnumerable<PostingLine> lines, DateOnly stornoDate) =>
        lines.Select(x => x with { Debit = -x.Debit, Credit = -x.Credit, LineType = LedgerLineTypes.Storno, PostingDate = stornoDate })
            .ToArray();

    /// <summary>
    /// Supplier invoice posting (legacy <c>GK_KnjizenjeRacunaTroska</c>): type 2 → 4350 C on the
    /// supplier / 5590 D (or ClosesAccount), line type 4; type 3 (credit note) → sides swapped,
    /// line type 7. Type 1 is posted with the invoice batch and type 9 has its own scheme.
    /// </summary>
    public static IReadOnlyList<PostingLine> BuildSupplierInvoice(SupplierPostingInfo s)
    {
        if (s.DocumentType is not (SupplierDocumentTypes.Actual or SupplierDocumentTypes.CreditNote))
        {
            throw new DomainRuleException("posting.supplier-document-type", "Samostalno se knjiže samo ulazni računi tipa 2 (izvršeni trošak) i 3 (odobrenje).");
        }

        var amount = FinanceRounding.Money(s.Amount);
        if (amount == 0m)
        {
            throw new DomainRuleException("posting.zero-amount", "Ulazni račun nema iznos za knjiženje.");
        }

        var creditNote = s.DocumentType == SupplierDocumentTypes.CreditNote;
        var lineType = creditNote ? LedgerLineTypes.Storno : LedgerLineTypes.SupplierInvoice;
        var counterAccount = string.IsNullOrWhiteSpace(s.ClosesAccount) ? LedgerAccounts.Expenses : s.ClosesAccount.Trim();
        return
        [
            new PostingLine(s.SupplierAccount, creditNote ? amount : 0m, creditNote ? 0m : amount, lineType, s.InvoiceDate,
                PartnerAccountId: s.SupplierPartnerAccountId, SubAccountId: s.SubAccountId, DueDate: s.PaymentDate,
                DocumentRef: s.CodeName, Parameters: CleanPaymentReference(s.PaymentReference),
                SupplierInvoiceId: s.Id, CollectionPriority: s.CollectionPriority, Note: s.Caption),
            new PostingLine(counterAccount, creditNote ? 0m : amount, creditNote ? amount : 0m, lineType, s.InvoiceDate,
                SubAccountId: s.SubAccountId, DueDate: s.PaymentDate, DocumentRef: s.CodeName,
                Parameters: CleanPaymentReference(s.PaymentReference), SupplierInvoiceId: s.Id, Note: s.Caption)
        ];
    }

    private static IEnumerable<PostingLine> BuildSupplierSide(
        IReadOnlyCollection<PostingLine> customerLines,
        DateOnly postingDate,
        string? documentRef,
        IReadOnlyDictionary<int, SupplierPostingInfo> suppliers,
        int customerLineType)
    {
        var supplierLineType = customerLineType == LedgerLineTypes.Storno ? LedgerLineTypes.Storno : LedgerLineTypes.SupplierInvoice;
        foreach (var group in customerLines.GroupBy(x => x.SupplierInvoiceId).OrderBy(x => x.Key ?? 0))
        {
            var amount = FinanceRounding.Money(group.Sum(x => x.Debit));
            if (amount == 0m) continue;
            var supplier = SupplierFor(suppliers, group.Key);
            yield return new PostingLine(LedgerAccounts.Revenue, 0m, amount, customerLineType, postingDate,
                SubAccountId: supplier?.SubAccountId, DocumentRef: documentRef, SupplierInvoiceId: group.Key, Note: supplier?.Caption);

            if (supplier?.DocumentType != SupplierDocumentTypes.Planned) continue;
            yield return new PostingLine(supplier.SupplierAccount, 0m, amount, supplierLineType, postingDate,
                PartnerAccountId: supplier.SupplierPartnerAccountId, SubAccountId: supplier.SubAccountId, DocumentRef: documentRef,
                Parameters: CleanPaymentReference(supplier.PaymentReference), SupplierInvoiceId: supplier.Id, Note: supplier.Caption);
            yield return new PostingLine(LedgerAccounts.Expenses, amount, 0m, supplierLineType, postingDate,
                SubAccountId: supplier.SubAccountId, DocumentRef: documentRef, SupplierInvoiceId: supplier.Id, Note: supplier.Caption);
        }
    }

    private static SupplierPostingInfo? SupplierFor(IReadOnlyDictionary<int, SupplierPostingInfo> suppliers, int? id) =>
        id is { } key
            ? suppliers.TryGetValue(key, out var s) ? s : throw new DomainRuleException("posting.supplier-invoice-missing", $"Ulazni račun {key} nije pronađen.")
            : null;
}
