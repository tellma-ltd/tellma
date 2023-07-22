CREATE FUNCTION [dal].[fn_Resource__FromDate] (
	@ResourceId INT
)
RETURNS DATE
AS
BEGIN
	RETURN 	(
		SELECT [FromDate] FROM [dbo].[Resources]
		WHERE [Id] = @ResourceId
	)
END
GO