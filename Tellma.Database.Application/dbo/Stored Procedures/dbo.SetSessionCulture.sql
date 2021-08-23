CREATE PROCEDURE [dbo].[SetSessionCulture]
	@Culture NVARCHAR (255),
	@NeutralCulture NVARCHAR (255)
AS
BEGIN
	SET NOCOUNT ON;

	-- Set the global values of the session context
	DECLARE @UserLanguageIndex TINYINT = [dbo].[fn_User__Language](@Culture, @NeutralCulture);
    EXEC sys.sp_set_session_context @key = N'UserLanguageIndex', @value = @UserLanguageIndex;
END;