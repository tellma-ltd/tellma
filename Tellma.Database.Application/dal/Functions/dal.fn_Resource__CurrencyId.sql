CREATE FUNCTION [dal].[fn_Resource__CurrencyId] (
	@ResourceId INT
)
RETURNS NCHAR (3)
AS
BEGIN
	RETURN 	(
		SELECT [CurrencyId] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END