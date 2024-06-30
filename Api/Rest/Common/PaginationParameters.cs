﻿namespace Api.Rest.Common;

public sealed class PaginationParameters
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
