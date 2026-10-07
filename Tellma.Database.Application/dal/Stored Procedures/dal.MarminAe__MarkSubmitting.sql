CREATE PROCEDURE [dal].[MarminAe__MarkSubmitting]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Claims a document for submission to Marmin by stamping state 0 (Submitting).
	 *
	 * Called inside the same transaction as the close (or as the Resubmit action's own claim),
	 * before any HTTP call. It accepts exactly the states dal.MarminAe__GetInvoices returns:
	 *   NULL  never submitted
	 *   0     claimed but never sent
	 *   -10   refused by the vendor, so not on the network
	 *   -20   failing Peppol validation, to be resubmitted with a PUT
	 *
	 * MarminAeDocumentId is deliberately left alone: on a -20 document it is the id the PUT
	 * resubmits, and it is NULL in every other accepted state anyway.
	 *
	 * State 0 is below the reopen guard's bar on purpose. Nothing has been sent at this point;
	 * dal.MarminAe__MarkSent moves the document to 2 immediately before the HTTP call, and that
	 * is the state that says "may already be on the network".
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = 0			-- Submitting
	WHERE [Id] = @Id
	AND ([MarminAeState] IS NULL OR [MarminAeState] IN (0, -10, -20));
END;
GO
