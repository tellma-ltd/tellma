CREATE FUNCTION [map].[Currencies] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[Name],
		[Name2],
		[Name3],
		[Description],
		[Description2],
		[Description3],
		[NumericCode],
		[E],
		[IsActive],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM [dbo].[Currencies]
);
