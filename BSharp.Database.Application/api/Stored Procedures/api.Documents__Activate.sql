CREATE PROCEDURE [api].[Documents__Activate]
	@param1 int = 0,
	@param2 int
AS
	SELECT @param1, @param2
	-- Posting date must not be within Archived period
RETURN 0
