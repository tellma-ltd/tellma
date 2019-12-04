CREATE PROCEDURE [bll].[Documents_Validate__Save]
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].[DocumentLineEntryList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- (SL Check)  Cannot save with a future date, (Settings dependent)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].DocumentDate',
		N'Error_TheTransactionDate0IsInTheFuture',
		[DocumentDate]
	FROM @Documents
	WHERE ([DocumentDate] > DATEADD(DAY, 1, @Now)) -- More accurately, FE.[DocumentDate] > CONVERT(DATE, SWITCHOFFSET(@Now, user_time_zone)) 

	-- (FE Check) If Resource = functional currency, the value must match the money amount
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST([DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST([DocumentLineIndex] AS NVARCHAR (255)) + '].Amount' + CAST([EntryNumber] AS NVARCHAR(255)),
		N'Error_TheAmount0DoesNotMatchTheValue1',
		[MonetaryValue],
		[Value]
	FROM @Entries E
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	WHERE (E.[CurrencyId] = dbo.[fn_FunctionalCurrencyId]())
	AND ([Value] <> [MonetaryValue] )

	-- (FE Check, DB constraint)  Cannot save with a date that lies in the archived period
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].DocumentDate',
		N'Error_TheTransactionDateIsBeforeArchiveDate0',
		(SELECT TOP 1 ArchiveDate FROM dbo.Settings)
	FROM @Documents
	WHERE [DocumentDate] < (SELECT TOP 1 ArchiveDate FROM dbo.Settings) 
	
	-- (FE Check, DB IU trigger) Cannot save a document not in ACTIVE state
	-- TODO: if it is not allowed to change a line once (Requested), report error
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].DocumentState',
		N'Error_CanOnlySaveADocumentInActiveState'
	FROM @Documents FE
	JOIN [dbo].[Documents] BE ON FE.[Id] = BE.[Id]
	WHERE (BE.[State] <> N'Active')

	-- TODO: For the cases below, add the condition that Entry Classification is enforced

	-- Entry Type Id is missing when required
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST([DocumentLineIndex] AS NVARCHAR (255)) + '].DocumentLineEntries[' + CAST([Index] AS NVARCHAR(255)) + ']',
		N'Error_TheAccountType0RequiresAnEntryClassification', A.[AccountTypeId]
	FROM @Entries E
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.AccountTypes [AT] ON A.[AccountTypeId] = [AT].[Id]
	WHERE (E.[EntryClassificationId] IS NULL) AND [AT].EntryClassificationParentCode IS NOT NULL;

	-- Invalid Entry Type Id
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST(E.[DocumentLineIndex] AS NVARCHAR (255)) + '].DocumentLineEntries[' + CAST([Index] AS NVARCHAR(255)) + ']',
		N'Error_IncompatibleResourceClassification0AndEntryClassification1',
		RC.[Code] AS [ResourceClassification], EC.[Code] AS [EntryClassification]
	FROM @Entries E
	JOIN dbo.ResourceClassifications RC ON E.ResourceClassificationId = RC.[Id]
	JOIN dbo.EntryClassifications EC ON E.EntryClassificationId = EC.[Id]
	LEFT JOIN dbo.[ResourceClassificationsEntryClassifications] RCEC ON (E.[ResourceClassificationId] = RCEC.[ResourceClassificationId]) AND (E.[EntryClassificationId] = RCEC.[EntryClassificationId])
	WHERE (E.[EntryClassificationId] IS NOT NULL AND RCEC.[EntryClassificationId] IS NULL);

	-- RelatedAgent is required for selected account definition, 
	--INSERT INTO @ValidationErrors([Key], [ErrorName])
	--SELECT
	--	'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
	--		CAST(E.[DocumentLineIndex] AS NVARCHAR (255)) + '].DocumentLineEntries[' + CAST([Index] AS NVARCHAR(255)) + ']',
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
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST(E.[DocumentLineIndex] AS NVARCHAR (255)) + '].IfrsEntryClassificationId' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
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
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST(E.[DocumentLineIndex] AS NVARCHAR (255)) + '].ExternalReference' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
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
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST(E.[DocumentLineIndex] AS NVARCHAR (255)) + '].RelatedReference' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
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
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST(E.[DocumentLineIndex] AS NVARCHAR (255)) + '].RelatedResourceId' + CAST(E.[EntryNumber] AS NVARCHAR(255)),
		N'Error_TheRelatedResourceIsNotSpecified'
	FROM @Entries E
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	JOIN dbo.[IfrsAccountClassifications] IA ON A.[IfrsClassificationId] = IA.Id
	WHERE (E.[RelatedResourceId] IS NULL)
	AND (IA.[RelatedResourceSetting] = N'Required');
*/
	SELECT TOP (@Top) * FROM @ValidationErrors;