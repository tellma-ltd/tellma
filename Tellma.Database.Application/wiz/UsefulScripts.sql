-- For auto-generating salaries:

DECLARE @GenerateArguments GenerateArgumentList;
INSERT INTO @GenerateArguments([Key], [Value]) VALUES
(N'MonthEnding', N'2021.07.24'),
(N'CenterId', N'2');

DECLARE @MonthEnding DATE = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N'MonthEnding') AS DATE);
DECLARE @CenterId INT = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N'CenterId') AS INT);

DECLARE @CenterNode HIERARCHYID = (SELECT [Node] FROM dbo.Centers WHERE [Id] = @CenterId);
DECLARE @WideLines WideLineList;

WITH ActiveEmployees AS (
	SELECT E.[AgentId] AS [EmployeeId], E.[CenterId]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	WHERE AC.[Concept] = N'CostCenterAssignmentExtension' -- HRM
	AND L.[State] = 4
	AND E.[ResourceId] IS NULL
	AND E.[Time1] <= @MonthEnding
	GROUP BY E.[AgentId], E.[CenterId]
	HAVING SUM([Direction] * [Quantity]) <> 0
)
INSERT INTO @WideLines([Index], [DocumentIndex], [PostingDate], [AgentId0])
SELECT ROW_NUMBER() OVER (Order By EmployeeId) - 1 AS [Index], 0 AS [DocumentIndex], @MonthEnding AS [PostingDate], EmployeeId
FROM ActiveEmployees E
JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
WHERE (@CenterId IS NULL OR C.[Node].IsDescendantOf(@CenterNode) = 1);

SELECT * FROM @WideLines;
GO
IF OBJECT_ID('dbo.CalendarDates') IS NULL
CREATE TABLE dbo.CalendarDates
(
	[GCDate]		DATE CONSTRAINT [PK_dbo.CalendarDates] PRIMARY KEY,
	[ETDateYear]	SMALLINT,
	[ETDateQuarter]	INT,
	[ETDateMonth]	INT,
	[ETDateDay]		INT,
	[UQDateYear]	SMALLINT,
	[UQDateQuarter]	INT,
	[UQDateMonth]	INT,
	[UQDateDay]		INT,
	
)
GO
IF NOT EXISTS(SELECT * FROM dbo.CalendarDates)
BEGIN
	DECLARE @StartingDate DATE = N'1900-04-30', @EndingDate DATE = N'2077-11-16', @Date DATE;
	SET NOCOUNT ON
	SET @Date = @StartingDate;
	WHILE @Date <  @EndingDate
	BEGIN
		INSERT INTO dbo.CalendarDates([GCDate],
			[ETDateYear], [ETDateQuarter], [ETDateMonth], [ETDateDay],
			[UQDateYear], [UQDateQuarter], [UQDateMonth], [UQDateDay]
			)
		VALUES(@Date,
			[dbo].[fn_Ethiopian_DatePart]('Y', @Date), 
			[dbo].[fn_Ethiopian_DatePart]('Q', @Date), 
			[dbo].[fn_Ethiopian_DatePart]('M', @Date), 
			[dbo].[fn_Ethiopian_DatePart]('D', @Date),
			[dbo].[fn_UmAlQura_DatePart]('Y', @Date), 
			[dbo].[fn_UmAlQura_DatePart]('Q', @Date), 
			[dbo].[fn_UmAlQura_DatePart]('M', @Date), 
			[dbo].[fn_UmAlQura_DatePart]('D', @Date)	
		);
		SET @Date = DATEADD(DAY, 1, @Date)
	END;
END
GO
-- Note the following takes 8 sec to create on a full table
IF IndexProperty(Object_Id('dbo.CalendarDates'), 'IX_CalendarDates__ETDateYear', 'IndexID') Is Null
BEGIN
	CREATE INDEX IX_CalendarDates__ETDateYear ON dbo.CalendarDates([ETDateYear]);
	CREATE INDEX IX_CalendarDates__ETDateQuarter ON dbo.CalendarDates([ETDateQuarter]);
	CREATE INDEX IX_CalendarDates__ETDateMonth ON dbo.CalendarDates([ETDateMonth]);
	CREATE INDEX IX_CalendarDates__ETDateDay ON dbo.CalendarDates([ETDateDay]);
	CREATE INDEX IX_CalendarDates__UQDateYear ON dbo.CalendarDates([UQDateYear]);
	CREATE INDEX IX_CalendarDates__UQDateQuarter ON dbo.CalendarDates([UQDateQuarter]);
	CREATE INDEX IX_CalendarDates__UQDateMonth ON dbo.CalendarDates([UQDateMonth]);
	CREATE INDEX IX_CalendarDates__UQDateDay ON dbo.CalendarDates([UQDateDay]);
END
GO