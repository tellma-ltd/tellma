CREATE FUNCTION [dal].[fn_Unit__BaseAmount](
	@UnitId INT
)
RETURNS FLOAT
BEGIN
	RETURN (SELECT [BaseAmount] FROM dbo.Units WHERE [Id] = @UnitId);
END
GO
