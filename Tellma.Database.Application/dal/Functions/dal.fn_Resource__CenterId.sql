CREATE FUNCTION [dal].[fn_Resource__CenterId] (
	@Id INT
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [CenterId] FROM [dbo].[Resources]
		WHERE [Id] = @Id
	)
END