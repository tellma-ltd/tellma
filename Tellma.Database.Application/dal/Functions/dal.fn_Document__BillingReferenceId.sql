CREATE FUNCTION [dal].[fn_Document__BillingReferenceId] (
	@Id INT
)
RETURNS NVARCHAR
AS
BEGIN -- assumes ZatcaDocumentType IN ('381', '383')
	RETURN (
		SELECT CAST(D.[Id] AS NVARCHAR)
		FROM dbo.Documents D
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		WHERE D.[State] = 1
		AND DD.[ZatcaDocumentType] = N'388'
		AND D.[NotedAgentId] = (SELECT [NotedAgentId] FROM dbo.Documents WHERE [Id] = @Id)
	)
END
GO