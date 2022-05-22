CREATE PROCEDURE [bll].[LD_AccountHasDebitBalance__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@FD_Index_Str NVARCHAR (255) = NULL,
	@FDLDE_Index_Str NVARCHAR (255) = NULL,
	@FE_Index_Str NVARCHAR (255)
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'This amount will cause the {0} balance to become negative'), (0, N'ar',  N'هذا المبلغ سيجعل رصيد {0} سالبا ');

INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
SELECT DISTINCT TOP (@Top)
	N'[' + CAST(FD.[Index] AS NVARCHAR (255)) + N'].Lines[' + CAST(FL.[Index] AS NVARCHAR (255)) + N'].' + @FE_Index_Str,
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0)  AS ErrorMessage,
	BD.[Code]
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.[DocumentIndex] = FL.[DocumentIndex]
JOIN dbo.Entries BE 
ON BE.[AccountId] = FE.[AccountId] AND BE.[AgentId] = FE.[AgentId]
AND (BE.[NotedDate] IS NULL AND FE.[NotedDate] IS NULL OR BE.[NotedDate]  = 
AND 
JOIN dbo.Lines BL ON BL.[Id] = BE.[LineId]
JOIN map.Documents() BD ON BD.[Id] = BL.[DocumentId] 
JOIN dbo.Accounts A ON A.[Id] = BE.[AccountId]
JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
JOIN dbo.Agents NAG ON NAG.[Id] = BE.[NotedAgentId]
WHERE BD.[Id] <> FD.[Id]
AND AC.[Concept] IN (
	N'CurrentTradeReceivables',
	N'NoncurrentTradeReceivables',
	N'CurrentFinancialAssetsAtAmortisedCost',
	N'NoncurrentFinancialAssetsAtAmortisedCost'
)
AND BL.[State] >= 4;

SELECT * FROM @ValidationErrors;