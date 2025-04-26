CREATE FUNCTION [dal].[fn_Center__BusinessUnit] (
	@CenterId	INT
)
RETURNS INT
AS
BEGIN	
	DECLARE @Node HIERARCHYID;
	SELECT @Node = [Node] FROM dbo.Centers WHERE [Id] = @CenterId;
	DECLARE @Result INT;
	SELECT @Result =  [Id] FROM dbo.Centers
	WHERE CenterType = N'BusinessUnit'
	AND @Node.IsDescendantOf([Node]) = 1;
	IF @Result IS NULL
		SELECT @Result =  [Id] FROM dbo.Centers
		WHERE ParentId IS NULL
		AND @Node.IsDescendantOf([Node]) = 1;
	RETURN @Result
END;
GO