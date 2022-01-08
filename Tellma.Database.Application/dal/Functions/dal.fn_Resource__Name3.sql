CREATE FUNCTION [dal].[fn_Resource__Name3] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Name3] FROM [dbo].[Resources]
		WHERE [Id] = @Id
	)
END