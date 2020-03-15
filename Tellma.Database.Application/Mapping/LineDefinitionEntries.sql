CREATE FUNCTION [map].[LineDefinitionEntries]()
RETURNS TABLE
AS
RETURN (
	SELECT 
		E.*, 
		T.[Id] AS [AccountTypeParentId], 
		T.[IsResourceClassification] AS [AccountTypeParentIsResourceClassification],
		T.[EntryTypeParentId] AS [EntryTypeParentId]
	FROM [dbo].[LineDefinitionEntries] E
	LEFT JOIN [dbo].[AccountTypes] T ON E.[AccountTypeParentCode] = T.[Code]
);
