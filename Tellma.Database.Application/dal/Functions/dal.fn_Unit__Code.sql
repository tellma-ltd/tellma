CREATE FUNCTION [dal].[fn_Unit__Code] (
	@Id	INT
)
RETURNS NVARCHAR (50)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].[Units]
		WHERE [Id] = @Id
	)
END