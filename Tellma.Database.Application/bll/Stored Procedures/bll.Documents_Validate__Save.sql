CREATE PROCEDURE [bll].[Documents_Validate__Save]
	@DefinitionId NVARCHAR(50),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();


	/* [C# Validation]
	
	 [✓] The DocumentDate is not after 1 day in the future
	 [✓] The DocumentDate cannot be before archive date
	 [✓] IF Resource == Functional currency THEN assert: Value == MonetaryValue

	*/

	-- TODO: Validate that all non-zero attachment Ids exist in the DB

	-- (FE Check) If CurrencyId = functional currency, the value must match the DECIMAL (19,4) amount
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	--SELECT
	--	'[' + CAST([DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
	--		CAST([LineIndex] AS NVARCHAR (255)) + '].Amount' + CAST([EntryNumber] AS NVARCHAR(255)),
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
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].DocumentState',
		N'Error_CanOnlySaveADocumentInActiveState'
	FROM @Documents FE
	JOIN [dbo].[Documents] BE ON FE.[Id] = BE.[Id]
	WHERE BE.[State] IN (-5, +5); -- Closed

	-- TODO: For the cases below, add the condition that Entry Type is enforced

	-- Missing Entry Type for given Account Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST([LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST([Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_TheAccountType0RequiresAnEntryType',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS AccountType
	FROM @Entries E
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	WHERE (E.[EntryTypeId] IS NULL) AND [AT].[EntryTypeParentId] IS NOT NULL;

	-- Invalid Entry Type for Account Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_IncompatibleAccountType0AndEntryType1',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS AccountType,
		ETE.[Code]
	FROM @Entries E
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].Id
	JOIN dbo.[EntryTypes] ETE ON E.[EntryTypeId] = ETE.Id
	JOIN dbo.[EntryTypes] ETA ON [AT].[EntryTypeParentId] = ETA.[Id]
	WHERE ETE.[Node].IsDescendantOf(ETA.[Node]) = 0

	-- If Account HasAgent = 1, then AgentId is required
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].AgentId' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
		N'Error_TheAgentIsNotSpecified'
	FROM @Entries E
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	WHERE (E.[AgentId] IS NULL)
	AND (A.[HasAgent] = 1);

	-- If Account HasResource = 1, then ResourceId is required
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].ResourceId' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
		N'Error_TheResourceIsNotSpecified'
	FROM @Entries E
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	WHERE (E.[ResourceId] IS NULL)
	AND (A.[HasResource] = 1);
	
	SELECT TOP (@Top) * FROM @ValidationErrors;