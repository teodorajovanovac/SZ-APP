using Microsoft.EntityFrameworkCore;

namespace SzApp.Data;

public interface IShortListValidator
{
    Task EnsureTypeAsync(int? shortListId, string expectedTableName, CancellationToken cancellationToken = default);
}

public sealed class ShortListValidator(SzAppDbContext dbContext) : IShortListValidator
{
    public async Task EnsureTypeAsync(int? shortListId, string expectedTableName, CancellationToken cancellationToken = default)
    {
        if (shortListId is null)
        {
            return;
        }

        var actualTableName = await dbContext.ShortLists
            .Where(x => x.Id == shortListId)
            .Select(x => x.TableName)
            .SingleOrDefaultAsync(cancellationToken);

        if (!string.Equals(actualTableName, expectedTableName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"ShortList {shortListId} mora pripadati grupi '{expectedTableName}'.");
        }
    }
}
