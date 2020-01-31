CREATE PROCEDURE [bll].[ReportDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@TOP INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP(@Top) * FROM @ValidationErrors;