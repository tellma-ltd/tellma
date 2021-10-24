CREATE FUNCTION [bll].[ft_ValidPeriodEntries]
(
-- Useful for templates and tracking events
-- It replaces the need to reverse previous entry.
	@AccountTypeConcept NVARCHAR (255),
	@AgentDefinitionCode NVARCHAR (255),
	@ResourceDefinitionCode NVARCHAR (255),
	@NotedResourceDefinitionCode NVARCHAR (255),
	@DocumentType TINYINT, -- 0: Template, 2: Event
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @result TABLE (
	[VId] INT PRIMARY KEY,
	[VTime1] DATE,
	[VTime2] DATE,
	[NextTime] DATE,
	[ValidFrom] DATE,
	[ValidTill] Date,
	[Span] DECIMAL (19, 4)
)
AS
BEGIN
	DECLARE @State TINYINT = IIF(@DocumentType = 0, 2, 4);
	DECLARE @AccountTypeNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = @AccountTypeConcept);
	DECLARE @AgentDefinitionId INT = (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @AgentDefinitionCode);
	DECLARE @ResourceDefinitionId INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = @ResourceDefinitionCode);
	DECLARE @NotedResourceDefinitionId INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = @NotedResourceDefinitionCode);
	INSERT INTO @result([VId], [VTime1], [NextTime], [VTime2])
	SELECT
		E.[Id],
		E.[Time1] AS VTime1,
		LEAD(E.[Time1], 1, N'9999.12.31') OVER (
			PARTITION BY E.[AgentId], E.[AccountId], E.[ResourceId], E.[NotedResourceId]
			ORDER BY E.[Time1]
		) As [NextTime],
		ISNULL(E.[Time2], N'9999.12.31') AS VTime2
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.AccountTypeId
	JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	LEFT JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
	WHERE DD.[DocumentType] = @DocumentType
	AND L.[State] = @State
	AND (AC.[Node].IsDescendantOf(@AccountTypeNode) = 1)
	AND (AG.[DefinitionId] = @AgentDefinitionId)
	AND (R.Id IS NULL AND @ResourceDefinitionId IS NULL 
		OR R.[DefinitionId] = @ResourceDefinitionId)
	AND (NR.[DefinitionId] = @NotedResourceDefinitionId);

	UPDATE @result
	SET 
		[ValidTill] = IIF ([NextTime] < [VTime2], DATEADD(DAY,-1,[NextTime]), [VTime2])

	UPDATE @result
	SET 
		[ValidFrom] = IIF (@PeriodStart > 	[VTime1], @PeriodStart, [VTime1]),
		[ValidTill] = IIF (@PeriodEnd < 	[ValidTill], @PeriodEnd, [ValidTill])

	UPDATE @result
	SET [Span] = DATEDIFF(DAY, [ValidFrom], [ValidTill]) + 1;

	DELETE @result
	WHERE [VTime2] < @PeriodStart OR [VTime1] > @PeriodEnd

	RETURN
END
GO