CREATE FUNCTION [dal].[fn_Resource__Name2] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name2] FROM [dbo].[Resources]
		WHERE [Id] = @Id
	)
END