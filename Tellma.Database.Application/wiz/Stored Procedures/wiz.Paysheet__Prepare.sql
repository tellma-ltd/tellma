CREATE PROCEDURE [wiz].[Paysheet__Prepare]
@LineDefinitionId INT,
@SalaryTemplateLineDefinitionId INT,
@DocumentIndex	INT = 0,
@MonthEnding DATE,
@CenterId INT = NULL
AS
BEGIN
	DECLARE @WideLines WideLineList;

	INSERT INTO @WideLines([Index], [DefinitionId],
			[PostingDate],
			[DocumentIndex],
			[NotedAgentId0])
	SELECT	ROW_NUMBER() OVER(ORDER BY [Id]) - 1, @LineDefinitionId,
			@MonthEnding,
			@DocumentIndex,
			[Id]
	FROM dbo.[Agents] RL
	-- TODO: Is this used in scripts? May be we should pass 'Employee' as parameter?
	WHERE RL.DefinitionId = (SELECT [Id] FROM [AgentDefinitions] WHERE Code = N'Employee')
	AND RL.ToDate <= @MonthEnding OR RL.ToDate IS NULL

	SELECT * FROM @WideLines;
END