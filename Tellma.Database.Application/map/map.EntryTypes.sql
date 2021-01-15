CREATE FUNCTION [map].[EntryTypes]()
RETURNS TABLE
AS
RETURN (
	SELECT Q.*,
	CC.[ActiveChildCount],
	CC.ChildCount
	FROM [dbo].[EntryTypes] Q
	CROSS APPLY (
		SELECT COUNT(*) AS [ChildCount],
		SUM(IIF([IsActive]=1,1,0)) AS  [ActiveChildCount]	
		FROM [dbo].[EntryTypes]
		WHERE [Node].IsDescendantOf(Q.[Node]) = 1
	) CC 
);
