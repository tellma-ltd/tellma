CREATE FUNCTION [dal].[fn_Currency__E] (
	@CurrencyId NCHAR (3)
)
RETURNS SMALLINT
AS
BEGIN
	RETURN (
		SELECT [E] FROM dbo.Currencies
		WHERE [Id] = @CurrencyId
	)
END;