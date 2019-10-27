CREATE PROCEDURE [api].[Documents__Void]
	@param1 int = 0,
	@param2 int
AS
-- Must move all lines to final negative states.
	SELECT @param1, @param2
RETURN 0
