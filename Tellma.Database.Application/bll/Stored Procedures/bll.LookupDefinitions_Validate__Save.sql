CREATE PROCEDURE [bll].[LookupDefinitions_Validate__Save]
	@Entities [LookupDefinitionList] READONLY, -- @ValidationErrorsJson NVARCHAR(MAX) OUTPUT,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP(@Top) * FROM @ValidationErrors;