CREATE PROCEDURE [bll].[EntryTypes_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotDeleteSystemRecords'
	FROM @Ids FE
    JOIN [dbo].[EntryTypes] BE ON FE.[Id] = BE.[Id]
	WHERE BE.[IsSystem] = 1;

	SELECT TOP (@Top) * FROM @ValidationErrors;