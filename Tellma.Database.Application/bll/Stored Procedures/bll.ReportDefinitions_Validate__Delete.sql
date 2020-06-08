CREATE PROCEDURE [bll].[ReportDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP(@Top) * FROM @ValidationErrors;