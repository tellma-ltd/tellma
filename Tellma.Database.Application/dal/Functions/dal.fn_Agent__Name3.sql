CREATE FUNCTION [dal].[fn_Agent__Name3] (
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