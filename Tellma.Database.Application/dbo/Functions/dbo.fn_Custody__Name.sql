CREATE FUNCTION [dbo].[fn_Custody__Name] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Custodies]
		WHERE [Id] = @Id
	)
END