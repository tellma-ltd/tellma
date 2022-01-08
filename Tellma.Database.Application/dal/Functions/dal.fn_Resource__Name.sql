CREATE FUNCTION [dal].[fn_Resource__Name] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name] FROM [dbo].[Resources]
		WHERE [Id] = @Id
	)
END