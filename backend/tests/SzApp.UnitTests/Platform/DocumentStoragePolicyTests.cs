using SzApp.Domain.Platform;

namespace SzApp.UnitTests.Platform;

public sealed class DocumentStoragePolicyTests
{
    [Fact]
    public void Validate_RejectsPathTraversalFileName()
    {
        Assert.Throws<InvalidOperationException>(() => DocumentStoragePolicy.Validate(
            1, "../racun.pdf", "application/pdf", 100, DateTimeOffset.UtcNow, Guid.Empty));
    }

    [Fact]
    public void Validate_RejectsMimeExtensionMismatch()
    {
        Assert.Throws<InvalidOperationException>(() => DocumentStoragePolicy.Validate(
            1, "racun.pdf", "image/png", 100, DateTimeOffset.UtcNow, Guid.Empty));
    }

    [Fact]
    public void ResolveUnderRoot_RejectsEscapingRelativePath()
    {
        var root = Path.Combine(Path.GetTempPath(), "szapp-storage-root");
        Assert.Throws<InvalidOperationException>(() => DocumentStoragePolicy.ResolveUnderRoot(root, Path.Combine("..", "outside.pdf")));
    }

    [Fact]
    public void Validate_CreatesTenantScopedOpaquePath()
    {
        var value = DocumentStoragePolicy.Validate(
            42, "račun.pdf", "application/pdf", 100, new DateTimeOffset(2026, 9, 22, 0, 0, 0, TimeSpan.Zero), Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));

        Assert.StartsWith(Path.Combine("42", "2026", "09"), value.RelativePath, StringComparison.Ordinal);
        Assert.DoesNotContain("račun", value.RelativePath, StringComparison.OrdinalIgnoreCase);
    }
}

