using SzApp.Api.Features.MasterData;
using SzApp.Contracts.MasterData;

namespace SzApp.IntegrationTests.MasterData;

public sealed class MasterDataValidationTests
{
    [Fact]
    public void PartnerValidation_RejectsMissingNamesAndInvalidLengths()
    {
        var request = new SavePartnerRequest(
            string.Empty,
            string.Empty,
            new string('1', 11),
            null,
            null,
            null,
            null,
            null,
            "sr-Latn",
            null);

        var errors = MasterDataValidation.Validate(request);

        Assert.Contains(nameof(request.ShortName), errors.Keys);
        Assert.Contains(nameof(request.Name), errors.Keys);
        Assert.Contains(nameof(request.RegistrationNumber), errors.Keys);
    }

    [Fact]
    public void StaffAccessValidation_AcceptsKnownRoleCaseInsensitively()
    {
        var errors = MasterDataValidation.Validate(new SaveStaffAccessRequest(42, "moderator"));

        Assert.Empty(errors);
    }

    [Fact]
    public void ContractPeriodValidation_RejectsOverlapWithinSinglePeriod()
    {
        var errors = MasterDataValidation.ValidateContractPeriod(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 2, 28));

        Assert.Contains("effectiveTo", errors.Keys);
        Assert.Contains("invoiceTo", errors.Keys);
    }

    [Theory]
    [InlineData(new byte[] { 1, 2, 3, 4 })]
    [InlineData(new byte[] { 255, 0, 127, 18, 88, 6, 90, 41 })]
    public void ETagCodec_RoundTrips(byte[] rowVersion)
    {
        var encoded = ETagCodec.Encode(rowVersion);

        var success = ETagCodec.TryDecode(encoded, out var decoded);

        Assert.True(success);
        Assert.Equal(rowVersion, decoded);
    }

    [Fact]
    public void ETagCodec_RejectsMalformedValue()
    {
        Assert.False(ETagCodec.TryDecode("not-base64!", out _));
    }
}

