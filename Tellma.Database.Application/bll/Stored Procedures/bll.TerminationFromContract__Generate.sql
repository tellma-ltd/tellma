CREATE PROCEDURE [bll].[TerminationFromContract__Generate]
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@TerminationDate DATE,
	@DurationUnitId INT,
	@AgentId INT,
	@ResourceId INT,
	@NotedAgentId INT,
	@NotedResourceId INT,
	@CenterId INT
AS
BEGIN
DECLARE @Lines LineList, @Entries EntryList;

INSERT INTO @Entries([LineIndex], [Index], [DocumentIndex], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],  [DurationUnitId],
			[Quantity], [MonetaryValue], [Value], [Time1], [Time2])
SELECT
	ROW_NUMBER () OVER(PARTITION BY [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId] ORDER BY [Direction] ASC) - 1 AS [LineIndex],
	E.[Index], 0 AS [DocumentIndex], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId],  [NotedAgentId], [NotedResourceId], [EntryTypeId], [DurationUnitId],
	-SUM([Quantity]) AS [Quantity], -SUM([MonetaryValue]) AS [MonetaryValue], -SUM([Value]) AS [Value], DATEADD(DAY, +1, @TerminationDate) AS [Time1], NULL AS [Time2]
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId)
AND L.[State] = 2
AND (@DurationUnitId IS NULL OR E.[DurationUnitId] = @DurationUnitId)
AND E.[Time1] <= @TerminationDate
AND (E.[Time2] IS NULL OR E.[Time2] >= @TerminationDate)
AND L.Id IN (
	SELECT DISTINCT LineId
	FROM dbo.Entries
	WHERE (@AgentId IS NULL OR AgentId = @AgentId)
	AND (@ResourceId IS NULL OR ResourceId = @ResourceId)
	AND (@NotedAgentId IS NULL OR NotedAgentId = @NotedAgentId)
	AND (@NotedResourceId IS NULL OR NotedResourceId = @NotedResourceId)
	AND (@CenterId IS NULL OR CenterID = @CenterId)
)
GROUP BY E.[Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId], [DurationUnitId]
HAVING SUM([Value]) <> 0

INSERT INTO @Lines([Index], [DocumentIndex], [Id])
SELECT DISTINCT[LineIndex], 0, 0
FROM @Entries;

EXEC bll.Lines__Pivot @Lines, @Entries
END
GO