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
	-- Never move a document backwards out of a verdict. MarminAeService maps an absent or
	-- unrecognised peppol_status to 1 (Submitted), which is the right reading for a document in
	-- flight but would otherwise silently undo a verdict -- and the vendor's status vocabulary is
	-- open, so an unrecognised value is expected rather than exceptional.
	--
	-- 10 (Delivered, APPROVED) and -30 (PeppolRejected, REJECTED) are final: only another final
	-- verdict may replace them.
	AND NOT ([MarminAeState] IN (10, -30) AND @MarminAeState NOT IN (10, -30))
	-- -20 (PeppolValidationFailed) is not final, because the vendor lets it be resubmitted, after
	-- which it may legitimately reach either verdict. But it must not be pushed back to 1 by an
	-- in-flight status: that crosses the >= 1 thresholds bll.Documents_Validate__Open uses to
	-- refuse a reopen, stranding a document whose remedy is precisely to be reopened, fixed and
	-- resubmitted. Our own resubmission records its outcome through
	-- dal.MarminAe__UpdateDocumentInfo, which this guard does not apply to.
	AND NOT ([MarminAeState] = -20 AND @MarminAeState NOT IN (10, -20, -30));

	SET @RowsAffected = @@ROWCOUNT;
END;
GO
