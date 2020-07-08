CREATE PROCEDURE [api].[Contracts__Delete]
	@DefinitionId INT,
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @Ids [dbo].[IdList];
	-- Add here Code that is handled by C#
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Contracts_Validate__Delete]
		@DefinitionId = @DefinitionId,
		@Ids = @IndexedIds;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Contracts__Delete] @Ids = @Ids;