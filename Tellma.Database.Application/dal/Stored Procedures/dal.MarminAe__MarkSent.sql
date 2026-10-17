CREATE PROCEDURE [dal].[MarminAe__MarkSent]
	@Id INT,
	@RowsAffected INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Moves a claimed document from 0 (Submitting) to 2 (SentAwaitingOutcome), in its own
	 * transaction, IMMEDIATELY before the HTTP call to the vendor.
	 *
	 * This is what lets state 0 mean only "claimed, never sent". Once the request leaves, a
	 * timeout or a crash can no longer tell us whether the vendor received it, and the document
	 * must be treated as possibly on the Peppol network: 2 is >= 1, so the reopen guard, the
	 * credit-note original count and the terminal checks all cover it without further change.
	 * "Refresh e-invoice status" and "Resubmit" resolve a document left at 2 by asking the vendor
	 * whether a document with its number exists.
	 *
	 * Conditional on the document still being closed and still at 0, and reported through
	 * @RowsAffected. If either changed since the claim -- reopened in Sandbox, where reopening is
	 * allowed in any state, or picked up by a concurrent resubmit -- the caller must not send.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = 2			-- SentAwaitingOutcome
	WHERE [Id] = @Id
	AND [State] = 1
	AND [MarminAeState] = 0;

	SET @RowsAffected = @@ROWCOUNT;
END;
GO
