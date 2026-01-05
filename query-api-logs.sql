-- ============================================
-- API Logging - Useful SQL Queries
-- ============================================

-- ============================================
-- 1. VIEW RECENT REQUEST/RESPONSE LOGS
-- ============================================

-- Last 20 requests
SELECT TOP 20 
    Id,
    Method,
    Path,
    ResponseStatusCode,
    DurationMs,
    UserId,
    IpAddress,
    CreatedAt
FROM RequestResponseLogs
ORDER BY CreatedAt DESC;

-- ============================================
-- 2. VIEW RECENT ERRORS
-- ============================================

-- Last 20 errors
SELECT TOP 20 
    Id,
    Message,
    Path,
    Method,
    UserId,
    CreatedAt
FROM ErrorLogs
ORDER BY CreatedAt DESC;

-- View error with full stack trace
SELECT 
    Id,
    Message,
    StackTrace,
    Path,
    Method,
    QueryString,
    UserId,
    CreatedAt
FROM ErrorLogs
WHERE Id = 1; -- Replace with actual error ID

-- ============================================
-- 3. PERFORMANCE MONITORING
-- ============================================

-- Slowest endpoints (last 24 hours)
SELECT TOP 10 
    Path,
    Method,
    AVG(DurationMs) as AvgDurationMs,
    MAX(DurationMs) as MaxDurationMs,
    MIN(DurationMs) as MinDurationMs,
    COUNT(*) as RequestCount
FROM RequestResponseLogs
WHERE CreatedAt > DATEADD(hour, -24, GETUTCDATE())
GROUP BY Path, Method
ORDER BY AvgDurationMs DESC;

-- Requests taking longer than 1 second
SELECT 
    Path,
    Method,
    DurationMs,
    ResponseStatusCode,
    UserId,
    CreatedAt
FROM RequestResponseLogs
WHERE DurationMs > 1000
ORDER BY DurationMs DESC;

-- ============================================
-- 4. ERROR ANALYSIS
-- ============================================

-- Most common errors (last 7 days)
SELECT TOP 10 
    Message,
    Path,
    COUNT(*) as ErrorCount
FROM ErrorLogs
WHERE CreatedAt > DATEADD(day, -7, GETUTCDATE())
GROUP BY Message, Path
ORDER BY ErrorCount DESC;

-- Error rate by endpoint (last 24 hours)
SELECT 
    Path,
    COUNT(*) as TotalRequests,
    SUM(CASE WHEN ResponseStatusCode >= 400 THEN 1 ELSE 0 END) as ErrorCount,
    CAST(SUM(CASE WHEN ResponseStatusCode >= 400 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as ErrorRatePercent
FROM RequestResponseLogs
WHERE CreatedAt > DATEADD(hour, -24, GETUTCDATE())
GROUP BY Path
HAVING COUNT(*) > 10 -- Only endpoints with more than 10 requests
ORDER BY ErrorRatePercent DESC;

-- HTTP status code distribution
SELECT 
    ResponseStatusCode,
    COUNT(*) as Count,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM RequestResponseLogs) AS DECIMAL(5,2)) as Percentage
FROM RequestResponseLogs
WHERE CreatedAt > DATEADD(day, -1, GETUTCDATE())
GROUP BY ResponseStatusCode
ORDER BY Count DESC;

-- ============================================
-- 5. USER ACTIVITY TRACKING
-- ============================================

-- Activity by specific user
SELECT 
    Method,
    Path,
    ResponseStatusCode,
    DurationMs,
    CreatedAt
FROM RequestResponseLogs
WHERE UserId = '123' -- Replace with actual user ID
ORDER BY CreatedAt DESC;

-- Most active users (last 24 hours)
SELECT TOP 10 
    UserId,
    COUNT(*) as RequestCount,
    AVG(DurationMs) as AvgDurationMs
FROM RequestResponseLogs
WHERE UserId IS NOT NULL
  AND CreatedAt > DATEADD(hour, -24, GETUTCDATE())
GROUP BY UserId
ORDER BY RequestCount DESC;

-- Users who encountered errors
SELECT 
    UserId,
    COUNT(*) as ErrorCount,
    MIN(CreatedAt) as FirstError,
    MAX(CreatedAt) as LastError
FROM ErrorLogs
WHERE UserId IS NOT NULL
  AND CreatedAt > DATEADD(day, -7, GETUTCDATE())
GROUP BY UserId
ORDER BY ErrorCount DESC;

-- ============================================
-- 6. ENDPOINT USAGE STATISTICS
-- ============================================

-- Most called endpoints (last 24 hours)
SELECT TOP 10 
    Path,
    Method,
    COUNT(*) as CallCount,
    AVG(DurationMs) as AvgDurationMs
FROM RequestResponseLogs
WHERE CreatedAt > DATEADD(hour, -24, GETUTCDATE())
GROUP BY Path, Method
ORDER BY CallCount DESC;

-- Endpoint usage by hour (last 24 hours)
SELECT 
    DATEPART(hour, CreatedAt) as Hour,
    COUNT(*) as RequestCount
FROM RequestResponseLogs
WHERE CreatedAt > DATEADD(hour, -24, GETUTCDATE())
GROUP BY DATEPART(hour, CreatedAt)
ORDER BY Hour;

-- ============================================
-- 7. SECURITY MONITORING
-- ============================================

-- Failed authentication attempts (401 errors)
SELECT 
    Path,
    IpAddress,
    UserAgent,
    COUNT(*) as FailedAttempts,
    MAX(CreatedAt) as LastAttempt
FROM RequestResponseLogs
WHERE ResponseStatusCode = 401
  AND CreatedAt > DATEADD(day, -1, GETUTCDATE())
GROUP BY Path, IpAddress, UserAgent
ORDER BY FailedAttempts DESC;

-- Requests from specific IP
SELECT 
    Method,
    Path,
    ResponseStatusCode,
    UserId,
    CreatedAt
FROM RequestResponseLogs
WHERE IpAddress = '192.168.1.100' -- Replace with actual IP
ORDER BY CreatedAt DESC;

-- Suspicious activity (multiple errors from same IP)
SELECT 
    IpAddress,
    COUNT(*) as ErrorCount,
    COUNT(DISTINCT Path) as UniqueEndpoints,
    MIN(CreatedAt) as FirstError,
    MAX(CreatedAt) as LastError
FROM RequestResponseLogs
WHERE ResponseStatusCode >= 400
  AND CreatedAt > DATEADD(hour, -1, GETUTCDATE())
GROUP BY IpAddress
HAVING COUNT(*) > 10
ORDER BY ErrorCount DESC;

-- ============================================
-- 8. DATA CLEANUP (MAINTENANCE)
-- ============================================

-- Count logs older than 30 days
SELECT 
    'RequestResponseLogs' as TableName,
    COUNT(*) as OldRecords
FROM RequestResponseLogs
WHERE CreatedAt < DATEADD(day, -30, GETUTCDATE())
UNION ALL
SELECT 
    'ErrorLogs' as TableName,
    COUNT(*) as OldRecords
FROM ErrorLogs
WHERE CreatedAt < DATEADD(day, -30, GETUTCDATE());

-- Delete logs older than 30 days (BE CAREFUL!)
-- Uncomment to execute
/*
DELETE FROM RequestResponseLogs
WHERE CreatedAt < DATEADD(day, -30, GETUTCDATE());

DELETE FROM ErrorLogs
WHERE CreatedAt < DATEADD(day, -30, GETUTCDATE());
*/

-- ============================================
-- 9. DETAILED REQUEST ANALYSIS
-- ============================================

-- View full request/response for specific log
SELECT 
    Id,
    Method,
    Path,
    QueryString,
    RequestBody,
    ResponseStatusCode,
    ResponseBody,
    DurationMs,
    IpAddress,
    UserAgent,
    UserId,
    CreatedAt
FROM RequestResponseLogs
WHERE Id = 1; -- Replace with actual log ID

-- Search requests by path pattern
SELECT TOP 20 
    Method,
    Path,
    ResponseStatusCode,
    DurationMs,
    CreatedAt
FROM RequestResponseLogs
WHERE Path LIKE '%products%'
ORDER BY CreatedAt DESC;

-- ============================================
-- 10. DASHBOARD QUERIES
-- ============================================

-- Summary statistics (last 24 hours)
SELECT 
    COUNT(*) as TotalRequests,
    COUNT(DISTINCT UserId) as UniqueUsers,
    AVG(DurationMs) as AvgDurationMs,
    SUM(CASE WHEN ResponseStatusCode = 200 THEN 1 ELSE 0 END) as SuccessCount,
    SUM(CASE WHEN ResponseStatusCode >= 400 THEN 1 ELSE 0 END) as ErrorCount,
    CAST(SUM(CASE WHEN ResponseStatusCode >= 400 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as ErrorRatePercent
FROM RequestResponseLogs
WHERE CreatedAt > DATEADD(hour, -24, GETUTCDATE());

-- Requests per minute (last hour)
SELECT 
    DATEADD(minute, DATEDIFF(minute, 0, CreatedAt), 0) as Minute,
    COUNT(*) as RequestCount
FROM RequestResponseLogs
WHERE CreatedAt > DATEADD(hour, -1, GETUTCDATE())
GROUP BY DATEADD(minute, DATEDIFF(minute, 0, CreatedAt), 0)
ORDER BY Minute DESC;

-- ============================================
-- 11. EXPORT FOR ANALYSIS
-- ============================================

-- Export last 1000 requests to CSV (use SSMS export feature)
SELECT TOP 1000 
    Id,
    Method,
    Path,
    QueryString,
    ResponseStatusCode,
    DurationMs,
    IpAddress,
    UserId,
    CreatedAt
FROM RequestResponseLogs
ORDER BY CreatedAt DESC;

-- Export errors for debugging
SELECT TOP 100 
    Id,
    Message,
    Path,
    Method,
    UserId,
    CreatedAt
FROM ErrorLogs
ORDER BY CreatedAt DESC;
