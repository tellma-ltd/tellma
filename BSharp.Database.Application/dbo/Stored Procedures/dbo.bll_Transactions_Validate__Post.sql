CREATE PROCEDURE [dbo].[bll_Transactions_Validate__Post]
	@Entities [dbo].[IndexedUuidList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot post unless in draft mode
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsNotInDraftState'
	FROM @Entities
	WHERE [Id] IN (
		SELECT [Id] FROM [dbo].[Documents]
		WHERE [State] <> N'Draft'
	);

	-- Cannot post with no entries
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheTransactionHasNoEntries'
	FROM @Entities 
	WHERE [Id] NOT IN (
		SELECT [DocumentLineId] FROM dbo.[DocumentLineEntries]
	);

	-- Cannot post a non-balanced transaction
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TransactionHasDebitCreditDifference0',
		SUM([Direction] * [Value])
	FROM @Entities FE
	JOIN dbo.[DocumentLineEntries] TE ON FE.[Id] = TE.[DocumentLineId]
	GROUP BY FE.[Index]
	HAVING SUM([Direction] * [Value]) <> 0;

	-- No inactive account
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsInactive',
		A.[Name]
	FROM @Entities FE
	JOIN dbo.[DocumentLineEntries] TE ON FE.[Id] = TE.[DocumentLineId]
	JOIN dbo.[Accounts] A ON TE.[AccountId] = A.[Id]
	WHERE (A.[IsActive] = 0);

	-- No inactive responsibility center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResponsibilityCenter0IsInactive',
		RC.[Name]
	FROM @Entities FE
	JOIN dbo.[DocumentLineEntries] TE ON FE.[Id] = TE.[DocumentLineId]
	JOIN dbo.[ResponsibilityCenters] RC ON TE.ResponsibilityCenterId = RC.[Id]
	WHERE (RC.[IsActive] = 0);

	-- No inactive Resource
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResource0IsInactive',
		R.[Name]
	FROM @Entities FE
	JOIN dbo.[DocumentLineEntries] TE ON FE.[Id] = TE.[DocumentLineId]
	JOIN dbo.[Resources] R ON TE.ResponsibilityCenterId = R.[Id]
	WHERE (R.[IsActive] = 0);

	SELECT @ValidationErrorsJson = (SELECT * FROM @ValidationErrors	FOR JSON PATH);