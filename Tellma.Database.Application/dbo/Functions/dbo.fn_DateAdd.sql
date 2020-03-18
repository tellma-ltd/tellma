CREATE FUNCTION [dbo].[fn_DateAdd]
(
	@UnitId INT,
	@Quantity DECIMAL (19,4),
	@Time1 DATETIME2
)
RETURNS DATETIME2
AS
BEGIN
	RETURN 
		CASE 
		WHEN @UnitID IN (
			SELECT [Id] FROM dbo.[Units] WHERE [Code] = N'MONTH'
			)
			THEN DATEADD(MONTH, @Quantity, @Time1)
		WHEN @UnitID IN (
			SELECT [Id] FROM dbo.[Units] WHERE [Code] = N'YEAR'
			)
			THEN DATEADD(YEAR, @Quantity, @Time1)
		END
END;