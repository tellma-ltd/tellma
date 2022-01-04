CREATE FUNCTION [dal].[fn_Lookup__Name2] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name2] FROM [dbo].[Lookups]
		WHERE [Id] = @Id
	)
END