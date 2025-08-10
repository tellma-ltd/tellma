CREATE FUNCTION [dal].[fn_Agent__Text2] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Text2] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END