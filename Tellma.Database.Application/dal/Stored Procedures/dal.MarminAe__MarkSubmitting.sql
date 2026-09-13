CREATE PROCEDURE [dal].[MarminAe__MarkSubmitting]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Claims a document for submission to Marmin by stamping state 0 (Submitting).
	 *
	 * Called inside the same transaction as the close, before any HTTP call. Two things depend
	 * on it:
	 *   1. [dal].[MarminAe__GetInvoices] only returns documents whose MarminAeState IS NULL, so
	 *      once this has run the document can never be picked up for submission a second time.
	 *   2. [bll].[Documents_Validate__Open] blocks reopening at MarminAeState >= 1, and state 0
	 *      deliberately sits below that: a document that never reached the vendor must still be
	 *      reopenable, otherwise a failed submission would strand it permanently.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = 0			-- Submitting
	WHERE [Id] = @Id
	AND [MarminAeState] IS NULL;
END;
GO
