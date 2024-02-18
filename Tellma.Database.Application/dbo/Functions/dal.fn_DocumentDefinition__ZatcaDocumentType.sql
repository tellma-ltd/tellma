CREATE FUNCTION [dal].[fn_DocumentDefinition__ZatcaDocumentType] (
	@Id INT
)
RETURNS NVARCHAR (3)
AS
BEGIN
	RETURN (SELECT [ZatcaDocumentType] FROM dbo.DocumentDefinitions WHERE [Id] = @Id)
END
GO