CREATE FUNCTION bll.[fn_firstDayOfPeriod]()
RETURNS TINYINT
AS
BEGIN
	RETURN (
		SELECT [FirstDayOfPeriod]
		FROM dbo.Settings
	)
END