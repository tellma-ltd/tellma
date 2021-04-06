CREATE FUNCTION [dbo].[fn_Relation__Name] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Relations]
		WHERE [Id] = @Id
	)
END