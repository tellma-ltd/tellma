CREATE FUNCTION [dal].[fn_Lookup__Name] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Lookups]
		WHERE [Id] = @Id
	)
END