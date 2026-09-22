namespace SzApp.Contracts.Billing;

public sealed record BillingPage<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public sealed record CalculationTypeResponse(
    int Id,
    string Name,
    string SupplierAmountRule,
    string AllocationRule,
    string QuantityRule,
    int UnitOfMeasureId,
    string? Note);

public sealed record CreateCalculationTypeRequest(
    string Name,
    string SupplierAmountRule,
    string AllocationRule,
    string QuantityRule,
    int UnitOfMeasureId,
    string? Note);

public sealed record SupplierInvoiceResponse(
    int Id,
    int InvoiceNo,
    string CodeName,
    string Caption,
    int CalculationTypeId,
    int PeriodYYMM,
    decimal AmountEur,
    decimal AmountRsd,
    decimal PostedAmount,
    DateOnly InvoiceDate,
    DateOnly TransactionDate,
    IReadOnlyList<int> UnitTypeIds,
    string RowVersion);

public sealed record CreateSupplierInvoiceRequest(
    int InvoiceNo,
    string CodeName,
    string Caption,
    int SupplierPartnerAccountId,
    int CalculationTypeId,
    int PeriodYYMM,
    decimal InvoiceTotalCalculationAmountEur,
    decimal InvoiceTotalCalculationAmountRsd,
    decimal CalculationAmountByCoefficientEur,
    decimal CalculationAmountByCoefficientRsd,
    int PaymentPriority,
    string? SubAccountId,
    int DocumentTypeId,
    string? ExtraordinaryInvoiceMarker,
    string? InvoiceNameRule,
    DateOnly InvoiceDate,
    DateOnly TransactionDate,
    DateOnly? PaymentDate,
    string? InvoiceDescription,
    string? PaymentReference,
    IReadOnlyList<int> UnitTypeIds);

public sealed record InvoiceBatchResponse(
    int Id,
    int PeriodYYMM,
    string Caption,
    int Month,
    int Year,
    DateOnly IssueDate,
    DateOnly DueDate,
    decimal ExchangeRateNbs,
    string Status,
    int? JournalEntryId,
    string RowVersion);

public sealed record CreateInvoiceBatchRequest(
    int PeriodYYMM,
    string Caption,
    int Month,
    int Year,
    string Place,
    DateOnly IssueDate,
    DateOnly ServiceDateFrom,
    DateOnly ServiceDateTo,
    DateOnly TransactionDate,
    DateOnly DueDate,
    decimal ExchangeRateNbs,
    string? ExtraordinaryInvoiceMarker,
    DateOnly? BalanceAsOfDate,
    DateOnly? PreviousValueDate,
    bool IsInterestCalculated,
    string? PaymentPurpose);

public sealed record InvoiceLineSeedRequest(
    int? SupplierInvoiceId,
    string Name,
    decimal Quantity,
    int? UnitOfMeasureId,
    decimal UnitPrice,
    decimal VatRate,
    decimal K1 = 1m,
    decimal K2 = 1m,
    decimal K3 = 1m,
    decimal K4 = 1m,
    decimal K5 = 1m,
    int SortIndex = 0);

public sealed record InvoiceSeedRequest(
    int PartnerId,
    string SequenceNumber,
    string PartnerName,
    string Address,
    string? PostalCode,
    string City,
    string? TaxNumber,
    string? RegistrationNumber,
    string Currency,
    string? InvoiceDeliveryLocation,
    string? DeliveryLocation,
    string? PaymentReference,
    decimal PreviousBalance,
    decimal BenefitAmount,
    decimal InterestAmount,
    int SortIndex,
    IReadOnlyList<int> ContractIds,
    IReadOnlyList<InvoiceLineSeedRequest> Lines);

public sealed record InvoiceGenerationRequest(IReadOnlyList<InvoiceSeedRequest> Invoices);

public sealed record InvoicePreviewLineResponse(
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal NetAmount,
    decimal VatAmount,
    decimal TotalAmount);

public sealed record InvoicePreviewResponse(
    int PartnerId,
    string SequenceNumber,
    decimal NetAmount,
    decimal BenefitAmount,
    decimal VatAmount,
    decimal InterestAmount,
    decimal TotalAmount,
    IReadOnlyList<InvoicePreviewLineResponse> Lines);

public sealed record InvoiceBatchPreviewResponse(
    int BatchId,
    string Fingerprint,
    int InvoiceCount,
    decimal NetAmount,
    decimal VatAmount,
    decimal InterestAmount,
    decimal TotalAmount,
    IReadOnlyList<InvoicePreviewResponse> Invoices);

public sealed record InvoiceBatchGenerationResponse(
    int BatchId,
    bool AlreadyGenerated,
    string Fingerprint,
    IReadOnlyList<int> InvoiceIds);

public sealed record InvoiceLineResponse(
    int Id,
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal NetAmount,
    decimal VatRate,
    decimal VatAmount,
    decimal TotalAmount,
    int SortIndex);

public sealed record InvoiceResponse(
    int Id,
    int PartnerId,
    int? InvoiceBatchId,
    string SequenceNumber,
    DateOnly IssueDate,
    DateOnly DueDate,
    string PartnerName,
    string Address,
    string? PostalCode,
    string City,
    string Currency,
    decimal Amount,
    decimal VatAmount,
    decimal Total,
    decimal InterestAmount,
    decimal InvoiceTotal,
    bool IsCancelled,
    string RowVersion,
    IReadOnlyList<InvoiceLineResponse>? Lines = null);

public sealed record CancelInvoiceRequest(string Reason, string RowVersion);

public sealed record BenefitResponse(int Id, int ContractId, int PeriodYYMM, decimal Amount, int? InvoiceId, string RowVersion);
public sealed record CreateBenefitRequest(int ContractId, int PeriodYYMM, decimal Amount);

public sealed record InterestRateResponse(int Id, DateOnly Date, decimal Rate, string TimeCode);
public sealed record CreateInterestRateRequest(DateOnly Date, decimal Rate, string TimeCode);
public sealed record CalculateInterestRequest(decimal Principal, DateOnly From, DateOnly To);
public sealed record InterestCalculationLineResponse(DateOnly From, DateOnly To, int Days, decimal Rate, decimal Coefficient, decimal Interest);
public sealed record InterestCalculationResponse(decimal Principal, decimal TotalInterest, IReadOnlyList<InterestCalculationLineResponse> Lines);
public sealed record CreateInterestStatementRequest(
    string Account,
    decimal Principal,
    decimal Balance,
    int PartnerAccountId,
    string? SubAccountId,
    int InvoiceBatchId,
    DateOnly From,
    DateOnly To);
public sealed record InterestStatementResponse(
    int Id,
    string Account,
    DateOnly Date,
    decimal Amount,
    decimal Balance,
    int Days,
    decimal Rate,
    decimal Coefficient,
    decimal Interest,
    int PartnerAccountId,
    int InvoiceBatchId);

public sealed record NoticeTemplateResponse(int Id, string Name, string Body, bool IsActive, string RowVersion);
public sealed record CreateNoticeTemplateRequest(string Name, string Body);
public sealed record NoticeBatchResponse(int Id, string Title, DateOnly Date, int NoticeTemplateId, int NoticeTypeId, string RowVersion);
public sealed record CreateNoticeBatchRequest(
    string Title,
    DateOnly Date,
    int MinUnpaidInvoiceCount,
    decimal DebtTolerance,
    decimal DebtToleranceByMonth,
    int NoticeTemplateId,
    int NoticeTypeId,
    DateOnly UpToClaimDate,
    DateOnly UpToPaymentDate,
    int? InvoiceBatchId,
    string? CustomCaptionOnSlip);

public sealed record NoticeLineSeedRequest(
    string DocumentRef,
    decimal Debit,
    decimal Credit,
    string Text,
    DateOnly DueDate,
    int? InvoiceId,
    DateOnly? InvoiceDate,
    string? UnitAddress);

public sealed record NoticeSeedRequest(
    int PartnerAccountId,
    int UnpaidInvoiceCount,
    decimal Debt,
    string? InvoiceText,
    string PaymentReference,
    decimal AdditionalCosts,
    IReadOnlyList<NoticeLineSeedRequest> Lines);

public sealed record GenerateNoticesRequest(IReadOnlyList<NoticeSeedRequest> Notices);
public sealed record NoticeGenerationResponse(int BatchId, bool AlreadyGenerated, IReadOnlyList<int> NoticeIds);
public sealed record NoticeResponse(
    int Id,
    int NoticeBatchId,
    int PartnerAccountId,
    int UnpaidInvoiceCount,
    decimal Debt,
    decimal AdditionalCosts,
    decimal Total,
    string PaymentReference,
    string DeliveryStatus,
    string? RenderedDocumentPath,
    string RowVersion);

public sealed record PaymentOrderResponse(
    int Id,
    string TemplateTitle,
    string PayerName,
    string RecipientName,
    string PaymentPurpose,
    decimal Amount,
    string Currency,
    DateOnly Date,
    DateOnly ValueDate,
    bool IsUrgent,
    bool IsFavorite,
    bool IsArchived,
    string RowVersion);

public sealed record CreatePaymentOrderRequest(
    string TemplateTitle,
    string PayerName,
    string PaymentPurpose,
    string RecipientName,
    int PaymentCode,
    string Currency,
    decimal Amount,
    string PayerAccountNumber,
    int? PayerModelNumber,
    string? PayerPaymentReference,
    string RecipientAccountNumber,
    int? RecipientModelNumber,
    string? RecipientPaymentReference,
    string Place,
    DateOnly Date,
    DateOnly ValueDate,
    bool IsUrgent,
    int PaymentOrderTypeId,
    bool IsFavorite);
