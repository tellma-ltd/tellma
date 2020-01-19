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
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].DocumentState',
		N'Error_CanOnlySaveADocumentInActiveState'
	FROM @Documents FE
	JOIN [dbo].[Documents] BE ON FE.[Id] = BE.[Id]
	WHERE (BE.[State] = 5); -- Closed

	-- TODO: For the cases below, add the condition that Entry Type is enforced

	-- Missing Entry Type for given Account Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST([LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST([Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_TheAccountType0RequiresAnEntryType',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS AccountType
	FROM @Entries E
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	WHERE (E.[EntryTypeId] IS NULL) AND [AT].EntryTypeParentCode IS NOT NULL;

	-- Invalid Entry Type for Account Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_IncompatibleAccountType0AndEntryType1',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS AccountType,
		ETE.[Code]
	FROM @Entries E
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].Id
	JOIN dbo.[EntryTypes] ETE ON E.[EntryTypeId] = ETE.Id
	JOIN dbo.[EntryTypes] ETA ON [AT].[EntryTypeParentCode] = ETA.[Code]
	WHERE ETE.[Node].IsDescendantOf(ETA.[Node]) = 0

	-- RelatedAgent is required for selected account definition, 
	--INSERT INTO @ValidationErrors([Key], [ErrorName])
	--SELECT
	--	'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
	--		CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST([Index] AS NVARCHAR(255)) + ']',
	--	N'Error_TheRelatedAgentIsNotSpecified'
	--FROM @Entries E
	--JOIN dbo.[Accounts] A On E.AccountId = A.Id
	--JOIN dbo.[AccountGroups] AD ON A.[AccountDefinitionId] = AD.Id
	--WHERE (E.[RelatedAgentId] IS NULL)
	--AND (AD.[HasRelatedAgent] = 1);

	/* TODO: Revisit after the design is stable
	-- No inactive Account Type
	-- No expired Ifrs Note
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].IfrsEntryClassificationId' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
		N'Error_TheIfrsEntryClassificationId0HasExpired',
		IC.[Label]
	FROM @Entries E
	JOIN @Documents T ON E.[DocumentIndex] = T.[Index]
	JOIN dbo.[IfrsEntryClassifications] N ON E.[IfrsEntryClassificationId] = N.Id
	JOIN dbo.[IfrsConcepts] IC ON N.Id = IC.Id
	WHERE (IC.ExpiryDate < T.[DocumentDate]);
	
	-- External Reference is required for selected account and direction, 
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].ExternalReference' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
		N'Error_TheReferenceIsNotSpecified'
	FROM @Entries E
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	JOIN dbo.[IfrsAccountClassifications] IA ON A.[IfrsClassificationId] = IA.Id
	WHERE (E.ExternalReference IS NULL)
	AND (E.[Direction] = 1 AND IA.[DebitExternalReferenceSetting] = N'Required' OR
		E.[Direction] = -1 AND IA.[CreditExternalReferenceSetting] = N'Required');

	-- Additional Reference is required for selected account and direction, 
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].RelatedReference' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
		N'Error_TheRelatedReferenceIsNotSpecified'
	FROM @Entries E
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	JOIN dbo.[IfrsAccountClassifications] IA ON A.[IfrsClassificationId] = IA.Id
	WHERE (E.[AdditionalReference] IS NULL)
	AND (E.[Direction] = 1 AND IA.[DebitAdditionalReferenceSetting] = N'Required' OR
		E.[Direction] = -1 AND IA.[CreditAdditionalReferenceSetting] = N'Required');


	
	-- RelatedResource is required for selected account and direction, 
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].RelatedResourceId' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
		N'Error_TheRelatedResourceIsNotSpecified'
	FROM @Entries E
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	JOIN dbo.[IfrsAccountClassifications] IA ON A.[IfrsClassificationId] = IA.Id
	WHERE (E.[RelatedResourceId] IS NULL)
	AND (IA.[RelatedResourceSetting] = N'Required');
*/
	SELECT TOP (@Top) * FROM @ValidationErrors;