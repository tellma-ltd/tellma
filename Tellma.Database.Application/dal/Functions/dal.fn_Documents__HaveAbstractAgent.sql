CREATE FUNCTION [dal].[fn_Documents__HaveAbstractAgent] (
@Documents DocumentList READONLY,
@AgentDefinitionCode NVARCHAR (50)
)
RETURNS BIT
AS
BEGIN
	IF EXISTS(
		SELECT *
		FROM Documents D
		JOIN Lines L ON L.[DocumentId] = D.[Id]
		JOIN Entries E ON E.[LineId] = L.[Id]
		JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
		JOIN dbo.AgentDefinitions AD ON AD.[Id] = Ag.DefinitionId
		WHERE AD.[Code] = @AgentDefinitionCode
		AND AG.[Code] = N'0'
		AND D.[Id] IN (SELECT [Id] FROM @Documents)
	)
		RETURN 1;
	
	RETURN 0;
END
GO