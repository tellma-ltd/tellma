CREATE FUNCTION [dal].[fn_PrepaymentInvoice__DocumentId] (
	@InvoiceId INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN (
		SELECT CAST(MIN([ZatcaSerialNumber]) AS NVARCHAR (255))
		FROM dbo.Documents
		WHERE dal.fn_Document__IsZatcaDocument([Id]) = 1
		AND [NotedAgentId] = @InvoiceId
		AND [State] = 1
	)
END
GO
