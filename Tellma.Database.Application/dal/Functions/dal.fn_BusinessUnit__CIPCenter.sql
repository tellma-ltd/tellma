CREATE FUNCTION [dal].[fn_BusinessUnit__CIPCenter] (
	@CenterId	INT
)
RETURNS INT
AS
-- TODO: Validate that only one OtherPL can come under a single business unit
BEGIN
DECLARE @BusinessUnitNode HIERARCHYID;
SELECT @BusinessUnitNode = [Node] FROM dbo.Centers WHERE [Id] = @CenterId ;
	RETURN (
		SELECT TOP 1 [Id] FROM dbo.Centers
		WHERE CenterType = N'ConstructionInProgressExpendituresControl'
		AND [Node].IsDescendantOf((@BusinessUnitNode)) = 1
	)
END;