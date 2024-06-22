CREATE FUNCTION [dal].[fn_Document__IsZatcaDocument] (
	@Id INT
)
RETURNS BIT
AS
BEGIN
	RETURN IIF([dal].[fn_Document__ZatcaDocumentType](@Id) IN (N'381', N'383', N'386', N'388'), 1, 0);
END
GO
