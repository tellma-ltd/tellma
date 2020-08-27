﻿CREATE FUNCTION [map].[AccountTypes] ()
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
    Q.[NotedRelationDefinitionId],
    Q.[Time1Label],
    Q.[Time1Label2],
    Q.[Time1Label3],
    Q.[Time2Label],
    Q.[Time2Label2],
    Q.[Time2Label3],
    Q.[ExternalReferenceLabel],
    Q.[ExternalReferenceLabel2],
    Q.[ExternalReferenceLabel3],
    Q.[AdditionalReferenceLabel],
    Q.[AdditionalReferenceLabel2],
    Q.[AdditionalReferenceLabel3],
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

    [Node].GetAncestor(1)  AS [ParentNode],
    [Node].GetLevel() AS [Level],
    [Node].ToString() AS [Path],
    (SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(Q.[Node]) = 1) As [ActiveChildCount],
        (SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [Node].IsDescendantOf(Q.[Node]) = 1) As [ChildCount]
 FROM [dbo].[AccountTypes] Q
);
