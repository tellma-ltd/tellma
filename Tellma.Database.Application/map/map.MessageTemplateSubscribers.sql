CREATE FUNCTION [map].[MessageTemplateSubscribers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[MessageTemplateSubscribers]
);
