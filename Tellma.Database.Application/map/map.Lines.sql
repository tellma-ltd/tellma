CREATE FUNCTION [map].[Lines]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],			
		[DocumentId],		
		[DefinitionId],		
		[State],	
		[PostingDate],			
		[Memo],
		[Index],
		[Boolean1],
		[Decimal1],
		[Text1],
		-- Auto computed
		[EmployeeId],
		[CustomerId],
		[SupplierId],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM [dbo].[Lines]
);
