CREATE PROCEDURE [bll].[Documents_Validate__Assign]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Can only assign while in (Active) state
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsIn0State',
		-- TODO: localize state name
		CAST(BE.[State] AS NVARCHAR(50))
	FROM @Ids FE
	JOIN [dbo].[Documents] BE ON FE.[Id] = BE.[Id]
	JOIN [dbo].[DocumentDefinitions] DT ON BE.[DefinitionId] = DT.Id
	--WHERE (BE.[State] <> N'Active');
	WHERE (BE.[State] = 5);

	SELECT TOP (@Top) * FROM @ValidationErrors;