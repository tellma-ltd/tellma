CREATE FUNCTION [dal].[fn_Agent__Name2] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name2] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END