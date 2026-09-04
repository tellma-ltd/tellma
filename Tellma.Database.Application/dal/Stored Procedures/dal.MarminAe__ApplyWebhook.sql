CREATE PROCEDURE [dal].[MarminAe__ApplyWebhook]
	@MarminAeDocumentId NVARCHAR (50),
	@MarminAeState INT,
	@MarminAeResult NVARCHAR (MAX) = NULL,
	@WebhookEventId UNIQUEIDENTIFIER,
	@EventTimestamp DATETIMEOFFSET(7),
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
	 * @RowsAffected lets the caller tell "already applied" from "no such document". Both are
	 * answered with HTTP 200: a 4xx/5xx would make the vendor retry-storm on a document that may
	 * not be ours at all, or that simply has not finished committing yet.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = @MarminAeState,
		[MarminAeResult] = @MarminAeResult,
		[MarminAeLastEventId] = @WebhookEventId,
		[MarminAeLastEventAt] = @EventTimestamp
	WHERE [MarminAeDocumentId] = @MarminAeDocumentId
	AND ([MarminAeLastEventId] IS NULL OR [MarminAeLastEventId] <> @WebhookEventId)
	AND ([MarminAeLastEventAt] IS NULL OR [MarminAeLastEventAt] <= @EventTimestamp);

	SET @RowsAffected = @@ROWCOUNT;
END;
GO
