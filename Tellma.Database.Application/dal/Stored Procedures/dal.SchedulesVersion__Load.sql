CREATE PROCEDURE [dal].[SchedulesVersion__Load]
	@Version NVARCHAR(255) OUTPUT
AS
BEGIN
	SET @Version = (SELECT [SchedulesVersion] FROM [dbo].[Settings])
END