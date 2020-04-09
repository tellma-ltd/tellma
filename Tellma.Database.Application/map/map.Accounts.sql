CREATE FUNCTION [map].[Accounts]()
RETURNS TABLE
AS
RETURN (
	SELECT A.*, 
	--IIF(AC.IdentifierAssignment = N'N', 0, 1) AS [HasIdentifier2],	
	--IIF(AC.ExternalReferenceLabel IS NULL, 0, 1) AS [HasExternalReference],	
	--IIF(AC.AdditionalReferenceLabel IS NULL, 0, 1) AS [HasAdditionalReference],	
	--IIF(AC.NotedAgentDefinition IS NULL, 0, 1) AS [HasNotedAgentId],
	--IIF(AC.NotedAgentNameLabel IS NULL, 0, 1) AS [HasNotedAgentName],
	--IIF(AC.NotedAmountLabel IS NULL, 0, 1) AS [HasNotedAmount],	
	--IIF(AC.NotedDateLabel IS NULL, 0, 1) AS [HasNotedDate],	
	~A.[IsDeprecated] AS [IsActive]
	FROM [dbo].[Accounts] A
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
);
