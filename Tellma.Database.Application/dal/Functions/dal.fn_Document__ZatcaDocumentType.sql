CREATE FUNCTION [dal].[fn_Document__ZatcaDocumentType] (
	@Id INT
)
RETURNS NVARCHAR (3)
AS
BEGIN
	DECLARE @ZatcaDocumentType NVARCHAR (3);
	SELECT @ZatcaDocumentType = DD.[ZatcaDocumentType]
	FROM dbo.Documents D
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	WHERE D.[Id] = @Id;
	RETURN @ZatcaDocumentType
END
GO