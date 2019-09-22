CREATE PROCEDURE [api].[Resources__Save]
	@DefinitionId NVARCHAR (255),
	@Resources [dbo].[ResourceList] READONLY,
	--@Picks [dbo].[ResourcePickList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @FilledResources [dbo].[ResourceList];

	INSERT INTO @FilledResources
	EXEC bll.Resources__Fill
		@DefinitionId = @DefinitionId,
		@Resources = @Resources;

	INSERT INTO @ValidationErrors
	EXEC [bll].[Resources_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @FilledResources;
	--	@Picks = @Picks;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Resources__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @FilledResources,
	--	@Picks = @Picks,
		@ReturnIds = @ReturnIds;
END;