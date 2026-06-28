using Respondr.Shared.Common;
using Respondr.Shared.Constants;
using Respondr.Shared.Time;

namespace Respondr.Application.Tests.BuildingBlocks;

public class SharedHelperTests
{
    [Fact]
    public void ResultSuccess_CreatesSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void ResultFailure_CreatesFailedResultWithError()
    {
        var error = Error.Validation("Incident.Title.Required", "Incident title is required.");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void GenericResultFailure_ThrowsWhenValueIsAccessed()
    {
        var result = Result<string>.Failure(Error.NotFound("Incident.NotFound", "Incident was not found."));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void PagedResult_CalculatesPagingMetadata()
    {
        var result = PagedResult<int>.Create([1, 2, 3], pageNumber: 2, pageSize: 3, totalCount: 10);

        Assert.Equal(4, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PaginationRequest_ClampsInvalidValues()
    {
        var request = new PaginationRequest(pageNumber: -10, pageSize: 500);

        Assert.Equal(1, request.PageNumber);
        Assert.Equal(PaginationRequest.MaxPageSize, request.PageSize);
        Assert.Equal(0, request.Skip);
    }

    [Fact]
    public void SharedConstants_ExposeExpectedArchitectureValues()
    {
        Assert.Equal("identity", SchemaNames.Identity);
        Assert.Equal("incidents", SchemaNames.Incidents);
        Assert.Equal("dispatch", SchemaNames.Dispatch);
        Assert.Equal("resources", SchemaNames.Resources);
        Assert.Equal("notifications", SchemaNames.Notifications);
        Assert.Equal("Dispatcher", RoleNames.Dispatcher);
        Assert.Equal("OperationsLead", RoleNames.OperationsLead);
    }

    [Fact]
    public void SystemDateTimeProvider_ReturnsUtcTimestamp()
    {
        IDateTimeProvider provider = new SystemDateTimeProvider();

        var utcNow = provider.UtcNow;

        Assert.Equal(TimeSpan.Zero, utcNow.Offset);
    }
}
