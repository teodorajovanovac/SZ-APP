using System.Security.Cryptography;

namespace SzApp.Domain.Platform;

public sealed record ValidatedDocument(string SafeFileName, string Extension, string RelativePath);

public static class DocumentStoragePolicy
{
    public const long MaximumFileSize = 20 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string[]> AllowedTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ["application/pdf"],
            [".png"] = ["image/png"],
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".csv"] = ["text/csv", "application/vnd.ms-excel", "text/plain"],
            [".xml"] = ["application/xml", "text/xml"],
            [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"]
        };

    public static ValidatedDocument Validate(
        int companyId,
        string fileName,
        string contentType,
        long length,
        DateTimeOffset now,
        Guid storageId)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (length <= 0 || length > MaximumFileSize) throw new InvalidOperationException("Veličina dokumenta nije dozvoljena.");
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName) || safeName != fileName || safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidOperationException("Naziv dokumenta nije dozvoljen.");
        var extension = Path.GetExtension(safeName).ToLowerInvariant();
        if (!AllowedTypes.TryGetValue(extension, out var mimeTypes) || !mimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Kombinacija tipa i ekstenzije dokumenta nije dozvoljena.");
        var relative = Path.Combine(companyId.ToString(), now.Year.ToString("0000"), now.Month.ToString("00"), $"{storageId:N}{extension}");
        return new ValidatedDocument(safeName, extension, relative);
    }

    public static string ResolveUnderRoot(string rootPath, string relativePath)
    {
        var root = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Putanja dokumenta izlazi iz skladišnog direktorijuma.");
        return full;
    }

    public static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken)
    {
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hash);
    }
}

