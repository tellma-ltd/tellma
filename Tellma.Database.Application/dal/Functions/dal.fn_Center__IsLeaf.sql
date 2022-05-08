CREATE FUNCTION [dal].[fn_Center__IsLeaf] (
	@CenterId	INT
)
RETURNS BIT
AS
BEGIN
	RETURN (
		SELECT [IsLeaf]  FROM dbo.Centers
		WHERE [Id] = @CenterId
	)
END;