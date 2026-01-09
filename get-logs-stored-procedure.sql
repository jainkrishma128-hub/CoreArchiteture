-- Stored Procedure for getting paginated request/response logs
CREATE PROCEDURE [dbo].[sp_GetRequestResponseLogs]
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(MAX) = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @StatusCode INT = NULL,
    @Method NVARCHAR(10) = NULL,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortOrder NVARCHAR(4) = 'desc'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalCount INT;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Create temp table to hold filtered results
    CREATE TABLE #FilteredLogs (
        Id INT,
        RequestPath NVARCHAR(MAX),
        RequestMethod NVARCHAR(10),
        RequestBody NVARCHAR(MAX),
        ResponseBody NVARCHAR(MAX),
        ResponseStatusCode INT,
        DurationMs BIGINT,
        IpAddress NVARCHAR(50),
        QueryString NVARCHAR(MAX),
        UserAgent NVARCHAR(MAX),
        UserId NVARCHAR(MAX),
        CreatedAt DATETIME2
    );
    
    -- Insert filtered results into temp table
    INSERT INTO #FilteredLogs
    SELECT 
        rrl.Id,
        rrl.[Path],
        rrl.Method,
        rrl.RequestBody,
        rrl.ResponseBody,
        rrl.ResponseStatusCode,
        rrl.DurationMs,
        rrl.IpAddress,
        rrl.QueryString,
        rrl.UserAgent,
        rrl.UserId,
        rrl.CreatedAt
    FROM [dbo].[RequestResponseLogs] rrl
    WHERE 1=1
        AND (@SearchTerm IS NULL OR 
             rrl.[Path] LIKE '%' + @SearchTerm + '%' OR
             rrl.RequestBody LIKE '%' + @SearchTerm + '%' OR
             rrl.ResponseBody LIKE '%' + @SearchTerm + '%' OR
             rrl.IpAddress LIKE '%' + @SearchTerm + '%')
        AND (@FromDate IS NULL OR rrl.CreatedAt >= @FromDate)
        AND (@ToDate IS NULL OR rrl.CreatedAt <= @ToDate)
        AND (@StatusCode IS NULL OR rrl.ResponseStatusCode = @StatusCode)
        AND (@Method IS NULL OR rrl.Method = @Method);
    
    -- Get total count
    SET @TotalCount = (SELECT COUNT(*) FROM #FilteredLogs);
    
    -- Return paginated results with sorting
    SELECT 
        Id,
        RequestMethod AS Method,
        RequestPath AS [Path],
        QueryString,
        RequestBody,
        ResponseStatusCode,
        ResponseBody,
        DurationMs,
        IpAddress,
        UserAgent,
        UserId,
        CreatedAt
    FROM #FilteredLogs
    ORDER BY 
        CASE 
            WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'asc' THEN CreatedAt
            WHEN @SortBy = 'DurationMs' AND @SortOrder = 'asc' THEN DurationMs
            WHEN @SortBy = 'ResponseStatusCode' AND @SortOrder = 'asc' THEN ResponseStatusCode
            WHEN @SortBy = 'Method' AND @SortOrder = 'asc' THEN RequestMethod
            WHEN @SortBy = 'Path' AND @SortOrder = 'asc' THEN RequestPath
        END ASC,
        CASE 
            WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'desc' THEN CreatedAt
            WHEN @SortBy = 'DurationMs' AND @SortOrder = 'desc' THEN DurationMs
            WHEN @SortBy = 'ResponseStatusCode' AND @SortOrder = 'desc' THEN ResponseStatusCode
            WHEN @SortBy = 'Method' AND @SortOrder = 'desc' THEN RequestMethod
            WHEN @SortBy = 'Path' AND @SortOrder = 'desc' THEN RequestPath
            ELSE CreatedAt
        END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    DROP TABLE #FilteredLogs;
END
