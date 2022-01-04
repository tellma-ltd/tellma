CREATE FUNCTION [dal].[fn_Lookup__Code] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].[Lookups]
		WHERE [Id] = @Id
	)
END