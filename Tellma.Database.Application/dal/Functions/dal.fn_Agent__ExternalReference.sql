CREATE FUNCTION [dal].[fn_Agent__ExternalReference] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [ExternalReference] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END