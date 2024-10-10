CREATE FUNCTION [dal].[fn_DocumentDefinition__IsZatcaDocumentType] (
	@Id INT
)
RETURNS BIT
AS
BEGIN
	RETURN IIF(
				[dal].[fn_DocumentDefinition__ZatcaDocumentType] (@Id)
					IN (N'381', N'383', N'388', N'389'), 1,	0
			)
END
GO