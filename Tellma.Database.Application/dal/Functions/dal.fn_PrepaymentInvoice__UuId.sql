CREATE FUNCTION [dal].[fn_PrepaymentInvoice__UuId] (
	@InvoiceId INT
)
RETURNS UNIQUEIDENTIFIER
AS
BEGIN
	RETURN (
		SELECT [ZatcaUuid]
		FROM dbo.Documents
		WHERE [ZatcaSerialNumber] = dal.[fn_PrepaymentInvoice__DocumentId](@InvoiceId)
	)
END
GO