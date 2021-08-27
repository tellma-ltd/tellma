CREATE FUNCTION bll.[fn_firstDayOfPeriod]()
-- TODO: Rename to fn_SalaryPeriodFirstDay, and fix 2 validation scripts
-- 	select * from Linedefinitions where ValidateScript like N'%fn_firstDayOfPeriod%' -- 2
-- Need to 
RETURNS TINYINT
AS
BEGIN
	RETURN (
		SELECT [FirstDayOfPeriod]
		FROM dbo.Settings
	)
END