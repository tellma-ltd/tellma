CREATE FUNCTION [dal].[fn_Center__CenterType] (
	@CenterId	INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN (
		SELECT [CenterType] FROM dbo.Centers
		WHERE [Id] = @CenterId
	)
END;