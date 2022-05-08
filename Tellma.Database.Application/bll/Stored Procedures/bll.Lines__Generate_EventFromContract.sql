CREATE PROCEDURE [bll].[Lines__Generate_EventFromContract]
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@ContractTerminationLineDefinitionId INT,
	@PeriodEnd DATE,
	@PostingDate DATE,
	@DurationUnitId INT,
	@AgentId INT = NULL,
	@ResourceId INT = NULL,
	@NotedAgentId INT = NULL,
	@NotedResourceId INT = NULL,
	@CenterId INT
AS
BEGIN
SET @PeriodEnd = ISNULL(dbo.fn_PeriodEnd(@DurationUnitId, @PeriodEnd), dbo.fn_PeriodEnd(@DurationUnitId, @PostingDate));
DECLARE @PeriodStart DATE = dbo.fn_PeriodStart(@DurationUnitId, @PeriodEnd);

SET NOCOUNT ON
DECLARE @T TABLE (
	[LineIndex] INT, [Index] INT, [CommencementDate] DATE, [DurationUnitId] INT, [PostingDate] DATE, [PeriodIndex] INT, [Time1] DATE, [Time2] DATE, [Ratio] DECIMAL (19,8),
	[AccountId] INT, [Direction] SMALLINT, [CenterId] INT, [AgentId] INT, [ResourceId] INT, [UnitId] INT, [Quantity] DECIMAL (19,4), [CurrencyId] NCHAR (3),
	[MonetaryValue] DECIMAL (19,4), [Value] DECIMAL (19,4), [NotedAgentId] INT, [NotedResourceId] INT, [EntryTypeId] INT
	);

INSERT INTO @T([LineIndex], [Index], [CommencementDate], [DurationUnitId],
	[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [Quantity], [CurrencyId],
	[MonetaryValue], [Value], [NotedAgentId], [NotedResourceId], [EntryTypeId])
SELECT L.[Index], E.[Index], E.[Time1], [DurationUnitId],
	[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [Quantity], [CurrencyId],
	[MonetaryValue], [Value], [NotedAgentId], [NotedResourceId], [EntryTypeId]
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId)
AND L.[State] = 2
AND E.DurationUnitId = @DurationUnitId
AND E.[Time1] <= @PeriodEnd
AND L.Id IN (
	SELECT DISTINCT LineId
	FROM dbo.Entries
	WHERE (@AgentId IS NULL OR AgentId = @AgentId)
	AND (@ResourceId IS NULL OR ResourceId = @ResourceId)
	AND (@NotedAgentId IS NULL OR NotedAgentId = @NotedAgentId)
	AND (@NotedResourceId IS NULL OR NotedResourceId = @NotedResourceId)
	AND (@CenterId IS NULL OR CenterId = @CenterId)
)
UNION
SELECT L.[Index], E.[Index], DATEADD(DAY, 1, E.[Time2]), [DurationUnitId],
	[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], -[Quantity], [CurrencyId],
	-[MonetaryValue], -[Value], [NotedAgentId], [NotedResourceId], [EntryTypeId]
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId)
AND L.[State] = 2
AND E.DurationUnitId = @DurationUnitId
AND E.[Time2] <= @PeriodEnd
AND L.Id IN (
	SELECT DISTINCT LineId
	FROM dbo.Entries
	WHERE (@AgentId IS NULL OR AgentId = @AgentId)
	AND (@ResourceId IS NULL OR ResourceId = @ResourceId)
	AND (@NotedAgentId IS NULL OR NotedAgentId = @NotedAgentId)
	AND (@NotedResourceId IS NULL OR NotedResourceId = @NotedResourceId)
	AND (@CenterId IS NULL OR CenterID = @CenterId)
)

UPDATE @T SET [PeriodIndex] = dbo.fn_CommencementDate_DurationUnit_PeriodEnd__PeriodIndex([CommencementDate], [DurationUnitId], @PeriodEnd)
UPDATE @T SET [Time1] = IIF([PeriodIndex] = 0 , [CommencementDate], @PeriodStart);
UPDATE @T SET [Time2] = IIF([Quantity] > 0, @PeriodEnd, DATEADD(DAY, -1, [Time1]));
UPDATE @T SET [Ratio] = 1.0 * (DATEDIFF(DAY, [Time1], @PeriodEnd) + 1) / (DATEDIFF(DAY, @PeriodStart, @PeriodEnd) + 1);

UPDATE @T
SET
	[Quantity] = [Ratio] * [Quantity],
	[MonetaryValue] = [Ratio] * [MonetaryValue],
	[Value] = [Ratio] * [Value]
--SELECT * FROM @T;

DECLARE @Lines LineList, @Entries EntryList;

INSERT INTO @Entries([LineIndex], [Index], [DocumentIndex], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId], [Quantity], [MonetaryValue], [Value], [Time1], [Time2])
SELECT
	ROW_NUMBER () OVER(PARTITION BY [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId] ORDER BY [Direction] ASC) - 1 AS [LineIndex],
	[Index], 0 AS [DocumentIndex], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId],  [NotedAgentId], [NotedResourceId], [EntryTypeId],
	SUM([Quantity]) AS [Quantity], SUM([MonetaryValue]) AS [MonetaryValue], SUM([Value]) AS [Value], MIN([Time1]) AS [Time1],  MIN([Time2]) AS [Time2]
FROM @T
GROUP BY [Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId]
HAVING SUM([Value]) <> 0

INSERT INTO @Lines([Index], [DocumentIndex], [Id])
SELECT DISTINCT[LineIndex], 0, 0
FROM @Entries;

EXEC bll.Lines__Pivot @Lines, @Entries
END
GO