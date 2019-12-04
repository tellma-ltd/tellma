CREATE PROCEDURE [dbo].[bll_Transactions_Validate__Unpost]
-- TODO: replace with bll.Lines_Validate__Unsign
	@Entities [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	DECLARE @ArchiveDate DATETIMEOFFSET(7) = (SELECT ArchiveDate FROM dbo.Settings);

	-- Cannot unpost if the period is closed	
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].[DocumentDate]',
		N'Error_TheDocumentDate0FallsBefore1ArchiveDate',
		BE.[DocumentDate],
		@ArchiveDate
	FROM @Entities FE
	JOIN [dbo].[Documents] BE ON FE.[Id] = BE.[Id]
	WHERE (BE.[DocumentDate] < @ArchiveDate)

	-- Cannot unsign if not the user who signed

	-- No inactive account
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].TransactionEntries[' +
			CAST(E.[Id] AS NVARCHAR (255)) + '].AccountId',
		N'Error_Account0IsInactive',
		A.[Name]
	FROM @Entities FE
	JOIN dbo.[Entries] E ON FE.[Id] = E.[LineId]
	JOIN dbo.[AccountClassifications] A ON E.[AccountId] = A.[Id]
	WHERE (A.[IsDeprecated] = 1);

	-- No inactive custody
	-- No inactive resource
	SELECT TOP (@Top) * FROM @ValidationErrors;