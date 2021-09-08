CREATE FUNCTION [dal].[fi_IfrsDisclosureDetails] (
	@fromDate Date = NULL, 
	@toDate Date = NULL
) RETURNS TABLE
AS
RETURN
	SELECT S.* FROM dbo.[IfrsDisclosureDetails] S
	JOIN (
		SELECT [IfrsDisclosureId], [Concept], MAX([ValidSince]) AS ValidSince
		FROM [dbo].[IfrsDisclosureDetails]
		WHERE [ValidSince] <= @toDate
		GROUP BY [IfrsDisclosureId], [Concept]
	) H ON S.[IfrsDisclosureId] = H.[IfrsDisclosureId]
	AND S.[Concept] = H.[Concept]
	AND S.[ValidSince] = H.[ValidSince];
