CREATE PROCEDURE [rpt].[UnbalancedCSI___Balance]
AS
-- Due to a bug in AVCO, some transactions were left unbalanced. This was used to fix them.
-- However, the bug may have been already fixed.
-- Adjust COGS to Average cost issued
DECLARE @DefinitionId INT = (SELECT Id FROM LineDefinitions WHERE Code = N'RevenueFromInventoryWithPointInvoice')
UPDATE E0
SET
	E0.MonetaryValue = E1.MonetaryValue,
	E0.[Value] = E1.[Value]
FROM dbo.Entries E0
JOIN dbo.Entries E1 ON E0.LineId = E1.LineId
JOIN dbo.Lines L ON E0.LineId = L.Id AND E1.LineId = L.Id
JOIN map.Documents() D ON L.DocumentId = D.Id
WHERE E0.[Index] = 0 AND E1.[Index] = 1
AND L.DefinitionId = @DefinitionId
AND  E0.MonetaryValue <> E1.MonetaryValue
AND D.Code LIKE N'CSI%'