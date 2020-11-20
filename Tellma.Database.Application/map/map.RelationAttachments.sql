CREATE FUNCTION [map].[RelationAttachments]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[RelationAttachments]
);
