CREATE FUNCTION [dal].[fn_Unit__UnitType] (
	@Id	INT
)
RETURNS NVARCHAR (50)
AS
BEGIN
	RETURN 	(
		SELECT [UnitType] FROM [dbo].[Units]
		WHERE [Id] = @Id
	)
END