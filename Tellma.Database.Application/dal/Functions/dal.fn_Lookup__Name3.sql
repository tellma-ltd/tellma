CREATE FUNCTION [dal].[fn_Lookup__Name3] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name3] FROM [dbo].[Lookups]
		WHERE [Id] = @Id
	)
END