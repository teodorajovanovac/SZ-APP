using System.Security.Cryptography;
using System.Text;

namespace SzApp.Etl.Csv;

public sealed record CsvRow(
    int RowNumber,
    string RawText,
    IReadOnlyDictionary<string, string> Values,
    string RowHash,
    // ponytail (#5): set when the row's column count didn't match the header (a structural shape
    // error) — Values is empty/unusable for such a row. Null means the row parsed cleanly (Values
    // may still fail per-field validation later in GenericRowValidator, a separate concern).
    string? StructuralError = null);

public sealed record CsvDocument(
    IReadOnlyList<string> Headers,
    IReadOnlyList<CsvRow> Rows);

public static class LegacyEncoding
{
    static LegacyEncoding() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static Encoding Resolve(string encodingName) => encodingName.Trim().ToLowerInvariant() switch
    {
        "windows-1250" or "cp1250" or "1250" =>
            Encoding.GetEncoding(1250, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback),
        "utf-8" or "utf8" => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
        _ => throw new ArgumentException($"Kodiranje '{encodingName}' nije podržano. Dozvoljeni su windows-1250 i utf-8.", nameof(encodingName))
    };
}

public sealed class LegacyCsvParser
{
    public async Task<CsvDocument> ParseAsync(
        Stream stream,
        string encodingName,
        char delimiter = ';',
        CancellationToken cancellationToken = default)
    {
        if (delimiter is '\r' or '\n' or '"')
        {
            throw new ArgumentException("Delimiter nije dozvoljen.", nameof(delimiter));
        }

        using var reader = new StreamReader(
            stream,
            LegacyEncoding.Resolve(encodingName),
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);
        var records = await ReadRecordsAsync(reader, delimiter, cancellationToken);
        if (records.Count == 0)
        {
            throw new InvalidDataException("CSV fajl nema zaglavlje.");
        }

        var headers = records[0].Fields.Select(NormalizeHeader).ToArray();
        if (headers.Any(string.IsNullOrWhiteSpace) || headers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != headers.Length)
        {
            throw new InvalidDataException("CSV zaglavlja moraju biti neprazna i jedinstvena.");
        }

        var rows = new List<CsvRow>(Math.Max(records.Count - 1, 0));
        for (var index = 1; index < records.Count; index++)
        {
            var record = records[index];
            if (record.Fields.Count != headers.Length)
            {
                // ponytail (#5): a bad column count used to throw and abort the WHOLE file before any
                // row reached quarantine — unlike value-format errors, which correctly quarantine
                // per-row (see GenericRowValidator in EtlPipelineService). Quarantine this row instead
                // and keep parsing the rest of the file; EtlPipelineService.ValidateAndStageAsync turns
                // a non-null StructuralError into a QuarantineRecord.
                var empty = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                rows.Add(new CsvRow(
                    record.StartLine,
                    record.RawText,
                    empty,
                    ComputeHash(headers, empty),
                    $"Red {record.StartLine} ima {record.Fields.Count} kolona; očekivano je {headers.Length}."));
                continue;
            }

            var values = headers.Zip(record.Fields, (header, value) => (header, value))
                .ToDictionary(x => x.header, x => x.value.Trim(), StringComparer.OrdinalIgnoreCase);
            rows.Add(new CsvRow(record.StartLine, record.RawText, values, ComputeHash(headers, values)));
        }

        return new CsvDocument(headers, rows);
    }

    public static string ComputeHash(
        IEnumerable<string> headers,
        IReadOnlyDictionary<string, string> values)
    {
        var canonical = string.Join('\u001f', headers.Select(x => $"{x}={values.GetValueOrDefault(x, string.Empty)}"));
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string NormalizeHeader(string value) => value.Trim().TrimStart('\uFEFF');

    private static async Task<List<ParsedRecord>> ReadRecordsAsync(
        TextReader reader,
        char delimiter,
        CancellationToken cancellationToken)
    {
        var records = new List<ParsedRecord>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var raw = new StringBuilder();
        var inQuotes = false;
        var line = 1;
        var recordStartLine = 1;
        var singleCharacter = new char[1];

        async ValueTask<int> ReadCharacterAsync()
        {
            var count = await reader.ReadAsync(singleCharacter.AsMemory(0, 1), cancellationToken);
            return count == 0 ? -1 : singleCharacter[0];
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await ReadCharacterAsync();
            if (read < 0)
            {
                if (inQuotes)
                {
                    throw new InvalidDataException($"Nezatvoren navodnik u redu {recordStartLine}.");
                }

                if (field.Length > 0 || fields.Count > 0 || raw.Length > 0)
                {
                    fields.Add(field.ToString());
                    records.Add(new ParsedRecord(recordStartLine, fields.ToArray(), raw.ToString()));
                }

                break;
            }

            var current = (char)read;
            raw.Append(current);
            if (current == '"')
            {
                if (inQuotes && reader.Peek() == '"')
                {
                    await ReadCharacterAsync();
                    raw.Append('"');
                    field.Append('"');
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (!inQuotes && current == delimiter)
            {
                fields.Add(field.ToString());
                field.Clear();
                continue;
            }

            if (!inQuotes && current is '\r' or '\n')
            {
                if (current == '\r' && reader.Peek() == '\n')
                {
                    await ReadCharacterAsync();
                    raw.Append('\n');
                }

                fields.Add(field.ToString());
                records.Add(new ParsedRecord(recordStartLine, fields.ToArray(), raw.ToString().TrimEnd('\r', '\n')));
                fields = [];
                field.Clear();
                line++;
                recordStartLine = line;
                raw.Clear();
                continue;
            }

            if (current == '\n')
            {
                line++;
            }

            field.Append(current);
        }

        return records;
    }

    private sealed record ParsedRecord(int StartLine, IReadOnlyList<string> Fields, string RawText);
}
