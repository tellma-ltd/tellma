CREATE FUNCTION [bll].[fn_ConvertUnits]
(
	@FromUnitId INT,
	@ToUnitId INT
)
RETURNS DECIMAL (19,4)
AS
BEGIN
	IF @FromUnitId = @ToUnitId
		RETURN 1;

	RETURN (
		SELECT [UnitAmount]/[BaseAmount]
		FROM dbo.[Units]
		WHERE [Id] = @FromUnitId
	) /	(
		SELECT [UnitAmount]/[BaseAmount]
		FROM dbo.[Units]
		WHERE [Id] = @ToUnitId
	);
END;