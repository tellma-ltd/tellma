CREATE PROCEDURE [api].[AccountTypes__Save]
	@Entities [AccountTypeList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	EXEC [bll].[AccountTypes_Validate__Save]
		@Entities = @Entities;

	-- Add here Code that is handled by C#

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[AccountTypes__Save]
		@Entities = @Entities;
END;