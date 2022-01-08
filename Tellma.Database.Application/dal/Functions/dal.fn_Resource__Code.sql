CREATE FUNCTION [dal].[fn_Resource__Code] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].[Resources]
		WHERE [Id] = @Id
	)
END