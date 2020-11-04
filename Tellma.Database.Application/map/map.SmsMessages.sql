CREATE FUNCTION [map].[SmsMessages]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[SmsMessages]
);