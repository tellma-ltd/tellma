CREATE FUNCTION [dal].[fn_Center__Node] (
	@CenterId	INT
)
RETURNS HIERARCHYID
AS
BEGIN
	RETURN (
		SELECT [Node] FROM dbo.Centers
		WHERE [Id] = @CenterId
	)
END;