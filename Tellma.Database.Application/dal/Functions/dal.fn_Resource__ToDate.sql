CREATE FUNCTION [dal].[fn_Resource__ToDate] (
	@Id INT
)
RETURNS DATE
AS
BEGIN
	RETURN 	(
		SELECT [ToDate] FROM [dbo].[Resources]
		WHERE [Id] = @Id
	)
END
GO