CREATE PROCEDURE [api].[Resources__Save]
	@Resources [dbo].[ResourceList] READONLY,
	@Instances [dbo].[ResourceInstanceList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Resources_Validate__Save]
		@Resources = @Resources,
		@Instances = @Instances;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Resources__Save]
		@Resources = @Resources,
		@Instances = @Instances,
		@ReturnIds = @ReturnIds;
END;