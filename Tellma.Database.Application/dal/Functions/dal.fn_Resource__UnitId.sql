CREATE FUNCTION [dal].[fn_Resource__UnitId] (
	@ResourceId INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [UnitId] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END
