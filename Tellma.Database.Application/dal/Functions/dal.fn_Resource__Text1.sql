CREATE FUNCTION [dal].[fn_Resource__Text1] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Text1] FROM [dbo].[Resources]
		WHERE [Id] = @Id
	)
END