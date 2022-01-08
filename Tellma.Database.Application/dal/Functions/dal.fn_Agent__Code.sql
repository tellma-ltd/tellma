CREATE FUNCTION [dal].[fn_Agent__Code] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END