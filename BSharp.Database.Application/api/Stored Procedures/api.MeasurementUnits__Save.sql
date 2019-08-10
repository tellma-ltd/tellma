CREATE PROCEDURE [api].[MeasurementUnits__Save]
	@Entities [MeasurementUnitList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
	INSERT INTO @ValidationErrors
	EXEC [bll].[MeasurementUnits_Validate__Save]
		@Entities = @Entities;
	
	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[MeasurementUnits__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END;