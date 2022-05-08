CREATE FUNCTION [dal].[fn_BusinessUnit__OtherPLCenter] (
	@CenterId	INT
)
RETURNS INT
AS
BEGIN
DECLARE @BusinessUnitNode HIERARCHYID;
SELECT @BusinessUnitNode = [Node] FROM dbo.Centers WHERE [Id] = @CenterId ;
	RETURN (
		SELECT TOP 1 [Id] FROM dbo.Centers
		WHERE CenterType = N'OtherPL'
		AND [Node].IsDescendantOf((@BusinessUnitNode)) = 1
	)
END;