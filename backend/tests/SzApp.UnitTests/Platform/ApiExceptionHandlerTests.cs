using SzApp.Api.Infrastructure;

namespace SzApp.UnitTests.Platform;

public sealed class ApiExceptionHandlerTests
{
    // A DbUpdateException wrapping a SQL Server unique-violation (error 2601/2627) is an
    // expected outcome of a race (duplicate import, racing idempotent requests) and must
    // map to 409, not fall into ApiExceptionHandler's generic 500 branch. SqlException has
    // no public constructor, so this tests the error-number predicate directly rather than
    // building a full exception.
    [Theory]
    [InlineData(2601, true)]
    [InlineData(2627, true)]
    [InlineData(547, false)]
    [InlineData(0, false)]
    public void IsUniqueViolation_RecognizesOnlyUniqueConstraintErrors(int sqlErrorNumber, bool expected) =>
        Assert.Equal(expected, SqlErrorNumbers.IsUniqueViolation(sqlErrorNumber));
}
