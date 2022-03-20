CREATE PROCEDURE [bll].[LD_PaymentFromCash__Validate]
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
(0, N'en',  N'Paid amount cannot be zero'), (0, N'ar',  N'المبلغ المدفوع لا يمكن أن يكون صفرا'),
(1, N'en',  N'Check # has already been used in Document {0}'), (1, N'ar',  N'رقم الشيك تم استخدامه في قيد {0}'),
(2, N'en',  N'Cash account currency and {0} account currency do not match'), (2, N'ar',  N'عملة الحساب النقدي وعملة حساب {0} مختلفتان'),
(3, N'en',  N'Amount paid cannot be more than the due amount'), (3, N'ar',  N'لا يصح سداد مبلغ أكثر من المستحق');

DECLARE
	@PayableAccountNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept),
	@AssetsNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);

INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].Entries[1].MonetaryValue',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0)  AS ErrorMessage
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	WHERE E.[Index] = 1 AND E.MonetaryValue = 0

INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].InternalReference',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 1)  AS ErrorMessage,
		BD.[Code]
	FROM @Documents FD
	JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
	JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.DocumentIndex = FL.DocumentIndex
	JOIN dbo.Entries BE ON BE.[AgentId] = FE.[AgentId]
		AND BE.[InternalReference] = FE.[InternalReference]
		AND BE.[Direction] = FE.[Direction]
	JOIN dbo.Lines BL ON BL.[Id] = BE.[LineId]
	JOIN map.Documents() BD ON BD.[Id] = BL.[DocumentId]
	WHERE BD.[Id] <> FD.[Id]
	AND ISNUMERIC(FE.[InternalReference]) = 1;

INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].Entries[1].CurrencyId',
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

IF @PayableAccountNode.IsDescendantOf(@AssetsNode) = 0 -- the following logic applies to settlement of liabilities only
WITH AgentPayments AS (
	SELECT E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId], E.[NotedDate], E.[InternalReference], E.[ExternalReference],
	[dal].[fn_Concept_Center_Currency_Agent__Balance](
		@ParentConcept,	E.CenterId, E.CurrencyId, E.AgentId, E.ResourceId, E.InternalReference,
		E.ExternalReference, E.NotedAgentId, E.NotedResourceId, E.NotedDate
	) AS DueBalance,
	SUM([Direction] * [MonetaryValue]) AS Payment
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	GROUP BY E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId], E.[NotedDate], E.[InternalReference], E.[ExternalReference]
)
INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].Entries[1].MonetaryValue',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 3)  AS ErrorMessage
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.DocumentIndex = L.DocumentIndex
	JOIN AgentPayments AP
		ON  (AP.[AccountId] = E.[AccountId])
		AND (AP.[CenterId] = E.[CenterId])
		AND (AP.[CurrencyId] = E.[CurrencyId])
		AND (AP.[AgentId] = E.[AgentId])
		AND (AP.[ResourceId] IS NULL		AND E.[ResourceId] IS NULL			OR AP.[ResourceId] = E.[ResourceId])
		AND (AP.[NotedAgentId] IS NULL		AND E.[NotedAgentId] IS NULL		OR AP.[NotedAgentId] = E.[NotedAgentId])
		AND (AP.[NotedResourceId] IS NULL	AND E.[NotedResourceId] IS NULL		OR AP.[NotedResourceId] = E.[NotedResourceId])
		AND (AP.[NotedDate] IS NULL			AND E.[NotedDate] IS NULL			OR AP.[NotedDate] = E.[NotedDate])
		AND (AP.[InternalReference] IS NULL AND E.[InternalReference] IS NULL	OR AP.[InternalReference] = E.[InternalReference])
		AND (AP.[ExternalReference] IS NULL AND E.[ExternalReference] IS NULL	OR AP.[ExternalReference] = E.[ExternalReference])
	WHERE AP.[Payment] + AP.[DueBalance] > 0
SELECT * FROM @ValidationErrors;