CREATE FUNCTION [map].[LineDefinitionEntries]()
RETURNS TABLE
AS
RETURN (
	SELECT 
		E.*, 
		T.[IsResourceClassification] AS [AccountTypeParentIsResourceClassification],
		T.[EntryTypeParentId] AS [EntryTypeParentId]
	FROM [dbo].[LineDefinitionEntries] E
	LEFT JOIN [dbo].[AccountTypes] T ON E.[AccountTypeParentId] = T.[Id]
);
