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

	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
	--          Common Validation (JV + Smart)
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
	
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

	
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
	--                 JV Validation
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

	-- Some Accounts of some Account Types require an Entry Type
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
		
	-- The Entry Type must be compatible with the Account Type
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
	
	-- If Account AgentDefinitionId IS NOT NULL, then AgentId is required
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
	
	-- If Account HasResource = 1, then ResourceId is required
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

	
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
	--             Smart Screen Validation
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
		
	-- The CurrencyId is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].CurrencyId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'CurrencyId'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[CurrencyId] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The AgentId is required if Line State >= RequiredState of line def column
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
	WHERE 
	(E.[AgentId] IS NULL) AND ISNULL(BEL.[State], 
		IIF(L.[DefinitionId] IN (SELECT DISTINCT W.[LineDefinitionId] FROM [dbo].[Workflows] W JOIN [dbo].[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]), 0, 4)
	) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';
	
	-- The ResourceId is required if Line State >= RequiredState of line def column
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

	-- TODO: Validate that ResourceId descends from LDE.AccountTypeParentId IFF it is has IsResourceClassification = 1
	
	-- The ResponsibilityCenterId is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].ResponsibilityCenterId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'ResponsibilityCenterId'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[ResponsibilityCenterId] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';
	
	-- The EntryTypeId is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].EntryTypeId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'EntryTypeId'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[EntryTypeId] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';
	
	---- Some Entry Definitions with some Account Types require an Entry Type
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT TOP (@Top)
	--	'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
	--		CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
	--	N'Error_TheField0IsRequired',
	--	dbo.fn_Localize([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [EntryTypeFieldName]
	--FROM @Entries E
	--JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	--JOIN [dbo].[LineDefinitionEntries] LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	--JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'EntryTypeId'
	--JOIN [dbo].[AccountTypes] [AT] ON LDE.[AccountTypeParentCode] = [AT].[Code] 
	--WHERE (E.[EntryTypeId] IS NULL) AND [AT].[EntryTypeParentId] IS NOT NULL AND L.DefinitionId <> N'ManualLine';

	-- The Entry Type must be compatible with the Account Type
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

	-- The DueDate is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].DueDate',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'DueDate'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[DueDate] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The MonetaryValue is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].MonetaryValue',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'MonetaryValue'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[MonetaryValue] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The Quantity is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].Quantity',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'Quantity'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[Quantity] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The UnitId is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].UnitId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'UnitId'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[UnitId] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The Value is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].Value',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'Value'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[Value] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The Time1 is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].Time1',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'Time1'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[Time1] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The Time2 is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].Time2',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'Time2'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[Time2] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The ExternalReference is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].ExternalReference',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'ExternalReference'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[ExternalReference] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The AdditionalReference is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].AdditionalReference',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'AdditionalReference'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[AdditionalReference] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';
	
	-- The NotedAgentId is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].NotedAgentId',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'NotedAgentId'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[NotedAgentId] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';
	
	-- The NotedAgentName is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].NotedAgentName',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'NotedAgentName'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[NotedAgentName] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The NotedAmount is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].NotedAmount',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'NotedAmount'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[NotedAmount] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';

	-- The NotedDate is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].NotedDate',
		N'Error_TheField0IsRequired',
		[dbo].[fn_Localize]([LDC].[Label], [LDC].[Label2], [LDC].[Label3]) AS [FieldName]
	FROM @Entries E	
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'NotedDate'
	LEFT JOIN [dbo].[Lines] BEL ON L.Id = BEL.Id
	WHERE (E.[NotedDate] IS NULL) AND ISNULL(BEL.[State], 0) >= LDC.[RequiredState] AND L.[DefinitionId] <> N'ManualLine';




	
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
	--                 Pending Review
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
		

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
	WHERE L.[DefinitionId] NOT IN (SELECT [LineDefinitionId] FROM [dbo].[Workflows] WHERE [Id] IN (SELECT [WorkflowId] FROM [dbo].[WorkflowSignatures]))
	AND E.[AccountId] IS NULL
	AND (E.[Value] <> 0 OR E.[Quantity] IS NOT NULL AND E.[Quantity] <> 0);

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