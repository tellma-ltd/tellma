CREATE FUNCTION [dal].[fn_Agent__ToDate] (
	@Id INT
)
RETURNS DATE
AS
BEGIN
	RETURN 	(
		SELECT [ToDate] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END