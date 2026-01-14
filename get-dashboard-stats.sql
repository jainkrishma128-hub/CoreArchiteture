CREATE PROCEDURE [dbo].[sp_GetDashboardStats]
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Daily Stats
    SELECT 
        CAST(l.CreatedAt AS DATE) AS [Date],
        COUNT(*) AS TotalRequests,
        AVG(CAST(l.DurationMs AS FLOAT)) AS AverageDuration
    FROM [dbo].[RequestResponseLogs] l
    WHERE l.CreatedAt >= @FromDate AND l.CreatedAt <= @ToDate
    GROUP BY CAST(l.CreatedAt AS DATE)
    ORDER BY [Date];

    -- 2. Status Distribution
    SELECT 
        SUM(CASE WHEN l.ResponseStatusCode >= 200 AND l.ResponseStatusCode < 300 THEN 1 ELSE 0 END) AS Success,
        SUM(CASE WHEN l.ResponseStatusCode >= 400 AND l.ResponseStatusCode < 500 THEN 1 ELSE 0 END) AS ClientError,
        SUM(CASE WHEN l.ResponseStatusCode >= 500 THEN 1 ELSE 0 END) AS ServerError
    FROM [dbo].[RequestResponseLogs] l
    WHERE l.CreatedAt >= @FromDate AND l.CreatedAt <= @ToDate;

    -- 3. Overall Average
    SELECT 
        AVG(CAST(l.DurationMs AS FLOAT)) AS AverageDuration
    FROM [dbo].RequestResponseLogs l
    WHERE l.CreatedAt >= @FromDate AND l.CreatedAt <= @ToDate;
END
GO
