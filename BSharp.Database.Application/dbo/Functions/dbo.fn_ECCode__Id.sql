CREATE FUNCTION [dbo].[fn_ECCode__Id]
(
	@Code NVARCHAR(255)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id] FROM dbo.EntryClassifications
		WHERE [Code] = @Code
	)
END;