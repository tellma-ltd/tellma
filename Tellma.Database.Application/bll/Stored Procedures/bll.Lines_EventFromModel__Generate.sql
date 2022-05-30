CREATE PROCEDURE [bll].[Lines_EventFromModel__Generate]
-- If a model expires during the period, it still generates an event
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@PeriodEnd DATE,
	@DurationUnitId INT,
	@EntryIndex	INT,
	@AgentId INT = NULL,
	@ResourceId INT = NULL,
	@NotedAgentId INT = NULL,
	@NotedResourceId INT = NULL,
	@CenterId INT = NULL
AS
BEGIN

DECLARE @PeriodStart DATE = dbo.fn_PeriodStart(@DurationUnitId, @PeriodEnd);
SET @ContractAmendmentLineDefinitionId = ISNULL(@ContractAmendmentLineDefinitionId, 0);

SET NOCOUNT ON
DECLARE @T TABLE (
	[LineKey] INT, [Index] INT, [DurationUnitId] INT, [Decimal1] DECIMAL (19, 6), [PeriodIndex] INT, [Time1] DATE, [Time2] DATE,
	[AccountId] INT, [Direction] SMALLINT, [CenterId] INT, [AgentId] INT, [ResourceId] INT, [UnitId] INT, [Quantity] DECIMAL (19,4), [CurrencyId] NCHAR (3),
	[MonetaryValue] DECIMAL (19,4), [Value] DECIMAL (19,4), [NotedAgentId] INT, [NotedResourceId] INT, [EntryTypeId] INT
	);

WITH FilteredLines AS (
	SELECT DISTINCT L.Id, L.LineKey, L.[Decimal1]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId)
	AND L.[State] = 2
	AND E.[DurationUnitId] = @DurationUnitId -- Should be moved to the line level
	AND E.[Index] = @EntryIndex -- Primary entry whose data needs to be filtered
	AND (@AgentId IS NULL OR AgentId = @AgentId)
	AND (@ResourceId IS NULL OR ResourceId = @ResourceId)
	AND (@NotedAgentId IS NULL OR NotedAgentId = @NotedAgentId)
	AND (@NotedResourceId IS NULL OR NotedResourceId = @NotedResourceId)
	AND (@CenterId IS NULL OR CenterId = @CenterId)
)
INSERT INTO @T([LineKey], [Index], [DurationUnitId], [Decimal1], [Time1], [Time2],
	[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
	[Quantity], [MonetaryValue], [Value])
SELECT L.[LineKey], E.[Index], [DurationUnitId], [Decimal1], E.[Time1], E.[Time2],
	[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
	[Quantity], [MonetaryValue], [Value] 
FROM dbo.Entries E
JOIN FilteredLines L ON L.[Id] = E.[LineId]
WHERE E.[Time1] <= @PeriodEnd
UNION
SELECT L.[LineKey], E.[Index], [DurationUnitId], [Decimal1], E.[Time1], E.[Time2],
	[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
	-[Quantity], -[MonetaryValue], -[Value] 
FROM dbo.Entries E
JOIN FilteredLines L ON L.[Id] = E.[LineId]
WHERE E.[Time2] < @PeriodEnd

UPDATE @T SET [PeriodIndex] = dbo.fn_CommencementDate_DurationUnit_PeriodEnd__PeriodIndex([Time1], [DurationUnitId], @PeriodEnd)
UPDATE @T SET [Time1] = IIF([PeriodIndex] = 0 , [Time1], @PeriodStart);
UPDATE @T SET [Time2] = IIF(ISNULL([Time2], @PeriodEnd) < @PeriodEnd, [Time2], @PeriodEnd);
UPDATE @T SET [Decimal1] = [Decimal1] * (DATEDIFF(DAY, [Time1], [Time2]) + 1) / (DATEDIFF(DAY, @PeriodStart, @PeriodEnd) + 1);

UPDATE @T
SET
	[Quantity] = [Decimal1] * [Quantity],
	[MonetaryValue] = [Decimal1] * [MonetaryValue],
	[Value] = [Decimal1] * [Value]
--SELECT * FROM @T;

DECLARE @T2 TABLE (
	[LineKey] INT, [Index] INT, [Time1] DATE, [Time2] DATE,
	[AccountId] INT, [Direction] SMALLINT, [CenterId] INT, [AgentId] INT, [ResourceId] INT, [UnitId] INT, [Quantity] DECIMAL (19,4), [CurrencyId] NCHAR (3),
	[MonetaryValue] DECIMAL (19,4), [Value] DECIMAL (19,4), [NotedAgentId] INT, [NotedResourceId] INT, [EntryTypeId] INT
	);
INSERT INTO @T2	([LineKey], [Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId],[CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
	 [Quantity], [MonetaryValue], [Value], [Time1], [Time2])
SELECT [LineKey], [Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId],  [NotedAgentId], [NotedResourceId], [EntryTypeId],
	SUM([Quantity]) AS [Quantity], SUM([MonetaryValue]) AS [MonetaryValue], SUM([Value]) AS [Value], MIN([Time1]) AS [Time1],  MIN([Time2]) AS [Time2]
FROM @T
GROUP BY [LineKey], [Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId]
HAVING SUM([Value]) <> 0
--SELECT * FROM @T2;

DECLARE @Lines LineList, @Entries EntryList;

INSERT INTO @Entries([LineIndex], [Index], [DocumentIndex], [Id], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId], [Quantity], [MonetaryValue], [Value], [Time1], [Time2])
SELECT
	ROW_NUMBER () OVER(PARTITION BY [Index] ORDER BY [LineKey], [Index] ASC) - 1 AS [LineIndex],
	[Index], 0 AS [DocumentIndex],  0 AS [Id], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId],[CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
	[Quantity], [MonetaryValue], [Value], [Time1], [Time2]
FROM @T2
ORDER BY [LineKey], [Index];

INSERT INTO @Lines([Index], [DocumentIndex], [Id])
SELECT DISTINCT [LineIndex], 0, 0
FROM @Entries;

EXEC bll.Lines__Pivot @Lines, @Entries;
END
GO