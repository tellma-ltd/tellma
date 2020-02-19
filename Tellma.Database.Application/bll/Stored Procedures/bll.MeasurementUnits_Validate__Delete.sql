CREATE PROCEDURE [bll].[MeasurementUnits_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: Make sure the unit is not in table Resource Units


	SELECT TOP(@Top) * FROM @ValidationErrors;