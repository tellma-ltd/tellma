CREATE FUNCTION [dal].[fn_BusinessUnit__SaleCenter] (
	@CenterId	INT
)
RETURNS INT
AS
-- TODO: Validate that only one OtherPL can come under a single business unit
BEGIN
DECLARE @BusinessUnitNode HIERARCHYID;
SELECT @BusinessUnitNode = [Node] FROM dbo.Centers WHERE [Id] = @CenterId ;
	RETURN (
		SELECT [Id] FROM dbo.Centers
		WHERE CenterType = N'Sale'
		AND [Node].IsDescendantOf((@BusinessUnitNode)) = 1
	)
END;