namespace SzApp.Domain.LedgerBanking;

/// <summary>One accepted allocation of a statement line, ready for posting.</summary>
public sealed record StatementPostingPart(int BankStatementLineId, bool Inflow, string PayerName, AllocationProposal Allocation);

public static class BankStatementPostingRules
{
    public static string DocumentRef(int statementNumber, DateOnly date) => $"IZVOD {statementNumber}/{date.Year}";

    /// <summary>
    /// 9.3 / FIN-14 / FIN-15: 2410 as TWO summary lines per statement (debit = all inflows, credit =
    /// all outflows), no partner, DocumentRef "IZVOD n/YYYY", line type 1; then one line per
    /// allocation on the partner's account (inflow → credit, outflow → debit) carrying the closed
    /// item's reference, document, invoice/supplier invoice, collection priority, sub-account and
    /// KNzaTIP. Every statement line must be fully allocated, so the journal balances by construction.
    /// </summary>
    public static IReadOnlyList<PostingLine> Build(
        string bankAccount,
        int statementNumber,
        DateOnly date,
        IReadOnlyCollection<(int LineId, decimal Debit, decimal Credit)> statementLines,
        IReadOnlyCollection<StatementPostingPart> parts)
    {
        foreach (var line in statementLines)
        {
            var allocated = FinanceRounding.Money(parts.Where(x => x.BankStatementLineId == line.LineId).Sum(x => x.Allocation.Amount));
            if (allocated != FinanceRounding.Money(line.Debit + line.Credit))
            {
                throw new DomainRuleException("statement.line-not-allocated", $"Stavka izvoda {line.LineId} nije u celosti raspoređena ({allocated:0.00}).");
            }
        }

        var reference = DocumentRef(statementNumber, date);
        var result = new List<PostingLine>();
        var inflows = FinanceRounding.Money(statementLines.Sum(x => x.Credit));
        var outflows = FinanceRounding.Money(statementLines.Sum(x => x.Debit));
        if (inflows != 0m)
            result.Add(new PostingLine(bankAccount, inflows, 0m, LedgerLineTypes.BankStatement, date, DueDate: date, DocumentRef: reference, Description: reference));
        if (outflows != 0m)
            result.Add(new PostingLine(bankAccount, 0m, outflows, LedgerLineTypes.BankStatement, date, DueDate: date, DocumentRef: reference, Description: reference));

        foreach (var part in parts.Where(x => x.Allocation.Amount != 0m))
        {
            var a = part.Allocation;
            var amount = FinanceRounding.Money(a.Amount);
            result.Add(new PostingLine(
                a.Account,
                part.Inflow ? 0m : amount,
                part.Inflow ? amount : 0m,
                LedgerLineTypes.BankStatement,
                date,
                PartnerAccountId: a.PartnerAccountId,
                SubAccountId: a.SubAccountId,
                DueDate: date,
                DocumentRef: a.DocumentRef,
                Parameters: a.Parameters,
                InvoiceId: a.InvoiceId,
                SupplierInvoiceId: a.SupplierInvoiceId,
                CollectionPriority: a.CollectionPriority,
                BankStatementLineId: part.BankStatementLineId,
                ClosesDocumentType: a.ClosesDocumentType,
                Description: part.PayerName.Length > 255 ? part.PayerName[..255] : part.PayerName));
        }

        return result;
    }
}
