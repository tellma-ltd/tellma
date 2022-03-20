CREATE FUNCTION [map].[EmailTemplateSubscribers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[EmailTemplateSubscribers]
);
