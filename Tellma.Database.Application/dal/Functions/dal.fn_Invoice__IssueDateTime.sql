CREATE FUNCTION [dal].[fn_Invoice__IssueDateTime] (
	@InvoiceId INT
)
RETURNS DATETIMEOFFSET
AS
BEGIN
	RETURN (
		SELECT MIN(StateAt)
		FROM dbo.Documents
		WHERE dal.fn_Document__IsZatcaDocument([Id]) = 1
		AND [NotedAgentId] = @InvoiceId
		AND [State] = 1
	)
END