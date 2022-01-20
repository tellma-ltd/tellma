CREATE FUNCTION [dal].[fn_Center__Code] (
	@CenterId	INT
)
RETURNS NVARCHAR (50)
AS
BEGIN
	RETURN (
		SELECT [Code] FROM dbo.Centers
		WHERE [Id] = @CenterId
	)
END;