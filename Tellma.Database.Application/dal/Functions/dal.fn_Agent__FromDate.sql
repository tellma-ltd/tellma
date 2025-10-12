CREATE FUNCTION [dal].[fn_Agent__FromDate] (
	@Id INT
)
RETURNS DATE
AS
BEGIN
	RETURN 	(
		SELECT [FromDate] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END