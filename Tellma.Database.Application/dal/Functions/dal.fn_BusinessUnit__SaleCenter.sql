CREATE FUNCTION [dal].[fn_BusinessUnit__SaleCenter] (
	@CenterId	INT
)
RETURNS INT
AS
BEGIN
DECLARE @BusinessUnitNode HIERARCHYID;
SELECT @BusinessUnitNode = [Node] FROM dbo.Centers WHERE [Id] = @CenterId ;
	RETURN (
		SELECT TOP 1 [Id] FROM dbo.Centers
		WHERE CenterType = N'Sale'
		AND [Node].IsDescendantOf((@BusinessUnitNode)) = 1
	)
END;