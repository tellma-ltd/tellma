CREATE PROCEDURE [dbo].[api_Settings__Save]
	@Settings [SettingList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

-- Validate
/*	EXEC [dbo].[bll_Settings_Validate__Save]
		@Settings = @Settings,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
*/
-- TODO: use Setting data type not Table type
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
		/*
	EXEC [dbo].[dal_Settings__Save]
		@Settings = @Settings
	*/
END;