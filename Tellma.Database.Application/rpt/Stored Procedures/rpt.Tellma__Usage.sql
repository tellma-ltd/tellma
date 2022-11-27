CREATE PROCEDURE [rpt].[Tellma__Usage]
	@param1 int = 0,
	@param2 int
AS
SELECT * from dbo.AgentDefinitions 
WHERE Id in
(SELECT DefinitionId FROM dbo.Agents WHERE Id in (
	SELECT AgentId FROM Entries
	UNION
	SELECT NotedAgentId FROM dbo.Entries)
	) --	17

SELECT * from resourcedefinitions 
WHERE Id in (
SELECT DefinitionId FROM dbo.Resources WHERE Id in (
	SELECT ResourceId FROM dbo.Entries
	UNION
	SELECT NotedResourceId FROM dbo.Entries)
	) -- 20
RETURN 0
