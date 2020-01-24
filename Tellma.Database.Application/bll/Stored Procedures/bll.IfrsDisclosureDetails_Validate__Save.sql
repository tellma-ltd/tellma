CREATE PROCEDURE [bll].[IfrsDisclosureDetails_Validate__Save]
	@Entities [IfrsDisclosureDetailList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Add any validation logic here

	SELECT TOP (@Top) * FROM @ValidationErrors;