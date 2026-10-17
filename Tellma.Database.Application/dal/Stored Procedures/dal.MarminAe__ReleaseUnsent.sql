CREATE PROCEDURE [dal].[MarminAe__ReleaseUnsent]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Drops a document from 2 (SentAwaitingOutcome) back to 0 (Submitting).
	 *
	 * Called only once the vendor itself has settled that the earlier request did not take
	 * effect, in one of two ways:
	 *   - no vendor id stored: the vendor has no document with this document's number, so the
	 *     POST never landed;
	 *   - vendor id stored (a PUT resubmission that timed out): the vendor still reports the
	 *     document as VALIDATION_FAILED, so it is resubmittable either way.
	 * Back at 0 the document is resubmittable again (dal.MarminAe__GetInvoices accepts 0).
	 *
	 * Conditional on the state still being 2, so a concurrent outcome write that has meanwhile
	 * recorded the vendor's answer is never undone.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = 0			-- Submitting
	WHERE [Id] = @Id
	AND [MarminAeState] = 2;
END;
GO
