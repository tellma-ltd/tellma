CREATE PROCEDURE [bll].[LD_CompatibleExpenseByNatureAgentAndEntryType__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@AccountEntryIndex INT,
	@ErrorEntryIndex INT,
	@ErrorFieldName NVARCHAR (255)
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'When the expense is related to a sale, the purpose should be cost of sales'), (0, N'ar',  N'عندما يكون المصروف مرتبطا ببيع، فإن الهدف ينبغي أن يكون تكلفة مبيعات '),
(1, N'en',  N'When the expense is unrelated to anything, the purpose should be Admin/Distribution/Other'), (1, N'ar',  N'عندما يكون المصروف غير مرتبط بشيئ، فإن الهدف لا بد أن يكون إداريا أو تسويقيا'),
(2, N'en',  N'When the expense is related to {0}, the purpose must be capitalization'), (2, N'ar',  N'عندما يكون المصروف مرتبطا ب{0}، فإن الهدف لا بد أن يكون الرسملة');

DECLARE @NullAgent INT = dal.fn_AgentDefinition_Code__Id(N'Null', N'Null');

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR(255)) + '].' + @ErrorFieldName,
dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0) AS ErrorMessage
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.DocumentIndex = FL.DocumentIndex
JOIN dbo.Accounts A ON FE.[AccountId] =  A.[Id]
JOIN dbo.Agents AG ON AG.[Id] = FE.[AgentId]
JOIN dbo.EntryTypes ET ON ET.[Id] = FE.[EntryTypeId]
WHERE FE.[Index] = @AccountEntryIndex
AND dal.fn_Agent__AgentDefinitionCode(AG.[DefinitionId]) = N'TradeReceivableAccount'
AND ET.[Concept] <> N'CostOfSales'
UNION
SELECT DISTINCT TOP (@Top)
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR(255)) + '].' + @ErrorFieldName,
dal.fn_ErrorNames_Index___Localize(@ErrorNames, 1) AS ErrorMessage
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.[DocumentIndex] = FL.[DocumentIndex]
JOIN dbo.Accounts A ON FE.[AccountId] =  A.[Id]
JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
JOIN dbo.EntryTypes ET ON ET.[Id] = FE.[EntryTypeId]
WHERE FE.[Index] = @AccountEntryIndex
AND FE.AgentId = @NullAgent
AND ET.[Concept] NOT IN (N'DistributionCosts', N'AdministrativeExpense', N'OtherExpenseByFunction');

INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR(255)) + '].' + @ErrorFieldName,
dal.fn_ErrorNames_Index___Localize(@ErrorNames, 2) AS ErrorMessage,
dbo.fn_Localize(AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS AgentDefinition
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.[DocumentIndex] = FL.[DocumentIndex]
JOIN dbo.Accounts A ON FE.[AccountId] =  A.[Id]
JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
JOIN dbo.Agents AG ON AG.[Id] = FE.[AgentId]
JOIN dbo.AgentDefinitions AD ON AD.[Id] = AG.[DefinitionId]
JOIN dbo.EntryTypes ET ON ET.[Id] = FE.[EntryTypeId]
WHERE FE.[Index] = @AccountEntryIndex
AND FE.AgentId <> @NullAgent
AND AD.[Code] <> N'TradeReceivableAccount'
AND ET.[Concept] <> N'CapitalizationExpenseByNatureExtension';

SELECT * FROM @ValidationErrors;