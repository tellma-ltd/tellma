CREATE PROCEDURE [bll].[LD_ReceiptToCash__Validate]
	@Documents DocumentList READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@ParentConcept NVARCHAR (255)
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'Received amount cannot be zero'), (0, N'ar',  N'المبلغ المستلم لا يمكن أن يكون صفرا'),
(1, N'en',  N'Check # has already been used in Document {0}'), (1, N'ar',  N'رقم الشيك تم استخدامه في قيد {0}'),
(2, N'en',  N'Cash account currency and {0} account currency do not match'), (2, N'ar',  N'عملة الحساب النقدي وعملة حساب {0} مختلفتان'),
(3, N'en',  N'Amount received cannot be more than the due amount'), (3, N'ar',  N'لا يصح سداد مبلغ أكثر من المستحق');

DECLARE
	@ReceivableAccountNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept),
	@LiabilitiesNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'Liabilities');

INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].Entries[0].MonetaryValue',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0)  AS ErrorMessage
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	WHERE E.[Index] = 0 AND E.MonetaryValue = 0

/* it is possible to get different checks with same number from different agents
INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ExternalReference',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 1)  AS ErrorMessage,
		BD.[Code]
	FROM @Documents FD
	JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
	JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.DocumentIndex = FL.DocumentIndex
	JOIN dbo.Entries BE ON BE.[AgentId] = FE.[AgentId]
		AND BE.[ExternalReference] = FE.[ExternalReference]
		AND BE.[Direction] = FE.[Direction]
		AND BE.[AgentId] = FE.[AgentId]
	JOIN dbo.Lines BL ON BL.[Id] = BE.[LineId]
	JOIN map.Documents() BD ON BD.[Id] = BL.[DocumentId]
	WHERE BD.[Id] <> FD.[Id]
	AND ISNUMERIC(FE.[ExternalReference]) = 1; */

INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].Entries[0].CurrencyId',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 2)  AS ErrorMessage,
		dbo.fn_Localize(AD.TitleSingular, AD.TitleSingular2, AD.TitleSingular3) AS AgentDefinition
	FROM @Documents FD
	JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
	JOIN @Entries FE0 ON FE0.[LineIndex] = FL.[Index] AND FE0.[DocumentIndex] = FL.[DocumentIndex]
	JOIN dbo.Agents AG ON AG.[Id] = FE0.[AgentId]
	JOIN dbo.AgentDefinitions AD ON AD.[Id] = AG.[DefinitionId]
	JOIN @Entries FE1 ON FE1.[LineIndex] = FL.[Index] AND FE1.[DocumentIndex] = FL.[DocumentIndex]
	WHERE FE0.[Index] = 0 AND FE1.[Index] = 1
	AND FE0.[CurrencyId] <> FE1.[CurrencyId];

IF @ReceivableAccountNode.IsDescendantOf(@LiabilitiesNode) = 0 -- the following logic applies to settlement of assets only
WITH AgentReceipts AS (
	SELECT E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId], E.[NotedDate], E.[InternalReference], E.[ExternalReference],
	[dal].[fn_Concept_Center_Currency_Agent__Balance](
		@ParentConcept,	E.CenterId, E.CurrencyId, E.AgentId, E.ResourceId, E.InternalReference,
		E.ExternalReference, E.NotedAgentId, E.NotedResourceId, E.NotedDate
	) AS DueBalance,
	SUM([Direction] * [MonetaryValue]) AS Receipt
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	GROUP BY E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId], E.[NotedDate], E.[InternalReference], E.[ExternalReference]
)
INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].Entries[0].MonetaryValue',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 3)  AS ErrorMessage
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.DocumentIndex = L.DocumentIndex
	JOIN AgentReceipts AR
		ON  (AR.[AccountId] = E.[AccountId])
		AND (AR.[CenterId] = E.[CenterId])
		AND (AR.[CurrencyId] = E.[CurrencyId])
		AND (AR.[AgentId] = E.[AgentId])
		AND (AR.[ResourceId] IS NULL		AND E.[ResourceId] IS NULL			OR AR.[ResourceId] = E.[ResourceId])
		AND (AR.[NotedAgentId] IS NULL		AND E.[NotedAgentId] IS NULL		OR AR.[NotedAgentId] = E.[NotedAgentId])
		AND (AR.[NotedResourceId] IS NULL	AND E.[NotedResourceId] IS NULL		OR AR.[NotedResourceId] = E.[NotedResourceId])
		AND (AR.[NotedDate] IS NULL			AND E.[NotedDate] IS NULL			OR AR.[NotedDate] = E.[NotedDate])
		AND (AR.[InternalReference] IS NULL AND E.[InternalReference] IS NULL	OR AR.[InternalReference] = E.[InternalReference])
		AND (AR.[ExternalReference] IS NULL AND E.[ExternalReference] IS NULL	OR AR.[ExternalReference] = E.[ExternalReference])
	WHERE AR.[Receipt] + AR.[DueBalance] < 0

IF EXISTS (SELECT * FROM @ValidationErrors)
	SELECT * FROM @ValidationErrors;
GO
