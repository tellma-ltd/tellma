CREATE FUNCTION [rpt].[Documents__Signatures] (
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE AS
RETURN
SELECT DISTINCT
		D.[Id],
		DT.Prefix + 
		REPLICATE(N'0', DT.[CodeWidth] - 1 - FLOOR(LOG10(D.SerialNumber))) +
		CAST(D.SerialNumber AS NVARCHAR(30)) AS [S/N],
		AG.[Name] AS [Signed By],
		RL.[Name] AS [Role],
		DS.[CreatedAt] AS [Signed At],
		DS.[ToState] AS [To Be],
		A2.[Name] AS [Recorded By],
		A3.[Name] AS [Revoked By],
		DS.[RevokedAt] AS [Revoked At]
	FROM dbo.Documents D
	JOIN dbo.[Lines] DL ON D.[Id] = DL.[DocumentId]
	JOIN dbo.[DocumentDefinitions] DT ON D.[DefinitionId] = DT.[Id]
	JOIN dbo.[LineSignatures] DS ON DL.[Id] = DS.[LineId]
	JOIN dbo.Agents AG ON DS.AgentId = AG.Id
	JOIN dbo.Roles RL ON DS.RoleId = RL.[Id]
	JOIN dbo.Agents A2 ON DS.[CreatedById] = A2.[Id]
	LEFT JOIN dbo.Agents A3 ON DS.[RevokedById] = A3.[Id]
	WHERE D.[Id] IN (SELECT [Id] FROM @Ids)