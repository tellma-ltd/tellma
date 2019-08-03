CREATE FUNCTION [dbo].[fi_IfrsDisclosureDetails] (
	@fromDate Date = NULL, 
	@toDate Date = NULL
) RETURNS TABLE
AS
RETURN
	SELECT S.* FROM dbo.[IfrsDisclosureDetails] S
	JOIN (
		SELECT [IfrsDisclosureId], MAX([ValidSince]) AS ValidSince
		FROM [dbo].[IfrsDisclosureDetails]
		WHERE [ValidSince] <= @toDate
		GROUP BY [IfrsDisclosureId]
	) H ON S.[IfrsDisclosureId] = H.[IfrsDisclosureId] AND S.[ValidSince] = H.[ValidSince];
