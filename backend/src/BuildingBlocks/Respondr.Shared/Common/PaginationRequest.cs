namespace Respondr.Shared.Common;

public sealed record PaginationRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public PaginationRequest(int pageNumber = DefaultPageNumber, int pageSize = DefaultPageSize)
    {
        PageNumber = Math.Max(DefaultPageNumber, pageNumber);
        PageSize = Math.Clamp(pageSize, 1, MaxPageSize);
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;
}
