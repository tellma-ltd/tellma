CREATE PROCEDURE [bll].[Documents_Validate__Unpost]
	@DefinitionId NVARCHAR(50),
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Cannot open it if it is not closed (C#)
	-- cannot open if the document posting date falls in an archived period.

	-- TODO: Might be useful to define a separate archive date for each operating segment
INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].DocumentDate',
		N'Error_FallsinArchivedPeriod'
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	WHERE D.[DocumentDate] < (SELECT [ArchiveDate] FROM dbo.Settings)

	SELECT TOP (@Top) * FROM @ValidationErrors;