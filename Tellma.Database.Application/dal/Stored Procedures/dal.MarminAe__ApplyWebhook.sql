CREATE PROCEDURE [dal].[MarminAe__ApplyWebhook]
	@MarminAeDocumentId NVARCHAR (50),
	@MarminAeState INT,
	@MarminAeResult NVARCHAR (MAX) = NULL,
	@WebhookEventId UNIQUEIDENTIFIER = NULL,
	@EventTimestamp DATETIMEOFFSET(7) = NULL,
	@RowsAffected INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Applies an asynchronous status update to the document the vendor identifies.
	 *
	 * Shared by BOTH the webhook and the "Refresh e-invoice status" poll, deliberately: the poll
	 * is how this logic gets exercised until the webhook can be live-tested, so they must not be
	 * two different code paths.
	 *
	 * The WHERE clause is the whole concurrency story, and needs no dedup table:
	 *   - MarminAeLastEventId <> @WebhookEventId drops an exact redelivery. Vendor delivery is
	 *     at-least-once, so the same event WILL arrive twice.
	 *   - MarminAeLastEventAt <= @EventTimestamp drops a stale one. The vendor keeps only the
	 *     latest pending delivery per document, so an old redelivery could otherwise overwrite
	 *     Delivered with Pending.
	 *
	 * Both are the WEBHOOK's story, and both are skipped when the caller passes NULL for them,
	 * which is how the poll identifies itself. A poll has no vendor event id and no vendor
	 * event instant: it has just read the current state, so it is never stale and never a
	 * redelivery. Stamping the app server's own clock into MarminAeLastEventAt to satisfy the
	 * guard would be comparing two different clocks, and would suppress every genuine webhook
	 * whose vendor-side instant happened to precede the poll. So a poll always applies, and
	 * leaves both ordering columns exactly as the last real event left them.
	 *
	 * @RowsAffected lets the caller tell "already applied" from "no such document". Both are
	 * answered with HTTP 200: a 4xx/5xx would make the vendor retry-storm on a document that may
	 * not be ours at all, or that simply has not finished committing yet.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = @MarminAeState,
		[MarminAeResult] = @MarminAeResult,
		[MarminAeLastEventId] = ISNULL(@WebhookEventId, [MarminAeLastEventId]),
		[MarminAeLastEventAt] = ISNULL(@EventTimestamp, [MarminAeLastEventAt])
	WHERE [MarminAeDocumentId] = @MarminAeDocumentId
	AND (@WebhookEventId IS NULL OR [MarminAeLastEventId] IS NULL OR [MarminAeLastEventId] <> @WebhookEventId)
	AND (@EventTimestamp IS NULL OR [MarminAeLastEventAt] IS NULL OR [MarminAeLastEventAt] <= @EventTimestamp)
	-- Never move a document backwards out of a terminal state. 10 (Delivered) and -20
	-- (PeppolRejected) are the two verdicts Peppol has actually returned; everything else is
	-- still in flight. MarminAeService maps an absent or unrecognised peppol_status to 1
	-- (Submitted), which is the right reading for a document in flight but would otherwise
	-- silently undo a verdict -- and the vendor's status vocabulary is open, so an unrecognised
	-- value is expected rather than exceptional. Promoting -20 to 1 is the damaging direction:
	-- it crosses the >= 1 thresholds that bll.Documents_Validate__Open uses to refuse a reopen
	-- and that bll.Documents_Validate__Close uses to count a credit note's original invoice, so
	-- a rejected document would become un-reopenable and a credit note could close against an
	-- invoice that never reached the network. Terminal-to-terminal stays allowed.
	AND NOT ([MarminAeState] IN (10, -20) AND @MarminAeState NOT IN (10, -20));

	SET @RowsAffected = @@ROWCOUNT;
END;
GO
