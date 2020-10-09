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
			[ParticipantId0])
	SELECT	ROW_NUMBER() OVER(ORDER BY [Id]) - 1, @LineDefinitionId,
			@MonthEnding,
			@DocumentIndex,
			[Id]
	FROM dbo.Relations RL
	WHERE RL.DefinitionId = (SELECT [Id] FROM RelationDefinitions WHERE Code = N'Employee')
	AND RL.ToDate <= @MonthEnding OR RL.Todate IS NULL

	SELECT * FROM @WideLines;
END
