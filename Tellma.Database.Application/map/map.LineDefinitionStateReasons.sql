CREATE FUNCTION [map].[LineDefinitionStateReasons]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionStateReasons]
);
