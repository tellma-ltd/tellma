CREATE PROCEDURE [api].[Resources__Save]
	@ResourceType NVARCHAR (255),
	@Resources [dbo].[ResourceList] READONLY,
	@Picks [dbo].[ResourcePickList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @FilledResources [dbo].[ResourceList];

	INSERT INTO @FilledResources
	EXEC bll.Resources__Fill
		@ResourceType = @ResourceType,
		@Resources = @Resources;

	INSERT INTO @ValidationErrors
	EXEC [bll].[Resources_Validate__Save]
		@ResourceType = @ResourceType,
		@Resources = @FilledResources,
		@Picks = @Picks;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Resources__Save]
		@ResourceType = @ResourceType,
		@Resources = @FilledResources,
		@Picks = @Picks,
		@ReturnIds = @ReturnIds;
END;