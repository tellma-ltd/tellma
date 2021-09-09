CREATE FUNCTION [bll].[fn_Center__BusinessUnit] (
	@CenterId	INT
)
RETURNS INT
AS
BEGIN
	DECLARE @Node HIERARCHYID;
	SELECT @Node = [Node] FROM dbo.Centers WHERE [Id] = @CenterId;
	RETURN (
		SELECT [Id] FROM dbo.Centers
		WHERE CenterType = N'BusinessUnit'
		AND @Node.IsDescendantOf([Node]) = 1
	)
END;