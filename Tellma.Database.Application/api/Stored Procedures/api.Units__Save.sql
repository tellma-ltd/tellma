CREATE PROCEDURE [api].[Units__Save]
	@Entities [UnitList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
	INSERT INTO @ValidationErrors
	EXEC [bll].[Units_Validate__Save]
		@Entities = @Entities;
	
	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Units__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END;