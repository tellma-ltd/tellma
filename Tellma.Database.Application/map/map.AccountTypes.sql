CREATE FUNCTION [map].[AccountTypes] ()
RETURNS TABLE
AS
RETURN (
SELECT
    Q.[Id],
    Q.[ParentId],
    Q.[Code],
    Q.[Concept],
    Q.[Name],
    Q.[Name2],
    Q.[Name3],
    Q.[Description],
    Q.[Description2],
    Q.[Description3],
    Q.[Node],
    Q.[IsMonetary],
    Q.[IsAssignable],
    Q.[StandardAndPure],
    Q.[CustodianDefinitionId],
    Q.[ParticipantDefinitionId],
    Q.[EntryTypeParentId],
    Q.[Time1Label],
    Q.[Time1Label2],
    Q.[Time1Label3],
    Q.[Time2Label],
    Q.[Time2Label2],
    Q.[Time2Label3],
    Q.[ExternalReferenceLabel],
    Q.[ExternalReferenceLabel2],
    Q.[ExternalReferenceLabel3],
    Q.[InternalReferenceLabel],
    Q.[InternalReferenceLabel2],
    Q.[InternalReferenceLabel3],
    Q.[NotedAgentNameLabel],
    Q.[NotedAgentNameLabel2],
    Q.[NotedAgentNameLabel3],
    Q.[NotedAmountLabel],
    Q.[NotedAmountLabel2],
    Q.[NotedAmountLabel3],
    Q.[NotedDateLabel],
    Q.[NotedDateLabel2],
    Q.[NotedDateLabel3],
    Q.[IsActive],
    Q.[IsSystem],
    Q.[SavedById],
    Q.[ValidFrom],
    Q.[ValidTo],
    CAST(IIF(    
        Q.[Code] LIKE N'1110112%' OR -- Construction in progress
        Q.[Code] LIKE N'111022%' OR -- Investment property under construction or development
        Q.[Code] LIKE N'112112%' OR -- Current work in progress
        Q.[Code] LIKE N'112114%' OR -- Current inventories in transit
        Q.[Code] LIKE N'2%' OR -- Profit Or Loss
        Q.[Code] LIKE N'3%' -- Other comprehensive income
    , 0, 1) AS BIT) AS IsBusinessUnit,
    [Node].GetAncestor(1)  AS [ParentNode],
    [Node].GetLevel() AS [Level],
    [Node].ToString() AS [Path],
	CC.[ActiveChildCount],
	CC.ChildCount
FROM [dbo].[AccountTypes] Q
CROSS APPLY (
		SELECT COUNT(*) AS [ChildCount],
		SUM(IIF([IsActive]=1,1,0)) AS  [ActiveChildCount]	
		FROM [dbo].[AccountTypes]
		WHERE [Node].IsDescendantOf(Q.[Node]) = 1
) CC 

);
