﻿CREATE PROCEDURE [bll].[LD_ResourceEnoughQuantity__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@ParentConcept NVARCHAR (255),
	@FE_Index_Str NVARCHAR (255),
	@InventoryCreditEntryIndex INT = 4
AS
-- MA2025-04-22: replacd E.CenterId with [dal].[fn_Center__BusinessUnit] (E.[CenterId])
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'Not Enough Quantity of {0} in {1} that belongs to {2}'),
(0, N'ar',  N'لا يوجد ما يكفي من {0} في {1} ما يخص وحدة الأعمال {2}');

DECLARE @DocumentsExcluded IdList;
INSERT INTO @DocumentsExcluded([Id]) SELECT [Id] FROM @Documents;
-- Summarize Quantity from this document
With CurrentDocs AS (
	SELECT L.[PostingDate], [dal].[fn_Center__BusinessUnit] (E.[CenterId]) AS CenterId, E.[AgentId], E.[ResourceId], E.[CurrencyId], 
		SUM(E.[Direction] * 
			bll.fn_Resource_EntryQuantity_EntryUnit__ResourceQuantity(E.[ResourceId], E.[Quantity], E.[UnitId])	
		) AS [CurrentUsage]
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	WHERE E.[Index] = @InventoryCreditEntryIndex
	GROUP BY L.[PostingDate], [dal].[fn_Center__BusinessUnit] (E.[CenterId]), E.[AgentId], E.[ResourceId], E.[CurrencyId]
),
Excesses AS (
	SELECT CD.CenterId, CD.[AgentId], CD.[ResourceId], CD.[CurrentUsage] + ISNULL(OD.[Quantity], 0) AS NetBalance
	FROM CurrentDocs CD
	-- we need to consider starting from State = 3
	-- Also cannot repeat the same E.[CenterId], E.[AccountId], D.[AgentId], E.[ResourceId] in the same document as it may cause neg inventory
	-- that is hard to detect
	CROSS APPLY [dal].[ft_Concept_Center_Agent_Resource__Balances_V2](-- MA:2024-11-13, replaced with V2 to exclude current docs
		@ParentConcept,
		CD.[CenterId],
		CD.[AgentId],
		CD.[ResourceId],
		CD.[CurrencyId],
		CD.[PostingDate],
		@DocumentsExcluded
	) OD
	WHERE SIGN(CD.[CurrentUsage] + ISNULL(OD.[Quantity], 0)) = -1
)--select * from CurrentDocs;select * from [dal].[ft_AccountType__Agent_Resource_Balance](N'Inventories', N'Warehouse', N'Merchandise', N'2022-05-08', 3)
INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].' + @FE_Index_Str,
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0)  AS ErrorMessage,
	dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource],
	dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]) AS [Agent],
	dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [Center]
FROM @Documents FE
JOIN @Lines L ON L.[DocumentIndex] = FE.[Index]
JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
JOIN Excesses S ON S.[CenterId] = [dal].[fn_Center__BusinessUnit] (E.[CenterId]) AND S.[AgentId] = E.[AgentId] AND S.[ResourceId] = E.[ResourceId]
JOIN dbo.Centers C ON C.[Id] = [dal].[fn_Center__BusinessUnit] (E.[CenterId])
-- MA: 2023-11-14 
--JOIN dbo.Centers P ON C.[Node].IsDescendantOf(P.[Node]) = 1
JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
-- MA: 2023-11-14 
--WHERE P.[CenterType] = N'BusinessUnit'

IF EXISTS (SELECT * FROM @ValidationErrors)
	SELECT * FROM @ValidationErrors;
