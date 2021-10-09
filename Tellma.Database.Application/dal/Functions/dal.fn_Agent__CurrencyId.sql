CREATE FUNCTION [dal].[fn_Agent__CurrencyId] (
	@Id INT
)
RETURNS NCHAR (3)
AS
BEGIN
	RETURN 	(
		SELECT [CurrencyId] FROM [dbo].[Agents]
		WHERE [Id] = @Id
	)
END