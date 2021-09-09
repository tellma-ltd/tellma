CREATE FUNCTION [dal].[fn_Agent__Name] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END