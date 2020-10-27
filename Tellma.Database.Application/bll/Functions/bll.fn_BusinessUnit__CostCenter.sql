CREATE FUNCTION [bll].[fn_BusinessUnit__CostCenter]
(
	@CenterId	INT
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.Centers
		WHERE CenterType = N'BusinessUnit'
		AND (
			SELECT [Node] FROM dbo.Centers
			WHERE [Id] = @CenterId
		).IsDescendantOf([Node]) = 1
	)
END;