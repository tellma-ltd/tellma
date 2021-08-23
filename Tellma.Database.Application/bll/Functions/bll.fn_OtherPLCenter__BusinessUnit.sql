CREATE FUNCTION [bll].[fn_OtherPLCenter__BusinessUnit] (
	@CenterId	INT
)
RETURNS INT
AS
BEGIN
DECLARE @BusinessUnitNode HIERARCHYID;
SELECT @BusinessUnitNode = [Node] FROM dbo.Centers WHERE [Id] = @CenterId ;
	RETURN (
		SELECT [Id] FROM dbo.Centers
		WHERE CenterType = N'OtherPL'
		AND [Node].IsDescendantOf((@BusinessUnitNode)) = 1
	)
END;