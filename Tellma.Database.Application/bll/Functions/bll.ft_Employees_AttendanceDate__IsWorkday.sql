CREATE FUNCTION [bll].[ft_Employees_AttendanceDate__IsWorkday] (  
    @EmployeeIds dbo.IdList READONLY,
    @AttendanceDate DATE
)
RETURNS @MyResult TABLE (
    [EmployeeId] INT,
    [IsWorkday] BIT NOT NULL DEFAULT(1)  
)
AS BEGIN
    DECLARE @IsHoliday BIT = dbo.fn_IsWeekendOrHoliday(@AttendanceDate) ;
    DECLARE @Weekday TINYINT = DATEPART(WEEKDAY, @AttendanceDate);
    
    INSERT @MyResult([EmployeeId], [IsWorkday])
    SELECT [Id] AS [EmployeeId],
    CASE
        WHEN @IsHoliday = 1 THEN 0
        -- Using Lookup7 to define the work days, starting Sunday:1 to Satuday:7.
        -- So, working from Mon - Fri is 0111110
        WHEN SUBSTRING([dal].[fn_Lookup__Code] (Lookup7Id), @Weekday, 1) = '0' THEN 0
        ELSE 1
    END AS [IsWorkday]
    FROM Agents A 
    WHERE Id IN (SELECT Id FROM @EmployeeIds)
  
    RETURN
END
GO