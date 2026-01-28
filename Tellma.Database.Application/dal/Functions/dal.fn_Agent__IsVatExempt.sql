CREATE FUNCTION [dal].[fn_Agent__IsVatExempt] (@Id int)
RETURNS BIT
AS
BEGIN
	RETURN 	(
		SELECT IIF(LK5.[Code] = 'Y', 1, 0)
		FROM [dbo].[Agents] AG
		JOIN dbo.Lookups LK5 ON LK5.Id = AG.[Lookup5Id]
		WHERE AG.[Id] = @Id
	)
END