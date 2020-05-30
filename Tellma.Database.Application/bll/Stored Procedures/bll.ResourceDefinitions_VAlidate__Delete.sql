CREATE PROCEDURE [bll].[ResourceDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that ResourceDefinitionId is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResourceDefinitionIsUsedInResource0',
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Ids FE
	JOIN dbo.Resources R ON R.[DefinitionId] = FE.[Id]

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	SELECT TOP(@Top) * FROM @ValidationErrors;