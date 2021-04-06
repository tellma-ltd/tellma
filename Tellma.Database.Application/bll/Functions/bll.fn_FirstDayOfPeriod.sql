CREATE FUNCTION bll.[fn_firstDayOfPeriod]()
RETURNS TINYINT
AS
BEGIN
	DECLARE @ShortCompanyName NVARCHAR (255) = (SELECT [ShortCompanyName] FROM dbo.Settings) ;

	RETURN (
		SELECT ISNULL([FirstDayOfPeriod],IIF(@ShortCompanyName LIKE N'Banan%', 1, 25))
		FROM dbo.Settings
	)
END