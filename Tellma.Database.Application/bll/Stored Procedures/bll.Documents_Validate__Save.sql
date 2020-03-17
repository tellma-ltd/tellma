CREATE PROCEDURE [bll].[Documents_Validate__Save]
	@DefinitionId NVARCHAR(50),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET()
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @IsOriginalDocument BIT = (SELECT IsOriginalDocument FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId);

	--=-=-=-=-=-=- [C# Validation]
	/* 
	
	 [✓] The SerialNumber is required if original document
	 [✓] The SerialNumber is not duplicated in the uploaded list
	 [✓] The DocumentDate is not after 1 day in the future
	 [✓] The DocumentDate cannot be before archive date
	 [✓] If Entry.CurrencyId is functional, the value must be the same as monetary value

	*/
	
	-- Serial number must not be already in the back end
	IF @IsOriginalDocument = 0
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].SerialNumber',
		N'Error_TheSerialNumber0IsUsed',
		CAST(FE.[SerialNumber] AS NVARCHAR (50))
	FROM @Documents FE
	JOIN [dbo].[Documents] BE ON FE.[SerialNumber] = BE.[SerialNumber]
	WHERE
		FE.[SerialNumber] IS NOT NULL
	AND BE.DefinitionId = @DefinitionId
	AND FE.Id <> BE.Id;

	-- TODO: Validate that all non-zero attachment Ids exist in the DB

	-- TODO: validate that the ResponsibilityType is conformant with the AccountType

	-- (FE Check) If CurrencyId = functional currency, the value must match the DECIMAL (19,4) amount
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	--SELECT
	--	'[' + CAST([DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
	--		CAST([LineIndex] AS NVARCHAR (255)) + '].Amount' + CAST([Index] AS NVARCHAR(255)),
	--	N'Error_TheAmount0DoesNotMatchTheValue1',
	--	[MonetaryValue],
	--	[Value]
	--FROM @Entries E
	--JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	--WHERE (E.[CurrencyId] = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId')),
	--AND ([Value] <> [MonetaryValue] )

	-- (FE Check, DB constraint)  Cannot save with a date that lies in the archived period
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + '].DocumentDate',
	--	N'Error_TheTransactionDateIsBeforeArchiveDate0',
	--	(SELECT TOP 1 ArchiveDate FROM dbo.Settings)
	--FROM @Documents
	--WHERE [DocumentDate] < (SELECT TOP 1 ArchiveDate FROM dbo.Settings) 
	
	-- (FE Check, DB IU trigger) Cannot save a CLOSED document
	-- TODO: if it is not allowed to change a line once (Requested), report error

	-- Must not edit a document that is already posted
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		CASE
			WHEN D.[PostingState] = 1 THEN N'Error_CannotEditPostedDocuments'
			WHEN D.[PostingState] = -1 THEN N'Error_CannotEditCanceledDocuments'
		END
	FROM @Documents FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[PostingState] <> 0; -- Posted or Canceled

	-- TODO: For the cases below, add the condition that Entry Type is enforced

	-- MANUAL Lines: Some Accounts of some Account Types require an Entry Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_ThePurposeIsRequiredBecauseAccountTypeIs0',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS [AccountType]
	FROM @Entries [E]
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[Accounts] [A] ON [E].[AccountId] = [A].[Id]
	JOIN [dbo].[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	WHERE ([E].[EntryTypeId] IS NULL) AND [AT].[EntryTypeParentId] IS NOT NULL AND L.DefinitionId = N'ManualLine';

	-- SMART Lines: Some Entry Definitions with some Account Types require an Entry Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_TheField0IsRequired',
		dbo.fn_Localize([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [EntryTypeFieldName]
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionEntries] LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'EntryTypeId'
	JOIN [dbo].[AccountTypes] [AT] ON LDE.[AccountTypeParentCode] = [AT].[Code] 
	WHERE (E.[EntryTypeId] IS NULL) AND [AT].[EntryTypeParentId] IS NOT NULL AND L.DefinitionId <> N'ManualLine';
		
	-- MANUAL Lines: The Entry Type must be compatible with the Account Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_IncompatibleAccountType0AndEntryType1',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS AccountType,
		dbo.fn_Localize([ETE].[Name], [ETE].[Name2], [ETE].[Name3]) AS AccountType
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].Id
	JOIN dbo.[EntryTypes] ETE ON E.[EntryTypeId] = ETE.Id
	JOIN dbo.[EntryTypes] ETA ON [AT].[EntryTypeParentId] = ETA.[Id]
	WHERE ETE.[Node].IsDescendantOf(ETA.[Node]) = 0 AND L.[DefinitionId] = N'ManualLine';

	-- SMART Lines: The Entry Type must be compatible with the Account Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_TheField0Value1IsIncompatible',
		dbo.fn_Localize([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [EntryTypeFieldName],
		dbo.fn_Localize([ETE].[Name], [ETE].[Name2], [ETE].[Name3]) AS AccountType
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionEntries] LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'EntryTypeId'
	JOIN [dbo].[AccountTypes] [AT] ON LDE.[AccountTypeParentCode] = [AT].[Code] 
	JOIN dbo.[EntryTypes] ETE ON E.[EntryTypeId] = ETE.Id
	JOIN dbo.[EntryTypes] ETA ON [AT].[EntryTypeParentId] = ETA.[Id]
	WHERE ETE.[Node].IsDescendantOf(ETA.[Node]) = 0 AND L.[DefinitionId] <> N'ManualLine';

	-- MANUAL Lines: If Account AgentDefinitionId IS NOT NULL, then AgentId is required
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].AgentId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([AD].[TitleSingular], [AD].[TitleSingular2], [AD].[TitleSingular3]) AS [FieldName]
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[Accounts] A On E.AccountId = A.Id
	JOIN [dbo].[AgentDefinitions] AD ON A.AgentDefinitionId = AD.Id -- Means there is A.AgentDefinitionId IS NOT NULL
	WHERE (E.[AgentId] IS NULL) AND (L.[DefinitionId] = N'ManualLine');

	-- SMART Lines: The AgentId is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].AgentId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'AgentId'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[AgentId] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- MANUAL Lines: If Account HasResource = 1, then ResourceId is required
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].ResourceId',
		N'Error_TheResourceIsRequired'
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	WHERE (E.[ResourceId] IS NULL)
	AND (A.[HasResource] = 1) AND L.[DefinitionId] <> N'ManualLine';
	
	-- SMART Lines: The ResourceId is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].ResourceId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'ResourceId'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[ResourceId] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';


	-- Validate W-less lines can be moved to state 4
	-- TODO: refactor code from here and bll.Lines_Validate__Sign
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0],[Argument1],[Argument2],[Argument3],[Argument4])
	SELECT TOP (@Top)
		'[' + CAST(E.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index]  AS NVARCHAR (255))+ '].AccountId',
		N'Error_LineNoAccountForEntryIndex0WithAccountType1Currency2Agent3Resource4',
		E.[Index],
		LDE.[AccountTypeParentCode],
		E.[CurrencyId],
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]),
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3])
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	LEFT JOIN dbo.LineDefinitionEntries LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	LEFT JOIN dbo.Agents AG ON E.AgentId = AG.Id
	LEFT JOIN dbo.Resources R ON E.ResourceId = R.Id
	WHERE L.DefinitionId NOT IN (SELECT LineDefinitionId FROM dbo.Workflows)
	AND E.AccountId IS NULL

	-- TODO: refactor code from here and bll.Lines_Validate__Sign
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST(E.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		A.[Name]
	FROM @Entries E
	JOIN dbo.[Accounts] A ON A.[Id] = E.[AccountId]
	WHERE (A.[IsDeprecated] = 1);

	SELECT TOP (@Top) * FROM @ValidationErrors;