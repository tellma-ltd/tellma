CREATE FUNCTION [bll].[fn_OtherPLCenter__BusinessUnit]
(
	@CenterId	INT
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Centers
		WHERE CenterType = N'OtherPL'
		AND [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = @CenterId)) = 1
	)
END;